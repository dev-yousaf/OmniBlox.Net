using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniBlox.Domain.Entities;

namespace OmniBlox.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(c => c.Phone)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Address)
            .HasMaxLength(500);

        builder.Property(c => c.WorkspaceUrl)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Industry)
            .HasMaxLength(100);

        builder.Property(c => c.Country)
            .HasMaxLength(100);

        builder.HasIndex(c => c.Email).IsUnique();
        builder.HasIndex(c => c.WorkspaceUrl).IsUnique();
    }
}
