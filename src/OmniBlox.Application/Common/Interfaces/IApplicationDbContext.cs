using Microsoft.EntityFrameworkCore;
using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Company> Companies { get; }
    DbSet<User> Users { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<SubCategory> SubCategories { get; }
    DbSet<Brand> Brands { get; }
    DbSet<Unit> Units { get; }
    DbSet<Warranty> Warranties { get; }
    DbSet<VariantAttribute> VariantAttributes { get; }
    DbSet<StockLedgerEntry> StockLedgerEntries { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
