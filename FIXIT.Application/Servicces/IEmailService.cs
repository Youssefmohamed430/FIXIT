namespace FIXIT.Application.Servicces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}
