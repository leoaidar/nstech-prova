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

  public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
  {
    _context.Orders.Update(order);
    return Task.CompletedTask; 
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedAsync(
      Guid? customerId, Domain.Enums.OrderStatus? status, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken cancellationToken = default)
  {
    var query = _context.Orders
        .AsNoTracking() 
        .Include(o => o.Items)
        .AsQueryable();

    if (customerId.HasValue)
      query = query.Where(o => o.CustomerId == customerId.Value);

    if (status.HasValue)
      query = query.Where(o => o.Status == status.Value);

    if (from.HasValue)
      query = query.Where(o => o.CreatedAt >= from.Value);

    if (to.HasValue)
      query = query.Where(o => o.CreatedAt <= to.Value);

    var totalCount = await query.CountAsync(cancellationToken);

    var orders = await query
        .OrderByDescending(o => o.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

    return (orders, totalCount);
  }
}