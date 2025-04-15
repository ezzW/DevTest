// Emtelaak.UserRegistration.Application/DTOs/DocumentUploadResultDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class DocumentUploadResultDto
    {
        public Guid DocumentId { get; set; }
        public string DocumentType { get; set; }
        public string FileName { get; set; }
        public DateTime UploadedAt { get; set; }
        public string Status { get; set; }
    }
}
