using System.Net;
using System.Net.Mail;

public class EmailService
{
    private readonly SmtpClient _smtpClient;
    private readonly string _fromEmail;

    public EmailService(string smtpHost, int smtpPort, string smtpUser, string smtpPass)
    {
        _smtpClient = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };
        _fromEmail = smtpUser;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var mailMessage = new MailMessage(_fromEmail, to, subject, body);
        await _smtpClient.SendMailAsync(mailMessage);
    }
}
