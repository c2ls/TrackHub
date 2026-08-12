using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class AccountSupportGrantConfiguration : IEntityTypeConfiguration<AccountSupportGrant>
{
    public void Configure(EntityTypeBuilder<AccountSupportGrant> builder)
    {
        builder.ToTable(name: TableMetadata.AccountSupportGrant, schema: SchemaMetadata.Application);
        builder.Property(x => x.AccountSupportGrantId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.SupportUserId).HasColumnName("supportuserid");
        builder.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(ColumnMetadata.DefaultDescriptionLength).IsRequired();
        builder.Property(x => x.TicketReference).HasColumnName("ticketreference").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.ApprovedBy).HasColumnName("approvedby").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.ApprovedAt).HasColumnName("approvedat");
        builder.Property(x => x.AccessLevel).HasColumnName("accesslevel").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.StartsAt).HasColumnName("startsat");
        builder.Property(x => x.EndsAt).HasColumnName("endsat");
        builder.Property(x => x.RevokedAt).HasColumnName("revokedat");
        builder.Property(x => x.RevokedBy).HasColumnName("revokedby").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.HasIndex(x => new { x.AccountId, x.SupportUserId, x.StartsAt, x.EndsAt });
    }
}
