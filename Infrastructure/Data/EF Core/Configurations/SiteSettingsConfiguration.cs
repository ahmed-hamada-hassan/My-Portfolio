using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EF_Core.Configurations;

public class SiteSettingsConfiguration : IEntityTypeConfiguration<SiteSettings>
{
    public void Configure(EntityTypeBuilder<SiteSettings> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SiteTitle)
            .IsRequired()
            .HasMaxLength(MaxLength.SiteTitle);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(MaxLength.FullName);

        builder.Property(x => x.JobTitle)
            .IsRequired()
            .HasMaxLength(MaxLength.JobTitle);

        builder.Property(x => x.ProfileImageUrl)
            .HasMaxLength(MaxLength.ProfileImageUrl);

        builder.Property(x => x.ProfileImagePublicId)
            .HasMaxLength(MaxLength.ProfileImagePublicId);

        builder.Property(x => x.CvUrl)
            .HasMaxLength(MaxLength.CvUrl);

        builder.Property(x => x.CvPublicId)
            .HasMaxLength(MaxLength.CvPublicId);

        builder.Property(x => x.AvailableForWork)
            .IsRequired();
    }
}