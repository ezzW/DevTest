// Emtelaak.UserRegistration.Application/Interfaces/IDocumentRequirementsService.cs
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emtelaak.UserRegistration.Application.Interfaces
{
    public interface IDocumentRequirementsService
    {
        /// <summary>
        /// Gets document requirements for a specific investor classification
        /// </summary>
        /// <param name="investorClassification">The investor classification to get requirements for</param>
        /// <returns>List of required documents</returns>
        Task<List<RequiredDocumentDto>> GetAccreditationDocumentRequirementsAsync(string investorClassification);
        
        /// <summary>
        /// Gets document requirements for all investor classifications
        /// </summary>
        /// <returns>List of required documents for all classifications</returns>
        Task<List<RequiredDocumentDto>> GetAllAccreditationDocumentRequirementsAsync();
        
        /// <summary>
        /// Gets document requirements for a specific investor classification
        /// </summary>
        /// <param name="classification">The investor classification enum value</param>
        /// <returns>List of required documents</returns>
        List<RequiredDocumentDto> GetRequirementsForClassification(InvestorClassification classification);
    }
}