using FluentAssertions;
using Moq;
using OrderService.Application.UseCases.Orders.Queries.ListOrders;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OrderService.Tests.Application.UseCases.Orders.Queries.ListOrders;

public class ListOrdersQueryHandlerTests
{
  private readonly Mock<IOrderRepository> _orderRepositoryMock;
  private readonly ListOrdersQueryHandler _handler;

  public ListOrdersQueryHandlerTests()
  {
    _orderRepositoryMock = new Mock<IOrderRepository>();
    _handler = new ListOrdersQueryHandler(_orderRepositoryMock.Object);
  }

  #region CAMINHO FELIZ

  [Fact]
  public async Task Handle_Should_Return_PaginatedResult_With_Orders_When_Found()
  {
    // Arrange
    var customerId = Guid.NewGuid();
    var productId1 = Guid.NewGuid();
    var productId2 = Guid.NewGuid();

    var order1 = new Order(customerId, "BRL");
    order1.AddItem(productId1, 100m, 1);

    var order2 = new Order(customerId, "BRL");
    order2.AddItem(productId2, 200m, 2);

    var ordersList = new List<Order> { order1, order2 };
    var totalCount = 15; // Simulando que existem 15 no total, mas trouxemos apenas 2 nesta pagina

    var query = new ListOrdersQuery(
        CustomerId: customerId,
        Status: null,
        From: null,
        To: null,
        Page: 1,
        PageSize: 2
    );

    // Repositorio retorna uma tupla(adoro esse nome tupla) (IEnumerable<Order>, int TotalCount)
    _orderRepositoryMock.Setup(repo => repo.GetPagedAsync(
        query.CustomerId, query.Status, query.From, query.To, query.Page, query.PageSize, It.IsAny<CancellationToken>()))
        .ReturnsAsync((ordersList, totalCount));

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.TotalCount.Should().Be(totalCount);
    result.Page.Should().Be(query.Page);
    result.PageSize.Should().Be(query.PageSize);

    result.Items.Should().HaveCount(2);

    // Valida se o mapeamento do primeiro item funcionou
    var firstDto = result.Items.First();
    firstDto.Id.Should().Be(order1.Id);
    firstDto.Total.Should().Be(100m);
    firstDto.Items.Should().HaveCount(1);
    firstDto.Items.First().ProductId.Should().Be(productId1);

    // Valida se o mapeamento do segundo item funcionou
    var secondDto = result.Items.Last();
    secondDto.Id.Should().Be(order2.Id);
    secondDto.Total.Should().Be(400m); // 200 * 2 = 400
  }

  #endregion FIM CAMINHO FELIZ

  #region CENARIOS ALTERNATIVOS

  [Fact]
  public async Task Handle_Should_Return_Empty_PaginatedResult_When_No_Orders_Match_Filters()
  {
    // Arrange
    var query = new ListOrdersQuery(
        CustomerId: Guid.NewGuid(),
        Status: OrderStatus.Confirmed,
        From: DateTime.UtcNow.AddDays(-1),
        To: DateTime.UtcNow,
        Page: 1,
        PageSize: 10
    );

    var emptyList = new List<Order>();

    _orderRepositoryMock.Setup(repo => repo.GetPagedAsync(
        query.CustomerId, query.Status, query.From, query.To, query.Page, query.PageSize, It.IsAny<CancellationToken>()))
        .ReturnsAsync((emptyList, 0)); // TotalCount zero

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.TotalCount.Should().Be(0);
    result.Items.Should().BeEmpty();
    result.Page.Should().Be(query.Page);
    result.PageSize.Should().Be(query.PageSize);
  }

  #endregion FIM CENARIOS ALTERNATIVOS
}