// Emtelaak.UserRegistration.Application/Services/DocumentRequirementsService.cs
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emtelaak.UserRegistration.Application.Services
{
    public class DocumentRequirementsService : IDocumentRequirementsService
    {
        private readonly ILogger<DocumentRequirementsService> _logger;

        public DocumentRequirementsService(ILogger<DocumentRequirementsService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<RequiredDocumentDto>> GetAccreditationDocumentRequirementsAsync(string investorClassification)
        {
            try
            {
                _logger.LogInformation("Getting document requirements for classification: {Classification}", investorClassification);
                
                var requirements = new List<RequiredDocumentDto>();
                
                // Add common documents for all investor types
                AddCommonDocumentRequirements(requirements);
                
                // Add specific document requirements based on investor classification
                if (!string.IsNullOrWhiteSpace(investorClassification))
                {
                    if (Enum.TryParse<InvestorClassification>(investorClassification, true, out var classification))
                    {
                        // Add requirements specific to this classification
                        var classRequirements = GetRequirementsForClassification(classification);
                        requirements.AddRange(classRequirements);
                    }
                    else
                    {
                        _logger.LogWarning("Invalid investor classification: {Classification}", investorClassification);
                    }
                }
                
                return requirements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document requirements for classification {Classification}: {Message}", 
                    investorClassification, ex.Message);
                throw;
            }
        }

        public async Task<List<RequiredDocumentDto>> GetAllAccreditationDocumentRequirementsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all document requirements");
                
                var requirements = new List<RequiredDocumentDto>();
                
                // Add common documents
                AddCommonDocumentRequirements(requirements);
                
                // Add documents for all classifications
                foreach (InvestorClassification classification in Enum.GetValues(typeof(InvestorClassification)))
                {
                    var classRequirements = GetRequirementsForClassification(classification);
                    // Avoid duplicates
                    foreach (var req in classRequirements)
                    {
                        if (!requirements.Exists(r => r.DocumentType == req.DocumentType))
                        {
                            requirements.Add(req);
                        }
                    }
                }
                
                return requirements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all document requirements: {Message}", ex.Message);
                throw;
            }
        }

        public List<RequiredDocumentDto> GetRequirementsForClassification(InvestorClassification classification)
        {
            var requirements = new List<RequiredDocumentDto>();
            
            switch (classification)
            {
                case InvestorClassification.Accredited:
                    requirements.Add(new RequiredDocumentDto { DocumentType = "TaxReturn", Required = true, Description = "Most recent tax return" });
                    requirements.Add(new RequiredDocumentDto { DocumentType = "FinancialStatement", Required = true, Description = "Financial statement from bank or broker" });
                    requirements.Add(new RequiredDocumentDto { DocumentType = "AccreditationCertificate", Required = false, Description = "Accreditation certificate if previously issued" });
                    requirements.Add(new RequiredDocumentDto { DocumentType = "NetWorthVerification", Required = true, Description = "Net worth verification letter" });
                    break;

                case InvestorClassification.Qualified:
                    requirements.Add(new RequiredDocumentDto { DocumentType = "TaxReturn", Required = true, Description = "Most recent tax return" });
                    requirements.Add(new RequiredDocumentDto { DocumentType = "FinancialStatement", Required = true, Description = "Financial statement from bank or broker" });
                    requirements.Add(new RequiredDocumentDto { DocumentType = "InvestmentHistory", Required = false, Description = "Investment history or portfolio statement" });
                    break;

                case InvestorClassification.Institutional:
                    requirements.Add(new RequiredDocumentDto { DocumentType = "CompanyRegistration", Required = true, Description = "Company registration certificate" });
                    requirements.Add(new RequiredDocumentDto { DocumentType = "FinancialStatement", Required = true, Description = "Institutional financial statement" });
                    requirements.Add(new RequiredDocumentDto { DocumentType = "BusinessLicense", Required = true, Description = "Business license" });
                    break;

                case InvestorClassification.NonAccredited:
                    requirements.Add(new RequiredDocumentDto { DocumentType = "BankStatement", Required = true, Description = "Recent bank statement" });
                    break;
            }
            
            return requirements;
        }

        private void AddCommonDocumentRequirements(List<RequiredDocumentDto> requirements)
        {
            requirements.Add(new RequiredDocumentDto { DocumentType = "IdCard", Required = true, Description = "Government-issued ID card" });
            requirements.Add(new RequiredDocumentDto { DocumentType = "IncomeProof", Required = true, Description = "Proof of income (pay stub, employment letter)" });
        }
    }
}