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

namespace TrackHub.Geofencing.Infrastructure.ManagerDB.Configurations;

public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        // Manager-owned table: never part of this repo's migrations
        builder.ToTable(name: TableMetadata.AuditEvent, schema: SchemaMetadata.Application, t => t.ExcludeFromMigrations());
        builder.Property(x => x.AuditEventId).HasColumnName("id");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.ActorType).HasColumnName("actortype").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.ActorId).HasColumnName("actorid").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Action).HasColumnName("action").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.ResourceType).HasColumnName("resourcetype").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.ResourceId).HasColumnName("resourceid").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.Result).HasColumnName("result").HasMaxLength(ColumnMetadata.DefaultNameLength).IsRequired();
        builder.Property(x => x.OldValuesJson).HasColumnName("oldvaluesjson").HasColumnType(ColumnMetadata.TextField);
        builder.Property(x => x.NewValuesJson).HasColumnName("newvaluesjson").HasColumnType(ColumnMetadata.TextField);
        builder.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(ColumnMetadata.DefaultDescriptionLength);
        builder.Property(x => x.IpAddress).HasColumnName("ipaddress").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.UserAgent).HasColumnName("useragent").HasMaxLength(ColumnMetadata.DefaultDescriptionLength);
        builder.Property(x => x.CorrelationId).HasColumnName("correlationid").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.OccurredAt).HasColumnName("occurredat");
        builder.HasKey(x => x.AuditEventId);
    }
}
