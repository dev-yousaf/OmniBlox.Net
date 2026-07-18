using FluentValidation;
using MediatR;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Billers.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;
using OmniBlox.Shared.Extensions;

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
    private readonly ICrudService<Biller, BillerDto> _crud;
    public CreateBillerCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, ICrudService<Biller, BillerDto> crud)
    {
        _context = context;
        _currentUser = currentUser;
        _crud = crud;
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
            Status = request.Status.ToEnumOrDefault(ActiveStatus.ACTIVE),
            CompanyId = companyId,
        };

        return await _crud.CreateAsync(entity, BillerDto.FromEntity, ct);
    }
}

public class CreateBillerCommandValidator : AbstractValidator<CreateBillerCommand>
{
    public CreateBillerCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
    }
}
