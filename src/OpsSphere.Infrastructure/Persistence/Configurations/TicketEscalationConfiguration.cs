using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Infrastructure.Persistence.Configurations;

internal sealed class TicketEscalationConfiguration : IEntityTypeConfiguration<TicketEscalation>
{
    public void Configure(EntityTypeBuilder<TicketEscalation> builder)
    {
        builder.ToTable("TicketEscalations");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EscalationReason).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.ReviewNotes).HasMaxLength(1000);
        builder.Property(e => e.IsActive).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.TicketId).HasDatabaseName("IX_TicketEscalations_TicketId");
        builder.HasIndex(e => e.EscalatedByUserId).HasDatabaseName("IX_TicketEscalations_EscalatedByUserId");
        builder.HasIndex(e => e.ReviewedByUserId).HasDatabaseName("IX_TicketEscalations_ReviewedByUserId");
        builder.HasIndex(e => e.IsActive).HasDatabaseName("IX_TicketEscalations_IsActive");
        builder.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_TicketEscalations_CreatedAt");

        builder.HasOne(e => e.EscalatedByUser).WithMany().HasForeignKey(e => e.EscalatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ReviewedByUser).WithMany().HasForeignKey(e => e.ReviewedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
