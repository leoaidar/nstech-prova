using OrderService.Domain.Enums;

namespace OrderService.Domain.Entities;

public class Order
{
  public Guid Id { get; private set; }
  public Guid CustomerId { get; private set; }
  public OrderStatus Status { get; private set; }
  public string Currency { get; private set; } = string.Empty;
  public decimal Total { get; private set; }
  public DateTime CreatedAt { get; private set; }

  private readonly List<OrderItem> _items = new();
  public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

  // Construtor protegido para uso do ORM(EF)
  protected Order() { }
}