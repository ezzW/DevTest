// Emtelaak.UserRegistration.Application/Queries/GetKycDocumentQuery.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Queries
{
    public class GetKycDocumentQuery : IRequest<DocumentDto>
    {
        public Guid UserId { get; set; }
        public Guid DocumentId { get; set; }
        public bool ForAccreditation { get; set; }
    }
}