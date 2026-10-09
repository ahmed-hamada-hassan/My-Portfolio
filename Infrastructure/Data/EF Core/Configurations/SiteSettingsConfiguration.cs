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

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.SiteTitle)
            .IsRequired()
            .HasMaxLength(MaxLength.SiteSettingsSiteTitle);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(MaxLength.SiteSettingsFullName);

        builder.Property(x => x.JobTitle)
            .IsRequired()
            .HasMaxLength(MaxLength.SiteSettingsJobTitle);

        builder.Property(x => x.ProfileImageURL)
            .HasMaxLength(MaxLength.SiteSettingsProfileImageURL);

        builder.Property(x => x.ProfileImagePublicId)
            .HasMaxLength(MaxLength.SiteSettingsProfileImagePublicId);

        builder.Property(x => x.CVURL)
            .HasMaxLength(MaxLength.SiteSettingsCVURL);

        builder.Property(x => x.AvailableForWork)
            .IsRequired();

        builder.ToTable("SiteSettings");
    }
}