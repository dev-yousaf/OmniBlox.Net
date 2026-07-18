using FluentValidation;
using MediatR;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Billers.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Billers.Commands;

public record CreateBillerCommand : IRequest<BillerDto>
{
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? Status { get; init; }
}

public class CreateBillerCommandHandler : IRequestHandler<CreateBillerCommand, BillerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public CreateBillerCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<BillerDto> Handle(CreateBillerCommand request, CancellationToken ct)
    {
        var companyId = _currentUser.CompanyId;

        var entity = new Biller
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            Status = request.Status is not null && Enum.TryParse<ActiveStatus>(request.Status, true, out var s) ? s : ActiveStatus.ACTIVE,
            CompanyId = companyId,
        };

        _context.Billers.Add(entity);
        await _context.SaveChangesAsync(ct);
        return BillerDto.FromEntity(entity);
    }
}

public class CreateBillerCommandValidator : AbstractValidator<CreateBillerCommand>
{
    public CreateBillerCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
    }
}
