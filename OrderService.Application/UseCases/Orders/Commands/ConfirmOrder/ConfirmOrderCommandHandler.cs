using MediatR;
using OrderService.Domain.Constants;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Repositories;

namespace OrderService.Application.UseCases.Orders.Commands.ConfirmOrder;

public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, bool>
{
  private readonly IOrderRepository _orderRepository;
  private readonly IProductRepository _productRepository;

  public ConfirmOrderCommandHandler(IOrderRepository orderRepository, IProductRepository productRepository)
  {
    _orderRepository = orderRepository;
    _productRepository = productRepository;
  }

  public async Task<bool> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
  {
    var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
    if (order == null) throw new DomainException(DomainErrors.Order.NotFound);

    // Verificação de Idempotência na camada de Aplicação
    if (order.Status == OrderStatus.Confirmed)
      return true; // Já foi processado anteriormente, retorna sucesso

    // Confirma o pedido no domínio
    order.Confirm();

    // Baixa o estoque de cada item (requisito pede isso no Confirm)
    foreach (var item in order.Items)
    {
      var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
      if (product == null) throw new DomainException(DomainErrors.Product.NotFound(item.ProductId));

      product.DecreaseStock(item.Quantity);
      await _productRepository.UpdateAsync(product, cancellationToken);
    }

    await _orderRepository.UpdateAsync(order, cancellationToken);
    return true;
  }
}