// Emtelaak.UserRegistration.Application/Interfaces/IEmailService.cs
using System;
using System.Threading.Tasks;

namespace Emtelaak.UserRegistration.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string email, string name, string token);
        Task SendPasswordResetEmailAsync(string email, string name, string token);
        Task SendWelcomeEmailAsync(string email, string name);
        Task SendLoginNotificationAsync(string email, string name, string ipAddress, string location, string device);
        Task SendKycApprovedEmailAsync(string email, string name);
        Task SendKycRejectedEmailAsync(string email, string name, string reason);
        Task SendAccountLockedEmailAsync(string email, string name);
        Task SendTwoFactorCodeAsync(string email, string name, string code);
        
        // Accreditation related emails
        Task SendAccreditationSubmittedEmailAsync(string email, string name);
        Task SendAccreditationApprovedEmailAsync(string email, string name, string investorClassification = null, decimal? investmentLimit = null, DateTime? expiryDate = null);
        Task SendAccreditationRejectedEmailAsync(string email, string name, string reason);
        Task SendAccreditationExpiredEmailAsync(string email, string name, string investorClassification = null);
    }
}