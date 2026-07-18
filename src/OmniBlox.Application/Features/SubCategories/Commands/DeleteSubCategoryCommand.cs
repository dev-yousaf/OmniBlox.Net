using MediatR;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.SubCategories.DTOs;
using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.SubCategories.Commands;

public record DeleteSubCategoryCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteSubCategoryCommandHandler : IRequestHandler<DeleteSubCategoryCommand>
{
    private readonly ICrudService<SubCategory, SubCategoryDto> _crud;
    public DeleteSubCategoryCommandHandler(ICrudService<SubCategory, SubCategoryDto> crud) => _crud = crud;

    public async Task Handle(DeleteSubCategoryCommand request, CancellationToken ct)
    {
        await _crud.DeleteAsync(request.Id, ct);
    }
}
