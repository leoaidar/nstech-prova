namespace OrderService.Application.DTOs;

public record OrderItemDto(Guid ProductId, int Quantity);