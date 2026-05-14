using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Infrastructure.Persistence.Configurations;

internal sealed class TicketAssignmentConfiguration : IEntityTypeConfiguration<TicketAssignment>
{
    public void Configure(EntityTypeBuilder<TicketAssignment> builder)
    {
        builder.ToTable("TicketAssignments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.AssignmentReason).HasMaxLength(500);
        builder.Property(a => a.CreatedAt).IsRequired();

        builder.HasIndex(a => a.TicketId).HasDatabaseName("IX_TicketAssignments_TicketId");
        builder.HasIndex(a => a.NewAgentUserId).HasDatabaseName("IX_TicketAssignments_NewAgentUserId");
        builder.HasIndex(a => a.AssignedByUserId).HasDatabaseName("IX_TicketAssignments_AssignedByUserId");
        builder.HasIndex(a => a.CreatedAt).HasDatabaseName("IX_TicketAssignments_CreatedAt");

        builder.HasOne(a => a.PreviousAgentUser).WithMany().HasForeignKey(a => a.PreviousAgentUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.NewAgentUser).WithMany().HasForeignKey(a => a.NewAgentUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.AssignedByUser).WithMany().HasForeignKey(a => a.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
