using JobHunter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Persistence;

public sealed class JobHunterDbContext : DbContext
{
    public JobHunterDbContext(DbContextOptions<JobHunterDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();
        user.ToTable("users");

        user.HasKey(x => x.Id);
        user.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        user.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        user.Property(x => x.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
        user.HasIndex(x => x.Email).IsUnique();

        user.Property(x => x.PasswordHash).HasColumnName("password").HasMaxLength(400).IsRequired();
        user.Property(x => x.Age).HasColumnName("age");
        user.Property(x => x.Gender).HasColumnName("gender").HasConversion<string>().HasMaxLength(10);
        user.Property(x => x.Address).HasColumnName("address").HasMaxLength(300);
        user.Property(x => x.Avatar).HasColumnName("avatar").HasMaxLength(300);
        user.Property(x => x.RefreshToken).HasColumnName("refresh_token").HasMaxLength(300);
        user.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        user.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        user.OwnsOne(x => x.Company, company =>
        {
            company.Property(c => c.Id).HasColumnName("company_id");
            company.Ignore(c => c.Name);
        });

        user.OwnsOne(x => x.Role, role =>
        {
            role.Property(r => r.Id).HasColumnName("role_id");
            role.Ignore(r => r.Name);
        });
    }
}
