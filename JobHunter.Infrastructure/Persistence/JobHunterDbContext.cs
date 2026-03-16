using JobHunter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Persistence;

public class JobHunterDbContext : DbContext
{
    public JobHunterDbContext(DbContextOptions<JobHunterDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Subscriber> Subscribers => Set<Subscriber>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Company Configuration
        ConfigureCompany(modelBuilder);

        // User Configuration
        ConfigureUser(modelBuilder);

        // Role Configuration
        ConfigureRole(modelBuilder);

        // Permission Configuration
        ConfigurePermission(modelBuilder);

        // Job Configuration
        ConfigureJob(modelBuilder);

        // Skill Configuration
        ConfigureSkill(modelBuilder);

        // Resume Configuration
        ConfigureResume(modelBuilder);

        // Subscriber Configuration
        ConfigureSubscriber(modelBuilder);

        // Many-to-Many: Job_Skill
        ConfigureJobSkill(modelBuilder);

        // Many-to-Many: Permission_Role
        ConfigurePermissionRole(modelBuilder);

        // Many-to-Many: Subscriber_Skill
        ConfigureSubscriberSkill(modelBuilder);
    }

    private static void ConfigureCompany(ModelBuilder modelBuilder)
    {
        var company = modelBuilder.Entity<Company>();
        company.ToTable("companies");

        company.HasKey(x => x.Id);
        company.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        company.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        company.Property(x => x.Description).HasColumnName("description").HasColumnType("NVARCHAR(MAX)");
        company.Property(x => x.Address).HasColumnName("address").HasMaxLength(300);
        company.Property(x => x.Logo).HasColumnName("logo").HasMaxLength(300);
        company.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        company.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        company.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(150);
        company.Property(x => x.UpdatedBy).HasColumnName("updated_by").HasMaxLength(150);

        company.HasMany(x => x.Users)
            .WithOne(u => u.Company)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        company.HasMany(x => x.Jobs)
            .WithOne(j => j.Company)
            .HasForeignKey(j => j.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();
        user.ToTable("users");

        user.HasKey(x => x.Id);
        user.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        user.Property(x => x.Name).HasColumnName("name").HasMaxLength(150);
        user.Property(x => x.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
        user.HasIndex(x => x.Email).IsUnique();
        user.Property(x => x.Password).HasColumnName("password").HasMaxLength(400).IsRequired();
        user.Property(x => x.Age).HasColumnName("age");
        user.Property(x => x.Gender).HasColumnName("gender").HasConversion<string>().HasMaxLength(10);
        user.Property(x => x.Address).HasColumnName("address").HasMaxLength(300);
        user.Property(x => x.Avatar).HasColumnName("avatar").HasMaxLength(300);
        user.Property(x => x.RefreshToken).HasColumnName("refresh_token").HasColumnType("NVARCHAR(MAX)");
        user.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        user.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        user.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(150);
        user.Property(x => x.UpdatedBy).HasColumnName("updated_by").HasMaxLength(150);

        user.HasOne(x => x.Company)
            .WithMany(c => c.Users)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        user.HasOne(x => x.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.SetNull);

        user.HasMany(x => x.Resumes)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureRole(ModelBuilder modelBuilder)
    {
        var role = modelBuilder.Entity<Role>();
        role.ToTable("roles");

        role.HasKey(x => x.Id);
        role.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        role.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        role.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
        role.Property(x => x.Active).HasColumnName("active").IsRequired();
        role.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        role.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        role.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(150);
        role.Property(x => x.UpdatedBy).HasColumnName("updated_by").HasMaxLength(150);
    }

    private static void ConfigurePermission(ModelBuilder modelBuilder)
    {
        var permission = modelBuilder.Entity<Permission>();
        permission.ToTable("permissions");

        permission.HasKey(x => x.Id);
        permission.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        permission.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        permission.Property(x => x.ApiPath).HasColumnName("api_path").HasMaxLength(500).IsRequired();
        permission.Property(x => x.Method).HasColumnName("method").HasMaxLength(20).IsRequired();
        permission.Property(x => x.Module).HasColumnName("module").HasMaxLength(100).IsRequired();
        permission.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        permission.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        permission.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(150);
        permission.Property(x => x.UpdatedBy).HasColumnName("updated_by").HasMaxLength(150);
    }

    private static void ConfigureJob(ModelBuilder modelBuilder)
    {
        var job = modelBuilder.Entity<Job>();
        job.ToTable("jobs");

        job.HasKey(x => x.Id);
        job.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        job.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        job.Property(x => x.Location).HasColumnName("location").HasMaxLength(300).IsRequired();
        job.Property(x => x.Salary).HasColumnName("salary");
        job.Property(x => x.Quantity).HasColumnName("quantity");
        job.Property(x => x.Level).HasColumnName("level").HasConversion<string>().HasMaxLength(20);
        job.Property(x => x.Description).HasColumnName("description").HasColumnType("NVARCHAR(MAX)");
        job.Property(x => x.StartDate).HasColumnName("start_date");
        job.Property(x => x.EndDate).HasColumnName("end_date");
        job.Property(x => x.Active).HasColumnName("active");
        job.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        job.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        job.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(150);
        job.Property(x => x.UpdatedBy).HasColumnName("updated_by").HasMaxLength(150);

        job.HasOne(x => x.Company)
            .WithMany(c => c.Jobs)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        job.HasMany(x => x.Resumes)
            .WithOne(r => r.Job)
            .HasForeignKey(r => r.JobId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureSkill(ModelBuilder modelBuilder)
    {
        var skill = modelBuilder.Entity<Skill>();
        skill.ToTable("skills");

        skill.HasKey(x => x.Id);
        skill.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        skill.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        skill.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        skill.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        skill.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(150);
        skill.Property(x => x.UpdatedBy).HasColumnName("updated_by").HasMaxLength(150);
    }

    private static void ConfigureResume(ModelBuilder modelBuilder)
    {
        var resume = modelBuilder.Entity<Resume>();
        resume.ToTable("resumes");

        resume.HasKey(x => x.Id);
        resume.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        resume.Property(x => x.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
        resume.Property(x => x.Url).HasColumnName("url").HasMaxLength(500).IsRequired();
        resume.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        resume.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        resume.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        resume.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(150);
        resume.Property(x => x.UpdatedBy).HasColumnName("updated_by").HasMaxLength(150);

        resume.HasOne(x => x.User)
            .WithMany(u => u.Resumes)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        resume.HasOne(x => x.Job)
            .WithMany(j => j.Resumes)
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureSubscriber(ModelBuilder modelBuilder)
    {
        var subscriber = modelBuilder.Entity<Subscriber>();
        subscriber.ToTable("subscribers");

        subscriber.HasKey(x => x.Id);
        subscriber.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        subscriber.Property(x => x.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
        subscriber.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        subscriber.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        subscriber.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        subscriber.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(150);
        subscriber.Property(x => x.UpdatedBy).HasColumnName("updated_by").HasMaxLength(150);
    }

    private static void ConfigureJobSkill(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Job>()
            .HasMany(j => j.Skills)
            .WithMany(s => s.Jobs)
            .UsingEntity(j => j.ToTable("job_skill"));
    }

    private static void ConfigurePermissionRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>()
            .HasMany(r => r.Permissions)
            .WithMany(p => p.Roles)
            .UsingEntity(j => j.ToTable("permission_role"));
    }

    private static void ConfigureSubscriberSkill(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subscriber>()
            .HasMany(s => s.Skills)
            .WithMany(sk => sk.Subscribers)
            .UsingEntity(j => j.ToTable("subscriber_skill"));
    }
}
