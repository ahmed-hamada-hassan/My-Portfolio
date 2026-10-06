using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EF_Core.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(MaxLength.UserEmail);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.HashedPassword)
            .IsRequired()
            .HasMaxLength(MaxLength.UserHashedPassword);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>();

        builder.ToTable("Users");
    }
}
