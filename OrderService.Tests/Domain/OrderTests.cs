using FluentAssertions;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;
using System;

namespace OrderService.Tests.Domain;

public class OrderTests
{
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
  public void AddItem_With_Zero_Quantity_Should_Throw_DomainException()
  {
    // Arrange
    var order = new Order(Guid.NewGuid(), "BRL");

    // Act
    Action action = () => order.AddItem(Guid.NewGuid(), 10m, 0);

    // Assert
    action.Should().Throw<DomainException>()
          .WithMessage("A quantidade do item deve ser maior que zero.");
  }

  [Fact]
  public void Confirm_Should_Change_Status_To_Confirmed_When_Placed()
  {

    // Arrange
    var order = new Order(Guid.NewGuid(), "BRL");

    order.AddItem(Guid.NewGuid(), 150m, 1);
    order.PlaceOrder(); // Muda de Draft para Placed com sucesso

    // Act
    order.Confirm();

    // Assert
    order.Status.Should().Be(OrderStatus.Confirmed);
  }

  [Fact]
  public void Confirm_Should_Whithout_Items_Should_Throw_DomainException()
  {
    // Arrange
    var order = new Order(Guid.NewGuid(), "BRL");
     // Muda de Draft para Placed com falha

    // Act
    Action action = () => order.PlaceOrder();

    // Assert
    action.Should().Throw<DomainException>()
          .WithMessage("Não é possível fechar um pedido sem itens.");
  }
}