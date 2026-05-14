using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Infrastructure.Persistence.Configurations;

internal sealed class SlaPolicyConfiguration : IEntityTypeConfiguration<SlaPolicy>
{
    public void Configure(EntityTypeBuilder<SlaPolicy> builder)
    {
        builder.ToTable("SlaPolicies");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Priority).IsRequired().HasMaxLength(50);
        builder.Property(p => p.TargetHours).IsRequired();
        builder.Property(p => p.AtRiskThresholdPercent).IsRequired();
        builder.Property(p => p.IsActive).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasIndex(p => p.AccountId).HasDatabaseName("IX_SlaPolicies_AccountId");
        builder.HasIndex(p => p.CampaignId).HasDatabaseName("IX_SlaPolicies_CampaignId");
        builder.HasIndex(p => p.Priority).HasDatabaseName("IX_SlaPolicies_Priority");
        builder.HasIndex(p => p.IsActive).HasDatabaseName("IX_SlaPolicies_IsActive");
        builder.HasIndex(p => new { p.AccountId, p.CampaignId, p.Priority }).HasDatabaseName("IX_SlaPolicies_AccountId_CampaignId_Priority");

        builder.HasOne(p => p.Account).WithMany(a => a.SlaPolicies).HasForeignKey(p => p.AccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Campaign).WithMany(c => c.SlaPolicies).HasForeignKey(p => p.CampaignId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(p => p.TicketSlaStates).WithOne(s => s.SlaPolicy).HasForeignKey(s => s.SlaPolicyId).OnDelete(DeleteBehavior.Restrict);
    }
}
