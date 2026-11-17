# TrackHub AuthorityServer

The OpenIddict identity server. It signs users, staff and service clients in, issues the access
tokens (audience `trackhub_api`) every other service validates, and hosts the login UI. It owns
only the `OpenIddict*` tables in the `TrackHubSecurity` database; users, roles and service-client
grants belong to [TrackHubSecurity](../TrackHubSecurity).

Wiki: [Security and Identity](https://github.com/shernandezp/TrackHub/wiki/Security-and-Identity).

## Build, run, test

From this folder:

```bash
dotnet build AuthorityServer.slnx
dotnet test AuthorityServer.slnx

# OpenIddict tables — after the TrackHubSecurity migration, same database
dotnet ef database update --project src/Infrastructure --startup-project src/Web --context AuthorityDbContext

# OAuth scopes and clients from src/ClientSeeder/clients.json (plus any clients.*.json overlay)
dotnet run --project src/ClientSeeder

dotnet run --project src/Web          # https://localhost:7158
```

Settings: `ConnectionStrings:Security` (and `ConnectionStrings:Logging`) in `src/Web/appsettings.json`,
and the token-signing certificate under `OpenIddict` (`LoadCertFromFile`, `Path`, `Password`). To
create a development certificate at the committed path:

```powershell
$cert = New-SelfSignedCertificate -DnsName "localhost" -CertStoreLocation "Cert:\CurrentUser\My"
$password = ConvertTo-SecureString -String "openiddict" -Force -AsPlainText
New-Item -ItemType Directory -Force C:\Certificates | Out-Null
Export-PfxCertificate -Cert $cert -FilePath "C:\Certificates\certificate.pfx" -Password $password
```

Every service validates tokens with the same file, so they must all point at it.

## Gotchas

- **`--context AuthorityDbContext` is required.** The project also carries a `SecurityDbContext`, a
  read-only projection of Security-owned tables whose migration is deliberately empty
  (`ExcludeFromMigrations()`). Never add DDL there.
- **The seeder never creates tables.** It stops with a diagnostic when the OpenIddict schema is
  missing; apply the migration first.
- **A service client's secret comes from the environment when set.** For each service client the
  seeder reads `{CLIENTID}_SECRET` (e.g. `TRIP_CLIENT_SECRET`) and lets it win over `clients.json`;
  every `*_CLIENT_SECRET` variable also registers its client even when `clients.json` omits it.
- **A service client's tenant is derived, never supplied.** The `account_id` claim comes from the
  client's active rows in `security.service_client_permissions`:

  | Client's effective grants | `account_id` claim |
  |---|---|
  | At least one `allowcrossaccount` grant (platform-internal identities) | Not issued — the token stays unscoped |
  | Grants naming exactly one account | Issued automatically |
  | Grants naming several accounts | Requires the `account_id` request parameter |
  | No account-bound grant | Not issued |

  The `account_id` parameter can only narrow to one of the client's own grants. An invalid value,
  an ungranted account, or a multi-account client omitting it answers `invalid_request`.
- **`UseCors` must run before `UseHealthChecks`**, which is terminal — otherwise the public status
  page cannot read the Sign-in tile cross-origin.
- **The issuer comes from the `Host` header.** `ForwardedHeaders` trusts `X-Forwarded-Proto` and
  `X-Forwarded-For` only, so a reverse proxy must forward the original host and port.
- **The role claim is for visibility scoping only.** Permissions are always checked by the Security
  API; users sign in again to pick up a new role.
- Login and validation messages come from `.resx` resources through `ResourceLocalizer`; never
  hardcode them.
