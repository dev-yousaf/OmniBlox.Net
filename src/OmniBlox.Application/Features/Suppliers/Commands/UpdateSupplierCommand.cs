using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Suppliers.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Suppliers.Commands;

public record UpdateSupplierCommand : IRequest<SupplierDto>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
}

public class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, SupplierDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateSupplierCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SupplierDto> Handle(UpdateSupplierCommand request, CancellationToken ct)
    {
        var supplier = await _context.Suppliers
            .AsTracking().FirstOrDefaultAsync(s => s.Id == request.Id, ct);

        if (supplier is null)
            throw new NotFoundException(nameof(Supplier), request.Id);

        if (request.Name is not null) supplier.Name = request.Name;
        if (request.Email is not null) supplier.Email = request.Email;
        if (request.Phone is not null) supplier.Phone = request.Phone;
        if (request.Address is not null) supplier.Address = request.Address;

        supplier.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return SupplierDto.FromEntity(supplier);
    }
}

public class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();

        RuleFor(v => v.Name)
            .MaximumLength(200).When(v => v.Name is not null);

        RuleFor(v => v.Email)
            .EmailAddress().When(v => v.Email is not null)
            .MaximumLength(200);

        RuleFor(v => v.Phone)
            .MaximumLength(50).When(v => v.Phone is not null);

        RuleFor(v => v.Address)
            .MaximumLength(500).When(v => v.Address is not null);
    }
}
