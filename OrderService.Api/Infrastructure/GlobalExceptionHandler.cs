using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrderService.Domain.Exceptions;

namespace OrderService.Api.Infrastructure;

public class GlobalExceptionHandler : IExceptionHandler
{
  private readonly ILogger<GlobalExceptionHandler> _logger;

  public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
  {
    _logger = logger;
  }

  public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
  {
    _logger.LogError(exception, "Ocorreu um erro: {Message}", exception.Message);

    var problemDetails = new ProblemDetails
    {
      Instance = httpContext.Request.Path
    };

    if (exception is DomainException domainException)
    {
      httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
      problemDetails.Title = "Erro de Regra de Negocio";
      problemDetails.Status = StatusCodes.Status400BadRequest;
      problemDetails.Detail = domainException.Message;
    }
    else
    {
      // Erros nao mapeados (Ex: NullReference, falha no banco)
      httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
      problemDetails.Title = "Erro Interno do Servidor";
      problemDetails.Status = StatusCodes.Status500InternalServerError;
      problemDetails.Detail = "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.";
    }

    await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

    return true; // Retorna verdadeiro para dizer ao pipeline do .NET que ja resolveu o problema
  }
}