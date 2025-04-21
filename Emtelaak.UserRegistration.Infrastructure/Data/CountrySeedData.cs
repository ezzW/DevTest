using Emtelaak.UserRegistration.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Emtelaak.UserRegistration.Infrastructure.Data
{
    /// <summary>
    /// Provides seed data for countries
    /// </summary>
    public static class CountrySeedData
    {
        /// <summary>
        /// Seeds countries data to the database
        /// </summary>
        /// <param name="modelBuilder">Model builder</param>
        public static void SeedCountries(this ModelBuilder modelBuilder)
        {
            var countries = new List<Country>
            {
                new Country { Id = Guid.NewGuid(), Code = "AF", NameEn = "Afghanistan", NameAr = "أفغانستان", PhoneCode = "93", Flag = "🇦🇫", IsActive = true, DisplayOrder = 1 },
                new Country { Id = Guid.NewGuid(), Code = "AL", NameEn = "Albania", NameAr = "ألبانيا", PhoneCode = "355", Flag = "🇦🇱", IsActive = true, DisplayOrder = 2 },
                new Country { Id = Guid.NewGuid(), Code = "DZ", NameEn = "Algeria", NameAr = "الجزائر", PhoneCode = "213", Flag = "🇩🇿", IsActive = true, DisplayOrder = 3 },
                new Country { Id = Guid.NewGuid(), Code = "AD", NameEn = "Andorra", NameAr = "أندورا", PhoneCode = "376", Flag = "🇦🇩", IsActive = true, DisplayOrder = 4 },
                new Country { Id = Guid.NewGuid(), Code = "AO", NameEn = "Angola", NameAr = "أنغولا", PhoneCode = "244", Flag = "🇦🇴", IsActive = true, DisplayOrder = 5 },
                new Country { Id = Guid.NewGuid(), Code = "AR", NameEn = "Argentina", NameAr = "الأرجنتين", PhoneCode = "54", Flag = "🇦🇷", IsActive = true, DisplayOrder = 6 },
                new Country { Id = Guid.NewGuid(), Code = "AM", NameEn = "Armenia", NameAr = "أرمينيا", PhoneCode = "374", Flag = "🇦🇲", IsActive = true, DisplayOrder = 7 },
                new Country { Id = Guid.NewGuid(), Code = "AU", NameEn = "Australia", NameAr = "أستراليا", PhoneCode = "61", Flag = "🇦🇺", IsActive = true, DisplayOrder = 8 },
                new Country { Id = Guid.NewGuid(), Code = "AT", NameEn = "Austria", NameAr = "النمسا", PhoneCode = "43", Flag = "🇦🇹", IsActive = true, DisplayOrder = 9 },
                new Country { Id = Guid.NewGuid(), Code = "AZ", NameEn = "Azerbaijan", NameAr = "أذربيجان", PhoneCode = "994", Flag = "🇦🇿", IsActive = true, DisplayOrder = 10 },
                new Country { Id = Guid.NewGuid(), Code = "BH", NameEn = "Bahrain", NameAr = "البحرين", PhoneCode = "973", Flag = "🇧🇭", IsActive = true, DisplayOrder = 11 },
                new Country { Id = Guid.NewGuid(), Code = "BD", NameEn = "Bangladesh", NameAr = "بنغلاديش", PhoneCode = "880", Flag = "🇧🇩", IsActive = true, DisplayOrder = 12 },
                new Country { Id = Guid.NewGuid(), Code = "BE", NameEn = "Belgium", NameAr = "بلجيكا", PhoneCode = "32", Flag = "🇧🇪", IsActive = true, DisplayOrder = 13 },
                new Country { Id = Guid.NewGuid(), Code = "BR", NameEn = "Brazil", NameAr = "البرازيل", PhoneCode = "55", Flag = "🇧🇷", IsActive = true, DisplayOrder = 14 },
                new Country { Id = Guid.NewGuid(), Code = "CA", NameEn = "Canada", NameAr = "كندا", PhoneCode = "1", Flag = "🇨🇦", IsActive = true, DisplayOrder = 15 },
                new Country { Id = Guid.NewGuid(), Code = "CN", NameEn = "China", NameAr = "الصين", PhoneCode = "86", Flag = "🇨🇳", IsActive = true, DisplayOrder = 16 },
                new Country { Id = Guid.NewGuid(), Code = "EG", NameEn = "Egypt", NameAr = "مصر", PhoneCode = "20", Flag = "🇪🇬", IsActive = true, DisplayOrder = 17 },
                new Country { Id = Guid.NewGuid(), Code = "FR", NameEn = "France", NameAr = "فرنسا", PhoneCode = "33", Flag = "🇫🇷", IsActive = true, DisplayOrder = 18 },
                new Country { Id = Guid.NewGuid(), Code = "DE", NameEn = "Germany", NameAr = "ألمانيا", PhoneCode = "49", Flag = "🇩🇪", IsActive = true, DisplayOrder = 19 },
                new Country { Id = Guid.NewGuid(), Code = "IN", NameEn = "India", NameAr = "الهند", PhoneCode = "91", Flag = "🇮🇳", IsActive = true, DisplayOrder = 20 },
                new Country { Id = Guid.NewGuid(), Code = "ID", NameEn = "Indonesia", NameAr = "إندونيسيا", PhoneCode = "62", Flag = "🇮🇩", IsActive = true, DisplayOrder = 21 },
                new Country { Id = Guid.NewGuid(), Code = "IR", NameEn = "Iran", NameAr = "إيران", PhoneCode = "98", Flag = "🇮🇷", IsActive = true, DisplayOrder = 22 },
                new Country { Id = Guid.NewGuid(), Code = "IQ", NameEn = "Iraq", NameAr = "العراق", PhoneCode = "964", Flag = "🇮🇶", IsActive = true, DisplayOrder = 23 },
                new Country { Id = Guid.NewGuid(), Code = "IE", NameEn = "Ireland", NameAr = "أيرلندا", PhoneCode = "353", Flag = "🇮🇪", IsActive = true, DisplayOrder = 24 },
                new Country { Id = Guid.NewGuid(), Code = "IT", NameEn = "Italy", NameAr = "إيطاليا", PhoneCode = "39", Flag = "🇮🇹", IsActive = true, DisplayOrder = 25 },
                new Country { Id = Guid.NewGuid(), Code = "JP", NameEn = "Japan", NameAr = "اليابان", PhoneCode = "81", Flag = "🇯🇵", IsActive = true, DisplayOrder = 26 },
                new Country { Id = Guid.NewGuid(), Code = "JO", NameEn = "Jordan", NameAr = "الأردن", PhoneCode = "962", Flag = "🇯🇴", IsActive = true, DisplayOrder = 27 },
                new Country { Id = Guid.NewGuid(), Code = "KW", NameEn = "Kuwait", NameAr = "الكويت", PhoneCode = "965", Flag = "🇰🇼", IsActive = true, DisplayOrder = 28 },
                new Country { Id = Guid.NewGuid(), Code = "LB", NameEn = "Lebanon", NameAr = "لبنان", PhoneCode = "961", Flag = "🇱🇧", IsActive = true, DisplayOrder = 29 },
                new Country { Id = Guid.NewGuid(), Code = "LY", NameEn = "Libya", NameAr = "ليبيا", PhoneCode = "218", Flag = "🇱🇾", IsActive = true, DisplayOrder = 30 },
                new Country { Id = Guid.NewGuid(), Code = "MY", NameEn = "Malaysia", NameAr = "ماليزيا", PhoneCode = "60", Flag = "🇲🇾", IsActive = true, DisplayOrder = 31 },
                new Country { Id = Guid.NewGuid(), Code = "MV", NameEn = "Maldives", NameAr = "جزر المالديف", PhoneCode = "960", Flag = "🇲🇻", IsActive = true, DisplayOrder = 32 },
                new Country { Id = Guid.NewGuid(), Code = "MX", NameEn = "Mexico", NameAr = "المكسيك", PhoneCode = "52", Flag = "🇲🇽", IsActive = true, DisplayOrder = 33 },
                new Country { Id = Guid.NewGuid(), Code = "MA", NameEn = "Morocco", NameAr = "المغرب", PhoneCode = "212", Flag = "🇲🇦", IsActive = true, DisplayOrder = 34 },
                new Country { Id = Guid.NewGuid(), Code = "NL", NameEn = "Netherlands", NameAr = "هولندا", PhoneCode = "31", Flag = "🇳🇱", IsActive = true, DisplayOrder = 35 },
                new Country { Id = Guid.NewGuid(), Code = "NZ", NameEn = "New Zealand", NameAr = "نيوزيلندا", PhoneCode = "64", Flag = "🇳🇿", IsActive = true, DisplayOrder = 36 },
                new Country { Id = Guid.NewGuid(), Code = "NG", NameEn = "Nigeria", NameAr = "نيجيريا", PhoneCode = "234", Flag = "🇳🇬", IsActive = true, DisplayOrder = 37 },
                new Country { Id = Guid.NewGuid(), Code = "OM", NameEn = "Oman", NameAr = "عمان", PhoneCode = "968", Flag = "🇴🇲", IsActive = true, DisplayOrder = 38 },
                new Country { Id = Guid.NewGuid(), Code = "PK", NameEn = "Pakistan", NameAr = "باكستان", PhoneCode = "92", Flag = "🇵🇰", IsActive = true, DisplayOrder = 39 },
                new Country { Id = Guid.NewGuid(), Code = "PS", NameEn = "Palestine", NameAr = "فلسطين", PhoneCode = "970", Flag = "🇵🇸", IsActive = true, DisplayOrder = 40 },
                new Country { Id = Guid.NewGuid(), Code = "PH", NameEn = "Philippines", NameAr = "الفلبين", PhoneCode = "63", Flag = "🇵🇭", IsActive = true, DisplayOrder = 41 },
                new Country { Id = Guid.NewGuid(), Code = "PL", NameEn = "Poland", NameAr = "بولندا", PhoneCode = "48", Flag = "🇵🇱", IsActive = true, DisplayOrder = 42 },
                new Country { Id = Guid.NewGuid(), Code = "PT", NameEn = "Portugal", NameAr = "البرتغال", PhoneCode = "351", Flag = "🇵🇹", IsActive = true, DisplayOrder = 43 },
                new Country { Id = Guid.NewGuid(), Code = "QA", NameEn = "Qatar", NameAr = "قطر", PhoneCode = "974", Flag = "🇶🇦", IsActive = true, DisplayOrder = 44 },
                new Country { Id = Guid.NewGuid(), Code = "RO", NameEn = "Romania", NameAr = "رومانيا", PhoneCode = "40", Flag = "🇷🇴", IsActive = true, DisplayOrder = 45 },
                new Country { Id = Guid.NewGuid(), Code = "RU", NameEn = "Russia", NameAr = "روسيا", PhoneCode = "7", Flag = "🇷🇺", IsActive = true, DisplayOrder = 46 },
                new Country { Id = Guid.NewGuid(), Code = "SA", NameEn = "Saudi Arabia", NameAr = "المملكة العربية السعودية", PhoneCode = "966", Flag = "🇸🇦", IsActive = true, DisplayOrder = 47 },
                new Country { Id = Guid.NewGuid(), Code = "SG", NameEn = "Singapore", NameAr = "سنغافورة", PhoneCode = "65", Flag = "🇸🇬", IsActive = true, DisplayOrder = 48 },
                new Country { Id = Guid.NewGuid(), Code = "ZA", NameEn = "South Africa", NameAr = "جنوب أفريقيا", PhoneCode = "27", Flag = "🇿🇦", IsActive = true, DisplayOrder = 49 },
                new Country { Id = Guid.NewGuid(), Code = "KR", NameEn = "South Korea", NameAr = "كوريا الجنوبية", PhoneCode = "82", Flag = "🇰🇷", IsActive = true, DisplayOrder = 50 },
                new Country { Id = Guid.NewGuid(), Code = "ES", NameEn = "Spain", NameAr = "إسبانيا", PhoneCode = "34", Flag = "🇪🇸", IsActive = true, DisplayOrder = 51 },
                new Country { Id = Guid.NewGuid(), Code = "LK", NameEn = "Sri Lanka", NameAr = "سريلانكا", PhoneCode = "94", Flag = "🇱🇰", IsActive = true, DisplayOrder = 52 },
                new Country { Id = Guid.NewGuid(), Code = "SD", NameEn = "Sudan", NameAr = "السودان", PhoneCode = "249", Flag = "🇸🇩", IsActive = true, DisplayOrder = 53 },
                new Country { Id = Guid.NewGuid(), Code = "SE", NameEn = "Sweden", NameAr = "السويد", PhoneCode = "46", Flag = "🇸🇪", IsActive = true, DisplayOrder = 54 },
                new Country { Id = Guid.NewGuid(), Code = "CH", NameEn = "Switzerland", NameAr = "سويسرا", PhoneCode = "41", Flag = "🇨🇭", IsActive = true, DisplayOrder = 55 },
                new Country { Id = Guid.NewGuid(), Code = "SY", NameEn = "Syria", NameAr = "سوريا", PhoneCode = "963", Flag = "🇸🇾", IsActive = true, DisplayOrder = 56 },
                new Country { Id = Guid.NewGuid(), Code = "TH", NameEn = "Thailand", NameAr = "تايلاند", PhoneCode = "66", Flag = "🇹🇭", IsActive = true, DisplayOrder = 57 },
                new Country { Id = Guid.NewGuid(), Code = "TN", NameEn = "Tunisia", NameAr = "تونس", PhoneCode = "216", Flag = "🇹🇳", IsActive = true, DisplayOrder = 58 },
                new Country { Id = Guid.NewGuid(), Code = "TR", NameEn = "Turkey", NameAr = "تركيا", PhoneCode = "90", Flag = "🇹🇷", IsActive = true, DisplayOrder = 59 },
                new Country { Id = Guid.NewGuid(), Code = "AE", NameEn = "United Arab Emirates", NameAr = "الإمارات العربية المتحدة", PhoneCode = "971", Flag = "🇦🇪", IsActive = true, DisplayOrder = 60 },
                new Country { Id = Guid.NewGuid(), Code = "GB", NameEn = "United Kingdom", NameAr = "المملكة المتحدة", PhoneCode = "44", Flag = "🇬🇧", IsActive = true, DisplayOrder = 61 },
                new Country { Id = Guid.NewGuid(), Code = "US", NameEn = "United States", NameAr = "الولايات المتحدة الأمريكية", PhoneCode = "1", Flag = "🇺🇸", IsActive = true, DisplayOrder = 62 },
                new Country { Id = Guid.NewGuid(), Code = "YE", NameEn = "Yemen", NameAr = "اليمن", PhoneCode = "967", Flag = "🇾🇪", IsActive = true, DisplayOrder = 63 }
            };

            modelBuilder.Entity<Country>().HasData(countries);
        }
    }
}