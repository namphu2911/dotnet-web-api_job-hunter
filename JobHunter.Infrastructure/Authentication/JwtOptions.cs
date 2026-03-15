namespace JobHunter.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "JobHunter";

    public string Audience { get; set; } = "JobHunter.Client";

    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 60;

    public int RefreshTokenMinutes { get; set; } = 1440;
}
