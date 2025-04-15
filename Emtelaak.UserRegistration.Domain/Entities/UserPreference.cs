// Emtelaak.UserRegistration.Domain/Entities/User.cs
using Emtelaak.UserRegistration.Domain.Enums;

namespace Emtelaak.UserRegistration.Domain.Entities
{
    public class UserPreference
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Language { get; set; }
        public ThemePreference Theme { get; set; }
        public bool NotificationEmail { get; set; }
        public bool NotificationSms { get; set; }
        public bool NotificationPush { get; set; }
        public bool LoginNotification { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public TwoFactorMethod TwoFactorMethod { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public User User { get; set; }
    }
}
