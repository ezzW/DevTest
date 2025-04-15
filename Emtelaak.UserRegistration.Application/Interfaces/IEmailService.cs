// Emtelaak.UserRegistration.Application/Interfaces/IEmailService.cs
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
    }
}
