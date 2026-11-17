# TrackHub Security

The authorization model: users, roles, policies, the resource/action catalog, staff credentials and
service-client grants, in the `security` schema of the `TrackHubSecurity` database. Every service
asks it, on each request, whether the caller may perform a `Resource + Action`; access is granted if
the role path **or** the policy path allows it. Default roles: Administrator, Manager, User, Audit.

Wiki: [Security and Identity](https://github.com/shernandezp/TrackHub/wiki/Security-and-Identity),
[User Permissions Overview](https://github.com/shernandezp/TrackHub/wiki/User-Permissions-Overview).

## Build, run, test

From this folder:

```bash
dotnet build TrackHub.Security.slnx
dotnet test TrackHub.Security.slnx

dotnet ef database update --project src/Infrastructure/SecurityDB --startup-project src/Web

# roles, resources, actions, role matrices, service-client names and grants
dotnet run --project src/DBInitializer

dotnet run --project src/Web          # https://localhost:5001/graphql
```

Settings: `ConnectionStrings:Security` in `src/Web/appsettings.json` and
`src/DBInitializer/appsettings.json`. Apply this migration before the
[AuthorityServer](../TrackHub.AuthorityServer) one — both target `TrackHubSecurity`.

## Gotchas

- **Seed data lives in `IRbacSeedContribution` files** under `src/DBInitializer`. A new resource
  must be listed in a contribution's `Resources` (and in `CustomActionResources` if it uses
  `Actions.Custom`) **and** granted in each role's `RoleGrants`. Role grants match role names
  exactly — nothing is inherited — so a missing grant is a `FORBIDDEN` for every non-Administrator.
  Administrator receives every resource-action automatically.
- **Seeding only ever adds.** Removing a grant from a contribution does not remove it from a
  seeded database; retracted grants need explicit cleanup SQL.
- **A service identity needs two registrations**: its name in `ServiceClientNames`
  (`security.clients`) and its grants in `security.service_client_permissions`. With only the first,
  every call returns `FORBIDDEN` while the calling service reports healthy.
- **`allowcrossaccount = true` marks a platform-internal identity**, whose token carries no
  `account_id`. A partner client is seeded with an `accountid` and without `allowcrossaccount`.
- **Identity queries pin the subject to the caller** (`IdentityCallerGuard`): a user token may only
  ask about itself, a service token only about its own client.
- Callers cache authorization decisions for `AppSettings:AuthorizationCacheSeconds` (default 30),
  so a permission change can take that long to apply.
- Password policy: at least 8 characters with an uppercase letter, a lowercase letter and a digit.
- After changing the GraphQL surface, run `dotnet test ../TrackHub.IntegrationTests` and then the
  portal's `npm run codegen`.
