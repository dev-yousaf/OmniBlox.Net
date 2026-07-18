using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Billers.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;
using OmniBlox.Shared.Extensions;

namespace OmniBlox.Application.Features.Billers.Commands;

public record UpdateBillerCommand : IRequest<BillerDto>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? Status { get; init; }
}

public class UpdateBillerCommandHandler : IRequestHandler<UpdateBillerCommand, BillerDto>
{
    private readonly IApplicationDbContext _context;
    public UpdateBillerCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<BillerDto> Handle(UpdateBillerCommand request, CancellationToken ct)
    {
        var entity = await _context.Billers.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(Biller), request.Id);

        if (request.Name is not null) entity.Name = request.Name;
        if (request.Email is not null) entity.Email = request.Email;
        if (request.Phone is not null) entity.Phone = request.Phone;
        if (request.Address is not null) entity.Address = request.Address;
        if (request.Status is not null) entity.Status = request.Status.ToEnumOrDefault(entity.Status);

        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return BillerDto.FromEntity(entity);
    }
}

public class UpdateBillerCommandValidator : AbstractValidator<UpdateBillerCommand>
{
    public UpdateBillerCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Name).MaximumLength(200).When(v => v.Name is not null);
    }
}
