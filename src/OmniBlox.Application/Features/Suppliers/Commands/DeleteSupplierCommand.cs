using MediatR;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Suppliers.DTOs;
using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.Suppliers.Commands;

public record DeleteSupplierCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand>
{
    private readonly ICrudService<Supplier, SupplierDto> _crud;

    public DeleteSupplierCommandHandler(ICrudService<Supplier, SupplierDto> crud)
    {
        _crud = crud;
    }

    public async Task Handle(DeleteSupplierCommand request, CancellationToken ct)
    {
        await _crud.DeleteAsync(request.Id, ct);
    }
}
