// Emtelaak.UserRegistration.Application/Validators/ProcessKycWebhookCommandValidator.cs
using FluentValidation;
using Emtelaak.UserRegistration.Application.Commands;

namespace Emtelaak.UserRegistration.Application.Validators
{
    public class ProcessKycWebhookCommandValidator : AbstractValidator<ProcessKycWebhookCommand>
    {
        public ProcessKycWebhookCommandValidator()
        {
            RuleFor(x => x.Payload)
                .NotEmpty().WithMessage("Webhook payload is required.");
        }
    }
}