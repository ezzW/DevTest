// Emtelaak.UserRegistration.Infrastructure/Services/EmailService.cs
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Infrastructure.Services
{
    public partial class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly bool _useSandbox;
        private readonly string _emailOutputPath;
        private readonly string _devRecipient;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _useSandbox = _configuration.GetValue<bool>("Email:UseSandbox", false);
            _emailOutputPath = _configuration.GetValue<string>("Email:OutputPath", "emails");

            // Create output directory if it doesn't exist and we're in sandbox mode
            if (_useSandbox && !Directory.Exists(_emailOutputPath))
            {
                Directory.CreateDirectory(_emailOutputPath);
            }
        }

        public async Task SendVerificationEmailAsync(string email, string name, string verificationCode)
        {
            try
            {
                _logger.LogInformation("Sending verification email to: {Email}", email);

                var template = File.ReadAllText("Templates/Email/VerificationEmail.html");
                template = template.Replace("{{Name}}", name ?? "User");
                template = template.Replace("{{VerificationCode}}", verificationCode);

                var content = GetVerificationEmailTemplate(name, verificationCode);

                await SendEmailAsync(
                    email,
                    "Verify Your Email - Emtelaak",
                    template,
                    isHtml: true);

                _logger.LogInformation("Verification email sent to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification email to {Email}: {Message}", email, ex.Message);
                throw;
            }
        }

        public async Task SendPasswordResetEmailAsync(string email, string name, string resetCode)
        {
            try
            {
                _logger.LogInformation("Sending password reset email to: {Email}", email);

                var template = File.ReadAllText("Templates/Email/PasswordResetEmail.html");
                template = template.Replace("{{Name}}", name ?? "User");
                template = template.Replace("{{ResetCode}}", resetCode);

                await SendEmailAsync(
                    email,
                    "Reset Your Password - Emtelaak",
                    template,
                    isHtml: true);

                _logger.LogInformation("Password reset email sent to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}: {Message}", email, ex.Message);
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(string email, string name)
        {
            var subject = "Welcome to Emtelaak";
            var content = GetWelcomeEmailTemplate(name);

            await SendEmailAsync(email, subject, content);
        }

        public async Task SendLoginNotificationAsync(string email, string name, string ipAddress, string location, string device)
        {
            var subject = "New Login Detected - Emtelaak";
            var content = GetLoginNotificationEmailTemplate(name, ipAddress, location, device);

            await SendEmailAsync(email, subject, content);
        }

        public async Task SendKycApprovedEmailAsync(string email, string name)
        {
            var subject = "KYC Verification Approved - Emtelaak";
            var content = GetKycApprovedEmailTemplate(name);

            await SendEmailAsync(email, subject, content);
        }

        public async Task SendKycRejectedEmailAsync(string email, string name, string reason)
        {
            var subject = "KYC Verification Rejected - Emtelaak";
            var content = GetKycRejectedEmailTemplate(name, reason);

            await SendEmailAsync(email, subject, content);
        }

        public async Task SendAccountLockedEmailAsync(string email, string name)
        {
            var subject = "Account Locked - Emtelaak";
            var content = GetAccountLockedEmailTemplate(name);

            await SendEmailAsync(email, subject, content);
        }


        public async Task SendTwoFactorCodeAsync(string email, string name, string code)
        {
            try
            {
                _logger.LogInformation("Sending two-factor authentication code to: {Email}", email);

                string subject = "Two-Factor Authentication Code - Emtelaak";

                // Get email template
                //string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Email", "TwoFactorEmail.html");
                string template = await File.ReadAllTextAsync("Templates/Email/TwoFactorEmail.html");

                // Replace placeholders
                string body = template
                    .Replace("{{Name}}", name)
                    .Replace("{{VerificationCode}}", code);

                if (_useSandbox)
                {
                    // In sandbox mode, save email to file
                    await SaveEmailToFileAsync(email, subject, body);
                    _logger.LogInformation("Two-factor code email saved to file for: {Email}", email);
                    return;
                }

                // Send actual email
                await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Two-factor code email sent to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending two-factor code email to {Email}: {Message}", email, ex.Message);
                throw;
            }
        }

        private async Task SendEmailAsync(string email, string subject, string message, bool isHtml = true)
        {
            try
            {
                _logger.LogInformation("Sending email to {Email} with subject: {Subject}", email, subject);

                if (_useSandbox)
                {
                    // In sandbox mode, save email to file
                    await SaveEmailToFileAsync(email, subject, message);
                    _logger.LogInformation("Email saved to file for: {Email}", email);
                    return;
                }

                // Send actual email
                using var client = new SmtpClient
                {
                    Host = _configuration["Email:SmtpServer"],
                    Port = int.Parse(_configuration["Email:SmtpPort"]),
                    EnableSsl = bool.Parse(_configuration["Email:UseSsl"]),
                    Credentials = new NetworkCredential(
                        _configuration["Email:SmtpUsername"],
                        _configuration["Email:SmtpPassword"])
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(
                        _configuration["Email:SenderEmail"],
                        _configuration["Email:SenderName"]),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = isHtml
                };
                mailMessage.To.Add(new MailAddress(email));

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}: {Message}", email, ex.Message);
                throw;
            }
        }

        private async Task SaveEmailToFileAsync(string recipient, string subject, string htmlContent)
        {
            try
            {
                // Create a filename based on timestamp, recipient and subject
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                var safeSubject = string.Join("_", subject.Split(Path.GetInvalidFileNameChars()));
                var safeRecipient = string.Join("_", recipient.Split(Path.GetInvalidFileNameChars()));
                var filename = Path.Combine(_emailOutputPath, $"{timestamp}_{safeRecipient}_{safeSubject}.html");

                // Add recipient and subject as HTML comments
                var emailContent = $@"<!--
To: {recipient}
Subject: {subject}
Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}
-->
{htmlContent}";

                await File.WriteAllTextAsync(filename, emailContent);
                _logger.LogInformation("Email saved to file: {Filename}", filename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save email to file for {Recipient} with subject: {Subject}", recipient, subject);
                throw;
            }
        }



        private string GetVerificationEmailTemplate(string name, string token)
        {
            var baseUrl = _configuration["Application:BaseUrl"];
            var verificationUrl = $"{baseUrl}/verify-email?token={WebUtility.UrlEncode(token)}";

            return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Verify Your Email</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='text-align: center; margin-bottom: 20px;'>
        <img src='/images/logo.png' alt='Emtelaak Logo' style='max-width: 200px;'>
    </div>
    <div style='background-color: #f9f9f9; border-radius: 5px; padding: 20px; margin-bottom: 20px;'>
        <h1 style='color: #2c3e50; margin-top: 0;'>Verify Your Email Address</h1>
        <p>Hello {{Name}},</p>
        <p>Thank you for registering with Emtelaak. To complete your registration, please use the verification code below:</p>
        <div style='text-align: center; margin: 30px 0;'>
            <div style='font-size: 32px; font-weight: bold; letter-spacing: 5px; padding: 15px; background-color: #f0f0f0; display: inline-block; border-radius: 4px;'>{{VerificationCode}}</div>
        </div>
        <p>Enter this code on the verification page to activate your account.</p>
        <p>This code will expire in 24 hours.</p>
    </div>
    <div style='font-size: 12px; color: #777; text-align: center; margin-top: 20px;'>
        <p>If you did not create an account with Emtelaak, please disregard this email.</p>
        <p>&copy; 2025 Emtelaak. All rights reserved.</p>
    </div>
</body>
</html>";
        }

        private string GetPasswordResetEmailTemplate(string name, string token)
        {
            var baseUrl = _configuration["Application:BaseUrl"];
            var resetUrl = $"{baseUrl}/reset-password?token={WebUtility.UrlEncode(token)}";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Reset Your Password</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='text-align: center; margin-bottom: 20px;'>
        <img src='{baseUrl}/images/logo.png' alt='Emtelaak Logo' style='max-width: 200px;'>
    </div>
    <div style='background-color: #f9f9f9; border-radius: 5px; padding: 20px; margin-bottom: 20px;'>
        <h1 style='color: #2c3e50; margin-top: 0;'>Reset Your Password</h1>
        <p>Hello {name},</p>
        <p>We received a request to reset your password for your Emtelaak account. Click the button below to reset your password:</p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{resetUrl}' style='background-color: #3498db; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block; font-weight: bold;'>Reset Password</a>
        </div>
        <p>If the button above doesn't work, you can also copy and paste the following link into your browser:</p>
        <p style='word-break: break-all;'><a href='{resetUrl}'>{resetUrl}</a></p>
        <p>This link will expire in 1 hour.</p>
        <p>If you did not request a password reset, please contact our support team immediately.</p>
    </div>
    <div style='font-size: 12px; color: #777; text-align: center; margin-top: 20px;'>
        <p>&copy; {DateTime.UtcNow.Year} Emtelaak. All rights reserved.</p>
    </div>
</body>
</html>";
        }

        private string GetWelcomeEmailTemplate(string name)
        {
            var baseUrl = _configuration["Application:BaseUrl"];

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Welcome to Emtelaak</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='text-align: center; margin-bottom: 20px;'>
        <img src='{baseUrl}/images/logo.png' alt='Emtelaak Logo' style='max-width: 200px;'>
    </div>
    <div style='background-color: #f9f9f9; border-radius: 5px; padding: 20px; margin-bottom: 20px;'>
        <h1 style='color: #2c3e50; margin-top: 0;'>Welcome to Emtelaak!</h1>
        <p>Hello {name},</p>
        <p>Thank you for joining Emtelaak, the premier platform for fractional real estate investment.</p>
        <p>Here's what you can do now:</p>
        <ul>
            <li>Complete your KYC verification to start investing</li>
            <li>Browse available property listings</li>
            <li>Learn about fractional ownership benefits</li>
            <li>Set up your investment preferences</li>
        </ul>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{baseUrl}/dashboard' style='background-color: #3498db; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block; font-weight: bold;'>Go to Dashboard</a>
        </div>
        <p>If you have any questions, feel free to contact our support team.</p>
    </div>
    <div style='font-size: 12px; color: #777; text-align: center; margin-top: 20px;'>
        <p>&copy; {DateTime.UtcNow.Year} Emtelaak. All rights reserved.</p>
    </div>
</body>
</html>";
        }

        private string GetLoginNotificationEmailTemplate(string name, string ipAddress, string location, string device)
        {
            var baseUrl = _configuration["Application:BaseUrl"];

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>New Login Detected</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='text-align: center; margin-bottom: 20px;'>
        <img src='{baseUrl}/images/logo.png' alt='Emtelaak Logo' style='max-width: 200px;'>
    </div>
    <div style='background-color: #f9f9f9; border-radius: 5px; padding: 20px; margin-bottom: 20px;'>
        <h1 style='color: #2c3e50; margin-top: 0;'>New Login Detected</h1>
        <p>Hello {name},</p>
        <p>We detected a new login to your Emtelaak account with the following details:</p>
        <ul>
            <li><strong>Date and Time:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}</li>
            <li><strong>IP Address:</strong> {ipAddress}</li>
            <li><strong>Location:</strong> {location}</li>
            <li><strong>Device:</strong> {device}</li>
        </ul>
        <p>If this was you, no action is required.</p>
        <p>If you don't recognize this activity, please reset your password immediately and contact our support team.</p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{baseUrl}/security' style='background-color: #e74c3c; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block; font-weight: bold;'>Review Account Security</a>
        </div>
    </div>
    <div style='font-size: 12px; color: #777; text-align: center; margin-top: 20px;'>
        <p>&copy; {DateTime.UtcNow.Year} Emtelaak. All rights reserved.</p>
    </div>
</body>
</html>";
        }

        private string GetKycApprovedEmailTemplate(string name)
        {
            var baseUrl = _configuration["Application:BaseUrl"];

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>KYC Verification Approved</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='text-align: center; margin-bottom: 20px;'>
        <img src='{baseUrl}/images/logo.png' alt='Emtelaak Logo' style='max-width: 200px;'>
    </div>
    <div style='background-color: #f9f9f9; border-radius: 5px; padding: 20px; margin-bottom: 20px;'>
        <h1 style='color: #2c3e50; margin-top: 0;'>KYC Verification Approved</h1>
        <p>Hello {name},</p>
        <p>Congratulations! Your KYC (Know Your Customer) verification has been successfully approved.</p>
        <p>You now have full access to all investment opportunities on the Emtelaak platform. You can start investing in real estate properties and enjoy the benefits of fractional ownership.</p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{baseUrl}/investments' style='background-color: #27ae60; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block; font-weight: bold;'>Start Investing Now</a>
        </div>
        <p>Thank you for choosing Emtelaak for your real estate investment journey.</p>
    </div>
    <div style='font-size: 12px; color: #777; text-align: center; margin-top: 20px;'>
        <p>&copy; {DateTime.UtcNow.Year} Emtelaak. All rights reserved.</p>
    </div>
</body>
</html>";
        }

        private string GetKycRejectedEmailTemplate(string name, string reason)
        {
            var baseUrl = _configuration["Application:BaseUrl"];

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>KYC Verification Rejected</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='text-align: center; margin-bottom: 20px;'>
        <img src='{baseUrl}/images/logo.png' alt='Emtelaak Logo' style='max-width: 200px;'>
    </div>
    <div style='background-color: #f9f9f9; border-radius: 5px; padding: 20px; margin-bottom: 20px;'>
        <h1 style='color: #2c3e50; margin-top: 0;'>KYC Verification Not Approved</h1>
        <p>Hello {name},</p>
        <p>We regret to inform you that your KYC (Know Your Customer) verification could not be approved.</p>
        <div style='background-color: #f8d7da; border-left: 4px solid #dc3545; padding: 15px; margin: 20px 0;'>
            <p style='margin: 0;'><strong>Reason:</strong> {reason}</p>
        </div>
        <p>You can resubmit your KYC verification with the correct information and documents by visiting your profile dashboard.</p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{baseUrl}/profile/kyc' style='background-color: #3498db; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block; font-weight: bold;'>Update KYC Information</a>
        </div>
        <p>If you have any questions or need assistance, please contact our support team.</p>
    </div>
    <div style='font-size: 12px; color: #777; text-align: center; margin-top: 20px;'>
        <p>&copy; {DateTime.UtcNow.Year} Emtelaak. All rights reserved.</p>
    </div>
</body>
</html>";
        }

        private string GetAccountLockedEmailTemplate(string name)
        {
            var baseUrl = _configuration["Application:BaseUrl"];

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Account Locked</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='text-align: center; margin-bottom: 20px;'>
        <img src='{baseUrl}/images/logo.png' alt='Emtelaak Logo' style='max-width: 200px;'>
    </div>
    <div style='background-color: #f9f9f9; border-radius: 5px; padding: 20px; margin-bottom: 20px;'>
        <h1 style='color: #2c3e50; margin-top: 0;'>Account Locked</h1>
        <p>Hello {name},</p>
        <p>Your Emtelaak account has been temporarily locked due to multiple failed login attempts.</p>
        <p>This is a security measure to protect your account from unauthorized access.</p>
        <p>Your account will be automatically unlocked after 15 minutes, or you can reset your password to unlock it immediately.</p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{baseUrl}/forgot-password' style='background-color: #3498db; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block; font-weight: bold;'>Reset Password</a>
        </div>
        <p>If you did not attempt to log in recently, please contact our support team immediately as your account may have been targeted.</p>
    </div>
    <div style='font-size: 12px; color: #777; text-align: center; margin-top: 20px;'>
        <p>&copy; {DateTime.UtcNow.Year} Emtelaak. All rights reserved.</p>
    </div>
</body>
</html>";
        }
    }
}