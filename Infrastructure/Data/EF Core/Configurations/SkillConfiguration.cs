using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EF_Core.Configurations;

internal sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(MaxLength.SkillName);

        builder.HasIndex(s => s.Name)
            .IsUnique();

        builder.Property(s => s.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(s => s.Icon)
            .IsRequired()
            .HasMaxLength(MaxLength.SkillIcon);

        builder.ToTable("Skills");
    }
}
