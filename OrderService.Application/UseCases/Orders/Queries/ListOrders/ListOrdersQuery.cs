using MediatR;
using OrderService.Application.DTOs;
using OrderService.Domain.Enums;

namespace OrderService.Application.UseCases.Orders.Queries.ListOrders;

public record ListOrdersQuery(
    Guid? CustomerId,
    OrderStatus? Status,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 10) : IRequest<PaginatedResult<OrderResponseDto>>;