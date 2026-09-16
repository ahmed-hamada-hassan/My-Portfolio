using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EF_Core.Configurations;

internal sealed class SocialLinkConfiguration : IEntityTypeConfiguration<SocialLink>
{
    public void Configure(EntityTypeBuilder<SocialLink> builder)
    {
        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Id)
            .ValueGeneratedNever();

        builder.Property(sl => sl.Name)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(sl => sl.Name)
            .IsUnique();

        builder.Property(sl => sl.URL)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(sl => sl.Icon)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sl => sl.DisplayOrder)
            .IsRequired();

        builder.HasIndex(sl => sl.DisplayOrder)
            .IsUnique();

        builder.Property(sl => sl.IsVisible)
            .IsRequired();

        builder.ToTable("SocialLinks");
    }
}
