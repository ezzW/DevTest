// Emtelaak.UserRegistration.Infrastructure/Data/Migrations/ModelBuilderExtensions.cs
using Microsoft.EntityFrameworkCore;
using Emtelaak.UserRegistration.Infrastructure.Data;

namespace Emtelaak.UserRegistration.Infrastructure.Data
{
    /// <summary>
    /// Extension methods for seeding data
    /// </summary>
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Seeds all initial data
        /// </summary>
        /// <param name="modelBuilder">Model builder</param>
        public static void SeedAllData(this ModelBuilder modelBuilder)
        {
            // Seed countries
            modelBuilder.SeedCountries();

            // Add other seed data methods here as needed
        }
    }
}