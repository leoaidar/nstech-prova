using FluentAssertions;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;
using System;

namespace OrderService.Tests.Domain;

public class OrderTests
{
  #region CAMINHO FELIZ 

  [Fact]
  public void AddItem_Should_Increase_Total_And_Add_To_List()
  {
    // Arrange
    var order = new Order(Guid.NewGuid(), "BRL");
    var productId = Guid.NewGuid();
    var unitPrice = 100m;
    var quantity = 2;

    // Act 
    order.AddItem(productId, unitPrice, quantity);

    // Assert
    order.Items.Should().HaveCount(1);
    order.Total.Should().Be(200m);
  }

  [Fact]
  public void PlaceOrder_Should_Change_Status_To_Placed_When_Valid()
  {
    // Arrange
    var order = new Order(Guid.NewGuid(), "BRL");
    order.AddItem(Guid.NewGuid(), 150m, 1);

    // Act
    order.PlaceOrder();

    // Assert
    order.Status.Should().Be(OrderStatus.Placed);
  }

  [Fact]
  public void Confirm_Should_Change_Status_To_Confirmed_When_Placed()
  {
    // Arrange
    var order = new Order(Guid.NewGuid(), "BRL");
    order.AddItem(Guid.NewGuid(), 150m, 1);
    order.PlaceOrder();

    // Act
    order.Confirm();

    // Assert
    order.Status.Should().Be(OrderStatus.Confirmed);
  }

  [Fact]
  public void Confirm_Should_Do_Nothing_When_Already_Confirmed_Idempotent()
  {
    // Arrange
    var order = new Order(Guid.NewGuid(), "BRL");
    order.AddItem(Guid.NewGuid(), 150m, 1);
    order.PlaceOrder();
    order.Confirm(); // Primeira confirmação

    // Act
    order.Confirm(); // Segunda confirmação (Idempotência)

    // Assert
    order.Status.Should().Be(OrderStatus.Confirmed);
  }

  [Fact]
  public void Cancel_Should_Change_Status_To_Canceled_When_Placed()
  {
    // Arrange
    var order = new Order(Guid.NewGuid(), "BRL");
    order.AddItem(Guid.NewGuid(), 150m, 1);
    order.PlaceOrder();

    // Act
    order.Cancel();

    // Assert
    order.Status.Should().Be(OrderStatus.Canceled);
  }

  [Fact]
  public void Cancel_Should_Do_Nothing_When_Already_Canceled_Idempotent()
  {
    // Arrange
    var order = new Order(Guid.NewGuid(), "BRL");
    order.AddItem(Guid.NewGuid(), 150m, 1);
    order.PlaceOrder();
    order.Cancel(); // Primeiro cancelamento

    // Act
    order.Cancel(); // Segundo cancelamento (Idempotência)

    // Assert
    order.Status.Should().Be(OrderStatus.Canceled);
  }

  #endregion FIM CAMINHO FELIZ

  #region EXCEÇÕES DO DOMÍNIO

  [Fact]
  public void AddItem_With_Zero_Quantity_Should_Throw_DomainException()
  {
    // Arrange
    var order = new Order(Guid.NewGuid(), "BRL");

    // Act
    Action action = () => order.AddItem(Guid.NewGuid(), 10m, 0);

    // Assert
    action.Should().Throw<DomainException>()
          .WithMessage(DomainErrors.OrderItem.InvalidItemQuantity);
  }

  [Fact]
  public void AddItem_With_Zero_Price_Should_Throw_DomainException()
  {
    // Arrange
    var order = new Order(Guid.NewGuid(), "BRL");

    // Act
    Action action = () => order.AddItem(Guid.NewGuid(), 0m, 2);

    // Assert
    action.Should().Throw<DomainException>()
          .WithMessage(DomainErrors.OrderItem.InvalidItemPrice);
  }

  [Fact]
  public void AddItem_When_Not_Draft_Should_Throw_DomainException()
  {
    // Arrange
    var order = new Order(Guid.NewGuid(), "BRL");
    order.AddItem(Guid.NewGuid(), 100m, 1);
    order.PlaceOrder(); // Status agora é Placed

    // Act
    Action action = () => order.AddItem(Guid.NewGuid(), 50m, 1);

    // Assert
    action.Should().Throw<DomainException>()
          .WithMessage(DomainErrors.Order.CannotAddItemsWhenNotDraft);
  }

  [Fact]
  public void PlaceOrder_Without_Items_Should_Throw_DomainException()
  {
    // Arrange
    var order = new Order(Guid.NewGuid(), "BRL"); // Nasce como Draft, mas sem itens

    // Act
    Action action = () => order.PlaceOrder();

    // Assert
    action.Should().Throw<DomainException>()
          .WithMessage(DomainErrors.Order.CannotPlaceEmptyOrder);
  }

  [Fact]
  public void PlaceOrder_When_Not_Draft_Should_Throw_DomainException()
  {
    // Arrange
    var order = new Order(Guid.NewGuid(), "BRL");
    order.AddItem(Guid.NewGuid(), 100m, 1);
    order.PlaceOrder(); // Muda para Placed

    // Act
    Action action = () => order.PlaceOrder(); // Tenta colocar como Placed novamente (não é idempotente nesta regra)

    // Assert
    action.Should().Throw<DomainException>()
          .WithMessage(DomainErrors.Order.OnlyDraftOrdersCanBePlaced);
  }

  [Fact]
  public void Confirm_When_Draft_Should_Throw_DomainException()
  {
    // Arrange
    var order = new Order(Guid.NewGuid(), "BRL"); // Status é Draft

    // Act
    Action action = () => order.Confirm();

    // Assert
    action.Should().Throw<DomainException>()
          .WithMessage(DomainErrors.Order.OnlyPlacedOrdersCanBeConfirmed);
  }

  [Fact]
  public void Cancel_When_Draft_Should_Throw_DomainException()
  {
    // Arrange
    var order = new Order(Guid.NewGuid(), "BRL"); // Status é Draft

    // Act
    Action action = () => order.Cancel();

    // Assert
    action.Should().Throw<DomainException>()
          .WithMessage(DomainErrors.Order.CannotCancelInCurrentStatus);
  }

  #endregion
}