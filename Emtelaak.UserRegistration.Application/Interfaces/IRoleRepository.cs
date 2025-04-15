// Emtelaak.UserRegistration.Application/Interfaces/IRoleRepository.cs
using Emtelaak.UserRegistration.Domain.Entities;

namespace Emtelaak.UserRegistration.Application.Interfaces
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<Role> GetRoleByNameAsync(string roleName);
        Task<List<Role>> GetRolesByUserIdAsync(Guid userId);
        Task<List<Permission>> GetPermissionsByRoleIdAsync(Guid roleId);
        Task<List<Permission>> GetPermissionsByUserIdAsync(Guid userId);
        Task<UserRole> AssignRoleToUserAsync(Guid userId, Guid roleId);
        Task RemoveRoleFromUserAsync(Guid userId, Guid roleId);
        Task<List<User>> GetUsersByRoleIdAsync(Guid roleId);
    }
}