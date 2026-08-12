// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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

using Common.Infrastructure;
using TrackHub.AuthorityServer.Infrastructure.Configurations;
using TrackHub.AuthorityServer.Infrastructure.Entities;
using TrackHub.AuthorityServer.Infrastructure.Interfaces;

namespace TrackHub.AuthorityServer.Infrastructure;

public class SecurityDbContext(DbContextOptions<SecurityDbContext> options) : DbContext(options), ISecurityDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<DriverCredential> DriverCredentials { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new UserConfiguration());
        builder.ApplyConfiguration(new ClientConfiguration());
        builder.ApplyConfiguration(new DriverCredentialConfiguration());
        builder.ApplyConfiguration(new RoleConfiguration());
        builder.ApplyConfiguration(new UserRoleConfiguration());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.UseUtcTimestamps();
    }
}
