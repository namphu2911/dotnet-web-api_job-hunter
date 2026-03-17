namespace JobHunter.Application.Abstractions;

public interface IEmailService
{
    Task SendSimpleEmailAsync(string to, string subject, string content, bool isHtml, CancellationToken cancellationToken = default);
    Task SendEmailFromTemplateAsync(string to, string subject, string templateName, string userName, object value, CancellationToken cancellationToken = default);
}
