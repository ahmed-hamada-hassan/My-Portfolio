using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.EF_Core;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectImage> ProjectImages => Set<ProjectImage>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<ProjectSkill> ProjectSkills => Set<ProjectSkill>();
    public DbSet<Experience> Experiences => Set<Experience>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<ContactLink> SocialLinks => Set<ContactLink>();
    public DbSet<FreelanceProfile> FreelanceProfiles => Set<FreelanceProfile>();
    public DbSet<AboutItem> AboutItems => Set<AboutItem>();
    public DbSet<User> Users => Set<User>();
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
