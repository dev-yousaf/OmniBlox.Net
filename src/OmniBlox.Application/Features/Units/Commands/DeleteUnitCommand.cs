using MediatR;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Units.DTOs;

namespace OmniBlox.Application.Features.Units.Commands;

public record DeleteUnitCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteUnitCommandHandler : IRequestHandler<DeleteUnitCommand>
{
    private readonly ICrudService<Domain.Entities.Unit, UnitDto> _crud;
    public DeleteUnitCommandHandler(ICrudService<Domain.Entities.Unit, UnitDto> crud) => _crud = crud;

    public async Task Handle(DeleteUnitCommand request, CancellationToken ct)
    {
        await _crud.DeleteAsync(request.Id, ct);
    }
}
