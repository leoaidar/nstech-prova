using OrderService.Domain.Entities;

namespace OrderService.Domain.Repositories;

public interface IProductRepository
{
  Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
}