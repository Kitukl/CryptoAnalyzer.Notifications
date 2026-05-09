using Notifications.Domain.Enums;

namespace Notifications.Core;

public interface IEmailSender
{
    Task SendEmailAsync(string email, NotificationType notificationType, string value);
}