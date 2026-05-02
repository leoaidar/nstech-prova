using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Orders.Commands.CreateOrder;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace OrderService.Tests.Integration.Controllers;

public class OrdersControllerTests : IClassFixture<OrderServiceWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly OrderServiceWebAppFactory _factory;

    public OrdersControllerTests(OrderServiceWebAppFactory factory)
    {
        _factory = factory;

        // TestAuthHandler já está registrado na factory com DefaultAuthenticateScheme = "Test"
        // Basta criar o client e enviar o header Authorization: Test
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
    }

    // Metodo pra criar produtos no banco antes do teste rodar
    private async Task SeedProductAsync(Guid productId, decimal price, int quantity)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        
        // Verifica se já existe para não dar erro de chave duplicada
        if (await dbContext.Products.FindAsync(productId) == null)
        {
            dbContext.Products.Add(new Product(productId, price, quantity));
            await dbContext.SaveChangesAsync();
        }
    }

    // Metodo auxiliar pra criar pedidos diretos no banco pro teste de GET
    private async Task SeedOrderAsync(Order order)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();
    }

    #region TESTES DE ESCRITA (COMMANDS)

    [Fact]
    public async Task Post_CreateOrder_Should_Return_201Created_When_Valid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        
        // Precisa ter o produto no banco real, senão Application lança DomainException
        await SeedProductAsync(productId, 100m, 50);

        var command = new CreateOrderCommand(customerId, "BRL", new List<OrderItemDto>
        {
            new OrderItemDto(productId, 2)
        });

        // Act
        var response = await _client.PostAsJsonAsync("/orders", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

    // Verifica se foi preenchido Header com a rota do GetOrderById
    response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/orders/");
    }

    #endregion FIM TESTES DE ESCRITA

    #region TESTES DE LEITURA (QUERIES E PAGINAÇÃO)

    [Fact]
    public async Task Get_ListOrders_Should_Return_Paginated_Data_Correctly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedProductAsync(productId, 50m, 100);

        // Criando 3 pedidos para o mesmo cliente direto no banco
        for (int i = 0; i < 3; i++)
        {
            var order = new Order(customerId, "BRL");
            order.AddItem(productId, 50m, 1);
            order.PlaceOrder();
            await SeedOrderAsync(order);
        }

        // Act - Request Página 1, trazendo apenas 2 itens (PageSize = 2)
        var response = await _client.GetAsync($"/orders?customerId={customerId}&page=1&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<OrderResponseDto>>();

        result.Should().NotBeNull();
        
        // Testando Paginacao
        result!.TotalCount.Should().Be(3); // No banco existem 3 no total
        result.Items.Should().HaveCount(2); // Mas só trouxe 2 nesta página
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task Get_OrderById_Should_Return_404NotFound_When_Not_Exists()
    {
        // Act
        var response = await _client.GetAsync($"/orders/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion FIM TESTES DE LEITURA
}