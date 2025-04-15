// Emtelaak.UserRegistration.Application/Commands/UploadKycDocumentCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class UploadKycDocumentCommand : IRequest<DocumentUploadResultDto>
    {
        public Guid UserId { get; set; }
        public IFormFile File { get; set; }
        public string DocumentType { get; set; }
    }
}