// Emtelaak.UserRegistration.Application/DTOs/KycStatusDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class KycStatusDto
    {
        public string Status { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string RejectionReason { get; set; }
        public List<RequiredDocumentDto> RequiredDocuments { get; set; }
        public string NextStep { get; set; }
    }
}
