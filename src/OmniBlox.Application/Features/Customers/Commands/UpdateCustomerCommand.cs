using FluentValidation;
using MediatR;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Customers.DTOs;
using OmniBlox.Domain.Entities;

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
    private readonly ICrudService<Customer, CustomerDto> _crud;

    public UpdateCustomerCommandHandler(ICrudService<Customer, CustomerDto> crud)
    {
        _crud = crud;
    }

    public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken ct)
    {
        return await _crud.UpdateAsync(request.Id, customer =>
        {
            if (request.Name is not null) customer.Name = request.Name;
            if (request.Email is not null) customer.Email = request.Email;
            if (request.Phone is not null) customer.Phone = request.Phone;
            if (request.Address is not null) customer.Address = request.Address;
        }, CustomerDto.FromEntity, ct);
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
