using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Infrastructure.Data.Context;
using System;
using System.Data.Common;
using System.Linq;

namespace OrderService.Tests.Integration;

public class OrderServiceWebAppFactory : WebApplicationFactory<Program>, IDisposable
{
  // Conexão compartilhada que fica viva durante toda a vida da factory
  private readonly SqliteConnection _keepAliveConnection;

  public OrderServiceWebAppFactory()
  {
    _keepAliveConnection = new SqliteConnection("DataSource=:memory:");
    _keepAliveConnection.Open();
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    // Avisa a API que estamos em ambiente de Testes
    builder.UseEnvironment("Testing");

    builder.ConfigureServices(services =>
    {
      // ── Substitui banco de dados ──────────────────────────────────────────
      // Remove TODOS os descritores relacionados ao OrderDbContext e providers de banco
      // (incluindo o registro com Npgsql feito pelo AddInfrastructure)
      var toRemove = services.Where(d =>
          d.ServiceType == typeof(DbContextOptions<OrderDbContext>) ||
          d.ServiceType == typeof(DbContextOptions) ||
          d.ServiceType == typeof(DbConnection) ||
          (d.ServiceType == typeof(OrderDbContext)) ||
          // Remove qualquer IDbContextOptionsConfiguration<OrderDbContext> que o EF injeta internamente
          (d.ServiceType.IsGenericType &&
           d.ServiceType.GetGenericTypeDefinition().FullName ==
               "Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration`1" &&
           d.ServiceType.GenericTypeArguments[0] == typeof(OrderDbContext))
      ).ToList();

      foreach (var descriptor in toRemove)
      {
        services.Remove(descriptor);
      }

      // Registra a conexão SQLite compartilhada como singleton para o DI
      services.AddSingleton<DbConnection>(_ => _keepAliveConnection);

      // Adiciona o SQLite In-Memory usando a conexão persistente
      services.AddDbContext<OrderDbContext>((sp, options) =>
      {
        var conn = sp.GetRequiredService<DbConnection>();
        options.UseSqlite(conn);
      });

      // Cria o schema de tabelas no SQLite in-memory
      var builtSp = services.BuildServiceProvider();
      using var scope = builtSp.CreateScope();
      var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
      db.Database.EnsureCreated();

      // ── Substitui autenticação JWT pelo TestAuthHandler ───────────────────
      // Remove os descritores de autenticação registrados pelo Program.cs
      var authDescriptors = services.Where(d =>
          d.ServiceType.FullName != null &&
          (d.ServiceType.FullName.StartsWith("Microsoft.AspNetCore.Authentication") ||
           d.ServiceType.FullName.Contains("JwtBearer"))
      ).ToList();

      foreach (var d in authDescriptors)
        services.Remove(d);

      // Adiciona o TestAuthHandler como default — bypassa JWT completamente
      services.AddAuthentication(options =>
      {
        options.DefaultAuthenticateScheme = "Test";
        options.DefaultChallengeScheme = "Test";
      })
      .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
    });
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    if (disposing)
    {
      _keepAliveConnection.Close();
      _keepAliveConnection.Dispose();
    }
  }
}