// Emtelaak.UserRegistration.Application/DTOs/UserPreferenceDto.cs
// Emtelaak.UserRegistration.Application/DTOs/UserPreferenceDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class UserPreferenceDto
    {
        public string Language { get; set; }
        public string Theme { get; set; }
        public bool NotificationEmail { get; set; }
        public bool NotificationSms { get; set; }
        public bool NotificationPush { get; set; }
        public bool LoginNotification { get; set; }
    }
}
