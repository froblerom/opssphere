using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(150);
        builder.Property(a => a.EntityType).IsRequired().HasMaxLength(150);
        builder.Property(a => a.IpAddress).HasMaxLength(100);
        builder.Property(a => a.UserAgent).HasMaxLength(500);
        builder.Property(a => a.CorrelationId).HasMaxLength(100);
        builder.Property(a => a.CreatedAt).IsRequired();

        builder.HasIndex(a => a.ActorUserId).HasDatabaseName("IX_AuditLogs_ActorUserId");
        builder.HasIndex(a => a.Action).HasDatabaseName("IX_AuditLogs_Action");
        builder.HasIndex(a => new { a.EntityType, a.EntityId }).HasDatabaseName("IX_AuditLogs_EntityType_EntityId");
        builder.HasIndex(a => new { a.EntityType, a.EntityId, a.CreatedAt }).HasDatabaseName("IX_AuditLogs_EntityType_EntityId_CreatedAt");
        builder.HasIndex(a => a.CreatedAt).HasDatabaseName("IX_AuditLogs_CreatedAt");
        builder.HasIndex(a => a.CorrelationId).HasDatabaseName("IX_AuditLogs_CorrelationId");

        builder.HasOne(a => a.ActorUser).WithMany().HasForeignKey(a => a.ActorUserId).OnDelete(DeleteBehavior.SetNull);
    }
}
