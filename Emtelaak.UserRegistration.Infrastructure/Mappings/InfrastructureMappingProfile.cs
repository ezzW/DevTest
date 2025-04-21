// Emtelaak.UserRegistration.Infrastructure/Mappings/InfrastructureMappingProfile.cs
using System;
using System.Linq;
using AutoMapper;
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Models;
using Emtelaak.UserRegistration.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Emtelaak.UserRegistration.Infrastructure.Mappings
{
    public class InfrastructureMappingProfile : Profile
    {
        public InfrastructureMappingProfile()
        {
            // Map from domain models to infrastructure types
            CreateMap<AuthUserModel, ApplicationUser>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.UserName != null ? src.UserName.ToUpper() : null))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email != null ? src.Email.ToUpper() : null))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.CountryOfResidence, opt => opt.MapFrom(src => src.CountryOfResidence))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.EmailConfirmed))
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => src.PhoneNumberConfirmed))
                .ForMember(dest => dest.PhoneVerified, opt => opt.MapFrom(src => src.PhoneVerified))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => src.LastLoginAt))
                .ForMember(dest => dest.TermsAcceptedVersion, opt => opt.MapFrom(src => src.TermsAcceptedVersion))
                .ForMember(dest => dest.TermsAcceptedAt, opt => opt.MapFrom(src => src.TermsAcceptedAt))
                .ForMember(dest => dest.ReferralCode, opt => opt.MapFrom(src => src.ReferralCode))
                .ForMember(dest => dest.DomainUserId, opt => opt.MapFrom(src => src.DomainUserId))
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.MapFrom(src => src.TwoFactorEnabled))
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore());

            // Map from infrastructure types to domain models
            CreateMap<ApplicationUser, AuthUserModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.CountryOfResidence, opt => opt.MapFrom(src => src.CountryOfResidence))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.EmailConfirmed))
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => src.PhoneNumberConfirmed))
                .ForMember(dest => dest.PhoneVerified, opt => opt.MapFrom(src => src.PhoneVerified))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => src.LastLoginAt))
                .ForMember(dest => dest.TermsAcceptedVersion, opt => opt.MapFrom(src => src.TermsAcceptedVersion))
                .ForMember(dest => dest.TermsAcceptedAt, opt => opt.MapFrom(src => src.TermsAcceptedAt))
                .ForMember(dest => dest.ReferralCode, opt => opt.MapFrom(src => src.ReferralCode))
                .ForMember(dest => dest.DomainUserId, opt => opt.MapFrom(src => src.DomainUserId))
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.MapFrom(src => src.TwoFactorEnabled));

            // Map from identity result to domain model
            CreateMap<IdentityResult, IdentityResultModel>()
                .ForMember(dest => dest.Succeeded, opt => opt.MapFrom(src => src.Succeeded))
                .ForMember(dest => dest.Errors, opt => opt.MapFrom(src => src.Errors));

            // Map from identity error to domain model
            CreateMap<IdentityError, IdentityErrorModel>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            // Map from domain model to identity error
            CreateMap<IdentityErrorModel, IdentityError>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            CreateMap<Country, CountryDto>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.NameEn, opt => opt.MapFrom(src => src.NameEn))
                .ForMember(dest => dest.NameAr, opt => opt.MapFrom(src => src.NameAr))
                .ForMember(dest => dest.PhoneCode, opt => opt.MapFrom(src => src.PhoneCode))
                .ForMember(dest => dest.Flag, opt => opt.MapFrom(src => src.Flag));

            CreateMap<Country, CountryPhoneCodeDto>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.NameEn, opt => opt.MapFrom(src => src.NameEn))
                .ForMember(dest => dest.NameAr, opt => opt.MapFrom(src => src.NameAr))
                .ForMember(dest => dest.PhoneCode, opt => opt.MapFrom(src => src.PhoneCode))
                .ForMember(dest => dest.Flag, opt => opt.MapFrom(src => src.Flag));

        }
    }
}