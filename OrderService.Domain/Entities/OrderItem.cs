using OrderService.Domain.Constants;
using OrderService.Domain.Exceptions;

namespace OrderService.Domain.Entities;

public class OrderItem
{
  public Guid ProductId { get; private set; }
  public decimal UnitPrice { get; private set; }
  public int Quantity { get; private set; }

  protected OrderItem() { }

  public OrderItem(Guid productId, decimal unitPrice, int quantity)
  {
    if (quantity <= 0) throw new DomainException(DomainErrors.OrderItem.InvalidItemQuantity);
    if (unitPrice <= 0) throw new DomainException(DomainErrors.OrderItem.InvalidItemPrice);

    ProductId = productId;
    UnitPrice = unitPrice;
    Quantity = quantity;
  }
}