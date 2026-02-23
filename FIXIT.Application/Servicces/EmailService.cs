
namespace FIXIT.Application.Servicces;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        this.logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var fromEmail = _configuration["EmailSettings:From"];
        var Password = Environment.GetEnvironmentVariable("EmailPassword")!;
        var SmtpServer = _configuration["EmailSettings:SmtpServer"];

        using var client = new SmtpClient(SmtpServer, 587)
        {
            Credentials = new NetworkCredential(fromEmail, Password),
            EnableSsl = true
        };

        using var message = new MailMessage(fromEmail, toEmail, subject, body)
        { IsBodyHtml = true };

        await client.SendMailAsync(message);

        logger.LogError("Happend Error at Email Service");
    }
}
