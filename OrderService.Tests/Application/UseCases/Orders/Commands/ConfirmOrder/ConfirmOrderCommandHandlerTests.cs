using FluentAssertions;
using Moq;
using OrderService.Application.UseCases.Orders.Commands.ConfirmOrder;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OrderService.Tests.Application.UseCases.Orders.Commands.ConfirmOrder;

public class ConfirmOrderCommandHandlerTests
{
  private readonly Mock<IOrderRepository> _orderRepositoryMock;
  private readonly Mock<IProductRepository> _productRepositoryMock;
  private readonly ConfirmOrderCommandHandler _handler;

  public ConfirmOrderCommandHandlerTests()
  {
    _orderRepositoryMock = new Mock<IOrderRepository>();
    _productRepositoryMock = new Mock<IProductRepository>();
    _handler = new ConfirmOrderCommandHandler(_orderRepositoryMock.Object, _productRepositoryMock.Object);
  }

  #region CAMINHO FELIZ

  [Fact]
  public async Task Handle_Should_Confirm_Order_And_Decrease_Stock_When_Valid()
  {
    // Arrange
    var productId = Guid.NewGuid();

    // Pedido Nasce Draft -> Adiciona Item -> Fica Placed para poder ser confirmado
    var order = new Order(Guid.NewGuid(), "BRL");
    order.AddItem(productId, 100m, 2);
    order.PlaceOrder();

    // Estoque inicial de 10
    var product = new Product(productId, 100m, 10);

    _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(order);

    _productRepositoryMock.Setup(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(product);

    // Command recebe o OrderId no construtor
    var command = new ConfirmOrderCommand(order.Id);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().BeTrue();
    order.Status.Should().Be(OrderStatus.Confirmed);
    product.AvailableQuantity.Should().Be(8); // Tinha 10, baixou 2

    _productRepositoryMock.Verify(repo => repo.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    _orderRepositoryMock.Verify(repo => repo.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task Handle_Should_Return_True_Immediately_When_Order_Is_Already_Confirmed_Idempotency()
  {
    // Arrange
    var order = new Order(Guid.NewGuid(), "BRL");
    order.AddItem(Guid.NewGuid(), 100m, 2);
    order.PlaceOrder();
    order.Confirm(); // Força o status para Confirmed

    _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(order);

    var command = new ConfirmOrderCommand(order.Id);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().BeTrue();

    // Verifica se o handler saiu cedo e NÃO chamou os repositórios para atualizar
    _productRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    _orderRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  #endregion FIM CAMINHO FELIZ

  #region EXCECOES DA APLICACAO

  [Fact]
  public async Task Handle_Should_Throw_DomainException_When_Order_Not_Found()
  {
    // Arrange
    var orderId = Guid.NewGuid();

    // Simula Banco não encontrando o pedido
    _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Order)null!);

    var command = new ConfirmOrderCommand(orderId);

    // Act
    Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

    // Assert
    await action.Should().ThrowAsync<DomainException>()
                .WithMessage(DomainErrors.Order.NotFound);
  }

  [Fact]
  public async Task Handle_Should_Throw_DomainException_When_Product_Not_Found_During_Stock_Decrease()
  {
    // Arrange
    var productId = Guid.NewGuid();

    var order = new Order(Guid.NewGuid(), "BRL");
    order.AddItem(productId, 100m, 2);
    order.PlaceOrder();

    _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(order);

    // Simulando Banco não encontrando o produto na hora de dar a baixa no estoque
    _productRepositoryMock.Setup(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((Product)null!);

    var command = new ConfirmOrderCommand(order.Id);

    // Act
    Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

    // Assert
    await action.Should().ThrowAsync<DomainException>()
                .WithMessage(DomainErrors.Product.NotFound(productId));

    // Garante que não tentou salvar o pedido caso tenha dado erro no meio do processo
    _orderRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  #endregion FIM EXCECOES DA APLICACAO
}