// Emtelaak.UserRegistration.Application/Queries/GetUserActivityLogQueryHandler.cs
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Queries
{
    public class GetUserActivityLogQueryHandler : IRequestHandler<GetUserActivityLogQuery, ActivityLogListDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUserActivityLogQueryHandler> _logger;

        public GetUserActivityLogQueryHandler(
            IUserRepository userRepository,
            ILogger<GetUserActivityLogQueryHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ActivityLogListDto> Handle(GetUserActivityLogQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting activity log for user: {UserId}, Limit: {Limit}",
                    request.UserId, request.Limit);

                // Get activity logs
                var activityLogs = await _userRepository.GetActivityLogsByUserIdAsync(request.UserId, request.Limit);

                // Map to DTO
                var activityLogDto = new ActivityLogListDto
                {
                    TotalCount = activityLogs.Count,
                    Activities = activityLogs.Select(log => new ActivityLogEntryDto
                    {
                        Id = log.Id,
                        ActivityType = log.ActivityType.ToString(),
                        Timestamp = log.Timestamp,
                        IpAddress = log.IpAddress,
                        DeviceInfo = log.UserAgent,
                        Status = log.Status.ToString(),
                        Details = log.Details
                    }).ToList()
                };

                _logger.LogInformation("Retrieved {Count} activity logs for user: {UserId}",
                    activityLogDto.Activities.Count, request.UserId);

                return activityLogDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity log: {Message}", ex.Message);
                throw;
            }
        }
    }
}