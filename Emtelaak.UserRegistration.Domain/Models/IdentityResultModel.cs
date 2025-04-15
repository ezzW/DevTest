namespace Emtelaak.UserRegistration.Domain.Models
{
    public class IdentityResultModel
    {
        public bool Succeeded { get; set; }
        public IEnumerable<IdentityErrorModel> Errors { get; set; } = new List<IdentityErrorModel>();
    }
}