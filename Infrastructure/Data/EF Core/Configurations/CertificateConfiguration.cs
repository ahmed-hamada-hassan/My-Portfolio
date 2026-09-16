using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EF_Core.Configurations;

internal sealed class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(MaxLength.CertificateName);

        builder.Property(c => c.Issuer)
            .IsRequired()
            .HasMaxLength(MaxLength.CertificateIssuer);

        builder.Property(c => c.IssuerLogoURL)
            .IsRequired()
            .HasMaxLength(MaxLength.CertificateIssuerLogoURL);

        builder.Property(c => c.IssueDate)
            .IsRequired();

        builder.Property(c => c.CredentialURL)
            .IsRequired()
            .HasMaxLength(MaxLength.CertificateCredentialURL);

        builder.Property(c => c.ImageURL)
            .IsRequired()
            .HasMaxLength(MaxLength.CertificateImageURL);

        builder.Property(c => c.DisplayOrder)
            .IsRequired();

        builder.HasIndex(c => c.DisplayOrder)
           .IsUnique();

        builder.HasIndex(c => c.CredentialURL)
            .IsUnique();

        builder.ToTable("Certificates");
    }
}
