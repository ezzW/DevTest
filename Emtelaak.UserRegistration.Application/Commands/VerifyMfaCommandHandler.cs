// Emtelaak.UserRegistration.Application/Commands/VerifyMfaCommandHandler.cs
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class VerifyMfaCommandHandler : IRequestHandler<VerifyMfaCommand, LoginResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<VerifyMfaCommandHandler> _logger;

        public VerifyMfaCommandHandler(
            IUserRepository userRepository,
            IIdentityService identityService,
            ITokenService tokenService,
            ILogger<VerifyMfaCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LoginResultDto> Handle(VerifyMfaCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Verifying MFA code");

                // First validate the MFA token
                var (isValidToken, email) = await _tokenService.ValidateMfaTokenAsync(request.VerificationData.MfaToken);
                if (!isValidToken)
                {
                    _logger.LogWarning("MFA verification failed: Invalid MFA token");
                    return new LoginResultDto();
                }

                // Get the identity user
                var identityUser = await _identityService.FindUserByEmailAsync(email);
                if (identityUser == null)
                {
                    _logger.LogWarning("MFA verification failed: User not found for email {Email}", email);
                    return new LoginResultDto();
                }

                // Verify the MFA code
                var provider = request.VerificationData.Method;
                var code = request.VerificationData.VerificationCode;
                bool isValid = false;
                switch (request.VerificationData.Method.ToLower())
                {
                    case "sms":
                        isValid = await _identityService.VerifyTwoFactorTokenAsync(identityUser, "Phone", request.VerificationData.VerificationCode);
                        break;
                    case "email":
                        isValid = await _identityService.VerifyTwoFactorTokenAsync(identityUser, "Email", request.VerificationData.VerificationCode);
                        break;
                    case "authenticator":
                        isValid = await _identityService.VerifyTwoFactorTokenAsync(identityUser, "Authenticator", request.VerificationData.VerificationCode);
                        break;
                    default:
                        _logger.LogWarning("Invalid MFA method: {Method}", request.VerificationData.Method);
                        isValid = false;
                        break;
                }

                if (!isValid)
                {
                    _logger.LogWarning("MFA verification failed: Invalid verification code for user {Email}", email);
                    return new LoginResultDto();
                }

                // Get domain user
                var domainUser = await _userRepository.GetUserByEmailAsync(email);
                if (domainUser == null)
                {
                    _logger.LogError("Domain user not found for email: {Email}", email);
                    throw new ApplicationException("Domain user not found");
                }

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
                    Details = $"{{\"sessionId\":\"{userSession.Id}\", \"mfaMethod\":\"{provider}\"}}"
                };
                await _userRepository.AddActivityLogAsync(loginLog);

                _logger.LogInformation("MFA verification successful for user: {Email}", email);

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
                _logger.LogError(ex, "Error during MFA verification: {Message}", ex.Message);
                throw;
            }
        }
    }
}