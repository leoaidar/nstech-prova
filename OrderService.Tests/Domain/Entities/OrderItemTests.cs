using FluentAssertions;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;
using System;
using Xunit;

namespace OrderService.Tests.Domain.Entities;

public class OrderItemTests
{
  #region CAMINHO FELIZ

  [Fact]
  public void Constructor_Should_Create_OrderItem_When_Arguments_Are_Valid()
  {
    // Arrange
    var productId = Guid.NewGuid();
    var price = 99.90m;
    var quantity = 2;

    // Act
    var orderItem = new OrderItem(productId, price, quantity);

    // Assert
    orderItem.ProductId.Should().Be(productId);
    orderItem.UnitPrice.Should().Be(price);
    orderItem.Quantity.Should().Be(quantity);
  }

  #endregion FIM CAMINHO FELIZ

  #region EXCECOES DO DOMINIO

  [Theory]
  [InlineData(0)]
  [InlineData(-5)]
  public void Constructor_With_Zero_Or_Negative_Quantity_Should_Throw_DomainException(int invalidQuantity)
  {
    // Arrange
    var productId = Guid.NewGuid();
    var price = 100m;

    // Act
    Action action = () => new OrderItem(productId, price, invalidQuantity);

    // Assert
    action.Should().Throw<DomainException>()
          .WithMessage(DomainErrors.OrderItem.InvalidItemQuantity);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-10.5)]
  public void Constructor_With_Zero_Or_Negative_Price_Should_Throw_DomainException(decimal invalidPrice)
  {
    // Arrange
    var productId = Guid.NewGuid();
    var quantity = 1;

    // Act
    Action action = () => new OrderItem(productId, invalidPrice, quantity);

    // Assert
    action.Should().Throw<DomainException>()
          .WithMessage(DomainErrors.OrderItem.InvalidItemPrice);
  }

  #endregion FIM EXCECOES DO DOMINIO
}