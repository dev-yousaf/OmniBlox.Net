using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Interfaces;

namespace OmniBlox.Infrastructure.Persistence;

public class AppDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUser;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<SubCategory> SubCategories => Set<SubCategory>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Warranty> Warranties => Set<Warranty>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
    public DbSet<StockAdjustmentItem> StockAdjustmentItems => Set<StockAdjustmentItem>();

    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Biller> Billers => Set<Biller>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<SalesReturn> SalesReturns => Set<SalesReturn>();
    public DbSet<SalesReturnItem> SalesReturnItems => Set<SalesReturnItem>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<PurchaseReturn> PurchaseReturns => Set<PurchaseReturn>();
    public DbSet<PurchaseReturnItem> PurchaseReturnItems => Set<PurchaseReturnItem>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationItem> QuotationItems => Set<QuotationItem>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ExpenseAttachment> ExpenseAttachments => Set<ExpenseAttachment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    DbSet<TEntity> IApplicationDbContext.Set<TEntity>() => Set<TEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    private static string GetEntityName(Type type)
    {
        var name = type.Name;
        if (name.EndsWith("Item") && type.Namespace?.Contains("Domain.Entities") == true)
            return name;
        return name;
    }

    private static string? GetEntityId(EntityEntry entry)
    {
        if (entry.Entity is BaseEntity baseEntity)
            return baseEntity.Id.ToString();
        if (entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id")?.CurrentValue is Guid guid)
            return guid.ToString();
        return null;
    }

    private static string? GetEntitySummary(EntityEntry entry)
    {
        try
        {
            var props = entry.Properties;
            var parts = new List<string>();

            var name = props.FirstOrDefault(p => p.Metadata.Name is "Name" or "Title" or "InvoiceNumber" or "ReferenceNumber" or "SKU");
            if (name?.CurrentValue is string s && !string.IsNullOrEmpty(s))
                parts.Add(s);

            var email = props.FirstOrDefault(p => p.Metadata.Name == "Email");
            if (email?.CurrentValue is string e && !string.IsNullOrEmpty(e))
                parts.Add($"({e})");

            return parts.Count > 0 ? string.Join(" ", parts) : null;
        }
        catch
        {
            return null;
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var userId = _currentUser?.UserId;
        var companyId = _currentUser?.CompanyId;

        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.CompanyId == Guid.Empty && companyId.HasValue)
            {
                entry.Entity.CompanyId = companyId.Value;
            }
        }

        var auditEntries = ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
                        && e.Entity is not AuditLog
                        && e.Entity.GetType().Namespace?.Contains("Domain.Entities") == true)
            .Select(e => new
            {
                e.State,
                EntityName = GetEntityName(e.Entity.GetType()),
                EntityId = GetEntityId(e),
                Summary = GetEntitySummary(e),
            })
            .ToList();

        var result = await base.SaveChangesAsync(ct);

        if (auditEntries.Count > 0 && userId.HasValue && companyId.HasValue)
        {
            var logs = auditEntries.Select(a => new AuditLog
            {
                UserId = userId.Value,
                CompanyId = companyId.Value,
                Action = a.State switch
                {
                    EntityState.Added => "CREATED",
                    EntityState.Modified => "UPDATED",
                    EntityState.Deleted => "DELETED",
                    _ => "UNKNOWN",
                },
                Entity = a.EntityName,
                EntityId = a.EntityId,
                Details = a.Summary,
            }).ToList();

            AuditLogs.AddRange(logs);
            await base.SaveChangesAsync(ct);
        }

        return result;
    }
}
