using FluentAssertions;
using Moq;
using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Queries.GetOrderById; 
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OrderService.Tests.Application.UseCases.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new GetOrderByIdQueryHandler(_orderRepositoryMock.Object);
    }

    #region CAMINHO FELIZ

    [Fact]
    public async Task Handle_Should_Return_OrderResponseDto_When_Order_Exists()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        // Criam Pedido e popula dados
        var order = new Order(customerId, "BRL");
        order.AddItem(productId, 150.5m, 2);
        order.PlaceOrder();

        _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(order);

        var query = new GetOrderByIdQuery(order.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.CustomerId.Should().Be(customerId);
        result.Status.Should().Be(order.Status.ToString());
        result.Currency.Should().Be("BRL");
        
        // Verifica o cálculo do total foi mapeado (150.5 * 2 = 301)
        result.Total.Should().Be(301m); 
        
        // Verifica se a lista de itens foi mapeada corretamente
        result.Items.Should().HaveCount(1);
        result.Items[0].ProductId.Should().Be(productId);
        result.Items[0].UnitPrice.Should().Be(150.5m);
        result.Items[0].Quantity.Should().Be(2);
    }

    #endregion FIM CAMINHO FELIZ

    #region CENARIOS ALTERNATIVOS 

    [Fact]
    public async Task Handle_Should_Return_Null_When_Order_Does_Not_Exist()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Simula repositório não encontrando nada e retornando null
        _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
                            .ReturnsAsync((Order)null!);

        var query = new GetOrderByIdQuery(orderId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion FIM CENARIOS ALTERNATIVOS 
}