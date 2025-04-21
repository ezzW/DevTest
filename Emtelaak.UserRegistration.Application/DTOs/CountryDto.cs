// Emtelaak.UserRegistration.Application/DTOs/CountryDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    /// <summary>
    /// Data transfer object for country information with localization support
    /// </summary>
    public class CountryDto
    {
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
    }

    /// <summary>
    /// Data transfer object for country phone code information with localization support
    /// </summary>
    public class CountryPhoneCodeDto
    {
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
        /// Country phone code with + prefix
        /// </summary>
        public string PhoneCode { get; set; }

        /// <summary>
        /// Flag emoji or URL to flag image
        /// </summary>
        public string Flag { get; set; }
    }
}