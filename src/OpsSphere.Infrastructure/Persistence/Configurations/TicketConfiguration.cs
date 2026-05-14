using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Infrastructure.Persistence.Configurations;

internal sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TicketNumber).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Category).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Priority).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Status).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Subject).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).IsRequired();
        builder.Property(t => t.SlaState).IsRequired().HasMaxLength(50);
        builder.Property(t => t.IsEscalated).IsRequired();
        builder.Property(t => t.IsDeleted).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();

        builder.HasIndex(t => t.TicketNumber).IsUnique().HasDatabaseName("UQ_Tickets_TicketNumber");
        builder.HasIndex(t => t.CustomerId).HasDatabaseName("IX_Tickets_CustomerId");
        builder.HasIndex(t => t.RegionId).HasDatabaseName("IX_Tickets_RegionId");
        builder.HasIndex(t => t.CountryId).HasDatabaseName("IX_Tickets_CountryId");
        builder.HasIndex(t => t.AccountId).HasDatabaseName("IX_Tickets_AccountId");
        builder.HasIndex(t => t.CampaignId).HasDatabaseName("IX_Tickets_CampaignId");
        builder.HasIndex(t => t.AssignedAgentUserId).HasDatabaseName("IX_Tickets_AssignedAgentUserId");
        builder.HasIndex(t => t.SupervisorUserId).HasDatabaseName("IX_Tickets_SupervisorUserId");
        builder.HasIndex(t => t.Status).HasDatabaseName("IX_Tickets_Status");
        builder.HasIndex(t => t.Priority).HasDatabaseName("IX_Tickets_Priority");
        builder.HasIndex(t => t.SlaState).HasDatabaseName("IX_Tickets_SlaState");
        builder.HasIndex(t => t.SlaDueAt).HasDatabaseName("IX_Tickets_SlaDueAt");
        builder.HasIndex(t => t.IsEscalated).HasDatabaseName("IX_Tickets_IsEscalated");
        builder.HasIndex(t => t.CreatedAt).HasDatabaseName("IX_Tickets_CreatedAt");
        builder.HasIndex(t => new { t.AccountId, t.CampaignId, t.Status }).HasDatabaseName("IX_Tickets_AccountId_CampaignId_Status");
        builder.HasIndex(t => new { t.AssignedAgentUserId, t.Status }).HasDatabaseName("IX_Tickets_AssignedAgentUserId_Status");
        builder.HasIndex(t => new { t.SupervisorUserId, t.Status }).HasDatabaseName("IX_Tickets_SupervisorUserId_Status");
        builder.HasIndex(t => new { t.SlaState, t.SlaDueAt }).HasDatabaseName("IX_Tickets_SlaState_SlaDueAt");

        builder.HasOne(t => t.Region).WithMany().HasForeignKey(t => t.RegionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Country).WithMany().HasForeignKey(t => t.CountryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Account).WithMany().HasForeignKey(t => t.AccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Campaign).WithMany().HasForeignKey(t => t.CampaignId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.CreatedByUser).WithMany().HasForeignKey(t => t.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.AssignedAgentUser).WithMany().HasForeignKey(t => t.AssignedAgentUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.SupervisorUser).WithMany().HasForeignKey(t => t.SupervisorUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Comments).WithOne(c => c.Ticket).HasForeignKey(c => c.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(t => t.Assignments).WithOne(a => a.Ticket).HasForeignKey(a => a.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(t => t.StatusHistory).WithOne(sh => sh.Ticket).HasForeignKey(sh => sh.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(t => t.Escalations).WithOne(e => e.Ticket).HasForeignKey(e => e.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.Resolution).WithOne(r => r.Ticket).HasForeignKey<TicketResolution>(r => r.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.SlaStateRecord).WithOne(s => s.Ticket).HasForeignKey<TicketSlaState>(s => s.TicketId).OnDelete(DeleteBehavior.Cascade);
    }
}
