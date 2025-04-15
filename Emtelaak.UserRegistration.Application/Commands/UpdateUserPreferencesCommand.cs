// Emtelaak.UserRegistration.Application/Commands/UpdateUserPreferencesCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class UpdateUserPreferencesCommand : IRequest<UserPreferenceUpdateResultDto>
    {
        public Guid UserId { get; set; }
        public UserPreferenceDto Preferences { get; set; }
    }
}