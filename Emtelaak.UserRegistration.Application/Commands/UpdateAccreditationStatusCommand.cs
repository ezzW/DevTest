// Emtelaak.UserRegistration.Application/Commands/UpdateAccreditationStatusCommand.cs
using System;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class UpdateAccreditationStatusCommand : IRequest<UpdateAccreditationStatusResultDto>
    {
        public Guid AdminUserId { get; set; } // The admin who is updating the status
        public Guid AccreditationId { get; set; }
        public UpdateAccreditationStatusDto StatusData { get; set; }
    }
}