// Emtelaak.UserRegistration.Application/Commands/VerifyPhoneCommandHandler.cs
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class VerifyPhoneCommandHandler : IRequestHandler<VerifyPhoneCommand, PhoneVerificationResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;
        private readonly ILogger<VerifyPhoneCommandHandler> _logger;

        public VerifyPhoneCommandHandler(
            IUserRepository userRepository,
            IIdentityService identityService,
            ILogger<VerifyPhoneCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PhoneVerificationResultDto> Handle(VerifyPhoneCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing phone verification for: {PhoneNumber}", request.VerificationData.PhoneNumber);

                // Get user by phone number
                var users = await _userRepository.GetAsync(u => u.PhoneNumber == request.VerificationData.PhoneNumber);
                var user = users.FirstOrDefault();

                if (user == null)
                {
                    _logger.LogWarning("Phone verification failed: User not found with phone {PhoneNumber}",
                        request.VerificationData.PhoneNumber);

                    return new PhoneVerificationResultDto
                    {
                        Verified = false,
                        Message = "Invalid phone number or verification code",
                        RemainingAttempts = 3 // Fixed value for security reasons
                    };
                }

                // Check if phone is already verified
                if (user.PhoneVerified)
                {
                    _logger.LogInformation("Phone already verified for user with phone: {PhoneNumber}",
                        request.VerificationData.PhoneNumber);

                    return new PhoneVerificationResultDto
                    {
                        Verified = true,
                        NextStep = "login",
                        Message = "Phone already verified"
                    };
                }

                // Get identity user
                var identityUser = await _identityService.FindUserByEmailAsync(user.Email);
                if (identityUser == null)
                {
                    _logger.LogWarning("Phone verification failed: Identity user not found for email {Email}", user.Email);

                    return new PhoneVerificationResultDto
                    {
                        Verified = false,
                        Message = "Invalid phone number or verification code",
                        RemainingAttempts = 3
                    };
                }

                // Verify phone token
                var result = await _identityService.VerifyPhoneNumberAsync(identityUser, request.VerificationData.VerificationCode);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Phone verification failed for user with phone: {PhoneNumber}, Errors: {Errors}",
                        request.VerificationData.PhoneNumber,
                        string.Join(", ", result.Errors.Select(e => e.Description)));

                    return new PhoneVerificationResultDto
                    {
                        Verified = false,
                        Message = "Invalid verification code",
                        RemainingAttempts = 3
                    };
                }

                // Update domain user phone verification status
                user.PhoneVerified = true;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                // Add activity log
                var activityLog = new ActivityLog
                {
                    UserId = user.Id,
                    ActivityType = ActivityType.PhoneVerification,
                    Timestamp = DateTime.UtcNow,
                    Status = ActivityStatus.Success,
                    Details = $"{{\"phoneNumber\":\"{user.PhoneNumber}\"}}"
                };
                await _userRepository.AddActivityLogAsync(activityLog);

                // Determine next step
                string nextStep = user.EmailVerified ? "KYC" : "emailVerification";

                _logger.LogInformation("Phone successfully verified for user: {PhoneNumber}",
                    request.VerificationData.PhoneNumber);

                return new PhoneVerificationResultDto
                {
                    Verified = true,
                    NextStep = nextStep,
                    Message = "Phone verified successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during phone verification: {Message}", ex.Message);
                throw;
            }
        }
    }
}