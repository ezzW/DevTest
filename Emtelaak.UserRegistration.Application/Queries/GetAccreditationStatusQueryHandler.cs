// Emtelaak.UserRegistration.Application/Queries/GetAccreditationStatusQueryHandler.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Queries
{
    public class GetAccreditationStatusQueryHandler : IRequestHandler<GetAccreditationStatusQuery, AccreditationStatusDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAccreditationStatusQueryHandler> _logger;

        public GetAccreditationStatusQueryHandler(
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<GetAccreditationStatusQueryHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AccreditationStatusDto> Handle(GetAccreditationStatusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting accreditation status for user: {UserId}", request.UserId);

                // Get the user with details including accreditation
                var user = await _userRepository.GetUserByIdWithDetailsAsync(request.UserId);
                if (user == null)
                {
                    throw new ApplicationException($"User with ID {request.UserId} not found");
                }

                // Check if user has an accreditation
                if (user.Accreditation == null)
                {
                    // Return empty status with suggestions
                    return new AccreditationStatusDto
                    {
                        Id = Guid.Empty,
                        Status = "NotStarted",
                        NextStep = "Submit an accreditation application to gain access to investment opportunities",
                        RequiredDocuments = GetRequiredDocumentsForAllClassifications(),
                        LastUpdatedAt = DateTime.UtcNow
                    };
                }

                // Map to DTO
                var accreditationDto = _mapper.Map<AccreditationStatusDto>(user.Accreditation);

                // Get documents associated with this user
                var userDocuments = await _userRepository.GetDocumentsByUserIdAsync(request.UserId);
                var accreditationDocuments = userDocuments
                    .Where(d => IsAccreditationDocumentType(d.DocumentType))
                    .ToList();

                // Map documents
                accreditationDto.Documents = _mapper.Map<List<DocumentDto>>(accreditationDocuments);

                // Add required documents based on investor classification
                accreditationDto.RequiredDocuments = GetRequiredDocuments(user.Accreditation.InvestorClassification);

                // Set next step based on status
                accreditationDto.NextStep = GetNextStep(user.Accreditation.Status, accreditationDocuments, accreditationDto.RequiredDocuments);

                return accreditationDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accreditation status: {Message}", ex.Message);
                throw;
            }
        }

        private bool IsAccreditationDocumentType(DocumentType documentType)
        {
            // These are document types commonly associated with accreditation
            return documentType == DocumentType.TaxReturn ||
                   documentType == DocumentType.FinancialStatement ||
                   documentType == DocumentType.InvestmentStatement ||
                   documentType == DocumentType.AccreditationCertificate ||
                   documentType == DocumentType.IncomeProof ||
                   documentType == DocumentType.BankStatement;
        }

        private List<string> GetRequiredDocuments(InvestorClassification classification)
        {
            var documents = new List<string>();
            
            // Common documents for all investor types
            documents.Add(DocumentType.IdCard.ToString());
            documents.Add(DocumentType.IncomeProof.ToString());
            
            // Specific documents based on investor classification
            switch (classification)
            {
                case InvestorClassification.Accredited:
                    documents.Add(DocumentType.TaxReturn.ToString());
                    documents.Add(DocumentType.FinancialStatement.ToString());
                    documents.Add(DocumentType.AccreditationCertificate.ToString());
                    break;
                    
                case InvestorClassification.Qualified:
                    documents.Add(DocumentType.TaxReturn.ToString());
                    documents.Add(DocumentType.FinancialStatement.ToString());
                    break;
                    
                case InvestorClassification.Institutional:
                    documents.Add(DocumentType.CompanyRegistration.ToString());
                    documents.Add(DocumentType.FinancialStatement.ToString());
                    break;
                    
                case InvestorClassification.NonAccredited:
                    documents.Add(DocumentType.BankStatement.ToString());
                    break;
            }
            
            return documents;
        }

        private List<string> GetRequiredDocumentsForAllClassifications()
        {
            var documents = new List<string>();
            
            // Add common documents
            documents.Add(DocumentType.IdCard.ToString());
            documents.Add(DocumentType.IncomeProof.ToString());
            documents.Add(DocumentType.TaxReturn.ToString());
            documents.Add(DocumentType.FinancialStatement.ToString());
            documents.Add(DocumentType.BankStatement.ToString());
            documents.Add(DocumentType.AccreditationCertificate.ToString());
            
            return documents;
        }

        private string GetNextStep(AccreditationStatus status, List<Document> uploadedDocuments, List<string> requiredDocuments)
        {
            switch (status)
            {
                case AccreditationStatus.Pending:
                    return "Your accreditation application is being reviewed. You will be notified once a decision has been made.";
                
                case AccreditationStatus.Approved:
                    return "Congratulations! Your application has been approved. You can now access accredited investor opportunities.";
                
                case AccreditationStatus.Rejected:
                    return "Your application was not approved. Please review the feedback and resubmit with the necessary corrections.";
                
                case AccreditationStatus.Expired:
                    return "Your accreditation has expired. Please submit a new application to renew your status.";
                
                default:
                    // Check missing documents
                    var uploadedDocumentTypes = uploadedDocuments.Select(d => d.DocumentType.ToString()).ToList();
                    var missingDocuments = requiredDocuments
                        .Where(rd => !uploadedDocumentTypes.Contains(rd))
                        .ToList();
                    
                    if (missingDocuments.Any())
                    {
                        return $"Upload required documents: {string.Join(", ", missingDocuments)}";
                    }
                    
                    return "Complete your investor profile and submit your accreditation application";
            }
        }
    }
}