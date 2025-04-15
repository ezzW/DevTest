// Emtelaak.UserRegistration.Domain/Entities/User.cs
namespace Emtelaak.UserRegistration.Domain.Entities
{
    public class UserRole
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public DateTime AssignedAt { get; set; }
        public Guid? AssignedBy { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Role Role { get; set; }
    }
}
