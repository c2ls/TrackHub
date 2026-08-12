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

public sealed class TripEventConfiguration : IEntityTypeConfiguration<TripEvent>
{
    public void Configure(EntityTypeBuilder<TripEvent> builder)
    {
        builder.ToTable(name: TableMetadata.TripEvent, schema: SchemaMetadata.Trip);
        builder.HasKey(x => x.TripEventId);

        builder.Property(x => x.TripEventId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.TripId).HasColumnName("tripid");
        builder.Property(x => x.TripStopId).HasColumnName("tripstopid");
        builder.Property(x => x.EventType).HasColumnName("eventtype").HasMaxLength(ColumnMetadata.DefaultFieldLength).IsRequired();
        builder.Property(x => x.OccurredAt).HasColumnName("occurredat");
        builder.Property(x => x.Source).HasColumnName("source").HasMaxLength(40).IsRequired();
        builder.Property(x => x.PayloadJson).HasColumnName("payloadjson").HasColumnType(ColumnMetadata.TextField);
        builder.Property(x => x.IdempotencyKey).HasColumnName("idempotencykey").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();

        builder.HasIndex(x => new { x.AccountId, x.TripId, x.OccurredAt })
            .HasDatabaseName("ix_trip_events_accountid_tripid_occurredat");

        // THE idempotency guarantee (acceptance 15). Writers catch the unique violation and
        // report a duplicate rather than throwing.
        builder.HasIndex(x => x.IdempotencyKey)
            .HasDatabaseName("ux_trip_events_idempotencykey")
            .IsUnique();

        builder.HasOne(x => x.Trip)
            .WithMany()
            .HasForeignKey(x => x.TripId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
