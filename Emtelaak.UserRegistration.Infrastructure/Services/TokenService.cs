// Emtelaak.UserRegistration.Infrastructure/Services/TokenService.cs
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Duende.IdentityModel;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Models;
using Emtelaak.UserRegistration.Infrastructure.Data;
using Emtelaak.UserRegistration.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Emtelaak.UserRegistration.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenService> _logger;
        private readonly IIdentityService _identityService;

        public TokenService(
            UserManager<ApplicationUser> userManager,
            AppDbContext dbContext,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<TokenService> logger,
            IIdentityService identityService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _identityService = identityService;
        }

        public async Task<TokenResult> GenerateAuthTokensAsync(AuthUserModel user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.DomainUserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("name", $"{user.FirstName} {user.LastName}"),
                new Claim("domainUserId", user.DomainUserId.ToString()),
            };

            // Add roles to claims
            var roles = await _identityService.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authentication:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Authentication:Issuer"],
                audience: _configuration["Authentication:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            string refreshToken = GenerateRefreshToken();

            return new TokenResult
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                ExpiresIn = 3600 // 1 hour in seconds
            };
        }
        

        public async Task<string> GenerateMfaTokenAsync(AuthUserModel user)
        {
            try
            {
                _logger.LogInformation("Generating MFA token for user: {UserId}", user.Id);

                // Create claims for MFA token
                var claims = new List<Claim>
                {
                    new Claim(JwtClaimTypes.Subject, user.Id.ToString()),
                    new Claim(JwtClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("mfa", "true")
                };

                // Get signing credentials
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authentication:SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Set token expiration (10 minutes for MFA tokens)
                var expiry = DateTime.UtcNow.AddMinutes(10);

                // Create JWT token
                var token = new JwtSecurityToken(
                    issuer: _configuration["Authentication:Issuer"],
                    audience: _configuration["Authentication:Audience"],
                    claims: claims,
                    expires: expiry,
                    signingCredentials: creds);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating MFA token: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<(bool isValid, string email)> ValidateMfaTokenAsync(string token)
        {
            try
            {
                _logger.LogInformation("Validating MFA token");
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authentication:SecretKey"]));

                // Set validation parameters
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Authentication:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Authentication:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // Validate token
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

                // Check if it's an MFA token
                var mfaClaim = principal.FindFirst("mfa");
                if (mfaClaim == null || mfaClaim.Value != "true")
                {
                    _logger.LogWarning("Token is not an MFA token");
                    return (false, string.Empty);
                }

                // Get email from token
                var emailClaim = principal.FindFirst(JwtClaimTypes.Email);
                if (emailClaim == null)
                {
                    _logger.LogWarning("MFA token does not contain email claim");
                    return (false, string.Empty);
                }

                return (true, emailClaim.Value);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "MFA token validation failed: {Message}", ex.Message);
                return (false, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating MFA token: {Message}", ex.Message);
                throw;
            }
        }
        public async Task<TokenResult> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation("Refreshing token");

                // Find the user session with this refresh token
                var session = await _dbContext.UserSessions
                    .FirstOrDefaultAsync(s => s.Token == refreshToken && s.IsActive);

                if (session == null)
                {
                    _logger.LogWarning("Refresh token not found or not active");
                    throw new SecurityTokenException("Invalid refresh token");
                }

                // Check if token is expired
                if (session.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("Refresh token expired");
                    throw new SecurityTokenException("Refresh token expired");
                }

                // Find the user
                var user = await _userManager.FindByIdAsync(session.UserId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("User not found for refresh token");
                    throw new SecurityTokenException("User not found");
                }

                // Revoke the current refresh token
                session.IsActive = false;
                session.RevokedAt = DateTime.UtcNow;
                session.RevokedReason = "Refresh token used";
                await _dbContext.SaveChangesAsync();

                // Generate new tokens
                var authUser = _mapper.Map<AuthUserModel>(user);
                return await GenerateAuthTokensAsync(authUser);
            }
            catch (SecurityTokenException)
            {
                // Rethrow security token exceptions
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token: {Message}", ex.Message);
                throw;
            }
        }

        public async Task RevokeTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation("Revoking token");

                // Find the user session with this refresh token
                var session = await _dbContext.UserSessions
                    .FirstOrDefaultAsync(s => s.Token == refreshToken);

                if (session != null)
                {
                    session.IsActive = false;
                    session.RevokedAt = DateTime.UtcNow;
                    session.RevokedReason = "User logout";
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token: {Message}", ex.Message);
                throw;
            }
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}