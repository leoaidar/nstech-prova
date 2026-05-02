using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

public class ProductMap : IEntityTypeConfiguration<Product>
{
  public void Configure(EntityTypeBuilder<Product> builder)
  {
    builder.ToTable("Products");

    builder.HasKey(p => p.Id);

    builder.Property(p => p.UnitPrice)
        .IsRequired()
        .HasPrecision(18, 2); 

    builder.Property(p => p.AvailableQuantity)
        .IsRequired();

    // --- SEED DATA ---
    // IDs estáticos fixos para facilitar o teste no Swagger
    builder.HasData(
        new { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), UnitPrice = 1500.00m, AvailableQuantity = 10 }, // Notebook
        new { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), UnitPrice = 89.90m, AvailableQuantity = 50 },  // Mouse
        new { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), UnitPrice = 120.50m, AvailableQuantity = 100 } // Teclado
    );
  }
}
