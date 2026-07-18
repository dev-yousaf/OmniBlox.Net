using MediatR;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Customers.DTOs;
using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.Customers.Commands;

public record DeleteCustomerCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand>
{
    private readonly ICrudService<Customer, CustomerDto> _crud;

    public DeleteCustomerCommandHandler(ICrudService<Customer, CustomerDto> crud)
    {
        _crud = crud;
    }

    public async Task Handle(DeleteCustomerCommand request, CancellationToken ct)
    {
        await _crud.DeleteAsync(request.Id, ct);
    }
}
