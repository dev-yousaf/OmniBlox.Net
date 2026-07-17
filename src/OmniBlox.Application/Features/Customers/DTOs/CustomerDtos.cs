namespace OmniBlox.Application.Features.Customers.DTOs;

public record CustomerDto
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

    public static CustomerDto FromEntity(Domain.Entities.Customer c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Email = c.Email,
        Phone = c.Phone,
        Address = c.Address,
        Status = c.Status.ToString(),
        CreatedAt = c.CreatedAt,
        CreditLimit = c.CreditLimit,
        Balance = c.Balance,
    };
}

public record CustomerListResponse
{
    public List<CustomerDto> Customers { get; init; } = [];
    public int Total { get; init; }
    public int Pages { get; init; }
}

public record CreateCustomerRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
}

public record UpdateCustomerRequest : CreateCustomerRequest
{
}
