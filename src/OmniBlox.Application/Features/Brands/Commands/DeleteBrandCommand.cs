using MediatR;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Brands.DTOs;
using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.Brands.Commands;

public record DeleteBrandCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteBrandCommandHandler : IRequestHandler<DeleteBrandCommand>
{
    private readonly ICrudService<Brand, BrandDto> _crud;
    public DeleteBrandCommandHandler(ICrudService<Brand, BrandDto> crud) => _crud = crud;

    public async Task Handle(DeleteBrandCommand request, CancellationToken ct)
    {
        await _crud.DeleteAsync(request.Id, ct);
    }
}
