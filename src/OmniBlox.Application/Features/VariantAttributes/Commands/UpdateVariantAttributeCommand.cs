using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.VariantAttributes.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.VariantAttributes.Commands;

public record UpdateVariantAttributeCommand : IRequest<VariantAttributeDto>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Slug { get; init; }
    public JsonDocument? Values { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public class UpdateVariantAttributeCommandHandler : IRequestHandler<UpdateVariantAttributeCommand, VariantAttributeDto>
{
    private readonly IApplicationDbContext _context;
    public UpdateVariantAttributeCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<VariantAttributeDto> Handle(UpdateVariantAttributeCommand request, CancellationToken ct)
    {
        var entity = await _context.VariantAttributes.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(VariantAttribute), request.Id);

        if (request.Name is not null) entity.Name = request.Name;
        if (request.Values is not null) entity.Values = request.Values;
        if (request.Description is not null) entity.Description = request.Description;
        if (request.Status is not null && Enum.TryParse<ActiveStatus>(request.Status, true, out var s)) entity.Status = s;

        if (request.Slug is not null)
        {
            var slug = request.Slug.ToLowerInvariant();
            var exists = await _context.VariantAttributes.AnyAsync(x => x.Slug == slug && x.Id != request.Id, ct);
            if (exists) throw new ConflictException($"Variant attribute with slug '{slug}' already exists.");
            entity.Slug = slug;
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return VariantAttributeDto.FromEntity(entity);
    }
}

public class UpdateVariantAttributeCommandValidator : AbstractValidator<UpdateVariantAttributeCommand>
{
    public UpdateVariantAttributeCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Name).MaximumLength(200).When(v => v.Name is not null);
    }
}
