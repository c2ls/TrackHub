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

public sealed class TransporterTollClassConfiguration : IEntityTypeConfiguration<TransporterTollClass>
{
    public void Configure(EntityTypeBuilder<TransporterTollClass> builder)
    {
        // Exactly one of the two keys must be set. Without this a both-NULL wildcard row and a
        // both-set ambiguous row are equally insertable, and neither has a defined meaning for
        // TransporterTollClassStore.ResolveClassAsync.
        builder.ToTable(
            name: TableMetadata.TransporterTollClass,
            schema: SchemaMetadata.Trip,
            t => t.HasCheckConstraint(
                "ck_transporter_toll_classes_type_xor_transporter",
                "(transportertypeid is null) <> (transporterid is null)"));
        builder.HasKey(x => x.TransporterTollClassId);

        builder.Property(x => x.TransporterTollClassId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.TransporterTypeId).HasColumnName("transportertypeid");
        builder.Property(x => x.TransporterId).HasColumnName("transporterid");
        builder.Property(x => x.TollVehicleClassCode).HasColumnName("tollvehicleclasscode").HasMaxLength(20).IsRequired();

        // A single unique index over (accountid, transportertypeid, transporterid) constrained
        // NOTHING for the two shapes actually stored: PostgreSQL treats NULLs as distinct, so every
        // type-level mapping (transporterid IS NULL) and every row-level override
        // (transportertypeid IS NULL) escaped it entirely, and the store's read-then-write upsert
        // had no DB backstop — two concurrent SetMapping calls both inserted and toll-class
        // resolution became non-deterministic. Two PARTIAL unique indexes, one per shape, are real
        // constraints over columns that are non-null in the rows they cover.
        builder.HasIndex(x => new { x.AccountId, x.TransporterTypeId }, "transporter_toll_classes_type")
            .HasDatabaseName("ux_transporter_toll_classes_acct_type")
            .IsUnique()
            .HasFilter("transporterid is null");

        builder.HasIndex(x => new { x.AccountId, x.TransporterId }, "transporter_toll_classes_override")
            .HasDatabaseName("ux_transporter_toll_classes_acct_transporter")
            .IsUnique()
            .HasFilter("transportertypeid is null");

        // A dangling class code matched no tariff, contributed 0 and reported PartialNoTariff — a
        // typo disguised as a legitimate catalog gap, which is exactly the silent understatement
        // §6.2 forbids. Restrict, not Cascade: deleting a vehicle class must not silently unmap a
        // tenant's fleet.
        builder.HasIndex(x => x.TollVehicleClassCode, "transporter_toll_classes_classcode_fk")
            .HasDatabaseName("ix_transporter_toll_classes_tollvehicleclasscode");

        builder.HasOne<TollVehicleClass>()
            .WithMany()
            .HasForeignKey(x => x.TollVehicleClassCode)
            .HasPrincipalKey(c => c.Code)
            .HasConstraintName("fk_transporter_toll_classes_tollvehicleclasscode")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
