// Emtelaak.UserRegistration.Infrastructure/Services/EmailService.AccreditationTemplates.cs
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Infrastructure.Services
{
    // Partial class implementation for Accreditation-related email methods
    public partial class EmailService
    {
        public async Task SendAccreditationApprovedEmailAsync(string email, string name, string investorClassification = null, decimal? investmentLimit = null, DateTime? expiryDate = null)
        {
            try
            {
                _logger.LogInformation("Sending accreditation approved email to: {Email}", email);

                var subject = "Investor Accreditation Approved - Emtelaak";
                var content = GetAccreditationApprovedEmailTemplate(name, investorClassification, investmentLimit, expiryDate);

                await SendEmailAsync(email, subject, content, isHtml: true);
                _logger.LogInformation("Accreditation approved email sent to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending accreditation approved email to {Email}: {Message}", email, ex.Message);
                throw;
            }
        }

        public async Task SendAccreditationRejectedEmailAsync(string email, string name, string reason)
        {
            try
            {
                _logger.LogInformation("Sending accreditation rejected email to: {Email}", email);

                var subject = "Investor Accreditation Not Approved - Emtelaak";
                var content = GetAccreditationRejectedEmailTemplate(name, reason);

                await SendEmailAsync(email, subject, content, isHtml: true);
                _logger.LogInformation("Accreditation rejected email sent to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending accreditation rejected email to {Email}: {Message}", email, ex.Message);
                throw;
            }
        }

        public async Task SendAccreditationSubmittedEmailAsync(string email, string name)
        {
            try
            {
                _logger.LogInformation("Sending accreditation submission confirmation email to: {Email}", email);

                var subject = "Investor Accreditation Submission Received - Emtelaak";
                var content = GetAccreditationSubmittedEmailTemplate(name);

                await SendEmailAsync(email, subject, content, isHtml: true);
                _logger.LogInformation("Accreditation submission confirmation email sent to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending accreditation submission confirmation email to {Email}: {Message}", email, ex.Message);
                throw;
            }
        }

        public async Task SendAccreditationExpiredEmailAsync(string email, string name, string investorClassification = null)
        {
            try
            {
                _logger.LogInformation("Sending accreditation expired notification email to: {Email}", email);

                var subject = "Investor Accreditation Expired - Emtelaak";
                var content = GetAccreditationExpiredEmailTemplate(name, investorClassification);

                await SendEmailAsync(email, subject, content, isHtml: true);
                _logger.LogInformation("Accreditation expired notification email sent to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending accreditation expired notification email to {Email}: {Message}", email, ex.Message);
                throw;
            }
        }

        private string GetAccreditationApprovedEmailTemplate(string name, string investorClassification = null, decimal? investmentLimit = null, DateTime? expiryDate = null)
        {
            var baseUrl = _configuration["Application:BaseUrl"];
            var classification = string.IsNullOrEmpty(investorClassification) ? "Investor" : investorClassification;
            var limitInfo = investmentLimit.HasValue && investmentLimit.Value < decimal.MaxValue 
                ? $"<p><strong>Investment Limit:</strong> {investmentLimit.Value:C0}</p>" 
                : "<p><strong>Investment Limit:</strong> No limit</p>";
            var expiryInfo = expiryDate.HasValue 
                ? $"<p><strong>Expiration Date:</strong> {expiryDate.Value:MMMM dd, yyyy}</p>" 
                : "<p><strong>Expiration Date:</strong> One year from approval</p>";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Investor Accreditation Approved</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='text-align: center; margin-bottom: 20px;'>
        <img src='{baseUrl}/images/logo.png' alt='Emtelaak Logo' style='max-width: 200px;'>
    </div>
    <div style='background-color: #f9f9f9; border-radius: 5px; padding: 20px; margin-bottom: 20px;'>
        <h1 style='color: #2c3e50; margin-top: 0;'>Investor Accreditation Approved</h1>
        <p>Hello {name},</p>
        <p>Congratulations! Your investor accreditation application has been approved.</p>
        
        <div style='background-color: #e8f4fd; border-radius: 5px; padding: 15px; margin: 20px 0;'>
            <h3 style='color: #2c3e50; margin-top: 0;'>Your Accreditation Details</h3>
            <p><strong>Investor Classification:</strong> {classification}</p>
            {limitInfo}
            {expiryInfo}
        </div>
        
        <p>You now have access to exclusive investment opportunities on the Emtelaak platform that are available to accredited investors.</p>
        <p>Your accreditation status will be visible on your profile and will remain valid until the expiration date shown above.</p>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{baseUrl}/investments' style='background-color: #27ae60; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block; font-weight: bold;'>Explore Investment Opportunities</a>
        </div>
        
        <p>If you have any questions about your accreditation status or available investment opportunities, please contact our investor relations team.</p>
    </div>
    <div style='font-size: 12px; color: #777; text-align: center; margin-top: 20px;'>
        <p>&copy; {DateTime.UtcNow.Year} Emtelaak. All rights reserved.</p>
    </div>
</body>
</html>";
        }

        private string GetAccreditationRejectedEmailTemplate(string name, string reason)
        {
            var baseUrl = _configuration["Application:BaseUrl"];

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Investor Accreditation Not Approved</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='text-align: center; margin-bottom: 20px;'>
        <img src='{baseUrl}/images/logo.png' alt='Emtelaak Logo' style='max-width: 200px;'>
    </div>
    <div style='background-color: #f9f9f9; border-radius: 5px; padding: 20px; margin-bottom: 20px;'>
        <h1 style='color: #2c3e50; margin-top: 0;'>Investor Accreditation Not Approved</h1>
        <p>Hello {name},</p>
        <p>We regret to inform you that your investor accreditation application could not be approved at this time.</p>
        <div style='background-color: #f8d7da; border-left: 4px solid #dc3545; padding: 15px; margin: 20px 0;'>
            <p style='margin: 0;'><strong>Reason:</strong> {reason}</p>
        </div>
        <p>You can resubmit your application with updated information or additional documentation to address the issues mentioned above.</p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{baseUrl}/profile/accreditation' style='background-color: #3498db; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block; font-weight: bold;'>Update Application</a>
        </div>
        <p>If you need assistance or have questions about the accreditation requirements, please contact our investor relations team.</p>
    </div>
    <div style='font-size: 12px; color: #777; text-align: center; margin-top: 20px;'>
        <p>&copy; {DateTime.UtcNow.Year} Emtelaak. All rights reserved.</p>
    </div>
</body>
</html>";
        }

        private string GetAccreditationSubmittedEmailTemplate(string name)
        {
            var baseUrl = _configuration["Application:BaseUrl"];

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Investor Accreditation Submission Received</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='text-align: center; margin-bottom: 20px;'>
        <img src='{baseUrl}/images/logo.png' alt='Emtelaak Logo' style='max-width: 200px;'>
    </div>
    <div style='background-color: #f9f9f9; border-radius: 5px; padding: 20px; margin-bottom: 20px;'>
        <h1 style='color: #2c3e50; margin-top: 0;'>Accreditation Application Received</h1>
        <p>Hello {name},</p>
        <p>Thank you for submitting your investor accreditation application with Emtelaak.</p>
        <p>Your application has been received and will be reviewed by our team. The review process typically takes 2-3 business days.</p>
        <p>You will be notified by email once a decision has been made regarding your accreditation status.</p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{baseUrl}/profile/accreditation' style='background-color: #3498db; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block; font-weight: bold;'>View Application Status</a>
        </div>
        <p>If you need to make any changes to your application or have questions about the process, please contact our investor relations team.</p>
    </div>
    <div style='font-size: 12px; color: #777; text-align: center; margin-top: 20px;'>
        <p>&copy; {DateTime.UtcNow.Year} Emtelaak. All rights reserved.</p>
    </div>
</body>
</html>";
        }

        private string GetAccreditationExpiredEmailTemplate(string name, string investorClassification = null)
        {
            var baseUrl = _configuration["Application:BaseUrl"];
            var classification = string.IsNullOrEmpty(investorClassification) ? "Investor" : investorClassification;

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Investor Accreditation Expired</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='text-align: center; margin-bottom: 20px;'>
        <img src='{baseUrl}/images/logo.png' alt='Emtelaak Logo' style='max-width: 200px;'>
    </div>
    <div style='background-color: #f9f9f9; border-radius: 5px; padding: 20px; margin-bottom: 20px;'>
        <h1 style='color: #2c3e50; margin-top: 0;'>Investor Accreditation Expired</h1>
        <p>Hello {name},</p>
        <p>This is to inform you that your <strong>{classification}</strong> accreditation status with Emtelaak has expired.</p>
        
        <div style='background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0;'>
            <h3 style='color: #2c3e50; margin-top: 0;'>What This Means For You</h3>
            <ul style='margin-top: 10px; padding-left: 20px;'>
                <li>You no longer have access to investment opportunities restricted to {classification} investors</li>
                <li>Your investment limit has been revised to the standard non-accredited investor level</li>
                <li>Any pending investments requiring {classification} status may be affected</li>
            </ul>
        </div>
        
        <p>To regain your accredited investor status and continue accessing exclusive investment opportunities, please renew your accreditation by submitting a new application with current documentation.</p>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{baseUrl}/profile/accreditation/renew' style='background-color: #3498db; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block; font-weight: bold;'>Renew Accreditation</a>
        </div>
        
        <p>If you have any questions about the renewal process or need assistance, please contact our investor relations team at <a href='mailto:investor.relations@emtelaak.com'>investor.relations@emtelaak.com</a>.</p>
    </div>
    <div style='font-size: 12px; color: #777; text-align: center; margin-top: 20px;'>
        <p>&copy; {DateTime.UtcNow.Year} Emtelaak. All rights reserved.</p>
    </div>
</body>
</html>";
        }
    }
}