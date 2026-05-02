using FluentAssertions;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;
using System;

namespace OrderService.Tests.Domain.Entities;

public class ProductTests
{

  #region CAMINHO FELIZ

  [Fact]
  public void Constructor_Should_Create_Product_When_Arguments_Are_Valid()
  {
    // Arrange
    var id = Guid.NewGuid();
    var price = 150.5m;
    var quantity = 10;

    // Act
    var product = new Product(id, price, quantity);

    // Assert
    product.Id.Should().Be(id);
    product.UnitPrice.Should().Be(price);
    product.AvailableQuantity.Should().Be(quantity);
  }

  [Fact]
  public void DecreaseStock_Should_Reduce_AvailableQuantity_When_Valid()
  {
    // Arrange
    var product = new Product(Guid.NewGuid(), 100m, 20);

    // Act
    product.DecreaseStock(5);

    // Assert
    product.AvailableQuantity.Should().Be(15);
  }


  [Fact]
  public void IncreaseStock_Should_Add_To_AvailableQuantity_When_Valid()
  {
    // Arrange
    var product = new Product(Guid.NewGuid(), 100m, 10);

    // Act
    product.IncreaseStock(5);

    // Assert
    product.AvailableQuantity.Should().Be(15);
  }

  #endregion FIM CAMINHO FELIZ

  #region EXCECOES DO DOMINIO

  [Theory]
  [InlineData(0)]
  [InlineData(-10.5)]
  public void Constructor_With_Invalid_Price_Should_Throw_DomainException(decimal invalidPrice)
  {
    // Arrange
    var id = Guid.NewGuid();

    // Act
    Action action = () => new Product(id, invalidPrice, 10);

    // Assert
    action.Should().Throw<DomainException>()
          .WithMessage(DomainErrors.Product.InvalidPrice);
  }

  [Fact]
  public void Constructor_With_Negative_Quantity_Should_Throw_DomainException()
  {
    // Arrange
    var id = Guid.NewGuid();

    // Act
    Action action = () => new Product(id, 100m, -5);

    // Assert
    action.Should().Throw<DomainException>()
          .WithMessage(DomainErrors.Product.InvalidInitialQuantity);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-5)]
  public void DecreaseStock_With_Zero_Or_Negative_Quantity_Should_Throw_DomainException(int invalidQuantity)
  {
    // Arrange
    var product = new Product(Guid.NewGuid(), 100m, 20);

    // Act
    Action action = () => product.DecreaseStock(invalidQuantity);

    // Assert
    action.Should().Throw<DomainException>()
          .WithMessage(DomainErrors.Product.InvalidDecreaseQuantity);
  }

  [Fact]
  public void DecreaseStock_When_Insufficient_Stock_Should_Throw_DomainException()
  {
    // Arrange
    var initialQuantity = 10;
    var product = new Product(Guid.NewGuid(), 100m, initialQuantity);

    // Act
    Action action = () => product.DecreaseStock(15); // Tentando baixar 15 de onde só tem 10

    // Assert
    action.Should().Throw<DomainException>()
          .WithMessage(DomainErrors.Product.InsufficientStock(initialQuantity));
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-10)]
  public void IncreaseStock_With_Zero_Or_Negative_Quantity_Should_Throw_DomainException(int invalidQuantity)
  {
    // Arrange
    var product = new Product(Guid.NewGuid(), 100m, 10);

    // Act
    Action action = () => product.IncreaseStock(invalidQuantity);

    // Assert
    action.Should().Throw<DomainException>()
          .WithMessage(DomainErrors.Product.InvalidIncreaseQuantity);
  }

  #endregion FIM EXCECOES DO DOMINIO
}