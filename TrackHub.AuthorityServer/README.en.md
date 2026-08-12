# TrackHub Authorization Service

[← Back to the landing page](README.md) · [Español](README.es.md)

The Authorization Service is TrackHub's **OpenIddict-based identity and OAuth 2.0 / OpenID Connect token server**. It authenticates users, drivers and service clients, and issues the access tokens (audience `trackhub_api`) that every other TrackHub service validates.

It also hosts the ASP.NET login UI, and owns the OpenIddict tables in the `TrackHubSecurity` database.

---

## What it provides

- **Authorization Code Flow with PKCE** for public clients — web, mobile and driver mobile
- **Client Credentials Flow** for backend services and partner integrations, with automatic tenant derivation
- **Token management** — issue, refresh and revoke, with audience-based scoping
- **A customizable login UI** with branding support
- **BCrypt-hashed credential validation** against the Security-owned `security.users` table
- **The role claim**, resolved at login and re-resolved on every refresh

Full detail: **[Security and Identity](https://github.com/shernandezp/TrackHub/wiki/Security-and-Identity)** and **[Technology](https://github.com/shernandezp/TrackHub/wiki/Technology)** in the wiki.

---

## Quick start

### Prerequisites

- .NET 10 SDK
- PostgreSQL 14+
- An SSL certificate — self-signed for development, CA-issued for production
- The `TrackHubCommon.*` packages available from a local NuGet feed

### Steps

1. **Clone**

   ```bash
   git clone https://github.com/shernandezp/TrackHub.AuthorityServer.git
   cd TrackHub.AuthorityServer
   ```

2. **Configure the database connection** in `src/Web/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=TrackHubSecurity;Username=postgres;Password=yourpassword"
     }
   }
   ```

3. **Generate a self-signed certificate** (development only):

   ```powershell
   $cert = New-SelfSignedCertificate -DnsName "localhost" -CertStoreLocation "Cert:\CurrentUser\My"
   $password = ConvertTo-SecureString -String "openiddict" -Force -AsPlainText
   Export-PfxCertificate -Cert $cert -FilePath "certificate.pfx" -Password $password
   ```

4. **Apply migrations** — this creates the OpenIddict tables:

   ```bash
   dotnet ef database update --project src/Infrastructure --startup-project src/Web
   ```

5. **Register the OAuth clients**

   ```bash
   dotnet run --project src/ClientSeeder
   ```

6. **Run**

   ```bash
   dotnet run --project src/Web
   ```

7. **Open the login page** at `https://localhost:<port>`.

---

## Project-specific notes

- **This service owns only the `OpenIddict*` tables.** Its `AuthorityDbContext` migration targets the `TrackHubSecurity` database and is part of the documented install sequence. Its `SecurityDbContext` is a narrow **read-only projection** of TrackHubSecurity-owned tables (`users`, `clients`, `driver_credentials`, `roles`, `user_role`, `service_client_permissions`), and its migration is deliberately **empty** — every configuration is `ExcludeFromMigrations()`. Do not add DDL there.
- **A service client's tenant is derived, never supplied by the caller.** The `account_id` claim comes from the client's active rows in `security.service_client_permissions`, so a partner credential cannot widen its own reach:

  | Client's effective grants | `account_id` claim |
  |---|---|
  | At least one `allowcrossaccount` grant (platform-internal identities) | **Not issued** — the token stays unscoped |
  | Grants naming exactly one account | Issued automatically |
  | Grants naming several accounts | **Requires the `account_id` request parameter** |
  | No account-bound grant | Not issued |

- **The optional `account_id` request parameter** lets a multi-account client name the tenant it wants. It can only *narrow*, never widen — the requested account must match one of the client's own active grants:

  ```bash
  curl -X POST https://<authority>/connect/token \
    -d grant_type=client_credentials \
    -d client_id=<partner_client> \
    -d client_secret=<secret> \
    -d scope=service_scope \
    -d account_id=<account-guid>
  ```

  The endpoint answers `invalid_request` when the value is not a valid identifier, when the client holds no active grant for it, or when a multi-account client omits it. That last case is a deliberate rejection — issuing an arbitrary tenant would silently point a partner at the wrong customer.

- **`UseCors` must run before `UseHealthChecks`.** This service originally had it inverted, which made the Sign-in tile on the public status page permanently unreadable — the page probes `/health` cross-origin with no token.
- **Behind a reverse proxy**, `ForwardedHeaders` middleware trusts `X-Forwarded-Proto` and `X-Forwarded-For`, so OpenIddict correctly identifies HTTPS requests even though internal container traffic is HTTP.
- **The role claim is for visibility scoping only.** Permission enforcement always goes through the Security API — this claim never grants anything. Users must sign in again to pick up a newly granted role.
- Server-rendered login and validation messages come from `.resx` resources through `ResourceLocalizer`, resolved against the ambient request culture. Never hardcode them.
- For production, use a certificate issued by a Certificate Authority.

---

## Documentation

- **Technical** — the [TrackHub wiki](https://github.com/shernandezp/TrackHub/wiki): [Security and Identity](https://github.com/shernandezp/TrackHub/wiki/Security-and-Identity), [Technology](https://github.com/shernandezp/TrackHub/wiki/Technology), [Database](https://github.com/shernandezp/TrackHub/wiki/Database)
- **User** — in the app: the Help button or **F1** on any screen
- **Deployment** — [TrackHub.Deployment](https://github.com/shernandezp/TrackHub.Deployment)

---

## License

Apache License 2.0. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.
