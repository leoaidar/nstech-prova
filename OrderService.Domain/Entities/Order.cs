using OrderService.Domain.Constants;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;

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

  // Constructor protegido para uso do ORM(EF)
  protected Order() { }

  public Order(Guid customerId, string currency)
  {
    if (customerId == Guid.Empty) throw new DomainException(DomainErrors.Order.InvalidCustomerId);
    if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3) throw new DomainException(DomainErrors.Order.InvalidCurrency);

    Id = Guid.NewGuid();
    CustomerId = customerId;
    Currency = currency;
    Status = OrderStatus.Draft; // Na prova diz que nasce como Placed ou Draft, Draft faz mais sentido antes de fechar o pedido
    CreatedAt = DateTime.UtcNow;
  }

  public void AddItem(Guid productId, decimal unitPrice, int quantity)
  {
    if (Status != OrderStatus.Draft)
      throw new DomainException(DomainErrors.Order.CannotAddItemsWhenNotDraft);

    var item = new OrderItem(productId, unitPrice, quantity);
    _items.Add(item);

    CalculateTotal();
  }

  public void PlaceOrder()
  {
    if (!_items.Any()) throw new DomainException(DomainErrors.Order.CannotPlaceEmptyOrder);
    if (Status != OrderStatus.Draft) throw new DomainException(DomainErrors.Order.OnlyDraftOrdersCanBePlaced);

    Status = OrderStatus.Placed;
  }

  public void Confirm()
  {
    if (Status == OrderStatus.Confirmed) return; // Idempotência: se já está confirmado, não faz nada

    if (Status != OrderStatus.Placed)
      throw new DomainException(DomainErrors.Order.OnlyPlacedOrdersCanBeConfirmed);

    Status = OrderStatus.Confirmed;
  }

  public void Cancel()
  {
    if (Status == OrderStatus.Canceled) return; // Idempotência básica no domínio

    if (Status != OrderStatus.Placed && Status != OrderStatus.Confirmed)
      throw new DomainException(DomainErrors.Order.CannotCancelInCurrentStatus);

    Status = OrderStatus.Canceled;
  }

  private void CalculateTotal()
  {
    Total = _items.Sum(i => i.UnitPrice * i.Quantity);
  }
}