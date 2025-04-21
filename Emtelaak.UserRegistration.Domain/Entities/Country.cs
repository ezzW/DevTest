// Emtelaak.UserRegistration.Domain/Entities/Country.cs
using System;

namespace Emtelaak.UserRegistration.Domain.Entities
{
    /// <summary>
    /// Country information entity with localization support
    /// </summary>
    public class Country
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ISO 3166-1 alpha-2 country code (2 letters)
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Country name in English
        /// </summary>
        public string NameEn { get; set; }

        /// <summary>
        /// Country name in Arabic
        /// </summary>
        public string NameAr { get; set; }

        /// <summary>
        /// Country phone code (without +)
        /// </summary>
        public string PhoneCode { get; set; }

        /// <summary>
        /// Flag emoji or URL to flag image
        /// </summary>
        public string Flag { get; set; }

        /// <summary>
        /// Display order for sorting
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Indicates if the country is active and should be displayed
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Two-letter language code for the country
        /// </summary>
        public string? LanguageCode { get; set; }

        /// <summary>
        /// Currency code for the country
        /// </summary>
        public string? CurrencyCode { get; set; }
    }
}