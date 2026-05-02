using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.UseCases.Orders.Commands.CancelOrder;
using OrderService.Application.UseCases.Orders.Commands.ConfirmOrder;
using OrderService.Application.UseCases.Orders.Commands.CreateOrder;
using OrderService.Application.UseCases.Orders.Queries.GetOrderById;
using OrderService.Application.UseCases.Orders.Queries.ListOrders;
using OrderService.Domain.Enums;

namespace OrderService.Api.Controllers;

[Authorize]
[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
  private readonly IMediator _mediator;

  public OrdersController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpPost]
  public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
  {
    var orderId = await _mediator.Send(command);
    // 201 Created
    return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, new { id = orderId });
  }

  [HttpPost("{id:guid}/confirm")]
  public async Task<IActionResult> ConfirmOrder(Guid id)
  {
    await _mediator.Send(new ConfirmOrderCommand(id));
    // 204 No Content
    return NoContent(); 
  }

  [HttpPost("{id:guid}/cancel")]
  public async Task<IActionResult> CancelOrder(Guid id)
  {
    await _mediator.Send(new CancelOrderCommand(id));
    // 204 No Content
    return NoContent();
  }

  [HttpGet("{id:guid}")]
  public async Task<IActionResult> GetOrderById(Guid id)
  {
    var order = await _mediator.Send(new GetOrderByIdQuery(id));

    if (order == null)
      return NotFound(); // 404 Not Found

    // 200 OK
    return Ok(order);
  }

  [HttpGet]
  public async Task<IActionResult> ListOrders(
      [FromQuery] Guid? customerId,
      [FromQuery] OrderStatus? status,
      [FromQuery] DateTime? from,
      [FromQuery] DateTime? to,
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 10)
  {
    if (pageSize > 100) pageSize = 100;

    var query = new ListOrdersQuery(customerId, status, from, to, page, pageSize);
    var result = await _mediator.Send(query);

    // 200 OK
    return Ok(result);
  }
}