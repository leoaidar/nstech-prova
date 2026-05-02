using FluentAssertions;
using Moq;
using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Commands.CreateOrder;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Repositories;

namespace OrderService.Tests.Application;

public class CreateOrderCommandHandlerTests
{
  private readonly Mock<IOrderRepository> _orderRepositoryMock;
  private readonly Mock<IProductRepository> _productRepositoryMock;
  private readonly CreateOrderCommandHandler _handler;

  public CreateOrderCommandHandlerTests()
  {
    _orderRepositoryMock = new Mock<IOrderRepository>();
    _productRepositoryMock = new Mock<IProductRepository>();
    _handler = new CreateOrderCommandHandler(_orderRepositoryMock.Object, _productRepositoryMock.Object);
  }

  #region CAMINHO FELIZ

  [Fact]
  public async Task Handle_Should_Create_Order_When_Products_Have_Stock()
  {
    // Arrange
    var productId = Guid.NewGuid();
    var customerId = Guid.NewGuid();

    var product = new Product(productId, 100m, 10);

    _productRepositoryMock.Setup(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(product);

    var command = new CreateOrderCommand(customerId, "BRL", new List<OrderItemDto>
        {
            new OrderItemDto(productId, 2)
        });

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeEmpty();
    _orderRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  #endregion FIM CAMINHO FELIZ

  #region EXCECOES DO DOMINIO

  [Fact]
  public async Task Handle_Should_Throw_DomainException_When_Items_Are_Null_Or_Empty()
  {
    // Arrange
    var commandWithNullItems = new CreateOrderCommand(Guid.NewGuid(), "BRL", null!);
    var commandWithEmptyItems = new CreateOrderCommand(Guid.NewGuid(), "BRL", new List<OrderItemDto>());

    // Act
    Func<Task> actionNull = async () => await _handler.Handle(commandWithNullItems, CancellationToken.None);
    Func<Task> actionEmpty = async () => await _handler.Handle(commandWithEmptyItems, CancellationToken.None);

    // Assert
    await actionNull.Should().ThrowAsync<DomainException>()
        .WithMessage(DomainErrors.Order.CannotCreateOrderWithoutItems);

    await actionEmpty.Should().ThrowAsync<DomainException>()
        .WithMessage(DomainErrors.Order.CannotCreateOrderWithoutItems);

    _orderRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Handle_Should_Throw_DomainException_When_Product_Not_Found()
  {
    // Arrange
    var productId = Guid.NewGuid();

    // Simula o banco n�o encontrando o produto
    _productRepositoryMock.Setup(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((Product)null!);

    var command = new CreateOrderCommand(Guid.NewGuid(), "BRL", new List<OrderItemDto>
        {
            new OrderItemDto(productId, 1)
        });

    // Act
    Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

    // Assert
    await action.Should().ThrowAsync<DomainException>()
        .WithMessage(DomainErrors.Product.NotFound(productId));

    _orderRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Handle_Should_Throw_DomainException_When_Stock_Is_Insufficient()
  {
    // Arrange
    var productId = Guid.NewGuid();

    var product = new Product(productId, 100m, 5);

    _productRepositoryMock.Setup(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(product);

    var command = new CreateOrderCommand(Guid.NewGuid(), "BRL", new List<OrderItemDto>
        {
            new OrderItemDto(productId, 10)
        });

    // Act
    Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

    // Assert
    await action.Should().ThrowAsync<DomainException>()
        .WithMessage(DomainErrors.Product.InsufficientStockForOrder(productId, 5));

    // Garante que se estourar erro, n�o insere no banco pela metade
    _orderRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  #endregion FIM EXCECOES DO DOMINIO
}