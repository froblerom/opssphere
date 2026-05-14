using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Infrastructure.Persistence.Configurations;

internal sealed class TicketResolutionConfiguration : IEntityTypeConfiguration<TicketResolution>
{
    public void Configure(EntityTypeBuilder<TicketResolution> builder)
    {
        builder.ToTable("TicketResolutions");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.ResolutionSummary).IsRequired();
        builder.Property(r => r.ResolutionCode).HasMaxLength(100);
        builder.Property(r => r.FinalSlaState).IsRequired().HasMaxLength(50);
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.HasIndex(r => r.ResolvedByUserId).HasDatabaseName("IX_TicketResolutions_ResolvedByUserId");
        builder.HasIndex(r => r.FinalSlaState).HasDatabaseName("IX_TicketResolutions_FinalSlaState");
        builder.HasIndex(r => r.CreatedAt).HasDatabaseName("IX_TicketResolutions_CreatedAt");

        builder.HasOne(r => r.ResolvedByUser).WithMany().HasForeignKey(r => r.ResolvedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
