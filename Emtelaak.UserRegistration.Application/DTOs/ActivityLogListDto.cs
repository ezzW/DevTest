// Emtelaak.UserRegistration.Application/DTOs/ActivityLogListDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class ActivityLogListDto
    {
        public List<ActivityLogEntryDto> Activities { get; set; } = new List<ActivityLogEntryDto>();
        public int TotalCount { get; set; }
    }
}
