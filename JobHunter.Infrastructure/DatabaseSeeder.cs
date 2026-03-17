using JobHunter.Domain.Entities;
using JobHunter.Domain.Enums;
using JobHunter.Domain.Repositories;
using JobHunter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobHunter.Infrastructure;

public class DatabaseSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(IServiceProvider serviceProvider, ILogger<DatabaseSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<JobHunterDbContext>();

        // Seed permissions if empty
        if (!await db.Permissions.AnyAsync(cancellationToken))
        {
            var permissions = new List<Permission>
            {
                new Permission { Name = "Create a company", ApiPath = "/api/v1/companies", Method = "POST", Module = "COMPANIES" },
                new Permission { Name = "Update a company", ApiPath = "/api/v1/companies", Method = "PUT", Module = "COMPANIES" },
                new Permission { Name = "Delete a company", ApiPath = "/api/v1/companies/{id}", Method = "DELETE", Module = "COMPANIES" },
                new Permission { Name = "Get a company by id", ApiPath = "/api/v1/companies/{id}", Method = "GET", Module = "COMPANIES" },
                new Permission { Name = "Get companies with pagination", ApiPath = "/api/v1/companies", Method = "GET", Module = "COMPANIES" },
                // ... Add all other permissions as in Java seeder ...
            };
            await db.Permissions.AddRangeAsync(permissions, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded permissions");
        }

        // Seed SUPER_ADMIN role if empty
        if (!await db.Roles.AnyAsync(cancellationToken))
        {
            var allPermissions = await db.Permissions.ToListAsync(cancellationToken);
            var adminRole = new Role
            {
                Name = "SUPER_ADMIN",
                Description = "Admin has all permissions",
                Active = true
            };
            foreach (var perm in allPermissions)
            {
                adminRole.Permissions.Add(perm);
            }
            await db.Roles.AddAsync(adminRole, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded SUPER_ADMIN role");
        }

        // Seed admin user if empty
        if (!await db.Users.AnyAsync(cancellationToken))
        {
            var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "SUPER_ADMIN", cancellationToken);
            var adminUser = new User
            {
                Email = "admin@gmail.com",
                Address = "hn",
                Age = 25,
                Gender = Gender.Male,
                Name = "I'm super admin",
                Password = "123456", // TODO: Hash in production
                Role = adminRole!
            };
            await db.Users.AddAsync(adminUser, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded admin user");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
