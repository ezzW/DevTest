// Emtelaak.UserRegistration.Application/Commands/UpdateUserProfileCommandHandler.cs
using AutoMapper;
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UserProfileUpdateResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateUserProfileCommandHandler> _logger;

        public UpdateUserProfileCommandHandler(
            IUserRepository userRepository,
            IIdentityService identityService,
            IMapper mapper,
            ILogger<UpdateUserProfileCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserProfileUpdateResultDto> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating profile for user: {UserId}", request.UserId);

                // Get user
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    throw new ApplicationException($"User not found: {request.UserId}");
                }

                // Check for phone number change
                bool phoneNumberChanged = !string.IsNullOrEmpty(request.ProfileData.PhoneNumber) &&
                                        user.PhoneNumber != request.ProfileData.PhoneNumber;

                // Update user properties
                _mapper.Map(request.ProfileData, user);

                // If phone number changed, mark as unverified
                if (phoneNumberChanged)
                {
                    user.PhoneVerified = false;
                }

                user.UpdatedAt = DateTime.UtcNow;

                // Update domain user
                await _userRepository.UpdateAsync(user);

                // Update identity user
                var domainUser = await _userRepository.GetByIdAsync(request.UserId);
                var identityUser = await _identityService.FindUserByEmailAsync(domainUser.Email);
                if (identityUser != null)
                {
                    identityUser.FirstName = user.FirstName;
                    identityUser.LastName = user.LastName;
                    identityUser.DateOfBirth = user.DateOfBirth;
                    identityUser.CountryOfResidence = user.CountryOfResidence;

                    // Handle phone number change
                    if (phoneNumberChanged)
                    {
                        // Update phone but mark as unverified
                        identityUser.PhoneNumber = user.PhoneNumber;
                        identityUser.PhoneNumberConfirmed = false;
                        identityUser.PhoneVerified = false;
                    }

                    await _identityService.UpdateUserAsync(identityUser);
                }

                // Add activity log
                var activityLog = new ActivityLog
                {
                    UserId = user.Id,
                    ActivityType = ActivityType.ProfileUpdate,
                    Timestamp = DateTime.UtcNow,
                    Status = ActivityStatus.Success,
                    Details = $"{{\"phoneChanged\":{phoneNumberChanged.ToString().ToLower()}}}"
                };
                await _userRepository.AddActivityLogAsync(activityLog);

                // Return result
                var result = new UserProfileUpdateResultDto
                {
                    UserId = user.Id,
                    Updated = true,
                    Profile = request.ProfileData
                };

                _logger.LogInformation("Profile updated successfully for user: {UserId}", request.UserId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile: {Message}", ex.Message);
                throw;
            }
        }
    }
}