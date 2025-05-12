// Emtelaak.UserRegistration.Application/Validators/UpdateAccreditationStatusCommandValidator.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Emtelaak.UserRegistration.Application.Commands;
using Emtelaak.UserRegistration.Domain.Enums;
using FluentValidation;

namespace Emtelaak.UserRegistration.Application.Validators
{
    public class UpdateAccreditationStatusCommandValidator : AbstractValidator<UpdateAccreditationStatusCommand>
    {
        private readonly IEnumerable<string> _validStatuses = Enum.GetNames(typeof(AccreditationStatus));

        public UpdateAccreditationStatusCommandValidator()
        {
            RuleFor(command => command.AdminUserId)
                .NotEmpty().WithMessage("Admin user ID is required");

            RuleFor(command => command.AccreditationId)
                .NotEmpty().WithMessage("Accreditation ID is required");

            RuleFor(command => command.StatusData).NotNull().WithMessage("Status data is required");

            When(command => command.StatusData != null, () =>
            {
                RuleFor(command => command.StatusData.Status)
                    .NotEmpty().WithMessage("Status is required")
                    .Must(status => _validStatuses.Contains(status))
                    .WithMessage($"Invalid status. Must be one of: {string.Join(", ", _validStatuses)}");

                // When status is Approved, additional fields are required
                When(command => command.StatusData.Status == AccreditationStatus.Approved.ToString(), () =>
                {
                    RuleFor(command => command.StatusData.ExpiresAt)
                        .NotNull().WithMessage("Expiration date is required for approved accreditations")
                        .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future");

                    RuleFor(command => command.StatusData.InvestmentLimitAmount)
                        .GreaterThanOrEqualTo(0).WithMessage("Investment limit amount must be greater than or equal to zero");
                });

                // When status is Rejected, a reason is required
                When(command => command.StatusData.Status == AccreditationStatus.Rejected.ToString(), () =>
                {
                    RuleFor(command => command.StatusData.ReviewNotes)
                        .NotEmpty().WithMessage("Review notes with rejection reason are required");
                });
            });
        }
    }
}