// Emtelaak.UserRegistration.Application/Commands/DeleteKycDocumentCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class DeleteKycDocumentCommand : IRequest
    {
        public Guid UserId { get; set; }
        public Guid DocumentId { get; set; }
    }
}