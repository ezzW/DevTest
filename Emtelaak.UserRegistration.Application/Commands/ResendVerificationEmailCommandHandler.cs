// Emtelaak.UserRegistration.Application/Commands/ResendVerificationEmailCommandHandler.cs
using Emtelaak.UserRegistration.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class ResendVerificationEmailCommandHandler : IRequestHandler<ResendVerificationEmailCommand>
    {
        private readonly IIdentityService _identityService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ResendVerificationEmailCommandHandler> _logger;

        public ResendVerificationEmailCommandHandler(
            IIdentityService identityService,
            IEmailService emailService,
            ILogger<ResendVerificationEmailCommandHandler> logger)
        {
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing resend verification email request for: {Email}", request.Email);

                // Get user
                var user = await _identityService.FindUserByEmailAsync(request.Email);

                // If user exists and email is not confirmed
                if (user != null && !await _identityService.IsEmailConfirmedAsync(user))
                {
                    // Generate email verification token
                    var token = await _identityService.GenerateEmailVerificationTokenAsync(user);

                    // Send verification email
                    await _emailService.SendVerificationEmailAsync(user.Email, user.FirstName, token);

                    _logger.LogInformation("Verification email resent to: {Email}", request.Email);
                }
                else
                {
                    // Even if user doesn't exist or email is already confirmed,
                    // log it but don't tell the caller (to prevent email enumeration)
                    if (user == null)
                    {
                        _logger.LogInformation("Resend verification attempted for non-existent email: {Email}", request.Email);
                    }
                    else
                    {
                        _logger.LogInformation("Resend verification attempted for already verified email: {Email}", request.Email);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification email: {Message}", ex.Message);
            }
        }
    }
}