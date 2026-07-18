using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Warranties.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;
using OmniBlox.Shared.Extensions;

namespace OmniBlox.Application.Features.Warranties.Commands;

public record UpdateWarrantyCommand : IRequest<WarrantyDto>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public int? Duration { get; init; }
    public string? DurationType { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public class UpdateWarrantyCommandHandler : IRequestHandler<UpdateWarrantyCommand, WarrantyDto>
{
    private readonly IApplicationDbContext _context;
    public UpdateWarrantyCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<WarrantyDto> Handle(UpdateWarrantyCommand request, CancellationToken ct)
    {
        var entity = await _context.Warranties.AsTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(Warranty), request.Id);

        if (request.Name is not null) entity.Name = request.Name;
        if (request.Duration.HasValue) entity.Duration = request.Duration.Value;
        if (request.DurationType is not null) entity.DurationType = request.DurationType;
        if (request.Description is not null) entity.Description = request.Description;
        if (request.Status is not null) entity.Status = request.Status.ToEnumOrDefault(entity.Status);

        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return WarrantyDto.FromEntity(entity);
    }
}

public class UpdateWarrantyCommandValidator : AbstractValidator<UpdateWarrantyCommand>
{
    public UpdateWarrantyCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Name).MaximumLength(200).When(v => v.Name is not null);
        RuleFor(v => v.Duration).GreaterThan(0).When(v => v.Duration.HasValue);
    }
}
