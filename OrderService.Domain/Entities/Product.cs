using OrderService.Domain.Exceptions;

namespace OrderService.Domain.Entities;

public class Product
{
  public Guid Id { get; private set; }
  public decimal UnitPrice { get; private set; }
  public int AvailableQuantity { get; private set; }

  protected Product() { }
}