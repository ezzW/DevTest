// Emtelaak.UserRegistration.Application/Queries/GetUserProfileQuery.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Queries
{
    public class GetUserProfileQuery : IRequest<UserProfileDto>
    {
        public Guid UserId { get; set; }
    }
}