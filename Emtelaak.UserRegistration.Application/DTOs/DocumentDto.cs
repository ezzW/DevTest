// Emtelaak.UserRegistration.Application/DTOs/DocumentDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class DocumentDto
    {
        public Guid Id { get; set; }
        public string DocumentType { get; set; }
        public string FileName { get; set; }
        public DateTime UploadedAt { get; set; }
        public string Status { get; set; }
        public string RejectionReason { get; set; }
        public string FileUrl { get; set; }
    }
}
