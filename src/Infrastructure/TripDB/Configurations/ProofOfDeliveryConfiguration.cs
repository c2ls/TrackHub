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

public sealed class ProofOfDeliveryConfiguration : IEntityTypeConfiguration<ProofOfDelivery>
{
    public void Configure(EntityTypeBuilder<ProofOfDelivery> builder)
    {
        builder.ToTable(name: TableMetadata.TripPod, schema: SchemaMetadata.Trip);
        builder.HasKey(x => x.ProofOfDeliveryId);

        builder.Property(x => x.ProofOfDeliveryId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.TripStopId).HasColumnName("tripstopid");
        builder.Property(x => x.DeliveryId).HasColumnName("deliveryid");
        builder.Property(x => x.ReceiverName).HasColumnName("receivername").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.ReceiverDocument).HasColumnName("receiverdocument").HasMaxLength(50);
        builder.Property(x => x.CapturedAt).HasColumnName("capturedat");
        builder.Property(x => x.Latitude).HasColumnName("latitude");
        builder.Property(x => x.Longitude).HasColumnName("longitude");
        builder.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(x => x.ClientEventId).HasColumnName("clienteventid");

        // Offline-outbox safety: a retried POD submission hits this index and returns the
        // existing record instead of creating a second one (acceptance 15).
        builder.HasIndex(x => new { x.TripStopId, x.ClientEventId })
            .HasDatabaseName("ux_trip_pods_tripstopid_clienteventid")
            .IsUnique();

        builder.HasOne(x => x.TripStop)
            .WithMany()
            .HasForeignKey(x => x.TripStopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
