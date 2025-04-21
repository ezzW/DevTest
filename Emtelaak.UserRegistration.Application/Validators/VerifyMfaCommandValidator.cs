// Emtelaak.UserRegistration.Application/Validators/VerifyMfaCommandValidator.cs
using FluentValidation;
using Emtelaak.UserRegistration.Application.Commands;

namespace Emtelaak.UserRegistration.Application.Validators
{
    public class VerifyMfaCommandValidator : AbstractValidator<VerifyMfaCommand>
    {
        private readonly string[] _validMethods = { "SMS", "Authenticator", "Email" };

        public VerifyMfaCommandValidator()
        {
            RuleFor(x => x.VerificationData.MfaToken)
                .NotEmpty().WithMessage("MFA token is required.");

            RuleFor(x => x.VerificationData.VerificationCode)
                .NotEmpty().WithMessage("Verification code is required.")
                .Matches("^[0-9]+$").WithMessage("Verification code must contain only digits.");

            RuleFor(x => x.VerificationData.Method)
                .NotEmpty().WithMessage("Verification method is required.")
                .Must(method => _validMethods.Contains(method))
                .WithMessage("Invalid verification method. Must be 'SMS', 'Authenticator', or 'Email'.");
        }
    }
}