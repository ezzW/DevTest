// Emtelaak.UserRegistration.Application/Interfaces/IInvestmentLimitService.cs
using Emtelaak.UserRegistration.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Emtelaak.UserRegistration.Application.Interfaces
{
    public interface IInvestmentLimitService
    {
        /// <summary>
        /// Calculates the investment limit for a user based on their accreditation status and financial information
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <returns>The user's investment limit</returns>
        Task<decimal> GetInvestmentLimitAsync(Guid userId);

        /// <summary>
        /// Validates whether a user can invest a specific amount based on their accreditation status
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <param name="investmentAmount">The amount the user wants to invest</param>
        /// <returns>True if the investment is allowed, false otherwise</returns>
        Task<bool> ValidateInvestmentAsync(Guid userId, decimal investmentAmount);

        /// <summary>
        /// Gets the accreditation status and limit details for a user
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <returns>Accreditation status and limit information</returns>
        Task<(bool IsAccredited, decimal InvestmentLimit, string InvestorClassification)> GetAccreditationStatusAsync(Guid userId);
    }
}