// Emtelaak.UserRegistration.Application/Commands/UploadProfilePictureCommand.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class UploadProfilePictureCommand : IRequest<DocumentUploadResultDto>
    {
        public Guid UserId { get; set; }
        public IFormFile File { get; set; }
    }
}