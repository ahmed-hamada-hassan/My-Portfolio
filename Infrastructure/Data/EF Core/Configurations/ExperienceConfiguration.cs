using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EF_Core.Configurations;

internal sealed class ExperienceConfiguration : IEntityTypeConfiguration<Experience>
{
    public void Configure(EntityTypeBuilder<Experience> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.CompanyName)
            .IsRequired()
            .HasMaxLength(MaxLength.ExperienceCompanyName);

        builder.Property(e => e.Position)
            .IsRequired()
            .HasMaxLength(MaxLength.ExperiencePosition);

        builder.Property(e => e.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(MaxLength.ExperienceDescription);

        builder.Property(e => e.StartDate)
            .IsRequired();

        builder.Property(e => e.EndDate)
            .IsRequired(false);

        builder.ToTable("Experiences");
    }
}
