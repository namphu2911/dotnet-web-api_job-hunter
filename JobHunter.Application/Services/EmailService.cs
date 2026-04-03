using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Emails;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace JobHunter.Application.Services;

public class EmailService : IEmailService
{
    private static readonly Regex NamePlaceholderRegex = new(
        "<span[^>]*th:text=\\\"\\$\\{name\\}\\\"[^>]*>\\s*</span>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex JobsLoopRegex = new(
        "<tr\\s+th:each=\\\"job : \\$\\{jobs\\}\\\"[\\s\\S]*?</tr>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex ThymeleafAttributeRegex = new(
        "\\s+th:[a-zA-Z-]+=\\\"[^\\\"]*\\\"",
        RegexOptions.Compiled);

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
        var jobs = ExtractJobs(value);
        var content = await RenderTemplateAsync(templateName, userName, jobs, cancellationToken);
        await SendSimpleEmailAsync(to, subject, content, true, cancellationToken);
    }

    private async Task<string> RenderTemplateAsync(string templateName, string userName, IReadOnlyCollection<ResEmailJobDto> jobs, CancellationToken cancellationToken)
    {
        var templatePath = ResolveTemplatePath(templateName);
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Email template '{templateName}' not found.", templatePath);
        }

        var template = await File.ReadAllTextAsync(templatePath, cancellationToken);
        var rendered = NamePlaceholderRegex.Replace(template, WebUtility.HtmlEncode(userName));
        rendered = JobsLoopRegex.Replace(rendered, BuildJobRows(jobs));
        rendered = ThymeleafAttributeRegex.Replace(rendered, string.Empty);
        return rendered;
    }

    private string ResolveTemplatePath(string templateName)
    {
        var normalizedTemplateName = templateName.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
            ? templateName
            : $"{templateName}.html";

        var configuredRoot = _configuration["Email:TemplateRootPath"];
        var currentDirectory = Directory.GetCurrentDirectory();
        var baseDirectory = AppContext.BaseDirectory;

        var candidateRoots = new List<string>();
        if (!string.IsNullOrWhiteSpace(configuredRoot))
        {
            candidateRoots.Add(Path.IsPathRooted(configuredRoot)
                ? configuredRoot
                : Path.GetFullPath(Path.Combine(currentDirectory, configuredRoot)));
        }

        // Prefer templates shipped with the .NET project.
        candidateRoots.Add(Path.GetFullPath(Path.Combine(currentDirectory, "JobHunter.Application", "Templates")));
        candidateRoots.Add(Path.GetFullPath(Path.Combine(currentDirectory, "Templates")));
        candidateRoots.Add(Path.GetFullPath(Path.Combine(baseDirectory, "Templates")));
        candidateRoots.Add(Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "JobHunter.Application", "Templates")));

        // Keep Java template folder as migration fallback only.
        candidateRoots.Add(Path.GetFullPath(Path.Combine(currentDirectory, "src", "main", "resources", "templates")));
        candidateRoots.Add(Path.GetFullPath(Path.Combine(currentDirectory, "..", "src", "main", "resources", "templates")));

        foreach (var root in candidateRoots.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var candidateFile = Path.Combine(root, normalizedTemplateName);
            if (File.Exists(candidateFile))
            {
                return candidateFile;
            }
        }

        return Path.Combine(candidateRoots.First(), normalizedTemplateName);
    }

    private static IReadOnlyCollection<ResEmailJobDto> ExtractJobs(object value)
    {
        if (value is IReadOnlyCollection<ResEmailJobDto> ready)
        {
            return ready;
        }

        if (value is IEnumerable<ResEmailJobDto> enumerable)
        {
            return enumerable.ToList();
        }

        throw new InvalidOperationException("Template payload must be a collection of ResEmailJobDto.");
    }

    private static string BuildJobRows(IEnumerable<ResEmailJobDto> jobs)
    {
        var builder = new StringBuilder();

        foreach (var job in jobs)
        {
            var jobName = WebUtility.HtmlEncode(job.Name);
            var companyName = WebUtility.HtmlEncode(job.Company);
            var salaryText = job.Salary.ToString("#,0", CultureInfo.InvariantCulture);
            var skillTags = string.Join(
                string.Empty,
                job.Skills.Select(skill =>
                    $"<span style=\"font-size: 14px; background: #e8e8e8; padding: 3px; margin-right: 5px;  border-radius: 3px;\">{WebUtility.HtmlEncode(skill)}</span>"));

            builder.AppendLine("<tr>");
            builder.AppendLine("    <td>");
            builder.AppendLine("        <div style=\"font-size: 16px;\">");
            builder.AppendLine($"            <a href=\"https://hoidanit.vn/\" target=\"_blank\" style=\"text-decoration: none;\">{jobName}</a>");
            builder.AppendLine("        </div>");
            builder.AppendLine($"        <div style=\"font-size: 14px;\">{companyName}</div>");
            builder.AppendLine($"        <div style=\"font-size: 14px;\">{salaryText} đ</div>");
            builder.AppendLine("        <div style=\"margin-top: 5px;\">");
            builder.AppendLine($"            {skillTags}");
            builder.AppendLine("        </div>");
            builder.AppendLine("        <div style=\"margin: 15px 0; border-top: 1px dashed rgba(5, 5, 5, 0.06);\"></div>");
            builder.AppendLine("    </td>");
            builder.AppendLine("</tr>");
        }

        return builder.ToString();
    }
}
