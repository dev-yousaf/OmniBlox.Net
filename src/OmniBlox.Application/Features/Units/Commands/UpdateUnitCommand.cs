using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Units.DTOs;
using OmniBlox.Shared.Exceptions;
using OmniBlox.Shared.Extensions;

namespace OmniBlox.Application.Features.Units.Commands;

public record UpdateUnitCommand : IRequest<UnitDto>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? ShortName { get; init; }
    public string? Slug { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public class UpdateUnitCommandHandler : IRequestHandler<UpdateUnitCommand, UnitDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICrudService<Domain.Entities.Unit, UnitDto> _crud;
    public UpdateUnitCommandHandler(IApplicationDbContext context, ICrudService<Domain.Entities.Unit, UnitDto> crud)
    {
        _context = context;
        _crud = crud;
    }

    public async Task<UnitDto> Handle(UpdateUnitCommand request, CancellationToken ct)
    {
        var slug = request.Slug?.ToLowerInvariant();
        if (slug is not null)
        {
            var exists = await _context.Units.AnyAsync(x => x.Slug == slug && x.Id != request.Id, ct);
            if (exists) throw new ConflictException($"Unit with slug '{slug}' already exists.");
        }

        return await _crud.UpdateAsync(request.Id, entity =>
        {
            if (request.Name is not null) entity.Name = request.Name;
            if (request.ShortName is not null) entity.ShortName = request.ShortName;
            if (request.Description is not null) entity.Description = request.Description;
            if (request.Status is not null) entity.Status = request.Status.ToEnumOrDefault(entity.Status);
            if (slug is not null) entity.Slug = slug;
        }, UnitDto.FromEntity, ct);
    }
}

public class UpdateUnitCommandValidator : AbstractValidator<UpdateUnitCommand>
{
    public UpdateUnitCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Name).MaximumLength(100).When(v => v.Name is not null);
        RuleFor(v => v.ShortName).MaximumLength(20).When(v => v.ShortName is not null);
    }
}
