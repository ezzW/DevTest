// Emtelaak.UserRegistration.Application/Commands/UpdateUserProfileCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Domain.Models;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class UpdateUserProfileCommand : IRequest<UserProfileUpdateResultDto>
    {
        public Guid UserId { get; set; }
        public UserProfileUpdateDto ProfileData { get; set; }
    }
}