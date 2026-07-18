using MediatR;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Billers.DTOs;
using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.Billers.Commands;

public record DeleteBillerCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteBillerCommandHandler : IRequestHandler<DeleteBillerCommand>
{
    private readonly ICrudService<Biller, BillerDto> _crud;
    public DeleteBillerCommandHandler(ICrudService<Biller, BillerDto> crud) => _crud = crud;

    public async Task Handle(DeleteBillerCommand request, CancellationToken ct)
    {
        await _crud.DeleteAsync(request.Id, ct);
    }
}
