// Emtelaak.UserRegistration.Application/Validators/UploadKycDocumentCommandValidator.cs
using FluentValidation;
using Emtelaak.UserRegistration.Application.Commands;
using Emtelaak.UserRegistration.Domain.Enums;

namespace Emtelaak.UserRegistration.Application.Validators
{
    public class UploadKycDocumentCommandValidator : AbstractValidator<UploadKycDocumentCommand>
    {
        private readonly string[] _allowedContentTypes = new[] { "application/pdf", "image/jpeg", "image/png" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10 MB

        public UploadKycDocumentCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.File)
                .NotNull().WithMessage("File is required.");

            RuleFor(x => x.File.ContentType)
                .Must(x => _allowedContentTypes.Contains(x))
                .WithMessage("File type not allowed. Only PDF, JPEG and PNG are supported.");

            RuleFor(x => x.File.Length)
                .LessThanOrEqualTo(_maxFileSize)
                .WithMessage("File size exceeds the maximum allowed (10 MB).");

            RuleFor(x => x.DocumentType)
                .NotEmpty().WithMessage("Document type is required.")
                .Must(BeAValidDocumentType).WithMessage("Invalid document type.");
        }

        private bool BeAValidDocumentType(string documentType)
        {
            return Enum.TryParse<DocumentType>(documentType, true, out _);
        }
    }
}