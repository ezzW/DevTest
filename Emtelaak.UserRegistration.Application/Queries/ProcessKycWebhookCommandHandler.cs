// Emtelaak.UserRegistration.Application/Commands/ProcessKycWebhookCommandHandler.cs
using Emtelaak.UserRegistration.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class ProcessKycWebhookCommandHandler : IRequestHandler<ProcessKycWebhookCommand, bool>
    {
        private readonly IKycVerificationService _kycVerificationService;
        private readonly ILogger<ProcessKycWebhookCommandHandler> _logger;

        public ProcessKycWebhookCommandHandler(
            IKycVerificationService kycVerificationService,
            ILogger<ProcessKycWebhookCommandHandler> logger)
        {
            _kycVerificationService = kycVerificationService ?? throw new ArgumentNullException(nameof(kycVerificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(ProcessKycWebhookCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing KYC verification webhook");

                // Validate and process the webhook payload
                var result = await _kycVerificationService.ProcessVerificationWebhookAsync(request.Payload);

                _logger.LogInformation("KYC webhook processed successfully: {Result}", result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing KYC webhook: {Message}", ex.Message);
                throw;
            }
        }
    }
}