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

public sealed class TollTariffConfiguration : IEntityTypeConfiguration<TollTariff>
{
    public void Configure(EntityTypeBuilder<TollTariff> builder)
    {
        builder.ToTable(name: TableMetadata.TollTariff, schema: SchemaMetadata.Trip);
        builder.HasKey(x => x.TollTariffId);

        builder.Property(x => x.TollTariffId).HasColumnName("id");
        builder.Property(x => x.TollStationId).HasColumnName("tollstationid");
        builder.Property(x => x.TollVehicleClassCode).HasColumnName("tollvehicleclasscode").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(x => x.EffectiveFrom).HasColumnName("effectivefrom");
        builder.Property(x => x.EffectiveTo).HasColumnName("effectiveto");

        // Two indexes over the same pair, so each needs an explicit model name or the second
        // definition would silently replace the first. This one serves tariff RESOLUTION, which
        // reads closed windows too and therefore cannot use the partial index below.
        builder.HasIndex(x => new { x.TollStationId, x.TollVehicleClassCode }, "toll_tariffs_lookup")
            .HasDatabaseName("ix_toll_tariffs_tollstationid_classcode");

        // At most ONE open row per (station, class): a price change closes the current row and
        // inserts a new one, so a historical trip estimate stays reproducible (acceptance 21).
        builder.HasIndex(x => new { x.TollStationId, x.TollVehicleClassCode }, "toll_tariffs_open")
            .HasDatabaseName("ux_toll_tariffs_station_class_open")
            .IsUnique()
            .HasFilter("effectiveto is null");

        // The class code was a dangling varchar(20) validated for length only: a typo'd code was
        // accepted, then matched no tariff, contributed 0 and reported PartialNoTariff — a
        // data-entry error disguised as a legitimate catalog gap (§6.2 forbids exactly that silent
        // understatement). Restrict, not Cascade: a vehicle class must not be deletable out from
        // under priced history, because tariffs are temporal so historical estimates stay
        // reproducible (acceptance 21).
        builder.HasIndex(x => x.TollVehicleClassCode, "toll_tariffs_classcode_fk")
            .HasDatabaseName("ix_toll_tariffs_tollvehicleclasscode");

        builder.HasOne<TollVehicleClass>()
            .WithMany()
            .HasForeignKey(x => x.TollVehicleClassCode)
            .HasPrincipalKey(c => c.Code)
            .HasConstraintName("fk_toll_tariffs_tollvehicleclasscode")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TollStation)
            .WithMany()
            .HasForeignKey(x => x.TollStationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
