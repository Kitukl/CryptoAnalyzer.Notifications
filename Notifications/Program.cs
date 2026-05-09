using MassTransit;
using Notifications.Core;
using Notifications.Extensions;
using Notifications.infrastructure;
using SendGrid;

var builder = Host.CreateApplicationBuilder(args);
//builder.Services.AddHostedService<>();


builder.Services.Configure<FrontEndOptions>(builder.Configuration.GetSection("FrontEnd"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services.AddScoped<ISendGridClient>(sg => new SendGridClient(builder.Configuration.GetValue<string>("EmailSettings:ApiKey")));

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<NotificationConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("EventBus"));

        cfg.ReceiveEndpoint("notification-service-queue", e =>
        {
            e.ConfigureConsumer<NotificationConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();