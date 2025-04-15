// Emtelaak.UserRegistration.Infrastructure/Services/KycVerificationService.cs
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Infrastructure.Services
{
    public class KycVerificationService : IKycVerificationService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<KycVerificationService> _logger;
        private readonly HttpClient _httpClient;
        private readonly bool _useSandbox;

        public KycVerificationService(
            IConfiguration configuration,
            IUserRepository userRepository,
            IEmailService emailService,
            IHttpClientFactory httpClientFactory,
            ILogger<KycVerificationService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClientFactory.CreateClient("KycProvider");

            // Check if sandbox mode is enabled (for development)
            _useSandbox = bool.Parse(_configuration["KycVerification:UseSandbox"] ?? "false");

            // Configure HttpClient
            if (!_useSandbox)
            {
                var apiUrl = _configuration["KycVerification:ApiUrl"];
                _httpClient.BaseAddress = new Uri(apiUrl);
            }
        }

        public async Task<KycVerification> SubmitVerificationAsync(Guid userId, KycVerification verification)
        {
            try
            {
                _logger.LogInformation("Submitting KYC verification for user {UserId}", userId);

                // Get user
                var user = await _userRepository.GetUserByIdWithDetailsAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for KYC verification: {UserId}", userId);
                    throw new ArgumentException($"User not found: {userId}");
                }

                // Check if user already has KYC verification in progress or approved
                var existingVerification = await _userRepository.GetKycVerificationByUserIdAsync(userId);
                if (existingVerification != null)
                {
                    if (existingVerification.Status == KycStatus.Approved)
                    {
                        _logger.LogWarning("User {UserId} already has approved KYC verification", userId);
                        throw new InvalidOperationException("User already has approved KYC verification");
                    }

                    if (existingVerification.Status == KycStatus.InProgress || existingVerification.Status == KycStatus.PendingReview)
                    {
                        _logger.LogWarning("User {UserId} already has KYC verification in progress", userId);
                        throw new InvalidOperationException("User already has KYC verification in progress");
                    }
                }

                // Set user ID and initial status
                verification.UserId = userId;
                verification.Status = KycStatus.InProgress;
                verification.SubmittedAt = DateTime.UtcNow;
                verification.LastUpdatedAt = DateTime.UtcNow;

                // Get documents
                var documents = await _userRepository.GetDocumentsByUserIdAsync(userId);
                if (documents == null || documents.Count == 0)
                {
                    _logger.LogWarning("No documents found for user {UserId}", userId);
                    throw new InvalidOperationException("No documents found for KYC verification");
                }

                // Save verification to database
                var createdVerification = await _userRepository.AddKycVerificationAsync(verification);

                // Submit to KYC provider if not in sandbox mode
                if (!_useSandbox)
                {
                    var externalId = await SubmitToKycProviderAsync(user, verification, documents);

                    // Update verification with external ID
                    createdVerification.VerificationId = externalId;
                    await _userRepository.UpdateKycVerificationAsync(createdVerification);
                }
                else
                {
                    // In sandbox mode, generate a random ID and simulate processing
                    createdVerification.VerificationId = Guid.NewGuid().ToString();
                    await _userRepository.UpdateKycVerificationAsync(createdVerification);

                    // Simulate async processing in sandbox mode
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30)); // Simulate processing delay

                        // Randomly approve or reject
                        var random = new Random();
                        if (random.Next(0, 10) < 8) // 80% approval rate in sandbox
                        {
                            await UpdateVerificationStatusAsync(createdVerification.Id, KycStatus.Approved);

                            // Send email notification
                            await _emailService.SendKycApprovedEmailAsync(user.Email, user.FirstName);
                        }
                        else
                        {
                            await UpdateVerificationStatusAsync(createdVerification.Id, KycStatus.Rejected, "Document validation failed. Please provide clearer images.");

                            // Send email notification
                            await _emailService.SendKycRejectedEmailAsync(user.Email, user.FirstName, "Document validation failed. Please provide clearer images.");
                        }
                    });
                }

                return createdVerification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting KYC verification: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<KycVerification> UpdateVerificationStatusAsync(Guid verificationId, KycStatus status, string remarks = null)
        {
            try
            {
                _logger.LogInformation("Updating KYC verification status for verification {VerificationId} to {Status}", verificationId, status);

                // Get verification
                var verification = await GetVerificationByIdAsync(verificationId);
                if (verification == null)
                {
                    _logger.LogWarning("Verification not found: {VerificationId}", verificationId);
                    throw new ArgumentException($"Verification not found: {verificationId}");
                }

                // Update status
                verification.Status = status;
                verification.LastUpdatedAt = DateTime.UtcNow;

                // Update status-specific fields
                if (status == KycStatus.Approved)
                {
                    verification.ApprovedAt = DateTime.UtcNow;
                    verification.ExpiresAt = DateTime.UtcNow.AddYears(1); // Set expiry for 1 year
                }
                else if (status == KycStatus.Rejected)
                {
                    verification.RejectedAt = DateTime.UtcNow;
                    verification.RejectionReason = remarks;
                }
                else if (status == KycStatus.AdditionalInfoRequired)
                {
                    verification.RejectionReason = remarks;
                }

                // Update verification
                await _userRepository.UpdateKycVerificationAsync(verification);

                // Get user for notifications
                var user = await _userRepository.GetUserByIdWithDetailsAsync(verification.UserId);
                if (user != null)
                {
                    // Send notification emails based on status
                    if (status == KycStatus.Approved)
                    {
                        await _emailService.SendKycApprovedEmailAsync(user.Email, user.FirstName);
                    }
                    else if (status == KycStatus.Rejected)
                    {
                        await _emailService.SendKycRejectedEmailAsync(user.Email, user.FirstName, verification.RejectionReason);
                    }
                }

                return verification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating KYC verification status: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> VerifyDocumentAsync(Guid documentId, bool isVerified, string rejectionReason = null)
        {
            try
            {
                _logger.LogInformation("Verifying document {DocumentId}, IsVerified: {IsVerified}", documentId, isVerified);

                // Get document
                var document = await _userRepository.GetDocumentByIdAsync(documentId);
                if (document == null)
                {
                    _logger.LogWarning("Document not found: {DocumentId}", documentId);
                    throw new ArgumentException($"Document not found: {documentId}");
                }

                // Update document status
                document.VerificationStatus = isVerified ? DocumentVerificationStatus.Verified : DocumentVerificationStatus.Rejected;
                document.VerifiedAt = isVerified ? DateTime.UtcNow : null;
                document.RejectionReason = !isVerified ? rejectionReason : null;

                // Update document
                await _userRepository.UpdateDocumentAsync(document);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying document: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<string> GetVerificationProviderUrlAsync(Guid userId)
        {
            try
            {
                // Get KYC verification
                var verification = await _userRepository.GetKycVerificationByUserIdAsync(userId);
                if (verification == null || string.IsNullOrEmpty(verification.VerificationId))
                {
                    _logger.LogWarning("No verification found for user {UserId}", userId);
                    throw new ArgumentException($"No verification found for user {userId}");
                }

                if (_useSandbox)
                {
                    return $"https://sandbox.kycprovider.com/applicant/{verification.VerificationId}";
                }

                // For actual implementation, return the URL from the KYC provider
                var provider = _configuration["KycVerification:Provider"]?.ToLower();
                switch (provider)
                {
                    case "sumsub":
                        var apiUrl = _configuration["KycVerification:ApiUrl"];
                        return $"{apiUrl}/resources/applicants/{verification.VerificationId}/status";
                    // Add other providers as needed
                    default:
                        throw new NotSupportedException($"KYC provider '{provider}' is not supported");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting verification provider URL: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> ProcessVerificationWebhookAsync(string payload)
        {
            try
            {
                _logger.LogInformation("Processing KYC verification webhook");

                if (_useSandbox)
                {
                    _logger.LogInformation("Sandbox mode: Ignoring webhook");
                    return true;
                }

                // Parse the payload
                using var document = JsonDocument.Parse(payload);
                var root = document.RootElement;

                // Extract verification ID and status
                var externalId = root.GetProperty("externalUserId").GetString();
                var reviewStatus = root.GetProperty("reviewStatus").GetString();
                var rejectReason = root.TryGetProperty("rejectReason", out var rejectReasonElement) ?
                    rejectReasonElement.GetString() : null;

                // Find the verification by external ID
                var verification = await FindVerificationByExternalIdAsync(externalId);
                if (verification == null)
                {
                    _logger.LogWarning("Verification not found for external ID: {ExternalId}", externalId);
                    return false;
                }

                // Map external status to our status
                KycStatus status;
                switch (reviewStatus.ToLower())
                {
                    case "approved":
                        status = KycStatus.Approved;
                        break;
                    case "rejected":
                        status = KycStatus.Rejected;
                        break;
                    case "pending":
                        status = KycStatus.PendingReview;
                        break;
                    case "pending_additional_info":
                        status = KycStatus.AdditionalInfoRequired;
                        break;
                    default:
                        _logger.LogWarning("Unknown review status: {ReviewStatus}", reviewStatus);
                        return false;
                }

                // Update verification status
                await UpdateVerificationStatusAsync(verification.Id, status, rejectReason);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing verification webhook: {Message}", ex.Message);
                return false;
            }
        }

        private async Task<KycVerification> GetVerificationByIdAsync(Guid verificationId)
        {
            // This would typically be a direct database query
            // For simplicity, we're using the user repository

            // Note: In a real implementation, you would add a specific method to your repository
            // This is a temporary solution for this implementation

            // Get all users with KYC verification
            var users = await _userRepository.GetAllAsync();
            foreach (var user in users)
            {
                var verification = await _userRepository.GetKycVerificationByUserIdAsync(user.Id);
                if (verification != null && verification.Id == verificationId)
                {
                    return verification;
                }
            }

            return null;
        }

        private async Task<KycVerification> FindVerificationByExternalIdAsync(string externalId)
        {
            // This would typically be a direct database query
            // For simplicity, we're using the user repository

            // Note: In a real implementation, you would add a specific method to your repository
            // This is a temporary solution for this implementation

            // Get all users with KYC verification
            var users = await _userRepository.GetAllAsync();
            foreach (var user in users)
            {
                var verification = await _userRepository.GetKycVerificationByUserIdAsync(user.Id);
                if (verification != null && verification.VerificationId == externalId)
                {
                    return verification;
                }
            }

            return null;
        }

        private async Task<string> SubmitToKycProviderAsync(User user, KycVerification verification, List<Document> documents)
        {
            try
            {
                var provider = _configuration["KycVerification:Provider"]?.ToLower();

                switch (provider)
                {
                    case "sumsub":
                        return await SubmitToSumsubAsync(user, verification, documents);
                    // Add other providers as needed
                    default:
                        throw new NotSupportedException($"KYC provider '{provider}' is not supported");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting to KYC provider: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<string> SubmitToSumsubAsync(User user, KycVerification verification, List<Document> documents)
        {
            try
            {
                var apiKey = _configuration["KycVerification:ApiKey"];
                var apiSecret = _configuration["KycVerification:ApiSecret"];
                var levelName = _configuration["KycVerification:LevelName"];

                // Generate timestamp for signature
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                // Create applicant request payload
                var applicantData = new
                {
                    externalUserId = user.Id.ToString(),
                    email = user.Email,
                    phone = user.PhoneNumber,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    dob = user.DateOfBirth?.ToString("yyyy-MM-dd"),
                    country = user.CountryOfResidence,
                    requiredIdDocs = new
                    {
                        docSets = new[]
                        {
                            new
                            {
                                idDocSetType = "IDENTITY",
                                types = new[] { "PASSPORT", "ID_CARD", "DRIVERS" }
                            },
                            new
                            {
                                idDocSetType = "SELFIE",
                                types = new[] { "SELFIE" }
                            },
                            new
                            {
                                idDocSetType = "PROOF_OF_RESIDENCE",
                                types = new[] { "UTILITY_BILL", "BANK_STATEMENT" }
                            }
                        }
                    }
                };

                // Create request to create applicant
                var request = new HttpRequestMessage(HttpMethod.Post, "/resources/applicants?levelName=" + levelName);
                request.Content = new StringContent(JsonSerializer.Serialize(applicantData), Encoding.UTF8, "application/json");

                // Add signature header
                AddSignatureHeader(request, timestamp, apiKey, apiSecret);

                // Send request
                var response = await _httpClient.SendAsync(request);

                // Check response
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error from Sumsub API: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    throw new HttpRequestException($"Error from Sumsub API: {response.StatusCode} - {errorContent}");
                }

                // Parse response to get applicant ID
                var responseContent = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseContent);
                var applicantId = document.RootElement.GetProperty("id").GetString();

                // Upload documents
                foreach (var doc in documents)
                {
                    await UploadDocumentToSumsubAsync(applicantId, doc, apiKey, apiSecret);
                }

                return applicantId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting to Sumsub: {Message}", ex.Message);
                throw;
            }
        }

        private async Task UploadDocumentToSumsubAsync(string applicantId, Document document, string apiKey, string apiSecret)
        {
            try
            {
                // Get document type for Sumsub
                var docType = MapDocumentTypeForSumsub(document.DocumentType);

                // Generate timestamp for signature
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                // Create request to upload document
                var request = new HttpRequestMessage(HttpMethod.Post, $"/resources/applicants/{applicantId}/info/idDoc");

                // Add document metadata
                var queryString = $"?idDocType={docType}";
                request.RequestUri = new Uri(_httpClient.BaseAddress + request.RequestUri.PathAndQuery + queryString);

                // Add signature header
                AddSignatureHeader(request, timestamp, apiKey, apiSecret);

                // Add document content
                // In a real implementation, you would get the document content from storage
                // For this example, we'll create a mock content
                var docContent = new ByteArrayContent(Encoding.UTF8.GetBytes("Mock document content"));
                docContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                request.Content = docContent;

                // Send request
                var response = await _httpClient.SendAsync(request);

                // Check response
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error uploading document to Sumsub: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    throw new HttpRequestException($"Error uploading document to Sumsub: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document to Sumsub: {Message}", ex.Message);
                throw;
            }
        }

        private string MapDocumentTypeForSumsub(DocumentType documentType)
        {
            switch (documentType)
            {
                case DocumentType.Passport:
                    return "PASSPORT";
                case DocumentType.IdCard:
                    return "ID_CARD";
                case DocumentType.DriversLicense:
                    return "DRIVERS";
                case DocumentType.UtilityBill:
                    return "UTILITY_BILL";
                case DocumentType.BankStatement:
                    return "BANK_STATEMENT";
                // Add other document types as needed
                default:
                    return "OTHER";
            }
        }

        private void AddSignatureHeader(HttpRequestMessage request, long timestamp, string apiKey, string apiSecret)
        {
            // Create signature data
            var method = request.Method.ToString().ToUpper();
            var path = request.RequestUri.PathAndQuery;
            var body = request.Content != null ? request.Content.ReadAsStringAsync().Result : "";

            var signatureData = timestamp + method + path + body;

            // Create HMAC-SHA256 signature
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret));
            var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureData));
            var signature = BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();

            // Add headers
            request.Headers.Add("X-App-Token", apiKey);
            request.Headers.Add("X-App-Access-Ts", timestamp.ToString());
            request.Headers.Add("X-App-Access-Sig", signature);
        }
    }
}