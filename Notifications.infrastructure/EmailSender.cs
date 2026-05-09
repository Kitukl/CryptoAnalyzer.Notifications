using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Notifications.Core;
using Notifications.Domain.Enums;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text.Json;
using Notifications.Core.Messages;

namespace Notifications.infrastructure;

public class EmailSender : IEmailSender
{
    private readonly ISendGridClient _client;
    private readonly EmailSettings _emailOptions;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(
        IOptions<EmailSettings> emailOptions, 
        ISendGridClient client, 
        ILogger<EmailSender> logger)
    {
        _client = client;
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, NotificationType notificationType, string value)
    {
        var sender = new EmailAddress(_emailOptions.SenderEmail, _emailOptions.SenderName);
        var receiver = new EmailAddress(email);

        var (subject, htmlContent) = GetEmailContent(notificationType, value);

        var message = MailHelper.CreateSingleEmail(sender, receiver, subject, null, htmlContent);

        var response = await _client.SendEmailAsync(message);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Body.ReadAsStringAsync();
            _logger.LogError("SendGrid API failed with status {StatusCode}. Details: {Details}", response.StatusCode, errorBody);
            throw new Exception($"SendGrid API failed with status {response.StatusCode}"); 
        }
    }

    private (string Subject, string Html) GetEmailContent(NotificationType type, string value)
    {
        return type switch
        {
            NotificationType.PasswordReset => 
                ("Відновлення Паролю - CryptoAuth", GenerateHtmlTemplate(value)),

            NotificationType.EmailConfirmation => 
                ("Підтвердження пошти - CryptoAuth", GenerateEmailConfirmation(value)),

            NotificationType.TrigerNews => 
                ("Крипто-сповіщення: Важлива новина", GenerateNewsTemplate(value)),

            _ => ("Нове сповіщення - CryptoAuth", $"<p>{value}</p>")
        };
    }

    private static string GenerateNewsTemplate(string jsonValue)
    {
        var news = JsonSerializer.Deserialize<NewsMessage>(jsonValue);

        return $@"
        <div style=""font-family: Arial, sans-serif; background-color: #f0fdf4; padding: 20px; border-radius: 8px; max-width: 600px; margin: 20px auto; border: 1px solid #bbf7d0;"">
            <h1 style=""color: #166534; text-align: center;"">Крипто Новини</h1>
            <div style=""background-color: #ffffff; padding: 20px; border-radius: 8px; border-left: 5px solid #22c55e;"">
                <p style=""font-size: 16px; color: #1e293b;"">{news?.Text}</p>
                <p style=""font-size: 14px; color: #64748b;"">Сентимент: <strong>{news?.Sentiment}</strong></p>
            </div>
            <p style=""text-align: center; color: #888; font-size: 12px; margin-top: 20px;"">CryptoAnalyzer Engine</p>
        </div>";
    }

    private static string GenerateHtmlTemplate(string otpCode)
    {
        var code = JsonSerializer.Deserialize<ForgotPasswordMessage>(otpCode);
        
        return $@"
        <div style=""font-family: Arial, sans-serif; background-color: #F4F0FF; padding: 20px; border-radius: 8px; max-width: 600px; margin: 20px auto; border: 1px solid #d0c0ff;"">
            <h1 style=""color: #3C2E59; text-align: center; margin-bottom: 30px;"">Відновлення Паролю</h1>
            <p style=""color: #4A4A4A; font-size: 16px; line-height: 1.6;"">Ви отримали цей лист, оскільки було надіслано запит на відновлення паролю.</p>
            <div style=""background-color: #ffffff; border: 1px dashed #d0c0ff; padding: 30px; text-align: center; margin: 30px 0; border-radius: 5px;"">
                <p style=""color: #3C2E59; font-size: 18px; margin-bottom: 15px;"">Ваш одноразовий код:</p>
                <p style=""color: #6A0DAD; font-size: 52px; font-weight: 900; letter-spacing: 5px; margin: 0;"">{code}</p>
            </div>
            <p style=""color: #888888; font-size: 12px; text-align: center;"">З повагою,<br>Команда CryptoAuth</p>
        </div>";
    }

    private static string GenerateEmailConfirmation(string url)
    {
        var uri = JsonSerializer.Deserialize<ConfirmationEmailMessage>(url);
        
        return $@"
        <div style=""font-family: 'Segoe UI', sans-serif; background-color: #F9F7FF; padding: 40px 20px; border-radius: 12px; max-width: 550px; margin: 20px auto; border: 1px solid #E6E0FF;"">
            <div style=""text-align: center; margin-bottom: 30px;"">
                <h1 style=""color: #3C2E59; font-size: 24px; font-weight: 800;"">Підтвердження пошти</h1>
            </div>
            <div style=""background-color: #ffffff; padding: 40px; border-radius: 16px; text-align: center;"">
                <p style=""color: #4A4A4A; margin-bottom: 30px;"">Вітаємо в <strong>CryptoAuth</strong>! Щоб почати, підтвердьте вашу адресу.</p>
                <a href=""{uri}"" style=""background-color: #6A0DAD; color: #ffffff; padding: 16px 36px; border-radius: 10px; text-decoration: none; font-weight: 700;"">
                    Підтвердити Email
                </a>
            </div>
        </div>";
    }
}

public class EmailSettings
{
    public string ApiKey { get; set; }
    public string SenderEmail { get; set; }
    public string SenderName { get; set; }
}