// Emtelaak.UserRegistration.Application/Commands/LogoutCommandHandler.cs
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
    {
        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<LogoutCommandHandler> _logger;

        public LogoutCommandHandler(
            ITokenService tokenService,
            IUserRepository userRepository,
            ILogger<LogoutCommandHandler> logger)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing logout request");

                // Find the user session
                var session = await _userRepository.GetSessionByTokenAsync(request.RefreshToken);
                if (session != null)
                {
                    // Revoke the token
                    await _tokenService.RevokeTokenAsync(request.RefreshToken);

                    // Add activity log
                    var activityLog = new ActivityLog
                    {
                        UserId = session.UserId,
                        ActivityType = ActivityType.Logout,
                        Timestamp = DateTime.UtcNow,
                        Status = ActivityStatus.Success,
                        Details = $"{{\"sessionId\":\"{session.Id}\"}}"
                    };
                    await _userRepository.AddActivityLogAsync(activityLog);

                    _logger.LogInformation("User logged out successfully: {UserId}", session.UserId);
                }
                else
                {
                    _logger.LogWarning("Logout attempted with invalid refresh token");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout: {Message}", ex.Message);
            }
        }
    }
}