// Emtelaak.UserRegistration.Application/Validators/SubmitAccreditationCommandValidator.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Emtelaak.UserRegistration.Application.Commands;
using Emtelaak.UserRegistration.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;

namespace Emtelaak.UserRegistration.Application.Validators
{
    public class SubmitAccreditationCommandValidator : AbstractValidator<SubmitAccreditationCommand>
    {
        private readonly IEnumerable<string> _validInvestorClassifications = Enum.GetNames(typeof(InvestorClassification));

        public SubmitAccreditationCommandValidator()
        {
            RuleFor(command => command.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(command => command.AccreditationData).NotNull().WithMessage("Accreditation data is required");

            When(command => command.AccreditationData != null, () =>
            {
                RuleFor(command => command.AccreditationData.InvestorClassification)
                    .NotEmpty().WithMessage("Investor classification is required")
                    .Must(classification => _validInvestorClassifications.Contains(classification))
                    .WithMessage($"Invalid investor classification. Must be one of: {string.Join(", ", _validInvestorClassifications)}");

                // Conditional rules based on investor classification
                When(command => command.AccreditationData.InvestorClassification == InvestorClassification.Accredited.ToString() ||
                              command.AccreditationData.InvestorClassification == InvestorClassification.Qualified.ToString(), () =>
                {
                    RuleFor(command => command.AccreditationData.IncomeLevel)
                        .NotNull().WithMessage("Income level is required for accredited and qualified investors")
                        .GreaterThan(0).WithMessage("Income level must be greater than zero");

                    RuleFor(command => command.AccreditationData.NetWorth)
                        .NotNull().WithMessage("Net worth is required for accredited and qualified investors")
                        .GreaterThan(0).WithMessage("Net worth must be greater than zero");
                });

                When(command => command.AccreditationData.InvestorClassification == InvestorClassification.Institutional.ToString(), () =>
                {
                    RuleFor(command => command.AccreditationData.DocumentIds)
                        .Must(docs => docs != null && docs.Any())
                        .WithMessage("At least one company registration document is required for institutional investors");
                });
            });
        }
    }
}