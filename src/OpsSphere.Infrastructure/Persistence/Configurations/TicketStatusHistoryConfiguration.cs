using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Infrastructure.Persistence.Configurations;

internal sealed class TicketStatusHistoryConfiguration : IEntityTypeConfiguration<TicketStatusHistory>
{
    public void Configure(EntityTypeBuilder<TicketStatusHistory> builder)
    {
        builder.ToTable("TicketStatusHistory");
        builder.HasKey(sh => sh.Id);
        builder.Property(sh => sh.PreviousStatus).HasMaxLength(50);
        builder.Property(sh => sh.NewStatus).IsRequired().HasMaxLength(50);
        builder.Property(sh => sh.ChangeReason).HasMaxLength(500);
        builder.Property(sh => sh.CreatedAt).IsRequired();

        builder.HasIndex(sh => sh.TicketId).HasDatabaseName("IX_TicketStatusHistory_TicketId");
        builder.HasIndex(sh => sh.NewStatus).HasDatabaseName("IX_TicketStatusHistory_NewStatus");
        builder.HasIndex(sh => sh.ChangedByUserId).HasDatabaseName("IX_TicketStatusHistory_ChangedByUserId");
        builder.HasIndex(sh => sh.CreatedAt).HasDatabaseName("IX_TicketStatusHistory_CreatedAt");

        builder.HasOne(sh => sh.ChangedByUser).WithMany().HasForeignKey(sh => sh.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
