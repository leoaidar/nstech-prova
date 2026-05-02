using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Domain.Repositories;

public interface IOrderRepository
{
  Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task AddAsync(Order order, CancellationToken cancellationToken = default);
  Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
  Task SaveChangesAsync(CancellationToken cancellationToken = default);
  Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedAsync(
      Guid? customerId,
      OrderStatus? status,
      DateTime? from,
      DateTime? to,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default);
}
