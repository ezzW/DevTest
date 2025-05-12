// Emtelaak.UserRegistration.Infrastructure/Services/InvestmentLimitService.cs
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Application.Services;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Emtelaak.UserRegistration.Infrastructure.Services
{
    public class InvestmentLimitService : IInvestmentLimitService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<InvestmentLimitService> _logger;

        public InvestmentLimitService(
            IUserRepository userRepository,
            ILogger<InvestmentLimitService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<decimal> GetInvestmentLimitAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting investment limit for user: {UserId}", userId);

                // Get user with accreditation details
                var user = await _userRepository.GetUserByIdWithDetailsAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return 0;
                }

                // If user has no accreditation, return default limit for non-accredited investors
                if (user.Accreditation == null)
                {
                    _logger.LogInformation("User {UserId} has no accreditation, using default non-accredited limit", userId);
                    return GetDefaultNonAccreditedLimit();
                }

                // If accreditation has expired, return default limit
                if (user.Accreditation.Status == AccreditationStatus.Approved &&
                    user.Accreditation.ExpiresAt.HasValue &&
                    user.Accreditation.ExpiresAt.Value < DateTime.UtcNow)
                {
                    _logger.LogInformation("User {UserId} accreditation expired on {ExpiryDate}, using default non-accredited limit", 
                        userId, user.Accreditation.ExpiresAt.Value);
                    
                    // Update accreditation status to expired
                    user.Accreditation.Status = AccreditationStatus.Expired;
                    user.Accreditation.LastUpdatedAt = DateTime.UtcNow;
                    await _userRepository.UpdateAccreditationAsync(user.Accreditation);
                    
                    return GetDefaultNonAccreditedLimit();
                }

                // Calculate investment limit based on accreditation
                var limit = Application.Services.InvestmentLimitService.CalculateInvestmentLimit(user.Accreditation);
                _logger.LogInformation("Calculated investment limit for user {UserId}: {Limit}", userId, limit);
                return limit;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating investment limit for user {UserId}: {Message}", userId, ex.Message);
                return GetDefaultNonAccreditedLimit();
            }
        }

        public async Task<bool> ValidateInvestmentAsync(Guid userId, decimal investmentAmount)
        {
            try
            {
                _logger.LogInformation("Validating investment of {Amount} for user {UserId}", investmentAmount, userId);

                // Get investment limit
                var limit = await GetInvestmentLimitAsync(userId);
                
                // Check if unlimited (for accredited/institutional investors)
                if (limit == decimal.MaxValue)
                {
                    return true;
                }
                
                // Compare with requested amount
                bool isAllowed = investmentAmount <= limit;
                _logger.LogInformation("Investment validation for user {UserId}: Amount={Amount}, Limit={Limit}, Allowed={Allowed}", 
                    userId, investmentAmount, limit, isAllowed);
                
                return isAllowed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating investment for user {UserId}: {Message}", userId, ex.Message);
                return false;
            }
        }

        public async Task<(bool IsAccredited, decimal InvestmentLimit, string InvestorClassification)> GetAccreditationStatusAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting accreditation status for user: {UserId}", userId);

                // Get user with accreditation details
                var user = await _userRepository.GetUserByIdWithDetailsAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return (false, GetDefaultNonAccreditedLimit(), InvestorClassification.NonAccredited.ToString());
                }

                // If user has no accreditation or it's not approved
                if (user.Accreditation == null || user.Accreditation.Status != AccreditationStatus.Approved)
                {
                    _logger.LogInformation("User {UserId} is not accredited", userId);
                    return (false, GetDefaultNonAccreditedLimit(), InvestorClassification.NonAccredited.ToString());
                }

                // If accreditation has expired
                if (user.Accreditation.ExpiresAt.HasValue && user.Accreditation.ExpiresAt.Value < DateTime.UtcNow)
                {
                    _logger.LogInformation("User {UserId} accreditation expired on {ExpiryDate}", 
                        userId, user.Accreditation.ExpiresAt.Value);
                    
                    // Update accreditation status to expired if not already
                    if (user.Accreditation.Status != AccreditationStatus.Expired)
                    {
                        user.Accreditation.Status = AccreditationStatus.Expired;
                        user.Accreditation.LastUpdatedAt = DateTime.UtcNow;
                        await _userRepository.UpdateAccreditationAsync(user.Accreditation);
                    }
                    
                    return (false, GetDefaultNonAccreditedLimit(), InvestorClassification.NonAccredited.ToString());
                }

                // Get limit and return status
                var limit = await GetInvestmentLimitAsync(userId);
                return (true, limit, user.Accreditation.InvestorClassification.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accreditation status for user {UserId}: {Message}", userId, ex.Message);
                return (false, GetDefaultNonAccreditedLimit(), InvestorClassification.NonAccredited.ToString());
            }
        }

        private decimal GetDefaultNonAccreditedLimit()
        {
            // Default limit for non-accredited investors
            return 50000; // $50,000
        }
    }
}