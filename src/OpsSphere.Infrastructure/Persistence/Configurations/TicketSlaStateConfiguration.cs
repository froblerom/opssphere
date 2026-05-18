using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Infrastructure.Persistence.Configurations;

internal sealed class TicketSlaStateConfiguration : IEntityTypeConfiguration<TicketSlaState>
{
    public void Configure(EntityTypeBuilder<TicketSlaState> builder)
    {
        builder.ToTable("TicketSlaStates");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.StartedAt).IsRequired();
        builder.Property(s => s.DueAt).IsRequired();
        builder.Property(s => s.AtRiskThresholdPercent).IsRequired().HasDefaultValue(80);
        builder.Property(s => s.State).IsRequired().HasMaxLength(50);
        builder.Property(s => s.FinalState).HasMaxLength(50);

        builder.HasIndex(s => s.TicketId).IsUnique().HasDatabaseName("UQ_TicketSlaStates_TicketId");
        builder.HasIndex(s => s.State).HasDatabaseName("IX_TicketSlaStates_State");
        builder.HasIndex(s => s.DueAt).HasDatabaseName("IX_TicketSlaStates_DueAt");
        builder.HasIndex(s => new { s.State, s.DueAt }).HasDatabaseName("IX_TicketSlaStates_State_DueAt");
        builder.HasIndex(s => s.LastEvaluatedAt).HasDatabaseName("IX_TicketSlaStates_LastEvaluatedAt");
    }
}
