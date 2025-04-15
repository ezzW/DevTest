// Emtelaak.UserRegistration.Application/Queries/GetUserSessionsQueryHandler.cs
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Queries
{
    public class GetUserSessionsQueryHandler : IRequestHandler<GetUserSessionsQuery, UserSessionsDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUserSessionsQueryHandler> _logger;

        public GetUserSessionsQueryHandler(
            IUserRepository userRepository,
            ILogger<GetUserSessionsQueryHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserSessionsDto> Handle(GetUserSessionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting user sessions for user: {UserId}", request.UserId);

                // Get user sessions
                var sessions = await _userRepository.GetActiveSessionsByUserIdAsync(request.UserId);

                // Get the current session ID from the token claims (in a real implementation)
                // For this demo, we'll assume the most recent session is the current one
                var currentSessionId = sessions.OrderByDescending(s => s.LastActivityAt).FirstOrDefault()?.Id;

                // Map to DTO
                var sessionsDto = new UserSessionsDto
                {
                    CurrentSessionId = currentSessionId,
                    Sessions = sessions.Select(s => new UserSessionDto
                    {
                        Id = s.Id,
                        IssuedAt = s.IssuedAt,
                        ExpiresAt = s.ExpiresAt,
                        LastActivityAt = s.LastActivityAt,
                        IpAddress = s.IpAddress,
                        DeviceInfo = s.DeviceInfo ?? "Unknown Device",
                        IsActive = s.IsActive,
                        IsCurrent = s.Id == currentSessionId
                    }).ToList()
                };

                _logger.LogInformation("Retrieved {Count} sessions for user: {UserId}",
                    sessionsDto.Sessions.Count, request.UserId);

                return sessionsDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user sessions: {Message}", ex.Message);
                throw;
            }
        }
    }
}