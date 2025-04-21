// Emtelaak.UserRegistration.Application/Validators/DeleteKycDocumentCommandValidator.cs
using FluentValidation;
using Emtelaak.UserRegistration.Application.Commands;

namespace Emtelaak.UserRegistration.Application.Validators
{
    public class DeleteKycDocumentCommandValidator : AbstractValidator<DeleteKycDocumentCommand>
    {
        public DeleteKycDocumentCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.DocumentId)
                .NotEmpty().WithMessage("Document ID is required.");
        }
    }
}