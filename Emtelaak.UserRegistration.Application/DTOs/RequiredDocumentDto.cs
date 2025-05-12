// Emtelaak.UserRegistration.Application/DTOs/RequiredDocumentDto.cs
using System;

namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class RequiredDocumentDto
    {
        public string DocumentType { get; set; }
        // Kept for backward compatibility with existing code
        public string Type { 
            get => DocumentType; 
            set => DocumentType = value; 
        }
        public string Status { get; set; }
        public string RejectionReason { get; set; }
        public bool Required { get; set; } = true;
        public string Description { get; set; }
        public bool Submitted { get; set; }
        public Guid? DocumentId { get; set; }
    }
}
