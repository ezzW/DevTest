// Emtelaak.UserRegistration.Application/Validators/RefreshTokenCommandValidator.cs
using FluentValidation;
using Emtelaak.UserRegistration.Application.Commands;

namespace Emtelaak.UserRegistration.Application.Validators
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required.");
        }
    }
}