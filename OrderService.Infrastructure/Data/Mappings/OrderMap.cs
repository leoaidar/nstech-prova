using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Data.Mappings;

public class OrderMap : IEntityTypeConfiguration<Order>
{
  public void Configure(EntityTypeBuilder<Order> builder)
  {
    builder.ToTable("Orders");

    builder.HasKey(o => o.Id);

    builder.Property(o => o.CustomerId)
        .IsRequired();

    builder.Property(o => o.Currency)
        .IsRequired()
        .HasMaxLength(3); 

    builder.Property(o => o.Total)
        .HasPrecision(18, 2);

    builder.Property(o => o.Status)
        .HasConversion<string>()
        .IsRequired();

    builder.Metadata.FindNavigation(nameof(Order.Items))!
        .SetPropertyAccessMode(PropertyAccessMode.Field);

    builder.OwnsMany(o => o.Items, i =>
    {
      i.ToTable("OrderItems");
      i.WithOwner().HasForeignKey("OrderId");

      i.Property<Guid>("Id").IsRequired(); 
      i.HasKey("Id");

      i.Property(x => x.ProductId).IsRequired();
      i.Property(x => x.Quantity).IsRequired();
      i.Property(x => x.UnitPrice).HasPrecision(18, 2).IsRequired();
    });
  }
}
