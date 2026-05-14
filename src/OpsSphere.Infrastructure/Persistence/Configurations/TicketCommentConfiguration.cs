using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Infrastructure.Persistence.Configurations;

internal sealed class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> builder)
    {
        builder.ToTable("TicketComments");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Body).IsRequired();
        builder.Property(c => c.IsDeleted).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();

        builder.HasIndex(c => c.TicketId).HasDatabaseName("IX_TicketComments_TicketId");
        builder.HasIndex(c => c.AuthorUserId).HasDatabaseName("IX_TicketComments_AuthorUserId");
        builder.HasIndex(c => c.CreatedAt).HasDatabaseName("IX_TicketComments_CreatedAt");

        builder.HasOne(c => c.Author).WithMany().HasForeignKey(c => c.AuthorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
