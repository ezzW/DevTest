// Emtelaak.UserRegistration.Application/Queries/GetUserActivityLogQuery.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Queries
{
    public class GetUserActivityLogQuery : IRequest<ActivityLogListDto>
    {
        public Guid UserId { get; set; }
        public int Limit { get; set; } = 20;
    }
}