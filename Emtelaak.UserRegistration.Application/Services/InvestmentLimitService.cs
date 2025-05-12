// Emtelaak.UserRegistration.Application/Services/InvestmentLimitService.cs
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using System;

namespace Emtelaak.UserRegistration.Application.Services
{
    public class InvestmentLimitService
    {
        /// <summary>
        /// Calculates the investment limit for a user based on their accreditation status and financial information
        /// </summary>
        /// <param name="accreditation">The user's accreditation information</param>
        /// <returns>Calculated investment limit amount</returns>
        public static decimal CalculateInvestmentLimit(Accreditation accreditation)
        {
            if (accreditation == null)
            {
                throw new ArgumentNullException(nameof(accreditation));
            }

            // If there's an admin override, use that value
            if (accreditation.OverrideEnabled && accreditation.InvestmentLimitAmount.HasValue)
            {
                return accreditation.InvestmentLimitAmount.Value;
            }

            // If the status is not approved, no investment is allowed
            if (accreditation.Status != AccreditationStatus.Approved)
            {
                return 0;
            }

            // Base the calculation on the investor classification
            switch (accreditation.InvestorClassification)
            {
                case InvestorClassification.Accredited:
                    // No limit for accredited investors
                    return decimal.MaxValue;

                case InvestorClassification.Qualified:
                    // Qualified investors typically have high limits based on their net worth
                    return CalculateQualifiedInvestorLimit(accreditation);

                case InvestorClassification.Institutional:
                    // No limit for institutional investors
                    return decimal.MaxValue;

                case InvestorClassification.NonAccredited:
                    // Non-accredited investors have strict limits
                    return CalculateNonAccreditedInvestorLimit(accreditation);

                default:
                    // Default to zero for unknown classification
                    return 0;
            }
        }

        /// <summary>
        /// Calculates the investment limit for a qualified investor
        /// </summary>
        private static decimal CalculateQualifiedInvestorLimit(Accreditation accreditation)
        {
            // If net worth is available, use that as a basis
            if (accreditation.NetWorth.HasValue)
            {
                // Qualified investors can typically invest up to 20% of their net worth
                return accreditation.NetWorth.Value * 0.2m;
            }
            
            // If income is available, use that
            if (accreditation.IncomeLevel.HasValue)
            {
                // Qualified investors can typically invest up to 50% of their annual income
                return accreditation.IncomeLevel.Value * 0.5m;
            }

            // Default qualified investor limit if no financial information is available
            return 1000000; // $1 million
        }

        /// <summary>
        /// Calculates the investment limit for a non-accredited investor
        /// </summary>
        private static decimal CalculateNonAccreditedInvestorLimit(Accreditation accreditation)
        {
            // Default non-accredited limit
            decimal baseLimit = 50000; // $50,000

            // If we have income information, use the lesser of $50,000 or 10% of income
            if (accreditation.IncomeLevel.HasValue)
            {
                decimal incomeBasedLimit = accreditation.IncomeLevel.Value * 0.1m;
                return Math.Min(baseLimit, incomeBasedLimit);
            }

            // If we have net worth information, use the lesser of $50,000 or 5% of net worth
            if (accreditation.NetWorth.HasValue)
            {
                decimal netWorthBasedLimit = accreditation.NetWorth.Value * 0.05m;
                return Math.Min(baseLimit, netWorthBasedLimit);
            }

            // If no financial information is available, use the regulatory minimum
            return baseLimit;
        }

        /// <summary>
        /// Checks if a user can invest a specific amount based on their accreditation status
        /// </summary>
        /// <param name="accreditation">The user's accreditation information</param>
        /// <param name="investmentAmount">The amount the user wants to invest</param>
        /// <returns>True if the investment is allowed, false otherwise</returns>
        public static bool CanInvest(Accreditation accreditation, decimal investmentAmount)
        {
            if (accreditation == null)
            {
                return false;
            }

            // If status is not approved, no investment is allowed
            if (accreditation.Status != AccreditationStatus.Approved)
            {
                return false;
            }

            // Check if the accreditation has expired
            if (accreditation.ExpiresAt.HasValue && accreditation.ExpiresAt.Value < DateTime.UtcNow)
            {
                return false;
            }

            // Calculate the investment limit
            decimal limit = CalculateInvestmentLimit(accreditation);

            // For unlimited amounts (accredited and institutional investors)
            if (limit == decimal.MaxValue)
            {
                return true;
            }

            // Check if the investment amount is within the limit
            return investmentAmount <= limit;
        }
    }
}