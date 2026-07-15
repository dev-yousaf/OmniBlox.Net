using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Warranties.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Warranties.Commands;

public record CreateWarrantyCommand : IRequest<WarrantyDto>
{
    public string Name { get; init; } = string.Empty;
    public int Duration { get; init; }
    public string? DurationType { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public class CreateWarrantyCommandHandler : IRequestHandler<CreateWarrantyCommand, WarrantyDto>
{
    private readonly IApplicationDbContext _context;
    public CreateWarrantyCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<WarrantyDto> Handle(CreateWarrantyCommand request, CancellationToken ct)
    {
        var exists = await _context.Warranties.AnyAsync(x => x.Name == request.Name, ct);
        if (exists) throw new ConflictException($"Warranty with name '{request.Name}' already exists.");

        var entity = new Warranty
        {
            Name = request.Name,
            Duration = request.Duration,
            DurationType = request.DurationType ?? "days",
            Description = request.Description,
            Status = request.Status is not null && Enum.TryParse<ActiveStatus>(request.Status, true, out var s) ? s : ActiveStatus.ACTIVE,
        };

        _context.Warranties.Add(entity);
        await _context.SaveChangesAsync(ct);
        return WarrantyDto.FromEntity(entity);
    }
}

public class CreateWarrantyCommandValidator : AbstractValidator<CreateWarrantyCommand>
{
    public CreateWarrantyCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Duration).GreaterThan(0);
        RuleFor(v => v.DurationType).MaximumLength(20);
    }
}
