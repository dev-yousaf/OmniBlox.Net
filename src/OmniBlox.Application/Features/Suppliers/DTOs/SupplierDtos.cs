namespace OmniBlox.Application.Features.Suppliers.DTOs;

public record SupplierDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public decimal? CreditLimit { get; init; }
    public decimal? Balance { get; init; }

    public static SupplierDto FromEntity(Domain.Entities.Supplier s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Email = s.Email,
        Phone = s.Phone,
        Address = s.Address,
        Status = s.Status.ToString(),
        CreatedAt = s.CreatedAt,
        CreditLimit = s.CreditLimit,
        Balance = s.Balance,
    };
}

public record SupplierListResponse
{
    public List<SupplierDto> Suppliers { get; init; } = [];
    public int Total { get; init; }
    public int Pages { get; init; }
}

public record CreateSupplierRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
}

public record UpdateSupplierRequest : CreateSupplierRequest
{
}
