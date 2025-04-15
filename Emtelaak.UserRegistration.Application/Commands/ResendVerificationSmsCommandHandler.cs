// Emtelaak.UserRegistration.Application/Commands/ResendVerificationSmsCommandHandler.cs
using Emtelaak.UserRegistration.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class ResendVerificationSmsCommandHandler : IRequestHandler<ResendVerificationSmsCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;
        private readonly ISmsService _smsService;
        private readonly ILogger<ResendVerificationSmsCommandHandler> _logger;

        public ResendVerificationSmsCommandHandler(
            IUserRepository userRepository,
            IIdentityService identityService,
            ISmsService smsService,
            ILogger<ResendVerificationSmsCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(ResendVerificationSmsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing resend verification SMS request for: {PhoneNumber}", request.PhoneNumber);

                // Find user by phone number
                var users = await _userRepository.GetAsync(u => u.PhoneNumber == request.PhoneNumber);
                var user = users.FirstOrDefault();

                if (user != null && !user.PhoneVerified)
                {
                    var identityUser = await _identityService.FindUserByEmailAsync(user.Email);
                    if (identityUser != null)
                    {
                        // Generate phone verification token
                        var token = await _identityService.GeneratePhoneVerificationTokenAsync(identityUser);

                        // Send verification SMS
                        await _smsService.SendVerificationSmsAsync(user.PhoneNumber, token);

                        _logger.LogInformation("Verification SMS resent to: {PhoneNumber}", request.PhoneNumber);
                    }
                }
                else
                {
                    // Don't reveal if phone number exists or is already verified
                    if (user == null)
                    {
                        _logger.LogInformation("Resend verification attempted for non-existent phone: {PhoneNumber}", request.PhoneNumber);
                    }
                    else
                    {
                        _logger.LogInformation("Resend verification attempted for already verified phone: {PhoneNumber}", request.PhoneNumber);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification SMS: {Message}", ex.Message);
            }
        }
    }
}