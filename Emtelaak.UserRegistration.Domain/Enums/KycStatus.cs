// Emtelaak.UserRegistration.Domain/Enums/KycStatus.cs
namespace Emtelaak.UserRegistration.Domain.Enums
{
    public enum KycStatus
    {
        NotStarted = 1,
        InProgress = 2,
        PendingReview = 3,
        AdditionalInfoRequired = 4,
        Approved = 5,
        Rejected = 6,
        Expired = 7
    }
}
