// Emtelaak.UserRegistration.Application/Commands/VerifyEmailCommandHandler.cs
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, EmailVerificationResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;
        private readonly ILogger<VerifyEmailCommandHandler> _logger;

        public VerifyEmailCommandHandler(
            IUserRepository userRepository,
            IIdentityService identityService,
            ILogger<VerifyEmailCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<EmailVerificationResultDto> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing email verification for: {Email}", request.VerificationData.Email);

                // Get identity user
                var identityUser = await _identityService.FindUserByEmailAsync(request.VerificationData.Email);
                if (identityUser == null)
                {
                    _logger.LogWarning("Email verification failed: User not found for email {Email}", request.VerificationData.Email);
                    return new EmailVerificationResultDto
                    {
                        Verified = false,
                        Message = "Invalid email or verification code",
                        RemainingAttempts = 3 // Fixed value for security reasons
                    };
                }

                // Check if email is already verified
                if (identityUser.EmailConfirmed)
                {
                    _logger.LogInformation("Email already verified for user: {Email}", request.VerificationData.Email);
                    return new EmailVerificationResultDto
                    {
                        Verified = true,
                        NextStep = "login",
                        Message = "Email already verified"
                    };
                }

                // Verify email code
                var result = await _identityService.VerifyEmailWithCodeAsync(identityUser, request.VerificationData.VerificationCode);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Email verification failed for user: {Email}, Errors: {Errors}",
                        request.VerificationData.Email, string.Join(", ", result.Errors.Select(e => e.Description)));

                    return new EmailVerificationResultDto
                    {
                        Verified = false,
                        Message = "Invalid verification code",
                        RemainingAttempts = 3 // Fixed value for security reasons
                    };
                }

                // Update domain user email verification status
                var user = await _userRepository.GetUserByEmailAsync(request.VerificationData.Email);
                if (user != null)
                {
                    user.EmailVerified = true;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user);

                    // Add activity log
                    var activityLog = new ActivityLog
                    {
                        UserId = user.Id,
                        ActivityType = ActivityType.EmailVerification,
                        Timestamp = DateTime.UtcNow,
                        Status = ActivityStatus.Success,
                        Details = $"{{\"email\":\"{user.Email}\"}}"
                    };
                    await _userRepository.AddActivityLogAsync(activityLog);
                }

                // Determine next step
                string nextStep = user.PhoneVerified ? "KYC" : "phoneVerification";

                _logger.LogInformation("Email successfully verified for user: {Email}", request.VerificationData.Email);
                return new EmailVerificationResultDto
                {
                    Verified = true,
                    NextStep = nextStep,
                    Message = "Email verified successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification: {Message}", ex.Message);
                throw;
            }
        }
    }
}