using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EF_Core.Configurations;

internal sealed class AboutItemConfiguration : IEntityTypeConfiguration<AboutItem>
{
    public void Configure(EntityTypeBuilder<AboutItem> builder)
    {
        builder.HasKey(ai => ai.Id);
        builder.Property(ai => ai.Id)
            .ValueGeneratedNever();

        builder.Property(ai => ai.Title)
            .IsRequired()
            .HasMaxLength(MaxLength.AboutItemTitle);

        builder.HasIndex(ai => ai.Title)
            .IsUnique();

        builder.Property(ai => ai.Description)
            .IsRequired()
            .HasMaxLength(MaxLength.AboutItemDescription);

        builder.Property(ai => ai.DisplayOrder)
            .IsRequired();

        builder.HasIndex(ai => ai.DisplayOrder)
            .IsUnique();

        builder.Property(ai => ai.IsVisible)
            .IsRequired();

        builder.ToTable("AboutItems");
    }
}
