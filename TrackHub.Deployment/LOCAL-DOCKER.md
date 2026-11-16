# TrackHub on local Docker Desktop (Windows)

Runs the backend services in Docker, while the portal keeps running on the
Vite dev server and any single service can be pulled out into the Visual Studio debugger.

The TrackHub proxy (`nginx-local`) serves 8080/8443.

Nothing production uses is modified. `docker-compose.backend.yml`,
`nginx.backend.conf`, `.env`, `deploy.sh` and
`generate-appsettings.sh` are all used unchanged. Every local-only behaviour lives in
`docker-compose.local.yml`, `.env.local`, `nginx/nginx.local.conf` and `scripts/local/`.

---

## Topology

```
  Browser  ──── https://localhost:3000 ─────────────>  Vite dev server (host)
     │
     └──────── https://trackhub.local:8443/Manager/graphql
                        │
                   nginx-local (container, publishes 8080/8443)
                        │
                        ├── /Identity/      → authority:8080
                        ├── /Manager/       → manager:8080
                        ├── /Router/        → router:8080
                        └── ...
                                 │
   container ──── http://manager:8080/graphql/ ────> container   (Docker network)
                                 │
                                 └──── host.docker.internal:5432 ──> Postgres 14 (host service)
```

### Why `trackhub.local` and not `localhost`

`AuthorityServer:Authority` is used for three different things at once:

| Used as | Where |
| --- | --- |
| JWT `ValidIssuer` | `Common.Infrastructure/DependencyInjection.cs:44` |
| OIDC discovery / JWKS URL | `Common.Infrastructure/DependencyInjection.cs:40` |
| `{Authority}/token` for client credentials | `Common.Infrastructure/GraphQLClientFactory.cs:114` |

So one single string has to resolve correctly **from the browser and from inside every
container**, and match the issuer the Authority Server stamps into tokens. `localhost`
cannot do that: inside a container `localhost` is the container itself.

`trackhub.local` resolves from both sides:

* on the host, through the `hosts` file entry `127.0.0.1 trackhub.local`;
* inside containers, through `extra_hosts: trackhub.local:host-gateway`.

### Ports

| | Port | Note |
| --- | --- | --- |
| nginx-local | 8080, 8443 | TrackHub entry point |
| Vite dev server | 3000 | portal, on the host |
| Postgres | 5432 | native Windows service, not a container |
| authority | 6001 | published for host-side debugging |
| security | 6002 | |
| manager | 6003 | |
| router | 6004 | |
| geofencing | 6005 | |
| tripmanagement | 6006 | |
| telemetry | 6007 | |
| reporting | 6008 | |

600x rather than 500x on purpose: `launchSettings.json` already binds 5002–5005 for F5 runs.

**The compose project name is pinned** to `trackhub-local` rather than derived from the
directory. That is what makes the names predictable wherever you cloned the repo -
`trackhub-manager` containers, `trackhub-local-manager` images,
`trackhub-local_manager-documents` volumes - and it keeps a second clone from quietly
building a parallel copy of the whole stack under a different prefix.

---

## First-time setup

```powershell
cd TrackHub\TrackHub.Deployment
.\scripts\local\Setup-TrackHubLocal.ps1
```

That generates the TLS certificate, installs it into the portal, copies the OpenIddict
signing certificate from `C:\Certificates\certificate.pfx`, builds the container CA bundle, and generates
`local/generated/appsettings.*.json`.

It then prints a command to finish in an **elevated** shell (hosts entry + trusting the
certificate):

```powershell
.\scripts\local\Setup-TrackHubLocal.ps1 -AdminTasks
```

Restart the browser afterwards so it picks up the new root certificate.

**Already have a trusted certificate?** `-CertFrom` copies `trackhub-local.crt`/`.key` from a
TrackHub.Deployment directory that already has them, instead of generating a new pair:

```powershell
.\scripts\local\Setup-TrackHubLocal.ps1 -CertFrom <path to>\TrackHub\TrackHub.Deployment
```

Worth doing when the certificate is already imported into `LocalMachine\Root`, since keeping
it means `-AdminTasks` has nothing left to import and the browser needs no new trust prompt.

---

## Certificates

Two certificates, different jobs. The setup script handles both.

**`trackhub-local.crt` / `.key` — TLS.** `SAN: trackhub.local, localhost, 127.0.0.1`, so
one certificate covers everything: nginx on `:8443`, the Vite dev server on `:3000` (the
script copies it to `TrackHub.Portal/cert.crt`+`.key`), and the containers, which trust it
through `local/ca-bundle.crt` (`SSL_CERT_FILE`) rather than the Windows store.

Do **not** substitute the portal's old mkcert certificate: it is `localhost`-only and
cannot serve `trackhub.local`. The script warns and keeps any `cert.crt` that differs from
the generated one; `-Force` replaces it.

**`certificate.pfx` — OpenIddict token signing, not TLS.** Copied from
`C:\Certificates\certificate.pfx`. Every process that mints or validates tokens on this
machine must use the same file, or tokens minted by one are rejected by another with a bare 401.

Regenerate:

```powershell
.\scripts\local\Setup-TrackHubLocal.ps1 -Force        # new TLS cert + portal copy + CA bundle
.\scripts\local\Setup-TrackHubLocal.ps1 -AdminTasks   # re-trust it (elevated)
.\scripts\local\Start-TrackHubLocal.ps1 restart
```

### Seeding

`db-init` runs locally, the same way it does in a deployment: ClientSeeder, then the
Security and Manager DBInitializers. All three are **additive** - create-or-update for the
clients listed in `clients.json`, add-if-missing for resources, roles and service-client
grants, upsert for the report catalog. None of them enumerate or delete, so seed rows this
repo does not know about are left alone. Change a seed - a client, an RBAC grant, a catalog row - and it reaches the local
databases by re-running the seeder, with no hand-written SQL.

Two things make that safe, and both matter:

* **It seeds from `local/clients.json`, which the setup script copies from
  `TrackHub.AuthorityServer/src/ClientSeeder/clients.json`** - the committed dev seed,
  **not** `config/clients.json.example`. The example is a production template full of
  `<<GENERATE_A_SECURE_SECRET>>` and `<<YOUR_DOMAIN>>`, and the seeder *updates* a client
  that already exists: seeding from it rewrites every service-client secret and the portal's
  redirect URIs with those literal strings, and the whole stack 401s. That is the failure
  this file used to describe as "the seeder clobbered `manager_client`".

The client secrets in `.env.local` must match `local/clients.json`. They do: both are
committed with working dev values, the same way `appsettings.json` carries the local DB
password. There is no template to fill in - clone and run.

### What the setup deliberately does NOT do

* **It does not start Postgres in a container.** The native Postgres 14 service keeps its
  data, and it is already reachable from Docker (`listen_addresses = '*'` plus a
  `host all all 172.16.0.0/12` rule in `pg_hba.conf`).

---

## Daily use

```powershell
.\scripts\local\Start-TrackHubLocal.ps1            # build if needed + start
.\scripts\local\Start-TrackHubLocal.ps1 ps
.\scripts\local\Start-TrackHubLocal.ps1 logs -Service manager
.\scripts\local\Start-TrackHubLocal.ps1 down
.\scripts\local\Start-TrackHubLocal.ps1 -Workers   # also start the syncworker
```

Then start the portal as usual:

```powershell
cd ..\TrackHub.Portal
npm start                      # https://localhost:3000
```

The portal picks up `TrackHub.Portal/.env.local`, which points every endpoint at
`https://trackhub.local:8443/...`. The committed `.env` points at the same stack, so
`.env.local` is only needed for machine-specific overrides.

> The Vite dev server only serves HTTPS when `cert.crt` and `cert.key` exist in
> `TrackHub.Portal/`. Without them it silently falls back to HTTP and the OAuth callback
> fails with `ERR_SSL_PROTOCOL_ERROR`. The setup script installs them - see
> [Certificates](#certificates).

---

## Debugging one service in Visual Studio

Say you are debugging the **Router** and it needs to call **Telemetry** in Docker.

```powershell
.\scripts\local\Set-DebugTarget.ps1 -Service router -ShowConfig
```

This:

1. rewrites `nginx/local.d/upstreams/router.conf` to
   `server host.docker.internal:5003;`,
2. stops the `router` container so only one instance answers,
3. validates and reloads nginx,
4. prints the settings the host-run Router needs.

Now:

```
Browser → https://trackhub.local:8443/Router/graphql → nginx → your breakpoint
Router  → http://localhost:6007/graphql/ → telemetry container
Router  → https://trackhub.local:8443/Identity/token → authority container
```

The portal URLs never change, so nothing in the frontend needs touching.

Put the host-side settings in a **new** `launchSettings.json` profile or in
`dotnet user-secrets` - both are additive and leave the committed `appsettings.json`
alone. The values are:

```jsonc
"ConnectionStrings": {
  "Logging": "server=localhost;user id=postgres;password=super;database=TrackHub;port=5432"
},
"AuthorityServer": { "Authority": "https://trackhub.local:8443/Identity" },
"AppSettings": {
  "GraphQLTelemetryService": "http://localhost:6007/graphql/",
  "GraphQLManagerService":   "http://localhost:6003/graphql/"
}
```

Note the host-run service uses `localhost:600x` (published ports) while a container uses
`telemetry:8080` (Docker network). Both reach the same container.

Restore when done:

```powershell
.\scripts\local\Set-DebugTarget.ps1 -Service router -Reset
```

---

## After changing service code

```powershell
.\scripts\local\Start-TrackHubLocal.ps1 build -Service manager
.\scripts\local\Start-TrackHubLocal.ps1 up    -Service manager
```

## After changing `.env.local`

Regenerate the appsettings, because some values are baked into the generated JSON:

```powershell
.\scripts\local\Setup-TrackHubLocal.ps1        # idempotent, regenerates them
.\scripts\local\Start-TrackHubLocal.ps1 up
```

---

## Troubleshooting

**Everything 401s.**
The issuer must match exactly. `nginx.local.conf` sends `Host $http_host` (with the
`:8443` port) rather than `$host`, because the Authority Server derives the issuer from
the `Host` header - `Program.cs` enables `ForwardedHeaders` for `XForwardedFor` and
`XForwardedProto` only, **not** `XForwardedHost`. With `$host` the issuer comes back as
`https://trackhub.local/Identity` and validation fails everywhere.

**A container cannot reach `https://trackhub.local:8443`.**
Check `SSL_CERT_FILE` and the bundle:

```powershell
docker compose --env-file .env.local -f docker-compose.backend.yml -f docker-compose.local.yml `
  exec manager sh -c 'echo $SSL_CERT_FILE; curl -sS -o /dev/null -w "%{http_code}\n" https://trackhub.local:8443/Identity/.well-known/openid-configuration'
```

Rebuild the bundle with `Setup-TrackHubLocal.ps1 -Force` after regenerating the certificate.

**The browser rejects the certificate.**
Re-run `Setup-TrackHubLocal.ps1 -AdminTasks` and restart the browser.

**Port 8443 already taken.**
Change the published port in `docker-compose.local.yml`, then update `AUTHORITY_URL` in
`.env.local`, the endpoints in `TrackHub.Portal/.env.local`, and the redirect target in
`nginx.local.conf`. They must all agree.

**A service exits complaining about `appsettings.json`.**
A missing bind-mount source makes Docker create a *directory* at that path. Re-run
`Setup-TrackHubLocal.ps1` to regenerate `local/generated/`, then recreate the container.
