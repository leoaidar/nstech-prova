using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.UseCases.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    Guid CustomerId, 
    string Currency, 
    List<OrderItemDto> Items) : IRequest<Guid>;