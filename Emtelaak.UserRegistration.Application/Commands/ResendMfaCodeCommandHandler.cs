// Emtelaak.UserRegistration.Application/Commands/ResendMfaCodeCommandHandler.cs
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class ResendMfaCodeCommandHandler : IRequestHandler<ResendMfaCodeCommand, ResendMfaCodeResultDto>
        {
            private readonly ITokenService _tokenService;
            private readonly IIdentityService _identityService;
            private readonly ISmsService _smsService;
            private readonly IEmailService _emailService;
            private readonly IUserRepository _userRepository;
            private readonly ILogger<ResendMfaCodeCommandHandler> _logger;

            public ResendMfaCodeCommandHandler(
                ITokenService tokenService,
                IIdentityService identityService,
                ISmsService smsService,
                IEmailService emailService,
                IUserRepository userRepository,
                ILogger<ResendMfaCodeCommandHandler> logger)
            {
                _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
                _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
                _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
                _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
                _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public async Task<ResendMfaCodeResultDto> Handle(ResendMfaCodeCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    _logger.LogInformation("Processing resend MFA code request for method: {Method}", request.Method);

                    // First validate the MFA token
                    var (isValidToken, email) = await _tokenService.ValidateMfaTokenAsync(request.MfaToken);
                    if (!isValidToken)
                    {
                        _logger.LogWarning("Resend MFA code failed: Invalid MFA token");
                        return new ResendMfaCodeResultDto
                        {
                            Success = false,
                            Message = "Invalid or expired session. Please try logging in again."
                        };
                    }

                    // Get the user from email
                    var identityUser = await _identityService.FindUserByEmailAsync(email);
                    if (identityUser == null)
                    {
                        _logger.LogWarning("User not found for email: {Email}", email);
                        return new ResendMfaCodeResultDto
                        {
                            Success = false,
                            Message = "User not found. Please try logging in again."
                        };
                    }

                    // Get domain user
                    var domainUser = await _userRepository.GetUserByEmailAsync(email);
                    if (domainUser == null)
                    {
                        _logger.LogWarning("Domain user not found for email: {Email}", email);
                        return new ResendMfaCodeResultDto
                        {
                            Success = false,
                            Message = "An error occurred while processing your request."
                        };
                    }

                    // Handle resend based on method
                    switch (request.Method.ToLower())
                    {
                        case "sms":
                            return await ResendSmsCode(identityUser, domainUser.PhoneNumber);

                        case "email":
                            return await ResendEmailCode(identityUser, domainUser.Email, domainUser.FirstName);

                        case "authenticator":
                            return new ResendMfaCodeResultDto
                            {
                                Success = true,
                                Message = "Your authenticator app generates codes automatically. Please open your authenticator app to get a new code."
                            };

                        default:
                            _logger.LogWarning("Invalid MFA method: {Method}", request.Method);
                            return new ResendMfaCodeResultDto
                            {
                                Success = false,
                                Message = "Invalid MFA method."
                            };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error resending MFA code: {Message}", ex.Message);
                    return new ResendMfaCodeResultDto
                    {
                        Success = false,
                        Message = "An error occurred while resending the verification code."
                    };
                }
            }

            private async Task<ResendMfaCodeResultDto> ResendSmsCode(Domain.Models.AuthUserModel user, string phoneNumber)
            {
                // Check if phone number is available
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    _logger.LogWarning("Cannot resend SMS code: Phone number not available for user {Id}", user.Id);
                    return new ResendMfaCodeResultDto
                    {
                        Success = false,
                        Message = "Phone number not available. Please contact support."
                    };
                }

                // Generate new SMS verification code
                var smsCode = await _identityService.GenerateTwoFactorTokenAsync(user, "Phone");

                // Send the code via SMS
                await _smsService.SendTwoFactorCodeAsync(phoneNumber, smsCode);

                _logger.LogInformation("MFA code resent via SMS to: {PhoneNumber}", phoneNumber);

                return new ResendMfaCodeResultDto
                {
                    Success = true,
                    Message = "A new verification code has been sent to your phone."
                };
            }

            private async Task<ResendMfaCodeResultDto> ResendEmailCode(Domain.Models.AuthUserModel user, string email, string name)
            {
                // Generate new email verification code
                var emailCode = await _identityService.GenerateTwoFactorTokenAsync(user, "Email");

                // Send the code via email
                await _emailService.SendTwoFactorCodeAsync(email, name, emailCode);

                _logger.LogInformation("MFA code resent via Email to: {Email}", email);

                return new ResendMfaCodeResultDto
                {
                    Success = true,
                    Message = "A new verification code has been sent to your email."
                };
            }
        }
    }
