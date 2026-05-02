using MediatR;

namespace OrderService.Application.UseCases.Orders.Commands.CancelOrder;

public record CancelOrderCommand(Guid OrderId) : IRequest<bool>;