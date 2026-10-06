using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EF_Core.Configurations;

internal sealed class FreelanceProfileConfiguration : IEntityTypeConfiguration<FreelanceProfile>
{
    public void Configure(EntityTypeBuilder<FreelanceProfile> builder)
    {
        builder.HasKey(fp => fp.Id);

        builder.Property(fp => fp.Id)
            .ValueGeneratedNever();

        builder.Property(fp => fp.PlatformName)
            .IsRequired()
            .HasMaxLength(MaxLength.FreelanceProfilePlatformName);

        builder.HasIndex(fp => fp.PlatformName)
            .IsUnique();

        builder.Property(fp => fp.ProfileURL)
            .IsRequired()
            .HasMaxLength(MaxLength.FreelanceProfileProfileURL);

        builder.HasIndex(fp => fp.ProfileURL)
            .IsUnique();

        builder.Property(fp => fp.Icon)
            .IsRequired()
            .HasMaxLength(MaxLength.FreelanceProfileIcon);

        builder.HasIndex(fp => fp.Icon)
            .IsUnique();

        builder.Property(fp => fp.DisplayOrder)
            .IsRequired();

        builder.HasIndex(fp => fp.DisplayOrder)
            .IsUnique();

        builder.Property(fp => fp.IsVisible)
            .IsRequired();

        builder.ToTable("FreelanceProfiles");
    }
}
