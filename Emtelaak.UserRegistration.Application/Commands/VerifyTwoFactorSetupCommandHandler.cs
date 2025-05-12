// Emtelaak.UserRegistration.Application/Commands/VerifyTwoFactorSetupCommandHandler.cs
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class VerifyTwoFactorSetupCommandHandler : IRequestHandler<VerifyTwoFactorSetupCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;
        private readonly ILogger<VerifyTwoFactorSetupCommandHandler> _logger;

        public VerifyTwoFactorSetupCommandHandler(
            IUserRepository userRepository,
            IIdentityService identityService,
            ILogger<VerifyTwoFactorSetupCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(VerifyTwoFactorSetupCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Verifying two-factor setup for user: {UserId}, Method: {Method}",
                    request.UserId, request.Method);

                // Get user
                var domainUser = await _userRepository.GetByIdAsync(request.UserId);
                var user = await _identityService.FindUserByEmailAsync(domainUser.Email);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    throw new ApplicationException($"User not found: {request.UserId}");
                }

                // Verify the code
                bool isValid = false;
                switch (request.Method.ToLower())
                {
                    case "authenticator":
                        isValid = await _identityService.VerifyTwoFactorTokenAsync(user, "Authenticator", request.VerificationCode);
                        break;

                    case "sms":
                        isValid = await _identityService.VerifyTwoFactorTokenAsync(user, "Phone", request.VerificationCode);
                        break;

                    case "email":
                        isValid = await _identityService.VerifyTwoFactorTokenAsync(user, "Email", request.VerificationCode);
                        break;

                    default:
                        _logger.LogWarning("Invalid two-factor method: {Method}", request.Method);
                        return false;
                }

                if (!isValid)
                {
                    _logger.LogWarning("Invalid verification code for user: {UserId}, Method: {Method}",
                        request.UserId, request.Method);
                    return false;
                }

                // Enable two-factor authentication
                var enableResult = await _identityService.SetTwoFactorEnabledAsync(user, true);
                if (!enableResult.Succeeded)
                {
                    _logger.LogWarning("Failed to enable two-factor for user {UserId}: {Errors}",
                        request.UserId, string.Join(", ", enableResult.Errors.Select(e => e.Description)));
                    return false;
                }

                // Update user preference
                var userPreference = await _userRepository.GetUserPreferenceByUserIdAsync(request.UserId);
                if (userPreference != null)
                {
                    userPreference.TwoFactorEnabled = true;
                    userPreference.TwoFactorMethod = GetTwoFactorMethod(request.Method);
                    userPreference.UpdatedAt = DateTime.UtcNow;
                    await _userRepository.UpdateUserPreferenceAsync(userPreference);
                }

                // Add activity log
                var activityLog = new ActivityLog
                {
                    UserId = request.UserId,
                    ActivityType = ActivityType.TwoFactorEnabled,
                    Timestamp = DateTime.UtcNow,
                    Status = ActivityStatus.Success,
                    Details = $"{{\"method\":\"{request.Method}\"}}"
                };
                await _userRepository.AddActivityLogAsync(activityLog);

                _logger.LogInformation("Two-factor enabled for user: {UserId}, Method: {Method}",
                    request.UserId, request.Method);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying two-factor setup: {Message}", ex.Message);
                throw;
            }
        }

        private TwoFactorMethod GetTwoFactorMethod(string method)
        {
            return method.ToLower() switch
            {
                "authenticator" => TwoFactorMethod.Authenticator,
                "sms" => TwoFactorMethod.SMS,
                "email" => TwoFactorMethod.Email,
                _ => TwoFactorMethod.SMS // Default
            };
        }
    }
}