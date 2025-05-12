using System;
using System.Reflection;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Application.Mappings;
using Emtelaak.UserRegistration.Application.Services;
using Emtelaak.UserRegistration.Infrastructure.Data;
using Emtelaak.UserRegistration.Infrastructure.Identity;
using Emtelaak.UserRegistration.Infrastructure.Repositories;
using Emtelaak.UserRegistration.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Emtelaak.UserRegistration.Application.Behaviors;
using FluentValidation;
using Emtelaak.UserRegistration.API.Middleware;
using Emtelaak.UserRegistration.Application.Commands;
using Emtelaak.UserRegistration.Application.Validators;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/emtelaak-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Emtelaak User Registration API",
        Version = "v1",
        Description = "API for user registration, authentication, and profile management"
    });

    // Configure Swagger to use JWT authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.MigrationsAssembly("Emtelaak.UserRegistration.Infrastructure");
        }));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Your identity options here
    options.SignIn.RequireConfirmedEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Configure Identity and IdentityServer
builder.Services.AddIdentityServer()
    .AddDeveloperSigningCredential() // Use AddSigningCredential with a real certificate in production
    .AddInMemoryIdentityResources(IdentityServerConfig.GetIdentityResources())
    .AddInMemoryApiResources(IdentityServerConfig.GetApiResources())
    .AddInMemoryApiScopes(IdentityServerConfig.GetApiScopes())
    .AddInMemoryClients(IdentityServerConfig.GetClients(builder.Configuration))
    .AddAspNetIdentity<ApplicationUser>();

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Authority = builder.Configuration["Authentication:Authority"]; // Your IdentityServer URL
    options.RequireHttpsMetadata = builder.Environment.IsProduction(); // False for development
    options.Audience = "emtelaak_api";

    // Add these configurations
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Authentication:Issuer"],
        ValidAudience = builder.Configuration["Authentication:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Authentication:SecretKey"]))
    };

    // Add this to see token validation errors in logs
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError("Authentication failed: {Exception}", context.Exception);
            return Task.CompletedTask;
        }
    };
    // This prevents redirects for API requests
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            // Only handle API requests
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";

                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    message = "Unauthorized. Authentication is required."
                });

                await context.Response.WriteAsync(result);

                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<Program>>();
                logger.LogInformation("API authentication challenge issued - returning 401 Unauthorized");
            }
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Token validated successfully");
            return Task.CompletedTask;
        }
    };
});

// Configure AutoMapper
builder.Services.AddAutoMapper(
    typeof(Emtelaak.UserRegistration.Application.Mappings.MappingProfile).Assembly,
    typeof(Emtelaak.UserRegistration.Infrastructure.Mappings.InfrastructureMappingProfile).Assembly);

// Configure MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<RegisterUserCommandValidator>());

// This registers all validators from the Application assembly
builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserCommandValidator>();

//builder.Services.AddValidatorsFromAssembly(Assembly.Load("Emtelaak.UserRegistration.Application"));
//builder.Services.AddValidatorsFromAssembly(Assembly.Load("Emtelaak.UserRegistration.Application"));
//builder.Services.AddScoped<IValidator<RegisterUserCommand>, RegisterUserCommandValidator>();

// And this adds the validation behavior to the MediatR pipeline
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));




// Configure Application Services
builder.Services.AddScoped<ApplicationUserManager>();
builder.Services.AddScoped<Emtelaak.UserRegistration.Application.Interfaces.IUserRepository, UserRepository>();
builder.Services.AddScoped<Emtelaak.UserRegistration.Application.Interfaces.IRoleRepository, RoleRepository>();
builder.Services.AddScoped<Emtelaak.UserRegistration.Application.Interfaces.IIdentityService, IdentityService>();
builder.Services.AddScoped<Emtelaak.UserRegistration.Application.Interfaces.ITokenService, TokenService>();
builder.Services.AddScoped<Emtelaak.UserRegistration.Application.Interfaces.IEmailService, EmailService>();
builder.Services.AddScoped<Emtelaak.UserRegistration.Application.Interfaces.ISmsService, SmsService>();
builder.Services.AddScoped<Emtelaak.UserRegistration.Application.Interfaces.IKycVerificationService, KycVerificationService>();
builder.Services.AddScoped<Emtelaak.UserRegistration.Application.Interfaces.IDocumentStorageService, AzureBlobStorageService>();
builder.Services.AddScoped<IDocumentRequirementsService, DocumentRequirementsService>();
builder.Services.AddScoped<UserManager<ApplicationUser>>(provider => provider.GetRequiredService<ApplicationUserManager>());
builder.Services.AddScoped<Emtelaak.UserRegistration.Application.Interfaces.ICountryRepository,CountryRepository>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder
            .WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
                new[] { "https://localhost:3000" })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});



// Configure Redis for distributed caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    options.InstanceName = "Emtelaak:";
});

var app = builder.Build();

// Configure the HTTP request pipeline

    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();


app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRouting();
app.UseSerilogRequestLogging();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        // Seed default data
        await SeedData.Initialize(app.Services, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}

app.Run();