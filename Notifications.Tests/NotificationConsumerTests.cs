using System.Threading;
using System.Threading.Tasks;
using CryptoAnalyzer.Core.Events;
using MassTransit;
using Microsoft.AspNetCore.Identity.UI.Services;
using Moq;
using Notifications.Core;
using Xunit;

namespace Notifications.Tests;

public class NotificationConsumerTests
{
    [Fact]
    public async Task Consume_SendsEmailForNotificationEvent()
    {
        var emailSender = new Mock<IEmailSender>();
        var consumer = new NotificationConsumer(emailSender.Object);

        var message = new NotificationEvent { Email = "user@test.com", NotificationType = "test", Value = "value" };
        var context = new Mock<ConsumeContext<NotificationEvent>>();
        context.SetupGet(x => x.Message).Returns(message);

        await consumer.Consume(context.Object);

        emailSender.Verify(x => x.SendEmailAsync("user@test.com", "test", "value"), Times.Once);
    }
}
