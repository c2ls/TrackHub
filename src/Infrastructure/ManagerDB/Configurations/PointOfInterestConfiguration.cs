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
using Common.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;
public sealed class PointOfInterestConfiguration : IEntityTypeConfiguration<PointOfInterest>
{
    public void Configure(EntityTypeBuilder<PointOfInterest> builder)
    {
        //Table name
        builder.ToTable(
            name: TableMetadata.PointOfInterest,
            schema: SchemaMetadata.Map,
            t => t.HasCheckConstraint("ck_pointsofinterest_type", EnumColumn.Check<PointOfInterestType>("type")));

        //Column names
        builder.Property(x => x.PointOfInterestId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.Description).HasColumnName("description");
        builder.Property(x => x.Type).HasColumnName("type")
            .HasComment(EnumColumn.Comment<PointOfInterestType>("Category of the point of interest."));
        builder.Property(x => x.Latitude).HasColumnName("latitude");
        builder.Property(x => x.Longitude).HasColumnName("longitude");
        builder.Property(x => x.Address).HasColumnName("address");
        builder.Property(x => x.Color).HasColumnName("color");
        builder.Property(x => x.GroupId).HasColumnName("groupid");
        builder.Property(x => x.Active).HasColumnName("active");
        builder.Property(x => x.AccountId).HasColumnName("accountid");

        builder.Property(x => x.Name)
            .HasMaxLength(ColumnMetadata.DefaultNameLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(ColumnMetadata.DefaultDescriptionLength);

        builder.Property(x => x.Address)
            .HasMaxLength(ColumnMetadata.DefaultDescriptionLength);

        builder
            .HasOne(d => d.Account)
            .WithMany()
            .HasForeignKey(d => d.AccountId)
            .IsRequired();

        builder
            .HasOne(d => d.Group)
            .WithMany()
            .HasForeignKey(d => d.GroupId)
            .IsRequired(false);

        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => new { x.AccountId, x.Active });
    }
}
