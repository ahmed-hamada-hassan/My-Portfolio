using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EF_Core.Configurations;

internal sealed class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.Id)
            .ValueGeneratedNever();

        builder.Property(cm => cm.FirstName)
            .IsRequired()
            .HasMaxLength(MaxLength.ContactMessageFirstName);

        builder.Property(cm => cm.LastName)
            .IsRequired()
            .HasMaxLength(MaxLength.ContactMessageLastName);

        builder.Property(cm => cm.Email)
            .IsRequired()
            .HasMaxLength(MaxLength.ContactMessageEmail);

        builder.Property(cm => cm.Subject)
            .IsRequired()
            .HasMaxLength(MaxLength.ContactMessageSubject);

        builder.Property(cm => cm.Message)
            .IsRequired()
            .HasMaxLength(MaxLength.ContactMessageMessage);

        builder.Property(cm => cm.CreatedAt)
            .IsRequired();

        builder.Property(cm => cm.IsRead)
            .IsRequired();

        builder.ToTable("ContactMessages");
    }
}
