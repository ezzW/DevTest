// Emtelaak.UserRegistration.Application/DTOs/UserSessionDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class UserSessionDto
    {
        public Guid Id { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime LastActivityAt { get; set; }
        public string IpAddress { get; set; }
        public string DeviceInfo { get; set; }
        public bool IsActive { get; set; }
        public bool IsCurrent { get; set; }
    }
}
