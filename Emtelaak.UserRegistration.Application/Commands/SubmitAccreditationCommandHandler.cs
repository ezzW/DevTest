// Emtelaak.UserRegistration.Application/Commands/SubmitAccreditationCommandHandler.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class SubmitAccreditationCommandHandler : IRequestHandler<SubmitAccreditationCommand, SubmitAccreditationResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger<SubmitAccreditationCommandHandler> _logger;

        public SubmitAccreditationCommandHandler(
            IUserRepository userRepository,
            IMapper mapper,
            IEmailService emailService,
            ILogger<SubmitAccreditationCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SubmitAccreditationResultDto> Handle(SubmitAccreditationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing accreditation submission for user: {UserId}", request.UserId);

                // Get user with details
                var user = await _userRepository.GetUserByIdWithDetailsAsync(request.UserId);
                if (user == null)
                {
                    throw new ApplicationException($"User with ID {request.UserId} not found");
                }

                // Check if user already has an accreditation
                if (user.Accreditation != null)
                {
                    // If accreditation is already in progress, approved, or expired, return appropriate message
                    if (user.Accreditation.Status != AccreditationStatus.Rejected)
                    {
                        return new SubmitAccreditationResultDto
                        {
                            AccreditationId = user.Accreditation.Id,
                            Status = user.Accreditation.Status.ToString(),
                            InvestorClassification = user.Accreditation.InvestorClassification.ToString(),
                            SubmittedAt = DateTime.UtcNow,
                            Successful = false,
                            Message = $"You already have an accreditation application with status: {user.Accreditation.Status}",
                            RequiredDocuments = new List<string>()
                        };
                    }
                    
                    // If rejected, we can update the existing one
                    user.Accreditation.InvestorClassification = Enum.Parse<InvestorClassification>(request.AccreditationData.InvestorClassification);
                    user.Accreditation.Status = AccreditationStatus.Pending;
                    user.Accreditation.IncomeLevel = request.AccreditationData.IncomeLevel;
                    user.Accreditation.NetWorth = request.AccreditationData.NetWorth;
                    user.Accreditation.LastUpdatedAt = DateTime.UtcNow;
                    user.Accreditation.ReviewNotes = request.AccreditationData.AdditionalInformation;
                    
                    // Update the new fields
                    user.Accreditation.YearsInvesting = request.AccreditationData.YearsInvesting;
                    user.Accreditation.HasPriorPrivateInvestments = request.AccreditationData.HasPriorPrivateInvestments;
                    user.Accreditation.InvestmentExperience = request.AccreditationData.InvestmentTypes != null ? 
                        System.Text.Json.JsonSerializer.Serialize(request.AccreditationData.InvestmentTypes) : null;
                    
                    // Entity information for institutional investors
                    user.Accreditation.EntityName = request.AccreditationData.EntityName;
                    user.Accreditation.EntityType = request.AccreditationData.EntityType;
                    user.Accreditation.EntityRegistrationNumber = request.AccreditationData.EntityRegistrationNumber;
                    user.Accreditation.EntityRegistrationDate = request.AccreditationData.EntityRegistrationDate;
                    
                    // Certification information
                    user.Accreditation.CertificationMethod = request.AccreditationData.CertificationMethod;
                    
                    await _userRepository.UpdateAccreditationAsync(user.Accreditation);
                    
                    // Create activity log for submission
                    var activityLog = new ActivityLog
                    {
                        UserId = request.UserId,
                        ActivityType = ActivityType.AccreditationSubmitted,
                        Timestamp = DateTime.UtcNow,
                        Status = ActivityStatus.Success,
                        Details = $"{{\"investorClassification\":\"{request.AccreditationData.InvestorClassification}\",\"resubmission\":\"true\"}}"
                    };
                    await _userRepository.AddActivityLogAsync(activityLog);
                    
                    return new SubmitAccreditationResultDto
                    {
                        AccreditationId = user.Accreditation.Id,
                        Status = user.Accreditation.Status.ToString(),
                        InvestorClassification = user.Accreditation.InvestorClassification.ToString(),
                        SubmittedAt = DateTime.UtcNow,
                        Successful = true,
                        Message = "Accreditation application resubmitted successfully",
                        RequiredDocuments = GetRequiredDocuments(user.Accreditation.InvestorClassification)
                    };
                }

                // Create new accreditation
                var accreditation = new Accreditation
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    InvestorClassification = Enum.Parse<InvestorClassification>(request.AccreditationData.InvestorClassification),
                    Status = AccreditationStatus.Pending,
                    IncomeLevel = request.AccreditationData.IncomeLevel,
                    NetWorth = request.AccreditationData.NetWorth,
                    LastUpdatedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    ReviewNotes = request.AccreditationData.AdditionalInformation,
                    
                    // Investment experience fields
                    YearsInvesting = request.AccreditationData.YearsInvesting,
                    HasPriorPrivateInvestments = request.AccreditationData.HasPriorPrivateInvestments,
                    InvestmentExperience = request.AccreditationData.InvestmentTypes != null ? 
                        System.Text.Json.JsonSerializer.Serialize(request.AccreditationData.InvestmentTypes) : null,
                    
                    // Entity information for institutional investors
                    EntityName = request.AccreditationData.EntityName,
                    EntityType = request.AccreditationData.EntityType,
                    EntityRegistrationNumber = request.AccreditationData.EntityRegistrationNumber,
                    EntityRegistrationDate = request.AccreditationData.EntityRegistrationDate,
                    
                    // Certification information
                    CertificationMethod = request.AccreditationData.CertificationMethod
                };

                // Link any provided documents
                if (request.AccreditationData.DocumentIds != null && request.AccreditationData.DocumentIds.Count > 0)
                {
                    foreach (var documentId in request.AccreditationData.DocumentIds)
                    {
                        var document = await _userRepository.GetDocumentByIdAsync(documentId);
                        if (document != null && document.UserId == request.UserId)
                        {
                            // Update document to associate it with the accreditation
                            document.VerificationStatus = DocumentVerificationStatus.Pending;
                            await _userRepository.UpdateDocumentAsync(document);
                        }
                    }
                }

                // Save to database
                var createdAccreditation = await _userRepository.AddAccreditationAsync(accreditation);

                // Create activity log for submission
                var submitActivityLog = new ActivityLog
                {
                    UserId = request.UserId,
                    ActivityType = ActivityType.AccreditationSubmitted,
                    Timestamp = DateTime.UtcNow,
                    Status = ActivityStatus.Success,
                    Details = $"{{\"investorClassification\":\"{request.AccreditationData.InvestorClassification}\"}}"
                };
                await _userRepository.AddActivityLogAsync(submitActivityLog);
                
                // Send email notification
                try
                {
                    await _emailService.SendAccreditationSubmittedEmailAsync(user.Email, user.FirstName);
                }
                catch (Exception ex)
                {
                    // Log but don't fail the operation if email fails
                    _logger.LogError(ex, "Error sending accreditation submission email: {Message}", ex.Message);
                }

                // Return result
                return new SubmitAccreditationResultDto
                {
                    AccreditationId = createdAccreditation.Id,
                    Status = createdAccreditation.Status.ToString(),
                    InvestorClassification = createdAccreditation.InvestorClassification.ToString(),
                    SubmittedAt = DateTime.UtcNow,
                    Successful = true,
                    Message = "Accreditation application submitted successfully",
                    RequiredDocuments = GetRequiredDocuments(createdAccreditation.InvestorClassification)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during accreditation submission: {Message}", ex.Message);
                return new SubmitAccreditationResultDto
                {
                    Successful = false,
                    Message = $"Error submitting accreditation: {ex.Message}",
                    RequiredDocuments = new List<string>()
                };
            }
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
    }
}