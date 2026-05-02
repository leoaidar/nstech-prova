using MediatR;
using OrderService.Application.DTOs;
using OrderService.Domain.Repositories;

namespace OrderService.Application.UseCases.Orders.Queries.ListOrders;

public class ListOrdersQueryHandler : IRequestHandler<ListOrdersQuery, PaginatedResult<OrderResponseDto>>
{
    private readonly IOrderRepository _orderRepository;

    public ListOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PaginatedResult<OrderResponseDto>> Handle(ListOrdersQuery request, CancellationToken cancellationToken)
    {
        var (orders, totalCount) = await _orderRepository.GetPagedAsync(
            request.CustomerId, request.Status, request.From, request.To, request.Page, request.PageSize, cancellationToken);

        var itemsDto = orders.Select(order => new OrderResponseDto(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.Currency,
            order.Total,
            order.CreatedAt,
            order.Items.Select(i => new OrderItemResponseDto(i.ProductId, i.UnitPrice, i.Quantity)).ToList()
        )).ToList();

        return new PaginatedResult<OrderResponseDto>(itemsDto, totalCount, request.Page, request.PageSize);
    }
}