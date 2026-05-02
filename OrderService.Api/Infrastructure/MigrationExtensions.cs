using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Data.Context;

namespace OrderService.Api.Infrastructure;

public static class MigrationExtensions
{
  public static void ApplyMigrations(this WebApplication app)
  {
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

    // Aplicar migration pendente automaticamente
    dbContext.Database.Migrate();
  }
}