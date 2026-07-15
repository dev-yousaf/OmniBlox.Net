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

public record CreateVariantAttributeCommand : IRequest<VariantAttributeDto>
{
    public string Name { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public JsonDocument? Values { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public class CreateVariantAttributeCommandHandler : IRequestHandler<CreateVariantAttributeCommand, VariantAttributeDto>
{
    private readonly IApplicationDbContext _context;
    public CreateVariantAttributeCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<VariantAttributeDto> Handle(CreateVariantAttributeCommand request, CancellationToken ct)
    {
        var slug = request.Slug?.ToLowerInvariant() ?? request.Name.ToLowerInvariant().Replace(" ", "-");
        var exists = await _context.VariantAttributes.AnyAsync(x => x.Slug == slug, ct);
        if (exists) throw new ConflictException($"Variant attribute with slug '{slug}' already exists.");

        var entity = new VariantAttribute
        {
            Name = request.Name,
            Slug = slug,
            Values = request.Values,
            Description = request.Description,
            Status = request.Status is not null && Enum.TryParse<ActiveStatus>(request.Status, true, out var s) ? s : ActiveStatus.ACTIVE,
        };

        _context.VariantAttributes.Add(entity);
        await _context.SaveChangesAsync(ct);
        return VariantAttributeDto.FromEntity(entity);
    }
}

public class CreateVariantAttributeCommandValidator : AbstractValidator<CreateVariantAttributeCommand>
{
    public CreateVariantAttributeCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
    }
}
