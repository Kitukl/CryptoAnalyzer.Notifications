using CryptoAnalyzer.Core.Events;
using MassTransit;

namespace Notifications.Core;

public class NotificationConsumer : IConsumer<NotificationEvent>
{
    private readonly IEmailSender _emailSender;

    public NotificationConsumer(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }
    public async Task Consume(ConsumeContext<NotificationEvent> context)
    {
        var message = context.Message;
        await _emailSender.SendEmailAsync(message.Email, message.NotificationType, message.Value);
    }
}