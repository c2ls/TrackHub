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

using Common.Domain.Constants;
using Common.Domain.Extensions;
using Microsoft.Extensions.Logging;
using TrackHub.Security.Infrastructure.Entities;
using TrackHub.Security.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Action = TrackHub.Security.Infrastructure.Entities.Action;

namespace DBInitializer;

// The RBAC seed DATA lives in IRbacSeedContribution implementations (one file per module,
// discovered from this assembly); this class owns the seeding MECHANICS: the fixed action
// catalog, roles, policies, the Administrator grant-all, the default admin user, and the
// idempotent additive writes.
internal class ApplicationDbContextInitializer(ILogger<ApplicationDbContextInitializer> logger, ApplicationDbContext context)
{
    private static readonly IReadOnlyList<IRbacSeedContribution> Contributions =
        [.. typeof(ApplicationDbContextInitializer).Assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } && typeof(IRbacSeedContribution).IsAssignableFrom(t))
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .Select(t => (IRbacSeedContribution)Activator.CreateInstance(t)!)];

    private static readonly string[] DefaultActions =
    [
        Actions.Read,
        Actions.Edit,
        Actions.Export,
        Actions.Execute,
        Actions.Write,
        Actions.Delete
    ];

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default data
        // Seed, if necessary
        foreach (var resourceName in Contributions.SelectMany(c => c.Resources).Distinct())
        {
            if (!await context.Resources.AnyAsync(x => x.ResourceName == resourceName))
            {
                context.Resources.Add(new Resource { ResourceName = resourceName });
            }
        }

        await context.SaveChangesAsync();

        foreach (var actionName in DefaultActions.Append(Actions.Custom))
        {
            if (!await context.Actions.AnyAsync(x => x.ActionName == actionName))
            {
                context.Actions.Add(new Action { ActionName = actionName });
            }
        }

        await context.SaveChangesAsync();

        var resources = await context.Resources.ToListAsync();
        var actions = await context.Actions.ToListAsync();
        var standardActions = actions.Where(x => DefaultActions.Contains(x.ActionName)).ToList();

        foreach (var resource in resources)
        {
            foreach (var action in standardActions)
            {
                if (!await context.ResourceActions.AnyAsync(x => x.ResourceId == resource.ResourceId && x.ActionId == action.ActionId))
                {
                    context.ResourceActions.Add(new ResourceAction { ResourceId = resource.ResourceId, ActionId = action.ActionId });
                }
            }
        }

        var customAction = await context.Actions.FirstAsync(x => x.ActionName == Actions.Custom);
        foreach (var resourceName in Contributions.SelectMany(c => c.CustomActionResources).Distinct())
        {
            var resource = await context.Resources.FirstAsync(x => x.ResourceName == resourceName);
            if (!await context.ResourceActions.AnyAsync(x => x.ResourceId == resource.ResourceId && x.ActionId == customAction.ActionId))
            {
                context.ResourceActions.Add(new ResourceAction { ResourceId = resource.ResourceId, ActionId = customAction.ActionId });
            }
        }

        await context.SaveChangesAsync();
        if (!context.Roles.Any())
        {
            // Seed the parents first, then resolve their generated ids by name so the
            // Administrator -> Manager -> User hierarchy is wired up without hardcoding row ids.
            context.Roles.Add(new Role { Name = Roles.Administrator, Description = string.Empty });
            await context.SaveChangesAsync();

            var administratorRole = await context.Roles.FirstAsync(x => x.Name == Roles.Administrator);
            context.Roles.Add(new Role { Name = Roles.Manager, Description = string.Empty, ParentRoleId = administratorRole.RoleId });
            await context.SaveChangesAsync();

            var managerRole = await context.Roles.FirstAsync(x => x.Name == Roles.Manager);
            context.Roles.Add(new Role { Name = Roles.User, Description = string.Empty, ParentRoleId = managerRole.RoleId });
            await context.SaveChangesAsync();
        }

        if (!context.Policies.Any())
        {
            context.Policies.Add(new Policy { Name = "FullAccess", Description = string.Empty });
            context.Policies.Add(new Policy { Name = "ManageUsers", Description = string.Empty });
            context.Policies.Add(new Policy { Name = "ReadOnly", Description = string.Empty });
            context.Policies.Add(new Policy { Name = "LimitedUpdate", Description = string.Empty });
            context.Policies.Add(new Policy { Name = "Audit", Description = string.Empty });
            await context.SaveChangesAsync();
        }

        // Administrator keeps grant-all (every catalogued resource/action pair).
        await SeedAdministratorResourceActionsAsync();

        // Role matrices from every contribution. SeedRoleResourceActionsAsync is idempotent and
        // silently skips any resource/action pair that isn't in the ResourceAction catalog, so
        // resources not listed for a role receive no grant. Administrator is intentionally absent
        // from the matrices — it is handled by SeedAdministratorResourceActionsAsync above.
        foreach (var contribution in Contributions)
        {
            foreach (var (roleName, grants) in contribution.RoleGrants)
            {
                foreach (var (resource, actionNames) in grants)
                {
                    await SeedRoleResourceActionsAsync(roleName, resource, actionNames);
                }
            }
        }

        // Service-client identities and their allowlists. IsValidServiceAsync requires the client
        // NAME to exist in security.clients before any permission row is even consulted.
        await SeedServiceClientRegistrationsAsync();
        foreach (var contribution in Contributions)
        {
            foreach (var (clients, grants) in contribution.ServiceClientGrants)
            {
                await SeedServiceClientPermissionsAsync(clients, grants);
            }
        }

        if (!context.Users.Any())
        {
            var password = "12345678".HashPassword();
            context.Users.Add(new User(
                "Administrator",
                password,
                "email@mail.com",
                "Admin",
                "Admin",
                "",
                null,
                null,
                true,
                0,
                Guid.NewGuid()));

            await context.SaveChangesAsync();

            var user = context.Users.First();
            var admin = context.Roles.First(x => x.Name == Roles.Administrator);
            var manager = context.Roles.First(x => x.Name == Roles.Manager);
            context.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = admin.RoleId });
            context.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = manager.RoleId });

            await context.SaveChangesAsync();
        }
    }

    private async Task SeedRoleResourceActionsAsync(string roleName, string resourceName, string[] actionNames)
    {
        var role = await context.Roles.FirstOrDefaultAsync(x => x.Name == roleName);
        var resource = await context.Resources.FirstOrDefaultAsync(x => x.ResourceName == resourceName);
        if (role is null || resource is null)
        {
            return;
        }

        foreach (var actionName in actionNames)
        {
            var action = await context.Actions.FirstOrDefaultAsync(x => x.ActionName == actionName);
            if (action is null)
            {
                continue;
            }

            if (!await context.ResourceActionRole.AnyAsync(x =>
                    x.RoleId == role.RoleId
                    && x.ResourceId == resource.ResourceId
                    && x.ActionId == action.ActionId))
            {
                context.ResourceActionRole.Add(new ResourceActionRole
                {
                    ResourceId = resource.ResourceId,
                    ActionId = action.ActionId,
                    RoleId = role.RoleId
                });
            }
        }

        await context.SaveChangesAsync();
    }

    // The service identities every deployment needs. The real credential (secret) lives in
    // the AuthorityServer's OpenIddict application store; this row is the app-level allowlist
    // that IsValidClientAsync checks by NAME. Without it, every service-token call is denied.
    private async Task SeedServiceClientRegistrationsAsync()
    {
        foreach (var name in Contributions.SelectMany(c => c.ServiceClientNames).Distinct())
        {
            if (await context.Clients.AnyAsync(c => c.Name == name))
            {
                continue;
            }

            context.Clients.Add(new Client(
                name,
                userId: null,
                description: "Service client (seeded; credential managed by the AuthorityServer)",
                secret: string.Empty,
                salt: string.Empty,
                processed: true));
        }

        await context.SaveChangesAsync();
    }

    // Every identity seeded here is a PLATFORM-INTERNAL service client: it runs with no account
    // claim and legitimately operates across every tenant (Router/SyncWorker feed positions for all
    // accounts; geofence/trip/security clients emit events and job runs for all accounts). That
    // reach used to be implicit — a NULL accountid silently matched any account. It is now
    // declared: allowCrossAccount = true. A PARTNER/tenant-bound client must be seeded WITHOUT this
    // flag and WITH an accountid, so its grant only matches a token carrying that same account.
    private async Task SeedServiceClientPermissionsAsync(
        string[] clients,
        (string Resource, string Action)[] grants,
        bool allowCrossAccount = true)
    {
        const string serviceScope = "service_scope";
        const string serviceAudience = "trackhub_api";

        foreach (var clientId in clients)
        {
            foreach (var (resource, action) in grants)
            {
                var existing = await context.ServiceClientPermissions.FirstOrDefaultAsync(p =>
                        p.ClientId == clientId
                        && p.Resource == resource
                        && p.Action == action
                        && p.Scope == serviceScope
                        && p.Audience == serviceAudience);

                if (existing is not null)
                {
                    // Upgrade path: rows seeded before the flag existed carry the default false.
                    // Re-running db-init restores the declared reach for the internal identities.
                    if (existing.AllowCrossAccount != allowCrossAccount)
                    {
                        existing.AllowCrossAccount = allowCrossAccount;
                    }

                    continue;
                }

                context.ServiceClientPermissions.Add(new ServiceClientPermission(
                    clientId,
                    accountId: null,
                    resource,
                    action,
                    serviceScope,
                    serviceAudience,
                    active: true,
                    allowCrossAccount: allowCrossAccount));
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedAdministratorResourceActionsAsync()
    {
        var administrator = await context.Roles.FirstAsync(x => x.Name == Roles.Administrator);
        var resourceActions = await context.ResourceActions.ToListAsync();

        foreach (var resourceAction in resourceActions)
        {
            if (!await context.ResourceActionRole.AnyAsync(x =>
                    x.RoleId == administrator.RoleId
                    && x.ResourceId == resourceAction.ResourceId
                    && x.ActionId == resourceAction.ActionId))
            {
                context.ResourceActionRole.Add(new ResourceActionRole
                {
                    ResourceId = resourceAction.ResourceId,
                    ActionId = resourceAction.ActionId,
                    RoleId = administrator.RoleId
                });
            }
        }

        await context.SaveChangesAsync();
    }
}
