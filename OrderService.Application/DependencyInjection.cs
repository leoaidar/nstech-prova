using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace OrderService.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    services.AddMediatR(config =>
    {
      config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

      // Registra o Pipeline de Log pra interceptar todos commands e queries
      config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(Behaviors.LoggingBehavior<,>));
    });

    return services;
  }
}