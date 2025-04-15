// Emtelaak.UserRegistration.Application/Commands/ChangePasswordCommand.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class ChangePasswordCommand : IRequest<PasswordChangeResultDto>
    {
        public Guid UserId { get; set; }
        public PasswordChangeDto PasswordData { get; set; }
    }
}