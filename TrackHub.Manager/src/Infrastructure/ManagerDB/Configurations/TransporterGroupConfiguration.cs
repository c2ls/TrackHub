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

internal class TransporterGroupConfiguration : IEntityTypeConfiguration<TransporterGroup>
{
    public void Configure(EntityTypeBuilder<TransporterGroup> builder)
    {
        //Table name
        builder.ToTable(name: TableMetadata.TransporterGroup, schema: SchemaMetadata.Application);

        //Column names
        builder.Property(x => x.TransporterId).HasColumnName("transporterid");
        builder.Property(x => x.GroupId).HasColumnName("groupid");

    }
}
