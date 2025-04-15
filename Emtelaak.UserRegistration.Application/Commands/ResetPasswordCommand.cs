// Emtelaak.UserRegistration.Application/Commands/ResetPasswordCommand.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class ResetPasswordCommand : IRequest<ResetPasswordResultDto>
    {
        public ResetPasswordDto ResetData { get; set; }
    }
}