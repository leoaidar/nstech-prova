using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace OrderService.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
  private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

  public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
  {
    _logger = logger;
  }

  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    var requestName = typeof(TRequest).Name;
    _logger.LogInformation("[START] Executando requisição {RequestName}", requestName);

    var timer = Stopwatch.StartNew();

    try
    {
      // O next() é onde Handler roda
      var response = await next();

      timer.Stop();
      _logger.LogInformation("[END] Requisição {RequestName} executada com sucesso em {ElapsedMilliseconds}ms", requestName, timer.ElapsedMilliseconds);

      return response;
    }
    catch (Exception ex)
    {
      timer.Stop();
      _logger.LogError(ex, "[ERROR] Falha na requisição {RequestName} após {ElapsedMilliseconds}ms", requestName, timer.ElapsedMilliseconds);
      throw; // Repassa o erro para o GlobalExceptionHandler da API tratar
    }
  }
}