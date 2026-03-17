
using JobHunter.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace JobHunter.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendSimpleEmailAsync(string to, string subject, string content, bool isHtml, CancellationToken cancellationToken = default)
    {
        var emailSection = _configuration.GetSection("Email");
        var from = emailSection["From"];
        if (string.IsNullOrWhiteSpace(from))
        {
            from = emailSection["Username"];
            if (string.IsNullOrWhiteSpace(from))
            {
                _logger.LogError("Email 'From' address is not configured. Please set Email:From or Email:Username in appsettings.json.");
                throw new ArgumentNullException("from", "Email 'From' address is not configured. Please set Email:From or Email:Username in appsettings.json.");
            }
        }
        var smtpClient = new SmtpClient(emailSection["Host"])
        {
            Port = int.Parse(emailSection["Port"] ?? "25"),
            Credentials = new NetworkCredential(emailSection["Username"], emailSection["Password"]),
            EnableSsl = bool.Parse(emailSection["EnableSsl"] ?? "true")
        };
        var mail = new MailMessage(from, to, subject, content)
        {
            IsBodyHtml = isHtml
        };
        try
        {
            await smtpClient.SendMailAsync(mail, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email");
            throw;
        }
    }

    public async Task SendEmailFromTemplateAsync(string to, string subject, string templateName, string userName, object value, CancellationToken cancellationToken = default)
    {
        // Placeholder: In production, use a template engine (e.g., RazorLight, Scriban)
        var content = $"<h1>Hello {userName}, here are your jobs:</h1><pre>{value}</pre>";
        await SendSimpleEmailAsync(to, subject, content, true, cancellationToken);
    }
}
