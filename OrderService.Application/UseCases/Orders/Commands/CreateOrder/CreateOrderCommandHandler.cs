using MediatR;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Repositories;

namespace OrderService.Application.UseCases.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
  private readonly IOrderRepository _orderRepository;
  private readonly IProductRepository _productRepository;

  public CreateOrderCommandHandler(IOrderRepository orderRepository, IProductRepository productRepository)
  {
    _orderRepository = orderRepository;
    _productRepository = productRepository;
  }

  public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
  {
    if (request.Items == null || !request.Items.Any())
      throw new DomainException(DomainErrors.Order.CannotCreateOrderWithoutItems);

    // Inicializa o pedido
    var order = new Order(request.CustomerId, request.Currency);

    // Adiciona os itens validando as regras do requisito
    foreach (var itemDto in request.Items)
    {
      var product = await _productRepository.GetByIdAsync(itemDto.ProductId, cancellationToken);

      if (product == null)
        throw new DomainException(DomainErrors.Product.NotFound(itemDto.ProductId));

      if (product.AvailableQuantity < itemDto.Quantity)
        throw new DomainException(DomainErrors.Product.InsufficientStockForOrder(itemDto.ProductId, product.AvailableQuantity));

      // Pega o UnitPrice direto da base de dados, garantindo a integridade conforme requisito
      order.AddItem(product.Id, product.UnitPrice, itemDto.Quantity);
    }

    // Muda o status para Placed (conforme o requisito: nasce como Draft -> Placed)
    order.PlaceOrder();

    // Salva no banco de dados
    await _orderRepository.AddAsync(order, cancellationToken);

    return order.Id;
  }
}