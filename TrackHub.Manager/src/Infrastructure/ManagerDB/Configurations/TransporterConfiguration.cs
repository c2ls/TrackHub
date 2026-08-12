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
public sealed class TransporterConfiguration : IEntityTypeConfiguration<Transporter>
{
    public void Configure(EntityTypeBuilder<Transporter> builder)
    {
        //Table name
        builder.ToTable(name: TableMetadata.Transporter, schema: SchemaMetadata.Application);

        //Column names
        builder.Property(x => x.TransporterId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.TransporterTypeId).HasColumnName("transportertypeid");
        builder.Property(x => x.AccountId).HasColumnName("accountid");

        builder.Property(t => t.Name)
            .HasMaxLength(ColumnMetadata.DefaultNameLength)
            .IsRequired();

        builder
            .HasOne(d => d.Position)
            .WithOne(d => d.Transporter)
            .HasForeignKey<TransporterPosition>(d => d.TransporterId)
            .IsRequired(false);

        builder
            .HasOne(d => d.TransporterType)
            .WithMany()
            .HasForeignKey(d => d.TransporterTypeId)
            .IsRequired(false);

        builder
            .HasOne(d => d.Account)
            .WithMany(a => a.Transporters)
            .HasForeignKey(d => d.AccountId)
            .IsRequired();

        builder.HasIndex(t => t.AccountId);
    }
}
