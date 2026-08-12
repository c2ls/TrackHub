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

namespace TrackHub.Geofencing.Infrastructure.Configurations;
public class VwUserConfiguration : IEntityTypeConfiguration<VwUser>
{
    public void Configure(EntityTypeBuilder<VwUser> builder)
    {
        //Table name
        builder.ToView(name: ViewMetadata.VwUsers, schema: SchemaMetadata.Geofencing);

        //Column names
        builder.Property(x => x.UserId).HasColumnName("id");
        builder.Property(x => x.Username).HasColumnName("username");
        builder.Property(x => x.AccountId).HasColumnName("accountid");

        builder.HasKey(e => e.UserId);
    }
}

