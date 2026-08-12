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

using System.Reflection;
using Common.Infrastructure;
using TrackHub.Security.Infrastructure.SecurityDB.Interfaces;
using Action = TrackHub.Security.Infrastructure.SecurityDB.Entities.Action;

namespace TrackHub.Security.Infrastructure.SecurityDB;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<Action> Actions { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<DriverCredential> DriverCredentials { get; set; }
    public DbSet<DriverDeviceRegistration> DriverDeviceRegistrations { get; set; }
    public DbSet<Policy> Policies { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<ResourceAction> ResourceActions { get; set; }
    public DbSet<ResourceActionPolicy> ResourceActionPolicy { get; set; }
    public DbSet<ResourceActionRole> ResourceActionRole { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<ServiceClientPermission> ServiceClientPermissions { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserPolicy> UserPolicies { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.UseUtcTimestamps();
    }
}
