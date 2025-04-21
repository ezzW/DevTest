// Emtelaak.UserRegistration.API/Middleware/ExceptionHandlingMiddleware.cs
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FluentValidation;
using ApplicationValidationException = Emtelaak.UserRegistration.Application.Exceptions.ValidationException;

namespace Emtelaak.UserRegistration.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            object response;

            switch (exception)
            {
                case ValidationException validationEx:
                    _logger.LogWarning("Validation error: {Message}", validationEx.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    // Extract errors from FluentValidation's ValidationException
                    var errors = new Dictionary<string, string[]>();
                    foreach (var error in validationEx.Errors)
                    {
                        string key = error.PropertyName;
                        if (!errors.ContainsKey(key))
                        {
                            errors[key] = new[] { error.ErrorMessage };
                        }
                        else
                        {
                            var existingMessages = errors[key];
                            var newMessages = new string[existingMessages.Length + 1];
                            Array.Copy(existingMessages, newMessages, existingMessages.Length);
                            newMessages[existingMessages.Length] = error.ErrorMessage;
                            errors[key] = newMessages;
                        }
                    }

                    response = new { errors };
                    break;

                case ApplicationValidationException appValidationEx:
                    _logger.LogWarning("Application validation error: {Message}", appValidationEx.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new { errors = appValidationEx.Errors };
                    break;

                case ApplicationException appEx:
                    _logger.LogWarning("Application error: {Message}", appEx.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    var generalErrors = new Dictionary<string, string[]>
                    {
                        { "general", new[] { appEx.Message } }
                    };
                    response = new { errors = generalErrors };
                    break;

                default:
                    _logger.LogError(exception, "Unhandled exception");
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    var defaultErrors = new Dictionary<string, string[]>
                    {
                        { "general", new[] { "An unexpected error occurred. Please try again later." } }
                    };
                    response = new { errors = defaultErrors };
                    break;
            }

            var result = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(result);
        }
    }
}