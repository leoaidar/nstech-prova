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
    if (unitPrice <= 0) throw new DomainException("Preço deve ser maior que zero.");
    if (initialQuantity < 0) throw new DomainException("Quantidade inicial não pode ser negativa.");

    Id = id;
    UnitPrice = unitPrice;
    AvailableQuantity = initialQuantity;
  }

  public void DecreaseStock(int quantity)
  {
    if (quantity <= 0) throw new DomainException("A quantidade a ser baixada deve ser maior que zero.");
    if (AvailableQuantity < quantity) throw new DomainException($"Estoque insuficiente. Disponível: {AvailableQuantity}");

    AvailableQuantity -= quantity;
  }

  public void IncreaseStock(int quantity)
  {
    if (quantity <= 0) throw new DomainException("A quantidade a ser reposta deve ser maior que zero.");
    AvailableQuantity += quantity;
  }
}