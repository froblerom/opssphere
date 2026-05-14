using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Infrastructure.Persistence.Configurations;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Code).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Description).HasMaxLength(500);
        builder.Property(a => a.IsActive).IsRequired();
        builder.Property(a => a.CreatedAt).IsRequired();

        builder.HasIndex(a => a.Code).IsUnique().HasDatabaseName("UQ_Accounts_Code");
        builder.HasIndex(a => a.CountryId).HasDatabaseName("IX_Accounts_CountryId");
        builder.HasIndex(a => a.IsActive).HasDatabaseName("IX_Accounts_IsActive");

        builder.HasMany(a => a.Campaigns).WithOne(c => c.Account).HasForeignKey(c => c.AccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(a => a.Customers).WithOne(c => c.Account).HasForeignKey(c => c.AccountId).OnDelete(DeleteBehavior.Restrict);
    }
}
