using OrderService.Domain.Constants;
using OrderService.Domain.Exceptions;

namespace OrderService.Domain.Entities;

public class Product
{
  public Guid Id { get; private set; }
  public decimal UnitPrice { get; private set; }
  public int AvailableQuantity { get; private set; }

  protected Product() { }

  public Product(Guid id, decimal unitPrice, int initialQuantity)
  {
    if (unitPrice <= 0) throw new DomainException(DomainErrors.Product.InvalidPrice);
    if (initialQuantity < 0) throw new DomainException(DomainErrors.Product.InvalidInitialQuantity);

    Id = id;
    UnitPrice = unitPrice;
    AvailableQuantity = initialQuantity;
  }

  public void DecreaseStock(int quantity)
  {
    if (quantity <= 0) throw new DomainException(DomainErrors.Product.InvalidDecreaseQuantity);
    if (AvailableQuantity < quantity) throw new DomainException(DomainErrors.Product.InsufficientStock(AvailableQuantity));

    AvailableQuantity -= quantity;
  }

  public void IncreaseStock(int quantity)
  {
    if (quantity <= 0) throw new DomainException(DomainErrors.Product.InvalidIncreaseQuantity);
    AvailableQuantity += quantity;
  }
}