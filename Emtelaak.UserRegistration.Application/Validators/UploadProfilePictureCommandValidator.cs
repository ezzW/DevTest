using FluentValidation;
using Emtelaak.UserRegistration.Application.Commands;

namespace Emtelaak.UserRegistration.Application.Validators
{
    public class UploadProfilePictureCommandValidator : AbstractValidator<UploadProfilePictureCommand>
    {
        private readonly string[] _allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5 MB

        public UploadProfilePictureCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.File)
                .NotNull().WithMessage("File is required.");

            When(x => x.File != null, () =>
            {
                RuleFor(x => x.File.ContentType)
                    .Must(x => _allowedContentTypes.Contains(x))
                    .WithMessage("File type not allowed. Only JPEG, PNG and GIF are supported.");

                RuleFor(x => x.File.Length)
                    .LessThanOrEqualTo(_maxFileSize)
                    .WithMessage("File size exceeds the maximum allowed (5 MB).");
            });
        }
    }
}