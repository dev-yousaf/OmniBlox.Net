using FluentValidation;
using MediatR;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Customers.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;

namespace OmniBlox.Application.Features.Customers.Commands;

public record CreateCustomerCommand : IRequest<CustomerDto>
{
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly ICrudService<Customer, CustomerDto> _crud;

    public CreateCustomerCommandHandler(ICrudService<Customer, CustomerDto> crud)
    {
        _crud = crud;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        var customer = new Customer
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            Status = ActiveStatus.ACTIVE,
        };

        return await _crud.CreateAsync(customer, CustomerDto.FromEntity, ct);
    }
}

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(200);

        RuleFor(v => v.Email)
            .EmailAddress().When(v => v.Email is not null)
            .MaximumLength(200);

        RuleFor(v => v.Phone)
            .MaximumLength(50);

        RuleFor(v => v.Address)
            .MaximumLength(500);
    }
}
