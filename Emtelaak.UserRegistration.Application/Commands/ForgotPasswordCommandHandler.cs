// Emtelaak.UserRegistration.Application/Commands/ForgotPasswordCommandHandler.cs
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResultDto>
    {
        private readonly IIdentityService _identityService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ForgotPasswordCommandHandler> _logger;

        public ForgotPasswordCommandHandler(
            IIdentityService identityService,
            IEmailService emailService,
            ILogger<ForgotPasswordCommandHandler> logger)
        {
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ForgotPasswordResultDto> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing password reset request for: {Email}", request.Email);

                // Find user
                var user = await _identityService.FindUserByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogInformation("Password reset requested for non-existent email: {Email}", request.Email);

                    // Don't reveal that the email doesn't exist for security reasons
                    return new ForgotPasswordResultDto
                    {
                        Message = "If your email is registered, you will receive a reset code shortly",
                        EmailSent = false
                    };
                }

                // Check if email is confirmed
                if (!await _identityService.IsEmailConfirmedAsync(user))
                {
                    _logger.LogWarning("Password reset attempted for unconfirmed email: {Email}", request.Email);

                    // Don't reveal that the email isn't confirmed for security reasons
                    return new ForgotPasswordResultDto
                    {
                        Message = "If your email is registered, you will receive a reset code shortly",
                        EmailSent = false
                    };
                }

                // Generate password reset code
                var resetCode = await _identityService.GeneratePasswordResetCodeAsync(user);

                // Send password reset email with code
                await _emailService.SendPasswordResetEmailAsync(user.Email, user.FirstName, resetCode);

                _logger.LogInformation("Password reset email sent to: {Email}", request.Email);

                return new ForgotPasswordResultDto
                {
                    Message = "If your email is registered, you will receive a reset code shortly",
                    EmailSent = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing password reset request: {Message}", ex.Message);

                // Don't reveal error details for security reasons
                return new ForgotPasswordResultDto
                {
                    Message = "If your email is registered, you will receive a reset code shortly",
                    EmailSent = false
                };
            }
        }
    }
}