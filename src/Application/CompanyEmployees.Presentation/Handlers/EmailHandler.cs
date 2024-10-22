using CompanyEmployees.Presentation.Notifications;
using Contracts;

namespace CompanyEmployees.Presentation.Handlers;

internal sealed class EmailHandler(ILoggerManager logger) : INotificationHandler<CompanyDeletedNotification>
{
    public async Task Handle(CompanyDeletedNotification notification, CancellationToken cancellationToken)
    {
        logger.LogWarn($"Delete action for the company with id: {notification.Id} has occurred.");

        await Task.CompletedTask;
    }
}