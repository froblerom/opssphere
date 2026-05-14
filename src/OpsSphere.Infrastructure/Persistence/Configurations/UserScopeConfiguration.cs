using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Infrastructure.Persistence.Configurations;

internal sealed class UserScopeConfiguration : IEntityTypeConfiguration<UserScope>
{
    public void Configure(EntityTypeBuilder<UserScope> builder)
    {
        builder.ToTable("UserScopes");
        builder.HasKey(us => us.Id);
        builder.Property(us => us.ScopeType).IsRequired().HasMaxLength(50);
        builder.Property(us => us.IsActive).IsRequired();
        builder.Property(us => us.CreatedAt).IsRequired();

        builder.HasIndex(us => us.UserId).HasDatabaseName("IX_UserScopes_UserId");
        builder.HasIndex(us => us.ScopeType).HasDatabaseName("IX_UserScopes_ScopeType");
        builder.HasIndex(us => us.RegionId).HasDatabaseName("IX_UserScopes_RegionId");
        builder.HasIndex(us => us.CountryId).HasDatabaseName("IX_UserScopes_CountryId");
        builder.HasIndex(us => us.AccountId).HasDatabaseName("IX_UserScopes_AccountId");
        builder.HasIndex(us => us.CampaignId).HasDatabaseName("IX_UserScopes_CampaignId");
        builder.HasIndex(us => us.IsActive).HasDatabaseName("IX_UserScopes_IsActive");

        builder.HasOne(us => us.Region).WithMany(r => r.UserScopes).HasForeignKey(us => us.RegionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(us => us.Country).WithMany(c => c.UserScopes).HasForeignKey(us => us.CountryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(us => us.Account).WithMany(a => a.UserScopes).HasForeignKey(us => us.AccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(us => us.Campaign).WithMany(c => c.UserScopes).HasForeignKey(us => us.CampaignId).OnDelete(DeleteBehavior.Restrict);
    }
}
