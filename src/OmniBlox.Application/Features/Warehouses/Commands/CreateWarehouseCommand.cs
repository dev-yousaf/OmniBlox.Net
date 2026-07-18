using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Warehouses.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Warehouses.Commands;

public record CreateWarehouseCommand : IRequest<WarehouseDto>
{
    public string Name { get; init; } = string.Empty;
    public string? Location { get; init; }
}

public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, WarehouseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public CreateWarehouseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<WarehouseDto> Handle(CreateWarehouseCommand request, CancellationToken ct)
    {
        var companyId = _currentUser.CompanyId;
        var exists = await _context.Warehouses.AnyAsync(x => x.CompanyId == companyId && x.Name == request.Name, ct);
        if (exists) throw new ConflictException($"Warehouse with name '{request.Name}' already exists.");

        var entity = new Warehouse
        {
            Name = request.Name,
            Location = request.Location,
            CompanyId = companyId,
        };

        _context.Warehouses.Add(entity);
        await _context.SaveChangesAsync(ct);
        return WarehouseDto.FromEntity(entity);
    }
}

public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Location).MaximumLength(500);
    }
}
