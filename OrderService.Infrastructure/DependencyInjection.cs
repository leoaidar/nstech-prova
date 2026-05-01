using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Infrastructure.Data.Context;

namespace OrderService.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddDbContext<OrderDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

    return services;
  }
}
