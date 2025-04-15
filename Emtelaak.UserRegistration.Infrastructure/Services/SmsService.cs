// Emtelaak.UserRegistration.Infrastructure/Services/SmsService.cs
using System;
using System.IO;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Infrastructure.Services
{
    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;
        private readonly bool _useSandbox;
        private readonly string _outputPath;

        public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Check if sandbox mode is enabled (for development)
            _useSandbox = bool.Parse(_configuration["Sms:UseSandbox"] ?? "false");
            _outputPath = _configuration["Sms:OutputPath"] ?? "sms";

            // Create output directory if it doesn't exist
            if (_useSandbox && !Directory.Exists(_outputPath))
            {
                Directory.CreateDirectory(_outputPath);
            }
        }

        public async Task<bool> SendVerificationSmsAsync(string phoneNumber, string code)
        {
            var message = $"Your Emtelaak verification code is: {code}. This code will expire in 10 minutes.";
            return await SendSmsAsync(phoneNumber, message);
        }

        public async Task<bool> SendTwoFactorCodeAsync(string phoneNumber, string code)
        {
            var message = $"Your Emtelaak authentication code is: {code}. This code will expire in 10 minutes.";
            return await SendSmsAsync(phoneNumber, message);
        }

        public async Task<bool> SendLoginNotificationSmsAsync(string phoneNumber, string location, string time)
        {
            var message = $"Emtelaak: New login detected from {location} at {time}. If this wasn't you, please reset your password immediately.";
            return await SendSmsAsync(phoneNumber, message);
        }

        private async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                if (_useSandbox)
                {
                    await SaveSmsToFileAsync(phoneNumber, message);
                    return true;
                }

                var provider = _configuration["Sms:Provider"]?.ToLower();

                switch (provider)
                {
                    case "twilio":
                        return await SendTwilioSmsAsync(phoneNumber, message);
                    // Add other SMS providers as needed
                    default:
                        _logger.LogError("Unsupported SMS provider: {Provider}", provider);
                        throw new NotSupportedException($"Unsupported SMS provider: {provider}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}: {Message}", phoneNumber, ex.Message);
                return false;
            }
        }

        private async Task<bool> SendTwilioSmsAsync(string phoneNumber, string message)
        {
            try
            {
                // This would use Twilio SDK in production
                var accountSid = _configuration["Sms:AccountSid"];
                var authToken = _configuration["Sms:AuthToken"];
                var fromNumber = _configuration["Sms:FromNumber"];

                _logger.LogInformation("Sending SMS via Twilio to {PhoneNumber}", phoneNumber);

                // Mock implementation for now - would use Twilio SDK in production
                // Example:
                // var client = new TwilioRestClient(accountSid, authToken);
                // var smsMessage = await MessageResource.CreateAsync(
                //     to: new Twilio.Types.PhoneNumber(phoneNumber),
                //     from: new Twilio.Types.PhoneNumber(fromNumber),
                //     body: message
                // );

                // Log success
                _logger.LogInformation("SMS sent successfully to {PhoneNumber}", phoneNumber);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Twilio SMS sending failed: {Message}", ex.Message);
                throw;
            }
        }

        private async Task SaveSmsToFileAsync(string phoneNumber, string message)
        {
            try
            {
                // Create a filename based on timestamp and phone number
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                var safePhoneNumber = string.Join("_", phoneNumber.Split(Path.GetInvalidFileNameChars()));
                var filename = Path.Combine(_outputPath, $"{timestamp}_{safePhoneNumber}.txt");

                // Format the SMS content
                var smsContent = $@"To: {phoneNumber}
Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}
Message:
{message}";

                await File.WriteAllTextAsync(filename, smsContent);
                _logger.LogInformation("SMS saved to file: {Filename}", filename);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save SMS to file for {PhoneNumber}", phoneNumber);
                throw;
            }
        }
    }
}