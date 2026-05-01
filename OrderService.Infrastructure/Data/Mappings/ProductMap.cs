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
  }
}
