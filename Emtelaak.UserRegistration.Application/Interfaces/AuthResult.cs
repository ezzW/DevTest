// Emtelaak.UserRegistration.Application/Interfaces/AuthResult.cs
namespace Emtelaak.UserRegistration.Application.Interfaces
{
    public class AuthResult
    {
        public bool Succeeded { get; set; }
        public string FailureReason { get; set; }
        public int RemainingAttempts { get; set; }
    }
}
