// Emtelaak.UserRegistration.Domain/Entities/Document.cs
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;

namespace Emtelaak.UserRegistration.Domain.Entities
{
    public class Document
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string FileName { get; set; }
        public int FileSize { get; set; }
        public string ContentType { get; set; }
        public string StoragePath { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DocumentVerificationStatus VerificationStatus { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? VerifiedAt { get; set; }
        
        // Accreditation-specific fields
        public bool IsAccreditationDocument { get; set; }
        public Guid? AccreditationId { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Accreditation Accreditation { get; set; }
    }
}