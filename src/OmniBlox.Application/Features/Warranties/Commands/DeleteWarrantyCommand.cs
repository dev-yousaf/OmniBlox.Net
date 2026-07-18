using MediatR;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Warranties.DTOs;
using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.Warranties.Commands;

public record DeleteWarrantyCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteWarrantyCommandHandler : IRequestHandler<DeleteWarrantyCommand>
{
    private readonly ICrudService<Warranty, WarrantyDto> _crud;
    public DeleteWarrantyCommandHandler(ICrudService<Warranty, WarrantyDto> crud) => _crud = crud;

    public async Task Handle(DeleteWarrantyCommand request, CancellationToken ct)
    {
        await _crud.DeleteAsync(request.Id, ct);
    }
}
