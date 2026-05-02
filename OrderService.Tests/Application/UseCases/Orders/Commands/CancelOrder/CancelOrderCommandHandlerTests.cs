using FluentAssertions;
using Moq;
using OrderService.Application.UseCases.Orders.Commands.CancelOrder;
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OrderService.Tests.Application.UseCases.Orders.Commands.CancelOrder;

public class CancelOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new CancelOrderCommandHandler(_orderRepositoryMock.Object, _productRepositoryMock.Object);
    }

    #region CAMINHO FELIZ

    [Fact]
    public async Task Handle_Should_Cancel_Order_And_Replenish_Stock_When_Status_Was_Confirmed()
    {
        // Arrange
        var productId = Guid.NewGuid();
        
        var order = new Order(Guid.NewGuid(), "BRL");
        order.AddItem(productId, 100m, 2);
        order.PlaceOrder();
        order.Confirm(); // Deixa o pedido como Confirmed (exige repor estoque no cancelamento)

        var product = new Product(productId, 100m, 8); // Simulando o estoque já sido baixado pra 8

        _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(order);
                            
        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(product);

        var command = new CancelOrderCommand(order.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Canceled);
        product.AvailableQuantity.Should().Be(10); // Repôs os 2 itens cancelados, voltou para 10

        _productRepositoryMock.Verify(repo => repo.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.Verify(repo => repo.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Cancel_Order_But_Not_Replenish_Stock_When_Status_Was_Placed()
    {
        // Arrange
        var productId = Guid.NewGuid();
        
        var order = new Order(Guid.NewGuid(), "BRL");
        order.AddItem(productId, 100m, 2);
        order.PlaceOrder(); // Deixa o pedido como Placed (não baixou estoque ainda)

        _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(order);

        var command = new CancelOrderCommand(order.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Canceled);

        // Garante que não tentou repor estoque de nenhum produto, não era necessário
        _productRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _productRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        
        _orderRepositoryMock.Verify(repo => repo.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_True_Immediately_When_Order_Is_Already_Canceled_Idempotency()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), "BRL");
        order.AddItem(Guid.NewGuid(), 100m, 2);
        order.PlaceOrder();
        order.Cancel(); // Força status pra Canceled ao inicializar

        _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(order);

        var command = new CancelOrderCommand(order.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

    // Verifica se o handler saiu cedo e NÃO chamou os repositórios para atualizar nada no banco(Times.Never)
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

        _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
                            .ReturnsAsync((Order)null!);

        var command = new CancelOrderCommand(orderId);

        // Act
        Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<DomainException>()
                    .WithMessage(DomainErrors.Order.NotFound);
    }

    #endregion FIM EXCECOES DA APLICACAO
}