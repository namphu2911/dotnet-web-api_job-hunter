using JobHunter.Domain.Entities;
using JobHunter.Domain.Enums;
using JobHunter.Domain.Repositories;
using JobHunter.Application.Services.Security;
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

                new Permission { Name = "Create a job", ApiPath = "/api/v1/jobs", Method = "POST", Module = "JOBS" },
                new Permission { Name = "Update a job", ApiPath = "/api/v1/jobs", Method = "PUT", Module = "JOBS" },
                new Permission { Name = "Delete a job", ApiPath = "/api/v1/jobs/{id}", Method = "DELETE", Module = "JOBS" },
                new Permission { Name = "Get a job by id", ApiPath = "/api/v1/jobs/{id}", Method = "GET", Module = "JOBS" },
                new Permission { Name = "Get jobs with pagination", ApiPath = "/api/v1/jobs", Method = "GET", Module = "JOBS" },

                new Permission { Name = "Create a permission", ApiPath = "/api/v1/permissions", Method = "POST", Module = "PERMISSIONS" },
                new Permission { Name = "Update a permission", ApiPath = "/api/v1/permissions", Method = "PUT", Module = "PERMISSIONS" },
                new Permission { Name = "Delete a permission", ApiPath = "/api/v1/permissions/{id}", Method = "DELETE", Module = "PERMISSIONS" },
                new Permission { Name = "Get a permission by id", ApiPath = "/api/v1/permissions/{id}", Method = "GET", Module = "PERMISSIONS" },
                new Permission { Name = "Get permissions with pagination", ApiPath = "/api/v1/permissions", Method = "GET", Module = "PERMISSIONS" },

                new Permission { Name = "Create a resume", ApiPath = "/api/v1/resumes", Method = "POST", Module = "RESUMES" },
                new Permission { Name = "Update a resume", ApiPath = "/api/v1/resumes", Method = "PUT", Module = "RESUMES" },
                new Permission { Name = "Delete a resume", ApiPath = "/api/v1/resumes/{id}", Method = "DELETE", Module = "RESUMES" },
                new Permission { Name = "Get a resume by id", ApiPath = "/api/v1/resumes/{id}", Method = "GET", Module = "RESUMES" },
                new Permission { Name = "Get resumes with pagination", ApiPath = "/api/v1/resumes", Method = "GET", Module = "RESUMES" },

                new Permission { Name = "Create a role", ApiPath = "/api/v1/roles", Method = "POST", Module = "ROLES" },
                new Permission { Name = "Update a role", ApiPath = "/api/v1/roles", Method = "PUT", Module = "ROLES" },
                new Permission { Name = "Delete a role", ApiPath = "/api/v1/roles/{id}", Method = "DELETE", Module = "ROLES" },
                new Permission { Name = "Get a role by id", ApiPath = "/api/v1/roles/{id}", Method = "GET", Module = "ROLES" },
                new Permission { Name = "Get roles with pagination", ApiPath = "/api/v1/roles", Method = "GET", Module = "ROLES" },

                new Permission { Name = "Create a user", ApiPath = "/api/v1/users", Method = "POST", Module = "USERS" },
                new Permission { Name = "Update a user", ApiPath = "/api/v1/users", Method = "PUT", Module = "USERS" },
                new Permission { Name = "Delete a user", ApiPath = "/api/v1/users/{id}", Method = "DELETE", Module = "USERS" },
                new Permission { Name = "Get a user by id", ApiPath = "/api/v1/users/{id}", Method = "GET", Module = "USERS" },
                new Permission { Name = "Get users with pagination", ApiPath = "/api/v1/users", Method = "GET", Module = "USERS" },

                new Permission { Name = "Create a subscriber", ApiPath = "/api/v1/subscribers", Method = "POST", Module = "SUBSCRIBERS" },
                new Permission { Name = "Update a subscriber", ApiPath = "/api/v1/subscribers", Method = "PUT", Module = "SUBSCRIBERS" },
                new Permission { Name = "Delete a subscriber", ApiPath = "/api/v1/subscribers/{id}", Method = "DELETE", Module = "SUBSCRIBERS" },
                new Permission { Name = "Get a subscriber by id", ApiPath = "/api/v1/subscribers/{id}", Method = "GET", Module = "SUBSCRIBERS" },
                new Permission { Name = "Get subscribers with pagination", ApiPath = "/api/v1/subscribers", Method = "GET", Module = "SUBSCRIBERS" },

                new Permission { Name = "Download a file", ApiPath = "/api/v1/files", Method = "GET", Module = "FILES" },
                new Permission { Name = "Upload a file", ApiPath = "/api/v1/files", Method = "POST", Module = "FILES" },
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
                Gender = Gender.MALE,
                Name = "I'm super admin",
                Password = PasswordSecurity.HashPassword("123456"),
                Role = adminRole!
            };
            await db.Users.AddAsync(adminUser, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded admin user");
        }

        // Backward compatibility: migrate legacy plaintext passwords seeded previously.
        var usersWithPlaintextPassword = await db.Users
            .Where(x => x.Password != null && x.Password != string.Empty && !EF.Functions.Like(x.Password, "$2%"))
            .ToListAsync(cancellationToken);

        if (usersWithPlaintextPassword.Count > 0)
        {
            foreach (var user in usersWithPlaintextPassword)
            {
                user.Password = PasswordSecurity.HashPassword(user.Password);
            }

            await db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Migrated {Count} legacy plaintext passwords to BCrypt", usersWithPlaintextPassword.Count);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
