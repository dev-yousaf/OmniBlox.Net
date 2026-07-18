using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Customers.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Customers.Commands;

public record UpdateCustomerCommand : IRequest<CustomerDto>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
}

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken ct)
    {
        var customer = await _context.Customers
            .AsTracking().FirstOrDefaultAsync(c => c.Id == request.Id, ct);

        if (customer is null)
            throw new NotFoundException(nameof(Customer), request.Id);

        if (request.Name is not null) customer.Name = request.Name;
        if (request.Email is not null) customer.Email = request.Email;
        if (request.Phone is not null) customer.Phone = request.Phone;
        if (request.Address is not null) customer.Address = request.Address;

        customer.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return CustomerDto.FromEntity(customer);
    }
}

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();

        RuleFor(v => v.Name)
            .MaximumLength(200).When(v => v.Name is not null);

        RuleFor(v => v.Email)
            .EmailAddress().When(v => v.Email is not null)
            .MaximumLength(200);

        RuleFor(v => v.Phone)
            .MaximumLength(50).When(v => v.Phone is not null);

        RuleFor(v => v.Address)
            .MaximumLength(500).When(v => v.Address is not null);
    }
}
