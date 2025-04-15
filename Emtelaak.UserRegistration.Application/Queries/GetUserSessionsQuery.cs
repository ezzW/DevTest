// Emtelaak.UserRegistration.Application/Queries/GetUserSessionsQuery.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Queries
{
    public class GetUserSessionsQuery : IRequest<UserSessionsDto>
    {
        public Guid UserId { get; set; }
    }
}