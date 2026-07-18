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

namespace TrackHub.Telemetry.Infrastructure.TelemetryDB.Configurations;

// Read-only scoping map: Telemetry reads this app-schema table cross-schema; it never writes it.
public sealed class OperatorConfiguration : IEntityTypeConfiguration<Operator>
{
    public void Configure(EntityTypeBuilder<Operator> builder)
    {
        builder.ToTable(name: TableMetadata.Operator, schema: SchemaMetadata.Application);
        builder.HasKey(x => x.OperatorId);
        builder.Property(x => x.OperatorId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
    }
}
