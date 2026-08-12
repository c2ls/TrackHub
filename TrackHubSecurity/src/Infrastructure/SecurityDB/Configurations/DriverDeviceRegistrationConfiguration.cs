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

namespace TrackHub.Security.Infrastructure.SecurityDB.Configurations;

public class DriverDeviceRegistrationConfiguration : IEntityTypeConfiguration<DriverDeviceRegistration>
{
    public void Configure(EntityTypeBuilder<DriverDeviceRegistration> builder)
    {
        builder.ToTable(name: TableMetadata.DriverDeviceRegistration, schema: SchemaMetadata.Security);

        builder.Property(x => x.DriverDeviceRegistrationId).HasColumnName("id");
        builder.Property(x => x.DriverId).HasColumnName("driverid");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.DeviceId).HasColumnName("deviceid");
        builder.Property(x => x.DeviceName).HasColumnName("devicename");
        builder.Property(x => x.Platform).HasColumnName("platform");
        builder.Property(x => x.AppVersion).HasColumnName("appversion");
        builder.Property(x => x.PushToken).HasColumnName("pushtoken");
        builder.Property(x => x.RefreshTokenFamilyId).HasColumnName("refreshtokenfamilyid");
        builder.Property(x => x.Active).HasColumnName("active");
        builder.Property(x => x.RegisteredAt).HasColumnName("registeredat");
        builder.Property(x => x.LastSeenAt).HasColumnName("lastseenat");
        builder.Property(x => x.RevokedAt).HasColumnName("revokedat");
        builder.Property(x => x.RevokedBy).HasColumnName("revokedby");

        builder.Property(x => x.DeviceId).HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Platform).HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.RefreshTokenFamilyId).HasMaxLength(ColumnMetadata.DefaultTokenLength).IsRequired();

        builder.HasIndex(x => new { x.DriverId, x.DeviceId });
        builder.HasIndex(x => x.RefreshTokenFamilyId);
    }
}
