using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Data.Context;

namespace OrderService.Infrastructure.Data.Repositories;

internal class OrderRepository : IOrderRepository
{
  private readonly OrderDbContext _context;

  public OrderRepository(OrderDbContext context)
  {
    _context = context;
  }

  public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _context.Orders
        .Include(o => o.Items) 
        .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
  }

  public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
  {
    await _context.Orders.AddAsync(order, cancellationToken);
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
  {
    _context.Orders.Update(order);
    await _context.SaveChangesAsync(cancellationToken);
  }
}