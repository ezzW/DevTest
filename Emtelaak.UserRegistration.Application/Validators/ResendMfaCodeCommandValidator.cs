// Emtelaak.UserRegistration.Application/Commands/ResendMfaCodeCommandValidator.cs
using Emtelaak.UserRegistration.Application.Commands;
using FluentValidation;

namespace Emtelaak.UserRegistration.Application.Validators
{
    public class ResendMfaCodeCommandValidator : AbstractValidator<ResendMfaCodeCommand>
    {
        private readonly string[] _validMethods = { "SMS", "Email", "Authenticator" };

        public ResendMfaCodeCommandValidator()
        {
            RuleFor(x => x.MfaToken)
                .NotEmpty().WithMessage("MFA token is required.");

            RuleFor(x => x.Method)
                .NotEmpty().WithMessage("MFA method is required.")
                .Must(method => _validMethods.Contains(method, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Invalid MFA method. Must be 'SMS', 'Email', or 'Authenticator'.");
        }
    }
}
