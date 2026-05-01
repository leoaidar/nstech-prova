using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Data.Context;

namespace OrderService.Infrastructure.Data.Repositories;

internal class ProductRepository : IProductRepository
{
  private readonly OrderDbContext _context;

  public ProductRepository(OrderDbContext context)
  {
    _context = context;
  }

  public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
  }

  public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
  {
    _context.Products.Update(product);
    await _context.SaveChangesAsync(cancellationToken);
  }
}