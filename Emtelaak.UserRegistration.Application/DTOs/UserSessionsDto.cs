// Emtelaak.UserRegistration.Application/DTOs/UserSessionsDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class UserSessionsDto
    {
        public List<UserSessionDto> Sessions { get; set; } = new List<UserSessionDto>();
        public Guid? CurrentSessionId { get; set; }
    }
}
