// Emtelaak.UserRegistration.Domain/Enums/DocumentType.cs
namespace Emtelaak.UserRegistration.Domain.Enums
{
    public enum DocumentType
    {
        // Standard identification documents
        IdCard = 1,
        Passport = 2,
        DriversLicense = 3,
        UtilityBill = 4,
        BankStatement = 5,
        IncomeProof = 6,
        CompanyRegistration = 7,
        ProfilePicture = 8,
        
        // Accreditation-specific financial documents
        TaxReturn = 9,
        FinancialStatement = 10,
        AccreditationCertificate = 11,
        InvestmentHistory = 12,
        InvestmentStatement = 20,
        EmploymentVerification = 13,
        NetWorthVerification = 14,
        SecuritiesStatement = 15,
        PropertyDeed = 16,
        BusinessLicense = 17,
        ProfessionalLicense = 18,
        InvestmentAdvisorLetter = 19
    }
}