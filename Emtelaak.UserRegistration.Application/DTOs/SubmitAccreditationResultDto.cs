// Emtelaak.UserRegistration.Application/DTOs/SubmitAccreditationResultDto.cs
using System;
using System.Collections.Generic;

namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class SubmitAccreditationResultDto
    {
        public Guid AccreditationId { get; set; }
        public string Status { get; set; }
        public string InvestorClassification { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public bool Successful { get; set; }
        public string Message { get; set; }
        public List<string> RequiredDocuments { get; set; } = new List<string>();
        public decimal? InvestmentLimitAmount { get; set; }
        public List<DocumentRequirementDto> DocumentRequirements { get; set; } = new List<DocumentRequirementDto>();
        public string NextSteps { get; set; }
    }

    public class DocumentRequirementDto
    {
        public string DocumentType { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public bool Submitted { get; set; }
        public string Status { get; set; }
    }
}