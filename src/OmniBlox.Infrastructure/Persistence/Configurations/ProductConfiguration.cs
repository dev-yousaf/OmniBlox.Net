using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmniBlox.Domain.Entities;

namespace OmniBlox.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.SKU)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Category)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.SubCategory)
            .HasMaxLength(100);

        builder.Property(p => p.Brand)
            .HasMaxLength(100);

        builder.Property(p => p.Unit)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.ImageUrl)
            .HasColumnType("text");

        builder.Property(p => p.SalePrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.CostPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.BarcodeSymbology)
            .HasMaxLength(50);

        builder.Property(p => p.TaxRate)
            .HasColumnType("decimal(5,2)");

        builder.Property(p => p.ItemCode)
            .HasMaxLength(100);

        builder.Property(p => p.Manufacturer)
            .HasMaxLength(200);

        builder.Property(p => p.Warranty)
            .HasMaxLength(200);

        builder.HasIndex(p => new { p.CompanyId, p.SKU }).IsUnique();

        builder.HasOne(p => p.Company)
            .WithMany()
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.CreatedByUser)
            .WithMany()
            .HasForeignKey(p => p.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
