# TrackHub Security API

[← Back to the landing page](README.md) · [Español](README.es.md)

The Security API owns TrackHub's **authorization model** and answers the identity and permission checks every other service makes on each request.

Built on .NET 10 with a HotChocolate GraphQL endpoint, following the platform's Clean Architecture and CQRS conventions.

---

## What it owns

| Area | Tables |
|---|---|
| **Identity** | `security.users`, `security.driver_credentials`, `security.driver_device_registrations` |
| **RBAC** | `security.roles`, `security.user_role`, `security.resource_action_role` |
| **Policies** | `security.policies`, `security.user_policy`, `security.resource_action_policy` |
| **Catalog** | `security.resources`, `security.actions`, `security.resource_action` |
| **Service clients** | `security.clients`, `security.service_client_permissions` |

Authorization is **dual-path**: access is granted if *either* the role path or the policy path resolves the requested `Resource + Action` pair.

Default roles: **Administrator** (full access), **Manager** (account-wide operational and administrative access), **User** (group-scoped), **Audit** (read-only audit access).

Full detail: **[Security and Identity](https://github.com/shernandezp/TrackHub/wiki/Security-and-Identity)** and **[User Permissions Overview](https://github.com/shernandezp/TrackHub/wiki/User-Permissions-Overview)** in the wiki.

---

## Quick start

### Prerequisites

- .NET 10 SDK
- PostgreSQL 14+
- A running TrackHub AuthorityServer, for authentication
- The `TrackHubCommon.*` packages available from a local NuGet feed

### Steps

1. **Clone**

   ```bash
   git clone https://github.com/shernandezp/TrackHubSecurity.git
   cd TrackHubSecurity
   ```

2. **Configure the database connection** in `src/Web/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=TrackHubSecurity;Username=postgres;Password=yourpassword"
     }
   }
   ```

3. **Apply migrations**

   ```bash
   dotnet ef database update --project src/Infrastructure --startup-project src/Web
   ```

4. **Seed roles, resources, actions and service-client permissions**

   ```bash
   dotnet run --project src/DBInitializer
   ```

5. **Run**

   ```bash
   dotnet run --project src/Web
   ```

6. **Open the GraphQL endpoint** at `https://localhost:<port>/graphql`.

---

## Project-specific notes

- **Adding a resource-action row and granting it to a role are two separate steps.** A new `Actions.Custom` pair needs both the `ResourceAction` seed (the `foreach (var resourceName …)` list) *and* an entry in each role's matrix. `ResourceActionRoleReader` matches role names **exactly**, with no `ParentRoleId` walk — nothing is inherited. Missing the second step returns `FORBIDDEN` for every non-Administrator on an otherwise correctly seeded deployment.
- **`SeedRoleResourceActionsAsync` only ever ADDS rows.** Removing a grant from the matrix does not remove it from an already-seeded database; retracted grants need explicit cleanup SQL.
- **A service identity needs two registrations, not one.** The client name must exist in `security.clients` *and* its grants in `security.service_client_permissions` (client, resource, action, scope, audience). Missing the second yields a clean `FORBIDDEN` from every call while the calling service reports healthy — the most common "everything is denied after an upgrade" cause.
- **New authorization resources must be added to `DefaultResources`** in `ApplicationDbContextInitializer`. The Administrator role automatically receives every resource-action.
- **`allowcrossaccount = true` marks a platform-internal identity.** Such a client's token carries **no** `account_id` claim, which is what a platform-wide grant matches against. A partner client is seeded the opposite way: with an `accountid` and **without** `allowcrossaccount`.
- **Identity queries pin the subject to the caller** (`IdentityCallerGuard`): a user token may only ask about itself, a service token only about its own client.
- Authorization decisions are cached 30 s in each caller's `Common.Infrastructure.IdentityService`, so a permission change takes up to that long to take effect.
- Password policy: minimum length 8, with at least one uppercase letter, one lowercase letter and one digit.
- After changing any GraphQL surface, run the contract tests:

  ```bash
  dotnet test ../TrackHub.IntegrationTests/TrackHub.IntegrationTests.slnx
  ```

---

## Documentation

- **Technical** — the [TrackHub wiki](https://github.com/shernandezp/TrackHub/wiki): [Security and Identity](https://github.com/shernandezp/TrackHub/wiki/Security-and-Identity), [User Permissions Overview](https://github.com/shernandezp/TrackHub/wiki/User-Permissions-Overview), [Database](https://github.com/shernandezp/TrackHub/wiki/Database), [Architecture](https://github.com/shernandezp/TrackHub/wiki/Architecture)
- **User** — in the app: the Help button or **F1** on any screen
- **Deployment** — [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment)

---

## License

Apache License 2.0. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.
