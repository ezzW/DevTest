// Emtelaak.UserRegistration.Application/Commands/ResetPasswordCommandHandler.cs
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResultDto>
    {
        private readonly IIdentityService _identityService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ResetPasswordCommandHandler> _logger;

        public ResetPasswordCommandHandler(
            IIdentityService identityService,
            IUserRepository userRepository,
            ILogger<ResetPasswordCommandHandler> logger)
        {
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResetPasswordResultDto> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing password reset");

                // Validate password match
                if (request.ResetData.NewPassword != request.ResetData.ConfirmPassword)
                {
                    _logger.LogWarning("Password reset failed: Passwords do not match");

                    return new ResetPasswordResultDto
                    {
                        Success = false,
                        Message = "Passwords do not match",
                        Errors = new Dictionary<string, string[]>
                        {
                            ["confirmPassword"] = new[] { "The new password and confirmation password do not match." }
                        }
                    };
                }

                // Parse token to get user email
                string email = null;
                try
                {
                    // In a real implementation, you'd use a proper token validation method
                    // This is just a placeholder since reset tokens are usually encrypted
                    var tokenParts = request.ResetData.Token.Split('|');
                    if (tokenParts.Length > 1)
                    {
                        email = tokenParts[0];
                    }
                }
                catch
                {
                    // Token parsing failed, we'll handle it below
                }

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Password reset failed: Invalid token format");

                    return new ResetPasswordResultDto
                    {
                        Success = false,
                        Message = "Invalid reset token",
                        Errors = new Dictionary<string, string[]>
                        {
                            ["token"] = new[] { "The password reset token is invalid or has expired." }
                        }
                    };
                }

                // Get user by email
                var user = await _identityService.FindUserByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Password reset failed: User not found for email {Email}", email);

                    return new ResetPasswordResultDto
                    {
                        Success = false,
                        Message = "Invalid reset token",
                        Errors = new Dictionary<string, string[]>
                        {
                            ["token"] = new[] { "The password reset token is invalid or has expired." }
                        }
                    };
                }

                // Reset password
                var result = await _identityService.ResetPasswordAsync(user, request.ResetData.Token, request.ResetData.NewPassword);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Password reset failed for user {Email}: {Errors}",
                        email,
                        string.Join(", ", result.Errors.Select(e => e.Description)));

                    var errors = result.Errors.GroupBy(e => e.Code)
                        .ToDictionary(
                            g => g.Key.ToLower(),
                            g => g.Select(e => e.Description).ToArray()
                        );

                    return new ResetPasswordResultDto
                    {
                        Success = false,
                        Message = "Failed to reset password",
                        Errors = errors
                    };
                }

                // Find domain user
                var domainUser = await _userRepository.GetUserByEmailAsync(email);
                if (domainUser != null)
                {
                    // Add activity log
                    var activityLog = new ActivityLog
                    {
                        UserId = domainUser.Id,
                        ActivityType = ActivityType.PasswordReset,
                        Timestamp = DateTime.UtcNow,
                        Status = ActivityStatus.Success
                    };
                    await _userRepository.AddActivityLogAsync(activityLog);
                }

                _logger.LogInformation("Password successfully reset for user: {Email}", email);

                return new ResetPasswordResultDto
                {
                    Success = true,
                    Message = "Password has been reset successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset: {Message}", ex.Message);

                return new ResetPasswordResultDto
                {
                    Success = false,
                    Message = "An error occurred while resetting your password",
                    Errors = new Dictionary<string, string[]>
                    {
                        ["general"] = new[] { "An unexpected error occurred. Please try again later." }
                    }
                };
            }
        }
    }
}