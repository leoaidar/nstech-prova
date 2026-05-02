namespace OrderService.Application.DTOs;

public record OrderItemResponseDto(Guid ProductId, decimal UnitPrice, int Quantity);

public record OrderResponseDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    string Currency,
    decimal Total,
    DateTime CreatedAt,
    List<OrderItemResponseDto> Items);