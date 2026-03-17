using JobHunter.Application.Abstractions;
using JobHunter.Domain.Repositories;
using JobHunter.Infrastructure.Authentication;
using JobHunter.Infrastructure.Persistence;
using JobHunter.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobHunter.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = ResolveConnectionString(configuration);

        services.AddDbContext<JobHunterDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("JobHunter.Infrastructure");
            }));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ISkillRepository, SkillRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IResumeRepository, ResumeRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ISubscriberRepository, SubscriberRepository>();
        services.AddHostedService<EmailJobHostedService>();
        services.AddHostedService<DatabaseSeeder>();
        return services;
    }

    private static string ResolveConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Dev fallback — override via user-secrets or appsettings.Development.json
            connectionString = "Server=localhost;Database=JobHunterDB;Trusted_Connection=True;TrustServerCertificate=True";
        }

        return connectionString;
    }
}
