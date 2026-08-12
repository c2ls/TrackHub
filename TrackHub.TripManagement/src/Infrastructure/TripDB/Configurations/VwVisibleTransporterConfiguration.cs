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

public sealed class VwVisibleTransporterConfiguration : IEntityTypeConfiguration<VwVisibleTransporter>
{
    public void Configure(EntityTypeBuilder<VwVisibleTransporter> builder)
    {
        builder.ToView(name: ViewMetadata.VwVisibleTransporter, schema: SchemaMetadata.Trip);
        builder.Property(x => x.TransporterId).HasColumnName("transporterid");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.UserId).HasColumnName("userid");

        // A (user, transporter) pair is the natural grain; the view can repeat it across groups,
        // so readers always de-duplicate with an EXISTS predicate rather than a join.
        builder.HasNoKey();
    }
}
