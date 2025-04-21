using FluentValidation;
using Emtelaak.UserRegistration.Application.Commands;

namespace Emtelaak.UserRegistration.Application.Validators
{
    public class RevokeAllSessionsExceptCommandValidator : AbstractValidator<RevokeAllSessionsExceptCommand>
    {
        public RevokeAllSessionsExceptCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.ActiveSessionId)
                .NotEmpty().WithMessage("Active session ID is required.");
        }
    }
}