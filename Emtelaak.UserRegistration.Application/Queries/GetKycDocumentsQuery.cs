// Emtelaak.UserRegistration.Application/Queries/GetKycDocumentsQuery.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Queries
{
    public class GetKycDocumentsQuery : IRequest<List<DocumentDto>>
    {
        public Guid UserId { get; set; }
    }
}