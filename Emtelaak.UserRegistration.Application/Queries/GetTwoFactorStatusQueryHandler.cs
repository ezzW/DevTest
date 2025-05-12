// Emtelaak.UserRegistration.Application/Queries/GetTwoFactorStatusQueryHandler.cs
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Queries
{
    public class GetTwoFactorStatusQueryHandler : IRequestHandler<GetTwoFactorStatusQuery, TwoFactorStatusDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;
        private readonly ILogger<GetTwoFactorStatusQueryHandler> _logger;

        public GetTwoFactorStatusQueryHandler(
            IUserRepository userRepository,
            IIdentityService identityService,
            ILogger<GetTwoFactorStatusQueryHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TwoFactorStatusDto> Handle(GetTwoFactorStatusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting two-factor status for user: {UserId}", request.UserId);

                // Get user
                var domainUser= await _userRepository.GetByIdAsync(request.UserId);
                var user = await _identityService.FindUserByEmailAsync(domainUser.Email);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    throw new ApplicationException($"User not found: {request.UserId}");
                }

                // Get two-factor status
                bool isTwoFactorEnabled = await _identityService.IsTwoFactorEnabledAsync(user);

                // Get user preference to determine the method
                var userPreference = await _userRepository.GetUserPreferenceByUserIdAsync(request.UserId);
                string method = userPreference?.TwoFactorMethod.ToString() ?? "None";

                return new TwoFactorStatusDto
                {
                    Enabled = isTwoFactorEnabled,
                    Method = isTwoFactorEnabled ? method : "None"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting two-factor status: {Message}", ex.Message);
                throw;
            }
        }
    }
}