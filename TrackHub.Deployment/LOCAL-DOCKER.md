# TrackHub on local Docker Desktop (Windows)

The development setup: the backend runs in Docker, the portal runs on the Vite dev server, and any
single service can be pulled out into the Visual Studio debugger. PostgreSQL runs on the host.

It uses the production `docker-compose.backend.yml`, `nginx.backend.conf` and
`generate-appsettings.sh` unchanged. Everything local-only lives in `docker-compose.local.yml`,
`.env.local`, `nginx/nginx.local.conf`, `nginx/local.d/` and `scripts/local/`.

## Topology

```
Browser ── https://localhost:3000 ──────────────> Vite dev server (host)
   │
   └────── https://trackhub.local:8443/<Service>/ ─> nginx-local (container, 8080/8443)
                                                        ├── /Identity/ → authority:8080
                                                        ├── /Manager/  → manager:8080
                                                        └── ...
containers ── http://<service>:8080/graphql/ ──> each other (Docker network)
           └─ host.docker.internal:5432 ──────> PostgreSQL on the host
```

**Why `trackhub.local` and not `localhost`.** `AuthorityServer:Authority` is at once the token issuer
services validate, the discovery/JWKS URL, and the base of the client-credentials token endpoint,
so one URL must resolve the same way from the browser and from inside every container — which
`localhost` cannot. `trackhub.local` resolves to the host from both sides: through the hosts file on
Windows, through `extra_hosts: trackhub.local:host-gateway` in the containers.

| | Port |
|---|---|
| nginx-local | 8080, 8443 |
| Vite dev server | 3000 |
| PostgreSQL (host) | 5432 |
| authority, security, manager, router, geofencing, tripmanagement, telemetry, reporting | 6001–6008, published on `127.0.0.1` for host-side debugging |

The compose project name is pinned to `trackhub-local`, so containers, images and volumes have the
same names wherever the repository is cloned, and a second clone reuses the stack instead of
building a parallel one.

## First-time setup

**PostgreSQL** must accept connections from Docker: `listen_addresses = '*'` and a
`host all all 172.16.0.0/12` rule in `pg_hba.conf`. Create the `TrackHub` and `TrackHubSecurity`
databases and the `postgis` extension in `TrackHub` ([QUICKSTART.md step 3](QUICKSTART.md#3-prepare-the-databases)).

**The OpenIddict signing certificate** is expected at `C:\Certificates\certificate.pfx`, password
`openiddict` — the same file the committed `appsettings.json` files use, so tokens minted in Docker
validate in a host-run service and vice versa. Create one with the snippet in the
[AuthorityServer README](../TrackHub.AuthorityServer/README.md), or pass `-PfxSource`.

Then, from `TrackHub.Deployment`:

```powershell
.\scripts\local\Setup-TrackHubLocal.ps1
```

It generates the `trackhub.local` TLS certificate, installs it into the portal as
`cert.crt`/`cert.key`, copies the signing certificate, builds the container CA bundle, writes
`local/clients.json` and generates `local/generated/appsettings.*.json`. It then prints the command
for the two elevated steps — the hosts entry and trusting the certificate:

```powershell
.\scripts\local\Setup-TrackHubLocal.ps1 -AdminTasks    # in an elevated shell
```

Restart the browser afterwards. Every step is idempotent. `-CertFrom <path to another TrackHub.Deployment>`
reuses a `trackhub-local.crt`/`.key` that is already trusted instead of generating a new pair.

**The schema** comes from EF migrations, never from `db-init`. The committed `appsettings.json`
files already target `localhost:5432` as `postgres`, so from the repository root:

```powershell
dotnet ef database update --project TrackHubSecurity/src/Infrastructure/SecurityDB --startup-project TrackHubSecurity/src/Web
dotnet ef database update --project TrackHub.AuthorityServer/src/Infrastructure --startup-project TrackHub.AuthorityServer/src/Web --context AuthorityDbContext
dotnet ef database update --project TrackHub.Manager/src/Infrastructure/ManagerDB --startup-project TrackHub.Manager/src/Web
dotnet ef database update --project TrackHub.Geofencing/src/Infrastructure/ManagerDB --startup-project TrackHub.Geofencing/src/Web
dotnet ef database update --project TrackHub.TripManagement/src/Infrastructure/TripDB --startup-project TrackHub.TripManagement/src/Web
```

## Daily use

```powershell
.\scripts\local\Start-TrackHubLocal.ps1                     # build if needed + start (up)
.\scripts\local\Start-TrackHubLocal.ps1 ps
.\scripts\local\Start-TrackHubLocal.ps1 logs -Service manager
.\scripts\local\Start-TrackHubLocal.ps1 down
.\scripts\local\Start-TrackHubLocal.ps1 -Workers            # also start the syncworker
.\scripts\local\Start-TrackHubLocal.ps1 config              # print the merged compose file
```

Actions: `up`, `down`, `restart`, `build`, `logs`, `ps`, `config`; `-Service` limits any of them to
one compose service. Then start the portal:

```powershell
cd ..\TrackHub.Portal
npm start                      # https://localhost:3000
```

The portal's committed `.env` already points at `https://trackhub.local:8443`; `.env.local` is for
machine-specific overrides.

After changing service code:

```powershell
.\scripts\local\Start-TrackHubLocal.ps1 build -Service manager
.\scripts\local\Start-TrackHubLocal.ps1 up -Service manager
```

After changing `.env.local`, re-run `Setup-TrackHubLocal.ps1` (it regenerates the appsettings) and
`Start-TrackHubLocal.ps1 up`.

## Seeding

`db-init` runs on `up` as in a deployment: the client seeder, then the Security and Manager
initializers, all additive and idempotent. It seeds from **`local/clients.json`, a copy of the
committed development seed `TrackHub.AuthorityServer/src/ClientSeeder/clients.json`** — never from
`config/clients.json.example`, whose placeholders would overwrite every client's secret and redirect
URIs and make the whole stack answer 401. The secrets in `.env.local` match that file; both are
committed development values.

## Debugging one service in Visual Studio

```powershell
.\scripts\local\Set-DebugTarget.ps1 -Service router -ShowConfig
```

It points nginx's upstream for that service at `host.docker.internal:<port>`, stops the service's
container, reloads nginx and prints the settings the host-run service needs. The default port is the
http port of the service's `launchSettings.json` profile; pass `-Port` if you run another one. The portal keeps using
`https://trackhub.local:8443/...` and reaches your breakpoint.

Put the host-side settings in a new `launchSettings.json` profile or in `dotnet user-secrets`, not in
the committed `appsettings.json`. A host-run service reaches its container peers on the published
ports, e.g. for the Router:

```jsonc
"AuthorityServer": { "Authority": "https://trackhub.local:8443/Identity" },
"AppSettings": {
  "GraphQLTelemetryService": "http://localhost:6007/graphql/",
  "GraphQLManagerService":   "http://localhost:6003/graphql/"
}
```

Restore when done:

```powershell
.\scripts\local\Set-DebugTarget.ps1 -Service router -Reset
```

## Troubleshooting

| Symptom | Fix |
|---|---|
| Everything answers 401 | The issuer must match exactly. `nginx.local.conf` forwards `Host $http_host` (with `:8443`) because the AuthorityServer builds the issuer from the `Host` header; also check every process uses the same `certificate.pfx` |
| A container cannot reach `https://trackhub.local:8443` | Check `SSL_CERT_FILE` and the bundle inside it; after regenerating the certificate, `Setup-TrackHubLocal.ps1 -Force`, then `-AdminTasks`, then `Start-TrackHubLocal.ps1 restart` |
| The browser rejects the certificate | `Setup-TrackHubLocal.ps1 -AdminTasks` and restart the browser |
| Sign-in fails with `ERR_SSL_PROTOCOL_ERROR` | `TrackHub.Portal/cert.crt` / `cert.key` missing, so Vite fell back to HTTP; re-run the setup script |
| Port 8443 is taken | Change it in `docker-compose.local.yml`, `AUTHORITY_URL` in `.env.local`, the portal's endpoints and the redirect in `nginx.local.conf` — they must agree |
| A service exits complaining about `appsettings.json` | A missing bind-mount source became a directory; re-run `Setup-TrackHubLocal.ps1` and recreate the container |

To test the container trust from inside, for example:

```powershell
docker compose --env-file .env.local -f docker-compose.backend.yml -f docker-compose.local.yml `
  exec manager sh -c 'curl -sS -o /dev/null -w "%{http_code}\n" https://trackhub.local:8443/Identity/.well-known/openid-configuration'
```
