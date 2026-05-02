using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.UseCases.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderResponseDto?>;