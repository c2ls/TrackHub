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
using TrackHub.AuthorityServer.Infrastructure.Entities;

namespace TrackHub.AuthorityServer.Infrastructure.Configurations;

public sealed class DriverCredentialConfiguration : IEntityTypeConfiguration<DriverCredential>
{
    public void Configure(EntityTypeBuilder<DriverCredential> builder)
    {
        builder.ToTable(name: TableMetadata.DriverCredential, schema: SchemaMetadata.Security);

        builder.Property(x => x.DriverCredentialId).HasColumnName("id");
        builder.Property(x => x.DriverId).HasColumnName("driverid");
        builder.Property(x => x.AccountId).HasColumnName("accountid");
        builder.Property(x => x.NormalizedLogin).HasColumnName("normalizedlogin");
        builder.Property(x => x.PasswordHash).HasColumnName("passwordhash");
        builder.Property(x => x.FailedAttempts).HasColumnName("failedattempts");
        builder.Property(x => x.LockedUntil).HasColumnName("lockeduntil");
        builder.Property(x => x.VerifiedAt).HasColumnName("verifiedat");
        builder.Property(x => x.LastLoginAt).HasColumnName("lastloginat");
        builder.Property(x => x.Active).HasColumnName("active");
        builder.Property(x => x.ResetRequired).HasColumnName("resetrequired");
        builder.Property(x => x.LastModified).HasColumnName("LastModified");

        builder.Property(x => x.NormalizedLogin).HasMaxLength(ColumnMetadata.DefaultUserNameLength).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(ColumnMetadata.DefaultPasswordLength).IsRequired();
    }
}
