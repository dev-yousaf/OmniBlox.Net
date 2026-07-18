using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Warehouses.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;
using OmniBlox.Shared.Extensions;

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
    private readonly ICrudService<Warehouse, WarehouseDto> _crud;
    public UpdateWarehouseCommandHandler(IApplicationDbContext context, ICrudService<Warehouse, WarehouseDto> crud)
    {
        _context = context;
        _crud = crud;
    }

    public async Task<WarehouseDto> Handle(UpdateWarehouseCommand request, CancellationToken ct)
    {
        if (request.Name is not null)
        {
            var exists = await _context.Warehouses.AnyAsync(x => x.Name == request.Name && x.Id != request.Id, ct);
            if (exists) throw new ConflictException($"Warehouse with name '{request.Name}' already exists.");
        }

        return await _crud.UpdateAsync(request.Id, entity =>
        {
            if (request.Name is not null) entity.Name = request.Name;
            if (request.Location is not null) entity.Location = request.Location;
            if (request.Status is not null) entity.Status = request.Status.ToEnumOrDefault(entity.Status);
        }, WarehouseDto.FromEntity, ct);
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
