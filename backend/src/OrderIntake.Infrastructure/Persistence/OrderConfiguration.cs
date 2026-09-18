using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderIntake.Domain.Orders;

namespace OrderIntake.Infrastructure.Persistence;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", table => table.HasCheckConstraint("CK_Orders_Status", "[Status] IN (0, 1, 2, 3)"));
        builder.HasKey(order => order.Id);
        builder.Property(order => order.Id).ValueGeneratedNever();
        builder.Property(order => order.ExternalReference).HasMaxLength(200)
            .UseCollation("Latin1_General_100_CI_AS").IsRequired();
        builder.HasIndex(order => order.ExternalReference).IsUnique();
        builder.Property(order => order.Currency).HasMaxLength(3).IsRequired();
        builder.Property(order => order.Notes).HasMaxLength(2000);
        builder.Property<byte[]>("RowVersion").IsRowVersion().IsRequired();
        builder.Ignore(order => order.Subtotal);
        builder.Ignore(order => order.Total);
        builder.OwnsOne(order => order.Customer, customer =>
        {
            customer.Property(value => value.Id).HasColumnName("CustomerId").ValueGeneratedNever();
            customer.Property(value => value.Email).HasColumnName("CustomerEmail").HasMaxLength(320).IsRequired();
            customer.Property(value => value.Name).HasColumnName("CustomerName").HasMaxLength(200).IsRequired();
        });
        builder.Navigation(order => order.Customer).IsRequired();
        builder.OwnsMany(order => order.LineItems, line =>
        {
            line.ToTable("OrderLineItems", table =>
            {
                table.HasCheckConstraint("CK_OrderLineItems_Quantity", "[Quantity] > 0");
                table.HasCheckConstraint("CK_OrderLineItems_UnitPrice", "[UnitPrice] >= 0");
            });
            line.WithOwner().HasForeignKey("OrderId");
            line.HasKey(item => item.Id);
            line.Property(item => item.Id).ValueGeneratedNever();
            line.Property(item => item.Code).HasMaxLength(100).IsRequired();
            line.Property(item => item.Name).HasMaxLength(200).IsRequired();
            line.Property(item => item.UnitPrice).HasPrecision(28, 8);
            line.Ignore(item => item.LineTotal);
        });
        builder.Navigation(order => order.LineItems).HasField("_lineItems").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
