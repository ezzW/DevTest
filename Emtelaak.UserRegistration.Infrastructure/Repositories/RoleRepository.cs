// Emtelaak.UserRegistration.Infrastructure/Repositories/RoleRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<RoleRepository> _logger;

        public RoleRepository(AppDbContext dbContext, ILogger<RoleRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Role> GetByIdAsync(Guid id)
        {
            return await _dbContext.Roles.FindAsync(id);
        }

        public async Task<IReadOnlyList<Role>> GetAllAsync()
        {
            return await _dbContext.Roles.ToListAsync();
        }

        public async Task<IReadOnlyList<Role>> GetAsync(Expression<Func<Role, bool>> predicate)
        {
            return await _dbContext.Roles.Where(predicate).ToListAsync();
        }

        public async Task<Role> AddAsync(Role entity)
        {
            await _dbContext.Roles.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<Role>> AddRangeAsync(IEnumerable<Role> entities)
        {
            await _dbContext.Roles.AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();
            return entities;
        }

        public async Task UpdateAsync(Role entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var role = await _dbContext.Roles.FindAsync(id);
            if (role != null)
            {
                _dbContext.Roles.Remove(role);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<Role> GetRoleByNameAsync(string roleName)
        {
            return await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Name == roleName);
        }

        public async Task<List<Role>> GetRolesByUserIdAsync(Guid userId)
        {
            return await _dbContext.UserRoles
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role)
                .ToListAsync();
        }

        public async Task<List<Permission>> GetPermissionsByRoleIdAsync(Guid roleId)
        {
            return await _dbContext.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission)
                .ToListAsync();
        }

        public async Task<List<Permission>> GetPermissionsByUserIdAsync(Guid userId)
        {
            // Get all permissions from all roles the user has
            var permissions = await _dbContext.UserRoles
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Join(
                    _dbContext.RolePermissions,
                    ur => ur.RoleId,
                    rp => rp.RoleId,
                    (ur, rp) => rp.Permission
                )
                .Distinct()
                .ToListAsync();

            return permissions;
        }

        public async Task<UserRole> AssignRoleToUserAsync(Guid userId, Guid roleId)
        {
            // Check if the role assignment already exists
            var existingAssignment = await _dbContext.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (existingAssignment != null)
            {
                // If it exists but is inactive, reactivate it
                if (!existingAssignment.IsActive)
                {
                    existingAssignment.IsActive = true;
                    existingAssignment.AssignedAt = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();
                    return existingAssignment;
                }

                // If it's already active, return it
                return existingAssignment;
            }

            // Create new role assignment
            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _dbContext.UserRoles.AddAsync(userRole);
            await _dbContext.SaveChangesAsync();
            return userRole;
        }

        public async Task RemoveRoleFromUserAsync(Guid userId, Guid roleId)
        {
            var userRole = await _dbContext.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.IsActive);

            if (userRole != null)
            {
                userRole.IsActive = false;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<User>> GetUsersByRoleIdAsync(Guid roleId)
        {
            return await _dbContext.UserRoles
                .Where(ur => ur.RoleId == roleId && ur.IsActive)
                .Include(ur => ur.User)
                .Select(ur => ur.User)
                .ToListAsync();
        }

        public Task<TEntity> GetByIdAsync<TEntity>(Guid id) where TEntity : class
        {
            throw new NotImplementedException();
        }
    }
}