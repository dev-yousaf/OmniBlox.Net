using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Warehouses.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Warehouses.Commands;

public record UpdateWarehouseCommand : IRequest<WarehouseDto>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Location { get; init; }
    public string? Status { get; init; }
}

public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, WarehouseDto>
{
    private readonly IApplicationDbContext _context;
    public UpdateWarehouseCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<WarehouseDto> Handle(UpdateWarehouseCommand request, CancellationToken ct)
    {
        var entity = await _context.Warehouses.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(Warehouse), request.Id);

        if (request.Name is not null)
        {
            var exists = await _context.Warehouses.AnyAsync(x => x.Name == request.Name && x.Id != request.Id, ct);
            if (exists) throw new ConflictException($"Warehouse with name '{request.Name}' already exists.");
            entity.Name = request.Name;
        }
        if (request.Location is not null) entity.Location = request.Location;
        if (request.Status is not null && Enum.TryParse<ActiveStatus>(request.Status, true, out var s)) entity.Status = s;

        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return WarehouseDto.FromEntity(entity);
    }
}

public class UpdateWarehouseCommandValidator : AbstractValidator<UpdateWarehouseCommand>
{
    public UpdateWarehouseCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Name).MaximumLength(200).When(v => v.Name is not null);
    }
}
