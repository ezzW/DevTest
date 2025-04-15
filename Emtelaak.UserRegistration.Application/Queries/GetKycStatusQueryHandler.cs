// Emtelaak.UserRegistration.Application/Queries/GetKycStatusQueryHandler.cs
using AutoMapper;
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Queries
{
    public class GetKycStatusQueryHandler : IRequestHandler<GetKycStatusQuery, KycStatusDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetKycStatusQueryHandler> _logger;

        public GetKycStatusQueryHandler(
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<GetKycStatusQueryHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<KycStatusDto> Handle(GetKycStatusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting KYC status for user: {UserId}", request.UserId);

                // Get user with KYC verification data
                var user = await _userRepository.GetUserByIdWithDetailsAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    throw new ApplicationException($"User not found: {request.UserId}");
                }

                // If no KYC verification exists, return a default status
                if (user.KycVerification == null)
                {
                    _logger.LogInformation("No KYC verification found for user: {UserId}", request.UserId);

                    return new KycStatusDto
                    {
                        Status = "NotStarted",
                        RequiredDocuments = GetRequiredDocuments(user),
                        NextStep = "submitDocuments"
                    };
                }

                // Map KYC verification to DTO
                var kycStatusDto = _mapper.Map<KycStatusDto>(user.KycVerification);

                // Get required documents and their status
                kycStatusDto.RequiredDocuments = GetRequiredDocuments(user);

                // Determine next step based on status
                kycStatusDto.NextStep = DetermineNextStep(user.KycVerification, kycStatusDto.RequiredDocuments);

                _logger.LogInformation("KYC status retrieved for user: {UserId}, Status: {Status}",
                    request.UserId, kycStatusDto.Status);

                return kycStatusDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving KYC status: {Message}", ex.Message);
                throw;
            }
        }

        private List<RequiredDocumentDto> GetRequiredDocuments(User user)
        {
            // Define the required document types based on user type
            var requiredDocumentTypes = new List<DocumentType>();

            switch (user.UserType)
            {
                case UserType.IndividualInvestor:
                    requiredDocumentTypes.Add(DocumentType.IdCard);
                    requiredDocumentTypes.Add(DocumentType.Passport);
                    requiredDocumentTypes.Add(DocumentType.UtilityBill);
                    break;

                case UserType.InstitutionalInvestor:
                    requiredDocumentTypes.Add(DocumentType.CompanyRegistration);
                    requiredDocumentTypes.Add(DocumentType.UtilityBill);
                    break;

                case UserType.PropertyIssuer:
                    requiredDocumentTypes.Add(DocumentType.CompanyRegistration);
                    requiredDocumentTypes.Add(DocumentType.UtilityBill);
                    break;
            }

            // Check which documents the user has already uploaded
            var userDocuments = user.Documents ?? new List<Document>();

            // Create list of required documents with their status
            var requiredDocuments = new List<RequiredDocumentDto>();

            foreach (var docType in requiredDocumentTypes)
            {
                var doc = userDocuments.FirstOrDefault(d => d.DocumentType == docType);

                if (doc != null)
                {
                    // Document exists, map its status
                    requiredDocuments.Add(_mapper.Map<RequiredDocumentDto>(doc));
                }
                else
                {
                    // Document doesn't exist, create a placeholder
                    requiredDocuments.Add(new RequiredDocumentDto
                    {
                        Type = docType.ToString(),
                        Status = "Missing"
                    });
                }
            }

            return requiredDocuments;
        }

        private string DetermineNextStep(KycVerification verification, List<RequiredDocumentDto> requiredDocuments)
        {
            // If any required document is missing, next step is to upload documents
            if (requiredDocuments.Any(d => d.Status == "Missing"))
            {
                return "uploadDocuments";
            }

            // Based on KYC status
            switch (verification.Status)
            {
                case KycStatus.NotStarted:
                    return "submitDocuments";

                case KycStatus.InProgress:
                    return "waitForVerification";

                case KycStatus.PendingReview:
                    return "waitForReview";

                case KycStatus.AdditionalInfoRequired:
                    return "provideAdditionalInfo";

                case KycStatus.Approved:
                    return "completeAccreditation";

                case KycStatus.Rejected:
                    return "resubmitDocuments";

                case KycStatus.Expired:
                    return "renewVerification";

                default:
                    return "submitDocuments";
            }
        }
    }
}