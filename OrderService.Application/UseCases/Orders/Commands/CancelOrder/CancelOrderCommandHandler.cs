using MediatR;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Repositories;

namespace OrderService.Application.UseCases.Orders.Commands.CancelOrder;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, bool>
{
  private readonly IOrderRepository _orderRepository;
  private readonly IProductRepository _productRepository;

  public CancelOrderCommandHandler(IOrderRepository orderRepository, IProductRepository productRepository)
  {
    _orderRepository = orderRepository;
    _productRepository = productRepository;
  }

  public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
  {
    var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
    if (order == null) throw new DomainException("Pedido não encontrado.");

    // Idempotência
    if (order.Status == OrderStatus.Canceled)
      return true;

    // Guarda o status anterior pra saber se precisa devolver o estoque
    var previousStatus = order.Status;

    order.Cancel();

    // Se tava confirmado, o estoque já havia sido baixado. Precisa repor.
    bool needsStockReplenishment = previousStatus == OrderStatus.Confirmed;
    if (needsStockReplenishment)
      foreach (var item in order.Items)
      {
        var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
        if (product != null)
        {
          product.IncreaseStock(item.Quantity);
          await _productRepository.UpdateAsync(product, cancellationToken);
        }
      }

    await _orderRepository.UpdateAsync(order, cancellationToken);
    return true;
  }
}