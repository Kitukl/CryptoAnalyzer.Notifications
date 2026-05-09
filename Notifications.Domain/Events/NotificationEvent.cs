using Notifications.Domain.Enums;

namespace CryptoAnalyzer.Core.Events;

public class NotificationEvent
{
    public string Email { get; set; }
    public string Value { get; set; }
    public NotificationType NotificationType { get; set; }
}