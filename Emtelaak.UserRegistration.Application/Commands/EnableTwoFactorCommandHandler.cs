// Emtelaak.UserRegistration.Application/Commands/EnableTwoFactorCommandHandler.cs
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class EnableTwoFactorCommandHandler : IRequestHandler<EnableTwoFactorCommand, EnableTwoFactorResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;
        private readonly ISmsService _smsService;
        private readonly ILogger<EnableTwoFactorCommandHandler> _logger;
        private readonly IEmailService _emailService;

        public EnableTwoFactorCommandHandler(
            IUserRepository userRepository,
            IIdentityService identityService,
            ISmsService smsService,
            ILogger<EnableTwoFactorCommandHandler> logger,
            IEmailService emailService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService;
        }

        public async Task<EnableTwoFactorResultDto> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing enable two-factor request for user: {UserId}, Method: {Method}, Enable: {Enable}",
                    request.UserId, request.Method, request.Enable);

                // Get user
                var domainUser = await _userRepository.GetByIdAsync(request.UserId);
                var user = await _identityService.FindUserByEmailAsync(domainUser.Email);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    throw new ApplicationException($"User not found: {request.UserId}");
                }

                // If disabling MFA
                if (!request.Enable)
                {
                    var disableResult = await _identityService.SetTwoFactorEnabledAsync(user, false);
                    if (!disableResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to disable two-factor for user {UserId}: {Errors}",
                            request.UserId, string.Join(", ", disableResult.Errors.Select(e => e.Description)));
                        return new EnableTwoFactorResultDto
                        {
                            Success = false,
                            Message = "Failed to disable two-factor authentication"
                        };
                    }

                    // Update user preference
                    var userPreference = await _userRepository.GetUserPreferenceByUserIdAsync(request.UserId);
                    if (userPreference != null)
                    {
                        userPreference.TwoFactorEnabled = false;
                        userPreference.UpdatedAt = DateTime.UtcNow;
                        await _userRepository.UpdateUserPreferenceAsync(userPreference);
                    }

                    // Add activity log
                    var activityLog = new ActivityLog
                    {
                        UserId = request.UserId,
                        ActivityType = ActivityType.TwoFactorDisabled,
                        Timestamp = DateTime.UtcNow,
                        Status = ActivityStatus.Success,
                        Details = $"{{\"method\":\"{request.Method}\"}}"
                    };
                    await _userRepository.AddActivityLogAsync(activityLog);

                    _logger.LogInformation("Two-factor disabled for user: {UserId}", request.UserId);

                    return new EnableTwoFactorResultDto
                    {
                        Success = true,
                        Message = "Two-factor authentication has been disabled"
                    };
                }

                // If enabling MFA
                switch (request.Method.ToLower())
                {
                    case "authenticator":
                        // Generate Authenticator key and QR code
                        var setupInfo = await GenerateAuthenticatorSetupInfo(user);
                        return setupInfo;

                    case "sms":
                        // Send verification code via SMS
                        var code = await _identityService.GenerateTwoFactorTokenAsync(user, "Phone");
                        var phoneNumber = user.PhoneNumber;

                        if (string.IsNullOrEmpty(phoneNumber))
                        {
                            return new EnableTwoFactorResultDto
                            {
                                Success = false,
                                Message = "Phone number is required for SMS verification"
                            };
                        }

                        await _smsService.SendTwoFactorCodeAsync(phoneNumber, code);

                        return new EnableTwoFactorResultDto
                        {
                            Success = true,
                            Message = "Verification code sent to your phone. Please verify to enable SMS two-factor authentication."
                        };

                    case "email":
                        // Check if email is verified
                        if (!await _identityService.IsEmailConfirmedAsync(user))
                        {
                            return new EnableTwoFactorResultDto
                            {
                                Success = false,
                                Message = "You must verify your email address before enabling email two-factor authentication."
                            };
                        }

                        // Generate email verification code
                        var emailCode = await _identityService.GenerateTwoFactorTokenAsync(user, "Email");

                        // Send verification code via email
                        await _emailService.SendTwoFactorCodeAsync(user.Email, user.FirstName, emailCode);

                        _logger.LogInformation("Two-factor setup code sent via Email to: {Email}", user.Email);

                        return new EnableTwoFactorResultDto
                        {
                            Success = true,
                            Message = "Verification code sent to your email. Please enter this code to complete setup of email two-factor authentication."
                        };

                    default:
                        return new EnableTwoFactorResultDto
                        {
                            Success = false,
                            Message = "Invalid two-factor method"
                        };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling two-factor: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<EnableTwoFactorResultDto> GenerateAuthenticatorSetupInfo(Domain.Models.AuthUserModel user)
        {
            // Generate authenticator key
            string authenticatorKey = await _identityService.GenerateTwoFactorTokenAsync(user, "Authenticator");

            // Generate the QR code
            string qrCodeData = $"otpauth://totp/Emtelaak:{user.Email}?secret={authenticatorKey}&issuer=Emtelaak";

            // Note: In a production application, you'd want to generate a proper QR code image
            // For this example, we'll just return the data needed for the client to generate it

            return new EnableTwoFactorResultDto
            {
                Success = true,
                Message = "Please scan the QR code with your authenticator app and verify the code to enable authenticator two-factor authentication.",
                SetupCode = authenticatorKey,
                QrCodeUrl = qrCodeData
            };
        }
    }
}