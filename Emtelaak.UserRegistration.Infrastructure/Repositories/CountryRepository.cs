// Emtelaak.UserRegistration.Infrastructure/Repositories/CountryRepository.cs
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Emtelaak.UserRegistration.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of ICountryRepository
    /// </summary>
    public class CountryRepository : ICountryRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<CountryRepository> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext">Database context</param>
        /// <param name="logger">Logger</param>
        public CountryRepository(
            AppDbContext dbContext,
            ILogger<CountryRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Country>> GetAllAsync()
        {
            return await _dbContext.Countries.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<Country>> GetAllCountriesOrderedAsync()
        {
            return await _dbContext.Countries
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.NameEn)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<Country>> GetActiveCountriesAsync()
        {
            return await _dbContext.Countries
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.NameEn)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Country> GetByIdAsync(Guid id)
        {
            return await _dbContext.Countries.FindAsync(id);
        }

        /// <inheritdoc/>
        public async Task<Country> GetCountryByCodeAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }

            return await _dbContext.Countries
                .FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant());
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Country>> GetAsync(Expression<Func<Country, bool>> predicate)
        {
            return await _dbContext.Countries
                .Where(predicate)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<Country>> SearchCountriesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetActiveCountriesAsync();
            }

            var term = searchTerm.Trim().ToLowerInvariant();

            return await _dbContext.Countries
                .Where(c => c.IsActive &&
                           (c.NameEn.ToLower().Contains(term) ||
                            c.NameAr.Contains(term) ||
                            c.Code.ToLower().Contains(term) ||
                            c.PhoneCode.Contains(term)))
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.NameEn)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Country> AddAsync(Country entity)
        {
            await _dbContext.Countries.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Country>> AddRangeAsync(IEnumerable<Country> entities)
        {
            await _dbContext.Countries.AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();
            return entities;
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Country entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Guid id)
        {
            var entity = await _dbContext.Countries.FindAsync(id);
            if (entity != null)
            {
                _dbContext.Countries.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}