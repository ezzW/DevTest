// Emtelaak.UserRegistration.Application/DTOs/ActivityLogEntryDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class ActivityLogEntryDto
    {
        public Guid Id { get; set; }
        public string ActivityType { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
        public string DeviceInfo { get; set; }
        public string Status { get; set; }
        public string Details { get; set; }
    }
}
