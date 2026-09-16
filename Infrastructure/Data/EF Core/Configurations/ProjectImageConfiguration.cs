using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EF_Core.Configurations;

internal sealed class ProjectImageConfiguration : IEntityTypeConfiguration<ProjectImage>
{
    public void Configure(EntityTypeBuilder<ProjectImage> builder)
    {
        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.Id)
            .ValueGeneratedNever();

        builder.Property(pi => pi.URL)
            .IsRequired()
            .HasMaxLength(MaxLength.ProjectImageURL);

        builder.Property(pi => pi.AltText)
            .IsRequired()
            .HasMaxLength(MaxLength.ProjectImageAltText);

        builder.Property(pi => pi.DisplayOrder)
            .IsRequired();

        builder.HasIndex(pi => new { pi.ProjectId, pi.DisplayOrder })
            .IsUnique();

        builder.HasIndex(pi => pi.ProjectId)
            .IsUnique()
            .HasFilter("[IsCover] = 1");

        builder.ToTable("ProjectImages");
    }
}
