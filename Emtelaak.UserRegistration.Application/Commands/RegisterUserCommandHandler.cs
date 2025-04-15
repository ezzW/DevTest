// Emtelaak.UserRegistration.Application/Commands/RegisterUserCommandHandler.cs
using AutoMapper;
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using Emtelaak.UserRegistration.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, UserRegistrationResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ILogger<RegisterUserCommandHandler> _logger;

        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IIdentityService identityService,
            IEmailService emailService,
            IMapper mapper,
            ILogger<RegisterUserCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserRegistrationResultDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing user registration for email: {Email}", request.RegistrationData.Email);

                // Map to our domain entity
                var user = _mapper.Map<User>(request.RegistrationData);

                // Create domain user
                var createdUser = await _userRepository.AddAsync(user);

                // Create identity user (using our domain model)
                var authUser = _mapper.Map<AuthUserModel>(request.RegistrationData);
                authUser.Id = Guid.NewGuid(); // Generate new ID for identity user
                authUser.DomainUserId = createdUser.Id; // Link to domain user

                var identityResult = await _identityService.CreateUserAsync(authUser, request.RegistrationData.Password);
                if (!identityResult.Succeeded)
                {
                    _logger.LogError("Failed to create identity user: {Errors}",
                        string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                    await _userRepository.DeleteAsync(createdUser.Id);
                    throw new ApplicationException("Failed to create user account.");
                }

                // Create default user preferences
                var userPreference = new UserPreference
                {
                    UserId = createdUser.Id,
                    Language = "en",
                    Theme = ThemePreference.Light,
                    NotificationEmail = true,
                    NotificationSms = false,
                    NotificationPush = true,
                    LoginNotification = true,
                    TwoFactorEnabled = false,
                    TwoFactorMethod = TwoFactorMethod.SMS,
                    UpdatedAt = DateTime.UtcNow
                };
                await _userRepository.AddUserPreferenceAsync(userPreference);

                // Generate email verification token and send email
                var emailToken = await _identityService.GenerateEmailVerificationTokenAsync(authUser);
                await _emailService.SendVerificationEmailAsync(authUser.Email, authUser.FirstName, emailToken);

                // Add activity log
                var activityLog = new ActivityLog
                {
                    UserId = createdUser.Id,
                    ActivityType = ActivityType.AccountCreation,
                    Timestamp = DateTime.UtcNow,
                    Status = ActivityStatus.Success,
                    Details = $"{{\"email\":\"{createdUser.Email}\",\"userType\":\"{createdUser.UserType}\"}}"
                };
                await _userRepository.AddActivityLogAsync(activityLog);

                // Return result
                var result = _mapper.Map<UserRegistrationResultDto>(createdUser);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration: {Message}", ex.Message);
                throw;
            }
        }
    }
}