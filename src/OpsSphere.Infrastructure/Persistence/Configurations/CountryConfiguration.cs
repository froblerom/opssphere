using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Infrastructure.Persistence.Configurations;

internal sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(20);
        builder.Property(c => c.IsActive).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();

        builder.HasIndex(c => c.Code).IsUnique().HasDatabaseName("UQ_Countries_Code");
        builder.HasIndex(c => c.RegionId).HasDatabaseName("IX_Countries_RegionId");
        builder.HasIndex(c => c.IsActive).HasDatabaseName("IX_Countries_IsActive");

        builder.HasMany(c => c.Accounts).WithOne(a => a.Country).HasForeignKey(a => a.CountryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(c => c.Campaigns).WithOne(camp => camp.Country).HasForeignKey(camp => camp.CountryId).OnDelete(DeleteBehavior.Restrict);
    }
}
