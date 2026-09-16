using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EF_Core.Configurations;

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(MaxLength.ProjectTitle);

        builder.HasIndex(p => p.Title)
            .IsUnique();

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(MaxLength.ProjectSlug);

        builder.HasIndex(p => p.Slug)
            .IsUnique();

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(MaxLength.ProjectDescription);

        builder.Property(p => p.StartDate)
            .IsRequired();

        builder.Property(p => p.EndDate)
            .IsRequired(false);

        builder.Property(p => p.LiveURL)
            .IsRequired(false)
            .HasMaxLength(MaxLength.ProjectLiveURL);

        builder.Property(p => p.GitHubURL)
            .IsRequired(false)
            .HasMaxLength(MaxLength.ProjectGitHubURL);


        builder.HasMany(p => p.Images)
            .WithOne(pi => pi.Project)
            .HasForeignKey(pi => pi.ProjectId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Metadata.FindNavigation(nameof(Project.Images))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.ToTable("Projects");
    }
}
