using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.LastName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Email).HasMaxLength(256);
        builder.Property(c => c.PhoneNumber).HasMaxLength(50);
        builder.Property(c => c.ExternalReference).HasMaxLength(100);
        builder.Property(c => c.IsActive).IsRequired();
        builder.Property(c => c.IsDeleted).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();

        builder.HasIndex(c => c.AccountId).HasDatabaseName("IX_Customers_AccountId");
        builder.HasIndex(c => c.Email).HasDatabaseName("IX_Customers_Email");
        builder.HasIndex(c => c.ExternalReference).HasDatabaseName("IX_Customers_ExternalReference");
        builder.HasIndex(c => c.IsActive).HasDatabaseName("IX_Customers_IsActive");

        builder.HasOne(c => c.Account).WithMany().HasForeignKey(c => c.AccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(c => c.Tickets).WithOne(t => t.Customer).HasForeignKey(t => t.CustomerId).OnDelete(DeleteBehavior.Restrict);
    }
}
