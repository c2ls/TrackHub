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

public sealed class TripDocumentConfiguration : IEntityTypeConfiguration<TripDocument>
{
    public void Configure(EntityTypeBuilder<TripDocument> builder)
    {
        builder.ToTable(name: TableMetadata.TripDocument, schema: SchemaMetadata.Trip);
        builder.HasKey(x => x.TripDocumentId);

        builder.Property(x => x.TripDocumentId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.TripId).HasColumnName("tripid");
        builder.Property(x => x.TripStopId).HasColumnName("tripstopid");
        builder.Property(x => x.ProofOfDeliveryId).HasColumnName("proofofdeliveryid");
        builder.Property(x => x.DocumentId).HasColumnName("documentid");
        builder.Property(x => x.Kind).HasColumnName("kind").HasMaxLength(40).IsRequired();

        builder.HasIndex(x => new { x.TripId, x.DocumentId })
            .HasDatabaseName("ux_trip_documents_tripid_documentid")
            .IsUnique();

        builder.HasOne(x => x.Trip)
            .WithMany()
            .HasForeignKey(x => x.TripId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
