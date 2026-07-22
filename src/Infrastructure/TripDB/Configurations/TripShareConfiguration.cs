// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using Common.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TrackHub.TripManagement.Infrastructure.TripDB.Configurations;

public sealed class TripShareConfiguration : IEntityTypeConfiguration<TripShare>
{
    public void Configure(EntityTypeBuilder<TripShare> builder)
    {
        builder.ToTable(name: TableMetadata.TripShare, schema: SchemaMetadata.Trip);
        builder.HasKey(x => x.TripShareId);

        builder.Property(x => x.TripShareId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.TripId).HasColumnName("tripid");
        builder.Property(x => x.PublicLinkGrantId).HasColumnName("publiclinkgrantid");
        builder.Property(x => x.IncludeDriverName).HasColumnName("includedrivername").HasDefaultValue(false);
        builder.Property(x => x.IncludeVehicle).HasColumnName("includevehicle").HasDefaultValue(false);
        builder.Property(x => x.IncludeLivePosition).HasColumnName("includeliveposition").HasDefaultValue(false);
        builder.Property(x => x.IncludeStopDetail).HasColumnName("includestopdetail").HasDefaultValue(false);
        builder.Property(x => x.IncludePodSummary).HasColumnName("includepodsummary").HasDefaultValue(false);

        // Fail closed: rows written before this column existed must read back as "route not shared".
        builder.Property(x => x.IncludeRoute).HasColumnName("includeroute").HasDefaultValue(false);
        builder.Property(x => x.CreatedByPrincipalId).HasColumnName("createdbyprincipalid").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnName("expiresat");
        builder.Property(x => x.RevokedAt).HasColumnName("revokedat");

        builder.HasIndex(x => new { x.AccountId, x.TripId })
            .HasDatabaseName("ix_trip_shares_accountid_tripid");

        // Anonymous resolution arrives with the Manager grant id and nothing else.
        builder.HasIndex(x => x.PublicLinkGrantId)
            .HasDatabaseName("ux_trip_shares_publiclinkgrantid")
            .IsUnique();

        builder.HasOne(x => x.Trip)
            .WithMany()
            .HasForeignKey(x => x.TripId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
