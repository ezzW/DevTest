// Emtelaak.UserRegistration.Domain/Entities/User.cs
namespace Emtelaak.UserRegistration.Domain.Entities
{
    public class RolePermission
    {
        public Guid Id { get; set; }
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
        public DateTime AssignedAt { get; set; }
        public Guid? AssignedBy { get; set; }

        // Navigation properties
        public Role Role { get; set; }
        public Permission Permission { get; set; }
    }
}
