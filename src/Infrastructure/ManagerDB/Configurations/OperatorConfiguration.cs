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

public sealed class OperatorConfiguration : IEntityTypeConfiguration<Operator>
{
    public void Configure(EntityTypeBuilder<Operator> builder)
    {
        //Table name
        builder.ToTable(
            name: TableMetadata.Operator,
            schema: SchemaMetadata.Application,
            t => t.HasCheckConstraint("ck_operators_protocoltype", EnumColumn.Check<ProtocolType>("protocoltype")));

        //Column names
        builder.Property(x => x.OperatorId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.Description).HasColumnName("description");
        builder.Property(x => x.PhoneNumber).HasColumnName("phonenumber");
        builder.Property(x => x.EmailAddress).HasColumnName("emailaddress");
        builder.Property(x => x.Address).HasColumnName("address");
        builder.Property(x => x.ContactName).HasColumnName("contactname");
        builder.Property(x => x.ProtocolType).HasColumnName("protocoltype")
            .HasComment(EnumColumn.Comment<ProtocolType>("Telematics protocol this operator speaks; selects the Router provider client."));
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.Enabled).HasColumnName("enabled").HasDefaultValue(true);
        builder.Property(x => x.SyncIntervalMinutes).HasColumnName("syncintervalminutes").HasDefaultValue(60);
        builder.Property(x => x.LastSuccessfulSyncAt).HasColumnName("lastsuccessfulsyncat");
        builder.Property(x => x.LastManualSyncAt).HasColumnName("lastmanualsyncat");
        builder.Property(x => x.LastDeviceSyncAt).HasColumnName("lastdevicesyncat");

        builder.Property(t => t.Name)
            .HasMaxLength(ColumnMetadata.DefaultNameLength)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(ColumnMetadata.DefaultDescriptionLength);

        builder.Property(t => t.PhoneNumber)
            .HasMaxLength(ColumnMetadata.DefaultPhoneNumberLength);

        builder.Property(t => t.EmailAddress)
            .HasMaxLength(ColumnMetadata.DefaultEmailLength);

        builder.Property(t => t.Address)
            .HasMaxLength(ColumnMetadata.DefaultAddressLength);

        builder.Property(t => t.ContactName)
            .HasMaxLength(ColumnMetadata.DefaultFieldLength);

        builder
            .HasOne(d => d.Credential)
            .WithOne(d => d.Operator)
            .HasForeignKey<Credential>(d => d.OperatorId)
            .IsRequired(false);

        builder.HasIndex(o => new { o.AccountId, o.Enabled, o.ProtocolType });
    }
}
