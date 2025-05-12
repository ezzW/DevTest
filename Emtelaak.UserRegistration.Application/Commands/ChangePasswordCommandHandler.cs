// Emtelaak.UserRegistration.Application/Commands/ChangePasswordCommandHandler.cs
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, PasswordChangeResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;
        private readonly ILogger<ChangePasswordCommandHandler> _logger;

        public ChangePasswordCommandHandler(
            IUserRepository userRepository,
            IIdentityService identityService,
            ILogger<ChangePasswordCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PasswordChangeResultDto> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Changing password for user: {UserId}", request.UserId);

                // Validate password match
                if (request.PasswordData.NewPassword != request.PasswordData.ConfirmPassword)
                {
                    _logger.LogWarning("Password change failed: Passwords do not match for user {UserId}", request.UserId);

                    return new PasswordChangeResultDto
                    {
                        Updated = false,
                        Message = "New password and confirmation password do not match",
                        Errors = new Dictionary<string, string[]>
                        {
                            ["confirmPassword"] = new[] { "The new password and confirmation password do not match." }
                        }
                    };
                }

                // Get identity user
                var domainUser = await _userRepository.GetByIdAsync(request.UserId);
                var user = await _identityService.FindUserByEmailAsync(domainUser.Email);
                if (user == null)
                {
                    _logger.LogWarning("Password change failed: User not found {UserId}", request.UserId);

                    return new PasswordChangeResultDto
                    {
                        Updated = false,
                        Message = "User not found",
                        Errors = new Dictionary<string, string[]>
                        {
                            ["user"] = new[] { "User not found." }
                        }
                    };
                }

                // Change password
                var result = await _identityService.ChangePasswordAsync(
                    user,
                    request.PasswordData.CurrentPassword,
                    request.PasswordData.NewPassword);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Password change failed for user {UserId}: {Errors}",
                        request.UserId,
                        string.Join(", ", result.Errors.Select(e => e.Description)));

                    var errors = result.Errors.GroupBy(e => e.Code.ToLower())
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.Description).ToArray()
                        );

                    return new PasswordChangeResultDto
                    {
                        Updated = false,
                        Message = "Failed to change password",
                        Errors = errors
                    };
                }

                // Add activity log
                var activityLog = new ActivityLog
                {
                    UserId = request.UserId,
                    ActivityType = ActivityType.PasswordChange,
                    Timestamp = DateTime.UtcNow,
                    Status = ActivityStatus.Success
                };
                await _userRepository.AddActivityLogAsync(activityLog);

                _logger.LogInformation("Password changed successfully for user: {UserId}", request.UserId);

                return new PasswordChangeResultDto
                {
                    Updated = true,
                    Message = "Password updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password: {Message}", ex.Message);
                throw;
            }
        }
    }
}