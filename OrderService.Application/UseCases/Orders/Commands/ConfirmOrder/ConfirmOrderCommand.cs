using MediatR;

namespace OrderService.Application.UseCases.Orders.Commands.ConfirmOrder;

public record ConfirmOrderCommand(Guid OrderId) : IRequest<bool>;