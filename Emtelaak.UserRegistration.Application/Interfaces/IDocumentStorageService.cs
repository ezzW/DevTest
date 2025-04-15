// Emtelaak.UserRegistration.Application/Interfaces/IDocumentStorageService.cs
using Microsoft.AspNetCore.Http;

namespace Emtelaak.UserRegistration.Application.Interfaces
{
    public interface IDocumentStorageService
    {
        Task<string> UploadDocumentAsync(IFormFile file, Guid userId, string documentType);
        Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string contentType, Guid userId, string documentType);
        Task<Stream> GetDocumentAsync(string path);
        Task<bool> DeleteDocumentAsync(string path);
        string GetDocumentUrl(string path);
    }
}