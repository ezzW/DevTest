﻿// Emtelaak.UserRegistration.Application/DTOs/ResetPasswordDto.cs
// Emtelaak.UserRegistration.Application/DTOs/ResetPasswordDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class ResetPasswordDto
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
