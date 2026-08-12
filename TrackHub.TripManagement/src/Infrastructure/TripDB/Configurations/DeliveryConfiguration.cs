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

public sealed class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.ToTable(name: TableMetadata.TripDelivery, schema: SchemaMetadata.Trip);
        builder.HasKey(x => x.DeliveryId);

        builder.Property(x => x.DeliveryId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.TripStopId).HasColumnName("tripstopid");
        builder.Property(x => x.Reference).HasColumnName("reference").HasMaxLength(80);
        builder.Property(x => x.ClientName).HasColumnName("clientname").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.BranchName).HasColumnName("branchname").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.ProductsSummary).HasColumnName("productssummary").HasMaxLength(1000);
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(40).IsRequired();
        builder.Property(x => x.Observations).HasColumnName("observations").HasMaxLength(1000);
        builder.Property(x => x.SequenceIndex).HasColumnName("sequenceindex");

        builder.HasIndex(x => new { x.AccountId, x.TripStopId })
            .HasDatabaseName("ix_trip_deliveries_accountid_tripstopid");

        builder.HasOne(x => x.TripStop)
            .WithMany()
            .HasForeignKey(x => x.TripStopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
