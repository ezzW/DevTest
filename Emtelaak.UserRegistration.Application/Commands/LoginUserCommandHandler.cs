// Emtelaak.UserRegistration.Application/Commands/LoginUserCommandHandler.cs
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<LoginUserCommandHandler> _logger;
        private readonly IEmailService _emailService; 
        private readonly ISmsService _smsService;
        public LoginUserCommandHandler(
            IUserRepository userRepository,
            IIdentityService identityService,
            ITokenService tokenService,
            ILogger<LoginUserCommandHandler> logger,
            IEmailService emailService,
            ISmsService smsService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService;
            _smsService = smsService;
        }

        public async Task<LoginResultDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing login for user: {Email}", request.LoginData.Email);

                // Authenticate user with identity
                var authResult = await _identityService.ValidateUserCredentialsAsync(
                    request.LoginData.Email,
                    request.LoginData.Password);

                if (!authResult.Succeeded)
                {
                    _logger.LogWarning("Login failed for user: {Email}, Reason: {Reason}",
                        request.LoginData.Email, authResult.FailureReason);

                    // Log failed login attempt
                    var user = await _userRepository.GetUserByEmailAsync(request.LoginData.Email);
                    if (user != null)
                    {
                        user.FailedLoginAttempts += 1;
                        await _userRepository.UpdateAsync(user);

                        // Add activity log
                        var activityLog = new ActivityLog
                        {
                            UserId = user.Id,
                            ActivityType = ActivityType.Login,
                            Timestamp = DateTime.UtcNow,
                            Status = ActivityStatus.Failure,
                            FailureReason = authResult.FailureReason,
                            IpAddress = request.IpAddress,
                            UserAgent = request.UserAgent
                        };
                        await _userRepository.AddActivityLogAsync(activityLog);
                    }

                    return new LoginResultDto
                    {
                        RequiresMfa = false
                    };
                }

                var identityUser = await _identityService.FindUserByEmailAsync(request.LoginData.Email);
                if (identityUser == null)
                {
                    _logger.LogError("User not found after successful authentication: {Email}", request.LoginData.Email);
                    throw new ApplicationException("User not found after successful authentication.");
                }

                // Get domain user
                var domainUser = await _userRepository.GetUserByEmailAsync(request.LoginData.Email);
                if (domainUser == null)
                {
                    _logger.LogError("Domain user not found: {Email}", request.LoginData.Email);
                    throw new ApplicationException("Domain user not found.");
                }

                // Check if MFA is required
                if (await _identityService.IsTwoFactorEnabledAsync(identityUser))
                {
                    _logger.LogInformation("MFA required for user: {Email}", request.LoginData.Email);

                    // Get user preference to determine the method
                    var userPreference = await _userRepository.GetUserPreferenceByUserIdAsync(domainUser.Id);

                    // Get the configured MFA method
                    string mfaMethod = "Unknown";
                    if (userPreference != null)
                    {
                        mfaMethod = userPreference.TwoFactorMethod.ToString();
                    }

                    // Generate MFA token for session tracking
                    var mfaToken = await _tokenService.GenerateMfaTokenAsync(identityUser);

                    // Generate and send verification code based on the configured method
                    switch (mfaMethod.ToLower())
                    {
                        case "sms":
                            // Generate SMS verification code
                            var smsCode = await _identityService.GenerateTwoFactorTokenAsync(identityUser, "Phone");

                            // Send the code via SMS
                            await _smsService.SendTwoFactorCodeAsync(domainUser.PhoneNumber, smsCode);

                            _logger.LogInformation("MFA code sent via SMS to: {PhoneNumber}", domainUser.PhoneNumber);
                            break;

                        case "email":
                            // Generate email verification code
                            var emailCode = await _identityService.GenerateTwoFactorTokenAsync(identityUser, "Email");

                            // Send the code via email
                            await _emailService.SendTwoFactorCodeAsync(domainUser.Email, domainUser.FirstName, emailCode);

                            _logger.LogInformation("MFA code sent via Email to: {Email}", domainUser.Email);
                            break;

                        case "authenticator":
                            // No need to send code for authenticator as it's generated by the app
                            _logger.LogInformation("Authenticator MFA method used, no code to send for user: {Email}", domainUser.Email);
                            break;

                        default:
                            _logger.LogWarning("Unknown MFA method: {Method} for user: {Email}", mfaMethod, domainUser.Email);
                            break;
                    }

                    // Only return the configured MFA method, not all available methods
                    return new LoginResultDto
                    {
                        RequiresMfa = true,
                        MfaToken = mfaToken,
                        MfaMethods = new[] { mfaMethod }
                    };
                }
                //// Check if MFA is required
                //if (await _identityService.IsTwoFactorEnabledAsync(identityUser))
                //{
                //    _logger.LogInformation("MFA required for user: {Email}", request.LoginData.Email);

                //    // Get user preference to determine the method
                //    var userPreference = await _userRepository.GetUserPreferenceByUserIdAsync(domainUser.Id);

                //    // Get the configured MFA method
                //    string mfaMethod = userPreference?.TwoFactorMethod.ToString() ?? "SMS";

                //    // Generate MFA token for session tracking
                //    var mfaToken = await _tokenService.GenerateMfaTokenAsync(identityUser);

                //    // Generate and send verification code based on the configured method
                //    if (mfaMethod.Equals("SMS", StringComparison.OrdinalIgnoreCase))
                //    {
                //        // Generate SMS verification code
                //        var smsCode = await _identityService.GenerateTwoFactorTokenAsync(identityUser, "Phone");

                //        // Send the code via SMS
                //        await _smsService.SendTwoFactorCodeAsync(domainUser.PhoneNumber, smsCode);

                //        _logger.LogInformation("MFA code sent via SMS to: {PhoneNumber}", domainUser.PhoneNumber);
                //    }
                //    else if (mfaMethod.Equals("Email", StringComparison.OrdinalIgnoreCase))
                //    {
                //        // Generate email verification code
                //        var emailCode = await _identityService.GenerateTwoFactorTokenAsync(identityUser, "Email");

                //        // Send the code via email
                //        await _emailService.SendTwoFactorCodeAsync(domainUser.Email, domainUser.FirstName, emailCode);

                //        _logger.LogInformation("MFA code sent via Email to: {Email}", domainUser.Email);
                //    }
                //    // For authenticator app, no need to send code as it's generated by the app

                //    // Only return the configured MFA method, not all available methods
                //    return new LoginResultDto
                //    {
                //        RequiresMfa = true,
                //        MfaToken = mfaToken,
                //        MfaMethods = new[] { mfaMethod }
                //    };
                //}

                // Update user login info
                domainUser.LastLoginAt = DateTime.UtcNow;
                domainUser.FailedLoginAttempts = 0;
                await _userRepository.UpdateAsync(domainUser);

                // Update identity user login info
                await _identityService.UpdateLastLoginDateAsync(identityUser);

                // Generate authentication tokens
                var tokenResult = await _tokenService.GenerateAuthTokensAsync(identityUser);

                // Create user session
                var userSession = new UserSession
                {
                    UserId = domainUser.Id,
                    Token = tokenResult.RefreshToken,
                    IssuedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(14), // 14-day refresh token
                    LastActivityAt = DateTime.UtcNow,
                    IpAddress = request.IpAddress,
                    UserAgent = request.UserAgent,
                    IsActive = true
                };
                await _userRepository.AddUserSessionAsync(userSession);

                // Add login activity log
                var loginLog = new ActivityLog
                {
                    UserId = domainUser.Id,
                    ActivityType = ActivityType.Login,
                    Timestamp = DateTime.UtcNow,
                    Status = ActivityStatus.Success,
                    IpAddress = request.IpAddress,
                    UserAgent = request.UserAgent,
                    Details = $"{{\"sessionId\":\"{userSession.Id}\"}}"
                };
                await _userRepository.AddActivityLogAsync(loginLog);

                _logger.LogInformation("User successfully logged in: {Email}", request.LoginData.Email);

                return new LoginResultDto
                {
                    AccessToken = tokenResult.AccessToken,
                    RefreshToken = tokenResult.RefreshToken,
                    ExpiresIn = tokenResult.ExpiresIn,
                    UserId = domainUser.Id,
                    RequiresMfa = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login: {Message}", ex.Message);
                throw;
            }
        }
    }
}
