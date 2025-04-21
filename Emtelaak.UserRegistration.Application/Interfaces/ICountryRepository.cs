// Emtelaak.UserRegistration.Application/Interfaces/ICountryRepository.cs
using Emtelaak.UserRegistration.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emtelaak.UserRegistration.Application.Interfaces
{
    /// <summary>
    /// Repository for country data
    /// </summary>
    public interface ICountryRepository : IRepository<Country>
    {
        /// <summary>
        /// Gets a country by its ISO code
        /// </summary>
        /// <param name="code">ISO 3166-1 alpha-2 country code</param>
        /// <returns>Country entity or null if not found</returns>
        Task<Country> GetCountryByCodeAsync(string code);

        /// <summary>
        /// Gets all active countries
        /// </summary>
        /// <returns>List of active countries</returns>
        Task<List<Country>> GetActiveCountriesAsync();

        /// <summary>
        /// Gets all active countries matching the search term
        /// </summary>
        /// <param name="searchTerm">Search term for country name (in English or Arabic)</param>
        /// <returns>List of matching countries</returns>
        Task<List<Country>> SearchCountriesAsync(string searchTerm);

        /// <summary>
        /// Gets all countries sorted by display order
        /// </summary>
        /// <returns>List of all countries</returns>
        Task<List<Country>> GetAllCountriesOrderedAsync();
    }
}