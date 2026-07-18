using FluentValidation;
using MediatR;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Suppliers.DTOs;
using OmniBlox.Domain.Entities;

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
    private readonly ICrudService<Supplier, SupplierDto> _crud;

    public UpdateSupplierCommandHandler(ICrudService<Supplier, SupplierDto> crud)
    {
        _crud = crud;
    }

    public async Task<SupplierDto> Handle(UpdateSupplierCommand request, CancellationToken ct)
    {
        return await _crud.UpdateAsync(request.Id, supplier =>
        {
            if (request.Name is not null) supplier.Name = request.Name;
            if (request.Email is not null) supplier.Email = request.Email;
            if (request.Phone is not null) supplier.Phone = request.Phone;
            if (request.Address is not null) supplier.Address = request.Address;
        }, SupplierDto.FromEntity, ct);
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
