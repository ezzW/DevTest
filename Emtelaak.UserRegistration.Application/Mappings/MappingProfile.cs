// Emtelaak.UserRegistration.Application/Mappings/MappingProfile.cs
using AutoMapper;
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using Emtelaak.UserRegistration.Domain.Models;

namespace Emtelaak.UserRegistration.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserRegistrationDto, User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => UserStatus.Active))
                .ForMember(dest => dest.EmailVerified, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.PhoneVerified, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.TermsAcceptedAt, opt => opt.MapFrom(src => src.TermsAccepted ? DateTime.UtcNow : (DateTime?)null))
                .ForMember(dest => dest.TermsAcceptedVersion, opt => opt.MapFrom(src => src.TermsAccepted ? "1.0" : null))
                .ForMember(dest => dest.Documents, opt => opt.Ignore())
                .ForMember(dest => dest.KycVerification, opt => opt.Ignore())
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
                .ForMember(dest => dest.UserPreference, opt => opt.Ignore())
                .ForMember(dest => dest.Accreditation, opt => opt.Ignore())
                .ForMember(dest => dest.ActivityLogs, opt => opt.Ignore());

            // Map UserRegistrationDto to AuthUserModel (for identity operations)
            CreateMap<UserRegistrationDto, AuthUserModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.PhoneVerified, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.TermsAcceptedAt, opt => opt.MapFrom(src => src.TermsAccepted ? DateTime.UtcNow : (DateTime?)null))
                .ForMember(dest => dest.TermsAcceptedVersion, opt => opt.MapFrom(src => src.TermsAccepted ? "1.0" : null))
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.MapFrom(src => false));

            CreateMap<User, UserRegistrationResultDto>()
                .ForMember(dest => dest.RequiresEmailVerification, opt => opt.MapFrom(src => !src.EmailVerified))
                .ForMember(dest => dest.RequiresPhoneVerification, opt => opt.MapFrom(src => !src.PhoneVerified));

            CreateMap<User, UserProfileDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.UserType.ToString()))
                .ForMember(dest => dest.KycStatus, opt => opt.MapFrom(src => src.KycVerification != null ? src.KycVerification.Status.ToString() : "NotStarted"))
                .ForMember(dest => dest.AccreditationStatus, opt => opt.MapFrom(src => src.Accreditation != null ? src.Accreditation.Status.ToString() : "NotStarted"))
                .ForMember(dest => dest.ProfileCompletionPercentage, opt => opt.Ignore()) // Calculated in service
                .ForMember(dest => dest.ProfilePictureUrl, opt => opt.Ignore())// Set in handler
                .ForMember(dest => dest.KycStatus, opt => opt.MapFrom(src =>src.KycVerification != null ? src.KycVerification.Status.ToString() : "NotStarted"))
                .ForMember(dest => dest.AccreditationStatus, opt => opt.MapFrom(src => src.Accreditation != null ? src.Accreditation.Status.ToString() : "NotStarted"))
                .ForMember(dest => dest.UserPreference, opt => opt.MapFrom(src => src.UserPreference))
                .ForMember(dest => dest.Documents, opt => opt.MapFrom(src => src.Documents)) 
                .ForMember(dest => dest.RecentActivities, opt => opt.MapFrom(src => src.ActivityLogs)); 

            CreateMap<KycVerification, KycVerificationDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.RiskLevel, opt => opt.MapFrom(src => src.RiskLevel.ToString()))
                .ForMember(dest => dest.VerificationType, opt => opt.MapFrom(src => src.VerificationType.ToString()));

            CreateMap<Accreditation, AccreditationDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.InvestorClassification, opt => opt.MapFrom(src => src.InvestorClassification.ToString()));

            CreateMap<UserPreference, UserPreferenceDto>()
                .ForMember(dest => dest.Theme, opt => opt.MapFrom(src => src.Theme.ToString()));


            CreateMap<Document, DocumentDto>()
                .ForMember(dest => dest.DocumentType, opt => opt.MapFrom(src => src.DocumentType.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.VerificationStatus.ToString()));

            CreateMap<ActivityLog, ActivityLogDto>()
                .ForMember(dest => dest.ActivityType, opt => opt.MapFrom(src => src.ActivityType.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));


            // AuthUserModel to User mapping (for syncing domain user with auth user)
            CreateMap<AuthUserModel, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // ID is already set
                .ForMember(dest => dest.EmailVerified, opt => opt.MapFrom(src => src.EmailConfirmed))
                .ForMember(dest => dest.Documents, opt => opt.Ignore())
                .ForMember(dest => dest.KycVerification, opt => opt.Ignore())
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
                .ForMember(dest => dest.UserPreference, opt => opt.Ignore())
                .ForMember(dest => dest.Accreditation, opt => opt.Ignore())
                .ForMember(dest => dest.ActivityLogs, opt => opt.Ignore());

            // User to AuthUserModel mapping
            CreateMap<User, AuthUserModel>()
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.EmailVerified))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

            // KYC mappings
            CreateMap<KycVerification, KycStatusDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.RequiredDocuments, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.NextStep, opt => opt.Ignore()); // Set in service

            CreateMap<Document, RequiredDocumentDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.DocumentType.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.VerificationStatus.ToString()));

            CreateMap<Document, DocumentUploadResultDto>()
                .ForMember(dest => dest.DocumentType, opt => opt.MapFrom(src => src.DocumentType.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.VerificationStatus.ToString()));

            CreateMap<KycSubmissionDto, KycVerification>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => KycStatus.InProgress))
                .ForMember(dest => dest.SubmittedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.LastUpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.RiskLevel, opt => opt.MapFrom(src => RiskLevel.Low)) // Default risk level
                .ForMember(dest => dest.VerificationType, opt => opt.MapFrom(src => VerificationType.Basic)) // Default type
                .ForMember(dest => dest.VerificationId, opt => opt.Ignore()) // Generated in service
                .ForMember(dest => dest.ApprovedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RejectedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RejectionReason, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            // User preference mappings
            CreateMap<UserPreferenceDto, UserPreference>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Theme, opt => opt.MapFrom(src => Enum.Parse<ThemePreference>(src.Theme)))
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorMethod, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            CreateMap<UserPreference, UserPreferenceDto>()
                .ForMember(dest => dest.Theme, opt => opt.MapFrom(src => src.Theme.ToString()));

            // Profile update mappings
            CreateMap<UserProfileUpdateDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UserType, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.EmailVerified, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneVerified, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore())
                .ForMember(dest => dest.FailedLoginAttempts, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEndDate, opt => opt.Ignore())
                .ForMember(dest => dest.TermsAcceptedVersion, opt => opt.Ignore())
                .ForMember(dest => dest.TermsAcceptedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ReferralCode, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore())
                .ForMember(dest => dest.KycVerification, opt => opt.Ignore())
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
                .ForMember(dest => dest.UserPreference, opt => opt.Ignore())
                .ForMember(dest => dest.Accreditation, opt => opt.Ignore())
                .ForMember(dest => dest.ActivityLogs, opt => opt.Ignore());

            // Add mapping for identity result model
            CreateMap<IdentityResultModel, IdentityErrorModel>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => "Error"))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => "Unknown error"));

        }
    }
}