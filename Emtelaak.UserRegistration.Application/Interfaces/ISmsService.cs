// Emtelaak.UserRegistration.Application/Interfaces/ISmsService.cs
namespace Emtelaak.UserRegistration.Application.Interfaces
{
    public interface ISmsService
    {
        Task<bool> SendVerificationSmsAsync(string phoneNumber, string code);
        Task<bool> SendTwoFactorCodeAsync(string phoneNumber, string code);
        Task<bool> SendLoginNotificationSmsAsync(string phoneNumber, string location, string time);
    }
}