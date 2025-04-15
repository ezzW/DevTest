using Emtelaak.UserRegistration.Domain.Enums;

namespace Emtelaak.UserRegistration.Domain.Entities
{
    public class ActivityLog
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public ActivityType ActivityType { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; } // Store as JSON
        public ActivityStatus Status { get; set; }
        public string? FailureReason { get; set; }

        // Navigation property
        public User User { get; set; }
    }
}
