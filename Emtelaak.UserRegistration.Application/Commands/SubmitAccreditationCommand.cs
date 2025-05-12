// Emtelaak.UserRegistration.Application/Commands/SubmitAccreditationCommand.cs
using System;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class SubmitAccreditationCommand : IRequest<SubmitAccreditationResultDto>
    {
        public Guid UserId { get; set; }
        public SubmitAccreditationDto AccreditationData { get; set; }
    }
}