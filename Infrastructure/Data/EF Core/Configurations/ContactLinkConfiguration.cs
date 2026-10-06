using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EF_Core.Configurations;

internal sealed class ContactLinkConfiguration : IEntityTypeConfiguration<ContactLink>
{
    public void Configure(EntityTypeBuilder<ContactLink> builder)
    {
        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Id)
            .ValueGeneratedNever();

        builder.Property(sl => sl.Name)
            .IsRequired()
            .HasMaxLength(MaxLength.ContactLinkName);

        builder.HasIndex(sl => sl.Name)
            .IsUnique();

        builder.Property(sl => sl.Contact)
            .IsRequired()
            .HasMaxLength(MaxLength.ContactLinkContact);

        builder.HasIndex(sl => sl.Contact)
            .IsUnique();

        builder.Property(sl => sl.DisplayOrder)
            .IsRequired();

        builder.HasIndex(sl => sl.DisplayOrder)
            .IsUnique();

        builder.Property(sl => sl.IsVisible)
            .IsRequired();

        builder.ToTable("ContactLinks");
    }
}
