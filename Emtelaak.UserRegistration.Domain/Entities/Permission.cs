﻿// Emtelaak.UserRegistration.Domain/Entities/User.cs
namespace Emtelaak.UserRegistration.Domain.Entities
{
    public class Permission
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public ICollection<RolePermission> RolePermissions { get; set; }

        public Permission()
        {
            RolePermissions = new List<RolePermission>();
        }
    }
}
