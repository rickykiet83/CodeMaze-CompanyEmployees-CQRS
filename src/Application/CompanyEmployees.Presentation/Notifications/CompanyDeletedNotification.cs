namespace CompanyEmployees.Presentation.Notifications;

public sealed record CompanyDeletedNotification(Guid Id, bool TrackChanges) : INotification
{
}