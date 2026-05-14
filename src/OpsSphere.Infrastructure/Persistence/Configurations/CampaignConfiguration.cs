using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Infrastructure.Persistence.Configurations;

internal sealed class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("Campaigns");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.IsActive).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();

        builder.HasIndex(c => c.Code).IsUnique().HasDatabaseName("UQ_Campaigns_Code");
        builder.HasIndex(c => c.AccountId).HasDatabaseName("IX_Campaigns_AccountId");
        builder.HasIndex(c => c.CountryId).HasDatabaseName("IX_Campaigns_CountryId");
        builder.HasIndex(c => c.IsActive).HasDatabaseName("IX_Campaigns_IsActive");
    }
}
