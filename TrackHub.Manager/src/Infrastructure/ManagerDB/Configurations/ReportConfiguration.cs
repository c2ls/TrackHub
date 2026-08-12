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

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        //Table name
        builder.ToTable(
            name: TableMetadata.Report,
            schema: SchemaMetadata.Application,
            t => t.HasCheckConstraint("ck_reports_type", EnumColumn.Check<ReportType>("type")));

        //Column names
        builder.Property(x => x.ReportId).HasColumnName("id");
        builder.Property(x => x.Code).HasColumnName("code");
        builder.Property(x => x.Description).HasColumnName("description");
        builder.Property(x => x.Type).HasColumnName("type")
            .HasComment(EnumColumn.Comment<ReportType>("Provenance of the report definition."));
        builder.Property(x => x.Active).HasColumnName("active");
        builder.Property(x => x.Category).HasColumnName("category").HasMaxLength(ColumnMetadata.DefaultFieldLength).IsRequired();
        builder.Property(x => x.RequiredFeatureKey).HasColumnName("requiredfeaturekey").HasMaxLength(ColumnMetadata.DefaultNameLength);
        builder.Property(x => x.ManagerOnly).HasColumnName("manageronly");
        builder.Property(x => x.SupportsPdf).HasColumnName("supportspdf");
        builder.Property(x => x.SortOrder).HasColumnName("sortorder");

        builder.Property(t => t.Description)
            .HasMaxLength(ColumnMetadata.DefaultDescriptionLength)
            .IsRequired();

    }
}
