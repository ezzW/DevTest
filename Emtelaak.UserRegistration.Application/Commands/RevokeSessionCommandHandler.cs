using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class RevokeSessionCommandHandler : IRequestHandler<RevokeSessionCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<RevokeSessionCommandHandler> _logger;

        public RevokeSessionCommandHandler(
            IUserRepository userRepository,
            ILogger<RevokeSessionCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Revoking session {SessionId} for user: {UserId}",
                    request.SessionId, request.UserId);

                // Get the session
                var session = await _userRepository.GetSessionByIdAsync(request.SessionId);

                // Check if session exists
                if (session == null)
                {
                    _logger.LogWarning("Session not found: {SessionId}", request.SessionId);
                    throw new ApplicationException($"Session not found: {request.SessionId}");
                }

                // Verify session belongs to the user
                if (session.UserId != request.UserId)
                {
                    _logger.LogWarning("Session {SessionId} does not belong to user {UserId}",
                        request.SessionId, request.UserId);
                    throw new ApplicationException("You do not have permission to revoke this session");
                }

                // Revoke the session
                await _userRepository.RevokeUserSessionAsync(request.SessionId, request.Reason);

                // Add activity log
                var activityLog = new ActivityLog
                {
                    UserId = request.UserId,
                    ActivityType = ActivityType.Logout,
                    Timestamp = DateTime.UtcNow,
                    Status = ActivityStatus.Success,
                    Details = $"{{\"sessionId\":\"{request.SessionId}\",\"reason\":\"{request.Reason}\"}}"
                };
                await _userRepository.AddActivityLogAsync(activityLog);

                _logger.LogInformation("Session {SessionId} revoked successfully", request.SessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking session: {Message}", ex.Message);
                throw;
            }
        }
    }
}