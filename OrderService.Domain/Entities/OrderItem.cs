using OrderService.Domain.Exceptions;

namespace OrderService.Domain.Entities;

public class OrderItem
{
  public Guid ProductId { get; private set; }
  public decimal UnitPrice { get; private set; }
  public int Quantity { get; private set; }

  protected OrderItem() { }
}