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
    public class RevokeAllSessionsExceptCommandHandler : IRequestHandler<RevokeAllSessionsExceptCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<RevokeAllSessionsExceptCommandHandler> _logger;

        public RevokeAllSessionsExceptCommandHandler(
            IUserRepository userRepository,
            ILogger<RevokeAllSessionsExceptCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(RevokeAllSessionsExceptCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Revoking all sessions except {ActiveSessionId} for user: {UserId}",
                    request.ActiveSessionId, request.UserId);

                // Verify active session exists and belongs to the user
                var activeSession = await _userRepository.GetSessionByIdAsync(request.ActiveSessionId);
                if (activeSession == null || activeSession.UserId != request.UserId)
                {
                    _logger.LogWarning("Active session {ActiveSessionId} not found or does not belong to user {UserId}",
                        request.ActiveSessionId, request.UserId);
                    throw new ApplicationException("Invalid active session");
                }

                // Revoke all other sessions
                await _userRepository.RevokeAllUserSessionsExceptAsync(
                    request.UserId,
                    request.ActiveSessionId,
                    request.Reason);

                // Add activity log
                var activityLog = new ActivityLog
                {
                    UserId = request.UserId,
                    ActivityType = ActivityType.Logout,
                    Timestamp = DateTime.UtcNow,
                    Status = ActivityStatus.Success,
                    Details = $"{{\"activeSessionId\":\"{request.ActiveSessionId}\",\"action\":\"revokeAllOthers\",\"reason\":\"{request.Reason}\"}}"
                };
                await _userRepository.AddActivityLogAsync(activityLog);

                _logger.LogInformation("All sessions except {ActiveSessionId} revoked successfully for user: {UserId}",
                    request.ActiveSessionId, request.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all sessions: {Message}", ex.Message);
                throw;
            }
        }
    }
}