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
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.Configurations;

public class AccountSettingsConfiguration : IEntityTypeConfiguration<AccountSettings>
{
    public void Configure(EntityTypeBuilder<AccountSettings> builder)
    {
        //Table name
        builder.ToTable(name: TableMetadata.AccountSettings, schema: SchemaMetadata.Application);
        builder.HasKey(x => x.AccountId);

        //Column names
        builder.Property(x => x.AccountId).HasColumnName("id");
        builder.Property(x => x.Maps).HasColumnName("maps");
        builder.Property(x => x.MapsKey).HasColumnName("mapskey");
        builder.Property(x => x.OnlineInterval).HasColumnName("onlineinterval");
        builder.Property(x => x.RefreshMap).HasColumnName("refreshmap");
        builder.Property(x => x.RefreshMapInterval).HasColumnName("refreshmapinterval");

    }
}
