// Emtelaak.UserRegistration.Infrastructure/Services/AzureBlobStorageService.cs
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Emtelaak.UserRegistration.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Infrastructure.Services
{
    public class AzureBlobStorageService : IDocumentStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureBlobStorageService> _logger;
        private readonly string _connectionString;
        private readonly string _documentContainerName;
        private readonly string _profilePictureContainerName;
        private readonly string _storageProvider;
        private readonly string _basePath;

        public AzureBlobStorageService(
            IConfiguration configuration,
            ILogger<AzureBlobStorageService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _storageProvider = _configuration["Storage:Provider"] ?? "AzureBlob";
            _connectionString = _configuration["Storage:ConnectionString"];
            _documentContainerName = _configuration["Storage:DocumentContainerName"] ?? "documents";
            _profilePictureContainerName = _configuration["Storage:ProfilePictureContainerName"] ?? "profilepictures";
            _basePath = _configuration["Storage:BasePath"] ?? "App_Data/uploads";
        }

        public async Task<string> UploadDocumentAsync(IFormFile file, Guid userId, string documentType)
        {
            try
            {
                using var stream = file.OpenReadStream();
                return await UploadDocumentAsync(stream, file.FileName, file.ContentType, userId, documentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload document for user {UserId}: {Message}", userId, ex.Message);
                throw;
            }
        }

        public async Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string contentType, Guid userId, string documentType)
        {
            try
            {
                // Generate a unique file name to prevent collisions
                var fileExtension = Path.GetExtension(fileName);
                var uniqueFileName = $"{userId}/{documentType}/{Guid.NewGuid()}{fileExtension}";

                // Determine container based on document type
                var containerName = documentType.ToLower() == "profilepicture"
                    ? _profilePictureContainerName
                    : _documentContainerName;

                if (_storageProvider.Equals("AzureBlob", StringComparison.OrdinalIgnoreCase))
                {
                    return await UploadToAzureBlobAsync(fileStream, uniqueFileName, contentType, containerName);
                }
                else if (_storageProvider.Equals("LocalFileSystem", StringComparison.OrdinalIgnoreCase))
                {
                    return await UploadToLocalFileSystemAsync(fileStream, uniqueFileName, containerName);
                }
                else
                {
                    throw new NotSupportedException($"Storage provider '{_storageProvider}' is not supported");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload document for user {UserId}: {Message}", userId, ex.Message);
                throw;
            }
        }

        public async Task<Stream> GetDocumentAsync(string path)
        {
            try
            {
                if (_storageProvider.Equals("AzureBlob", StringComparison.OrdinalIgnoreCase))
                {
                    return await GetFromAzureBlobAsync(path);
                }
                else if (_storageProvider.Equals("LocalFileSystem", StringComparison.OrdinalIgnoreCase))
                {
                    return await GetFromLocalFileSystemAsync(path);
                }
                else
                {
                    throw new NotSupportedException($"Storage provider '{_storageProvider}' is not supported");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get document {Path}: {Message}", path, ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteDocumentAsync(string path)
        {
            try
            {
                if (_storageProvider.Equals("AzureBlob", StringComparison.OrdinalIgnoreCase))
                {
                    return await DeleteFromAzureBlobAsync(path);
                }
                else if (_storageProvider.Equals("LocalFileSystem", StringComparison.OrdinalIgnoreCase))
                {
                    return await DeleteFromLocalFileSystemAsync(path);
                }
                else
                {
                    throw new NotSupportedException($"Storage provider '{_storageProvider}' is not supported");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete document {Path}: {Message}", path, ex.Message);
                return false;
            }
        }

        public string GetDocumentUrl(string path)
        {
            try
            {
                if (_storageProvider.Equals("AzureBlob", StringComparison.OrdinalIgnoreCase))
                {
                    // Parse container and blob name from path
                    var pathParts = path.Split('/', 2);
                    if (pathParts.Length != 2)
                    {
                        throw new ArgumentException($"Invalid path format: {path}");
                    }

                    var containerName = pathParts[0];
                    var blobName = pathParts[1];

                    // Get container client
                    var blobServiceClient = new BlobServiceClient(_connectionString);
                    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                    var blobClient = containerClient.GetBlobClient(blobName);

                    return blobClient.Uri.ToString();
                }
                else if (_storageProvider.Equals("LocalFileSystem", StringComparison.OrdinalIgnoreCase))
                {
                    var applicationBaseUrl = _configuration["Application:BaseUrl"];
                    return $"{applicationBaseUrl}/uploads/{path}";
                }
                else
                {
                    throw new NotSupportedException($"Storage provider '{_storageProvider}' is not supported");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get document URL {Path}: {Message}", path, ex.Message);
                throw;
            }
        }

        #region Azure Blob Storage Implementation

        private async Task<string> UploadToAzureBlobAsync(Stream fileStream, string blobName, string contentType, string containerName)
        {
            try
            {
                // Create the blob client and ensure the container exists
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

                var blobClient = containerClient.GetBlobClient(blobName);

                // Upload the file with metadata
                var blobUploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = contentType
                    }
                };

                await blobClient.UploadAsync(fileStream, blobUploadOptions);

                _logger.LogInformation("Document uploaded to Azure Blob Storage: {ContainerName}/{BlobName}", containerName, blobName);

                // Return the storage path for retrieval
                return $"{containerName}/{blobName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure Blob Storage upload failed: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<Stream> GetFromAzureBlobAsync(string path)
        {
            try
            {
                // Parse container and blob name from path
                var pathParts = path.Split('/', 2);
                if (pathParts.Length != 2)
                {
                    throw new ArgumentException($"Invalid path format: {path}");
                }

                var containerName = pathParts[0];
                var blobName = pathParts[1];

                // Get container client
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                // Check if blob exists
                if (!await blobClient.ExistsAsync())
                {
                    throw new FileNotFoundException($"Document not found: {path}");
                }

                // Download to memory stream
                var memoryStream = new MemoryStream();
                await blobClient.DownloadToAsync(memoryStream);
                memoryStream.Position = 0;

                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure Blob Storage download failed: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<bool> DeleteFromAzureBlobAsync(string path)
        {
            try
            {
                // Parse container and blob name from path
                var pathParts = path.Split('/', 2);
                if (pathParts.Length != 2)
                {
                    throw new ArgumentException($"Invalid path format: {path}");
                }

                var containerName = pathParts[0];
                var blobName = pathParts[1];

                // Get container client
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                // Delete the blob
                var response = await blobClient.DeleteIfExistsAsync();

                _logger.LogInformation("Document deleted from Azure Blob Storage: {Path}", path);

                return response.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure Blob Storage delete failed: {Message}", ex.Message);
                throw;
            }
        }

        #endregion

        #region Local File System Implementation

        private async Task<string> UploadToLocalFileSystemAsync(Stream fileStream, string fileName, string containerName)
        {
            try
            {
                // Create directory if it doesn't exist
                var containerPath = Path.Combine(_basePath, containerName);
                Directory.CreateDirectory(containerPath);

                // Create the file path
                var filePath = Path.Combine(containerPath, fileName);
                var directoryPath = Path.GetDirectoryName(filePath);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Save the file
                using (var fileStream2 = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(fileStream2);
                }

                _logger.LogInformation("Document uploaded to local file system: {FilePath}", filePath);

                // Return the storage path for retrieval
                return $"{containerName}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Local file system upload failed: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<Stream> GetFromLocalFileSystemAsync(string path)
        {
            try
            {
                var filePath = Path.Combine(_basePath, path);

                // Check if file exists
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Document not found: {path}");
                }

                // Open file as stream
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return fileStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Local file system download failed: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<bool> DeleteFromLocalFileSystemAsync(string path)
        {
            try
            {
                var filePath = Path.Combine(_basePath, path);

                // Check if file exists
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Document not found for deletion: {Path}", path);
                    return false;
                }

                // Delete the file
                File.Delete(filePath);

                _logger.LogInformation("Document deleted from local file system: {Path}", path);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Local file system delete failed: {Message}", ex.Message);
                throw;
            }
        }

        #endregion
    }
}