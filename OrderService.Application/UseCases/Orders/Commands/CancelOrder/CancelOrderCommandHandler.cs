using MediatR;
using OrderService.Domain.Constants;
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
    if (order == null) throw new DomainException(DomainErrors.Order.NotFound);

    // Idempotência
    if (order.Status == OrderStatus.Canceled)
      return true;

    // Guarda o status anterior pra saber se precisa devolver o estoque
    var previousStatus = order.Status;

    order.Cancel();

    // Se tava confirmado, o estoque já havia sido baixado. Precisa repor.
    bool needsStockReplenishment = previousStatus == OrderStatus.Confirmed;
    if (needsStockReplenishment)
    {
      // Marca cada produto com estoque reposto
      foreach (var item in order.Items)
      {
        var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
        if (product != null)
        {
          product.IncreaseStock(item.Quantity);
          await _productRepository.UpdateAsync(product, cancellationToken);
        }
      }
    }

    // Marca o Pedido com novo status 
    await _orderRepository.UpdateAsync(order, cancellationToken);

    // Confirmação única — cancelamento de pedido + reposição de estoque em uma transação só
    await _orderRepository.SaveChangesAsync(cancellationToken);

    return true;
  }
}