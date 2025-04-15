// Emtelaak.UserRegistration.Application/Commands/UpdateUserPreferencesCommandHandler.cs
using AutoMapper;
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class UpdateUserPreferencesCommandHandler : IRequestHandler<UpdateUserPreferencesCommand, UserPreferenceUpdateResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateUserPreferencesCommandHandler> _logger;

        public UpdateUserPreferencesCommandHandler(
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<UpdateUserPreferencesCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserPreferenceUpdateResultDto> Handle(UpdateUserPreferencesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating preferences for user: {UserId}", request.UserId);

                // Get user preferences
                var userPreferences = await _userRepository.GetUserPreferenceByUserIdAsync(request.UserId);

                if (userPreferences == null)
                {
                    // Create new preferences if they don't exist
                    userPreferences = new UserPreference
                    {
                        UserId = request.UserId,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // Map preferences from request
                    _mapper.Map(request.Preferences, userPreferences);

                    // Add new preferences
                    await _userRepository.AddUserPreferenceAsync(userPreferences);
                }
                else
                {
                    // Update existing preferences
                    _mapper.Map(request.Preferences, userPreferences);
                    userPreferences.UpdatedAt = DateTime.UtcNow;

                    // Update preferences
                    await _userRepository.UpdateUserPreferenceAsync(userPreferences);
                }

                // Add activity log
                var activityLog = new ActivityLog
                {
                    UserId = request.UserId,
                    ActivityType = ActivityType.ProfileUpdate,
                    Timestamp = DateTime.UtcNow,
                    Status = ActivityStatus.Success,
                    Details = $"{{\"preferencesUpdated\":true}}"
                };
                await _userRepository.AddActivityLogAsync(activityLog);

                // Return result
                var result = new UserPreferenceUpdateResultDto
                {
                    Updated = true,
                    Preferences = _mapper.Map<UserPreferenceDto>(userPreferences)
                };

                _logger.LogInformation("Preferences updated successfully for user: {UserId}", request.UserId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user preferences: {Message}", ex.Message);
                throw;
            }
        }
    }
}