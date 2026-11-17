# TrackHub Deployment Guide

The operator runbook for a Docker deployment on Linux. A first installation follows
[QUICKSTART.md](QUICKSTART.md); this guide is everything around it. Topology, routes, configuration
groups and the background job catalog are in the wiki:
[Deployment and Operations](https://github.com/shernandezp/TrackHub/wiki/Deployment-and-Operations).

1. [Requirements](#requirements)
2. [External dependencies](#external-dependencies)
3. [Configuration](#configuration)
4. [Certificates](#certificates)
5. [Database](#database)
6. [Split deployment](#split-deployment)
7. [Updating](#updating)
8. [Upgrading from a previous version](#upgrading-from-a-previous-version)
9. [Moving to a new server](#moving-to-a-new-server)
10. [Operations](#operations)
11. [Troubleshooting](#troubleshooting)
12. [Security checklist](#security-checklist)

---

## Requirements

### Server requirements

| | Minimum | Recommended |
|---|---|---|
| CPU | 2 cores | 4+ cores |
| RAM | 4 GB + 4 GB swap | 8 GB + 4 GB swap |
| Storage | 20 GB | 50+ GB SSD |
| OS | Ubuntu 22.04 LTS | Ubuntu 24.04 LTS |

Image builds spike above the RAM of a small host; without swap the kernel kills the build or the
host stops answering SSH. `deploy.sh` warns when no swap is configured, and refuses to build with
less than 10 GB free under the Docker root, less than 1.5 GB of available memory, or no answer from
nuget.org.

```bash
sudo fallocate -l 4G /swapfile && sudo chmod 600 /swapfile && sudo mkswap /swapfile && sudo swapon /swapfile
echo "/swapfile none swap sw 0 0" | sudo tee -a /etc/fstab
```

Every backend container has a memory ceiling, `<SERVICE>_MEMORY_LIMIT` (and
`<SERVICE>_MEMORY_RESERVATION`) in `.env`; the defaults add up to about 3.7 GB. Keep their sum below
the host's RAM minus about 1.5 GB: .NET caps its heap at 75% of the container limit, but the kernel
kills the largest process when the host itself runs out.

### Software

| | Version |
|---|---|
| Docker Engine | 24.0+ |
| Docker Compose | v2.20+ |
| Git | 2.34+ |
| OpenSSL | 3.0+ |
| .NET SDK + `dotnet-ef` | 10.0 — only on the host that applies migrations; images build with their own SDK |

### PostgreSQL

PostgreSQL 14+, installed and reachable from the application server, with two databases:
`TrackHubSecurity` (identity, users, roles, policies, OAuth clients) and `TrackHub` (everything
else, including the `public.logs` table of the log sink). The `TrackHub` database needs the
`postgis` extension, created by a superuser.

---

## External dependencies

### OpenRouteService (required)

TripManagement plans routes, distances, ETAs and corridors through OpenRouteService. Get a key from
the free plan at <https://openrouteservice.org/dev/#/signup>, or self-host ORS and point
`ORS_BASE_URL` at it:

```bash
ROUTING_PROVIDER=OpenRouteService
ORS_BASE_URL=https://api.openrouteservice.org   # or http://ors:8080/ors when self-hosted
ORS_API_KEY=your-openrouteservice-api-key
ORS_PROFILE=driving-hgv
```

Without a key, trips keep working but every route plan fails with `ROUTING_NOT_CONFIGURED` and ETAs
fall back to the planned times — treat that as a broken deployment, not a mode. The throttle and
admission keys (`ORS_REQUESTS_PER_SECOND`, `ORS_TIMEOUT_SECONDS`, `ORS_MAX_WAYPOINTS`,
`ORS_INTERACTIVE_TIMEOUT_SECONDS`, `ORS_BACKGROUND_*`) are documented in `.env.example`.

Reverse geocoding of positions is configured separately, as a **Geocoding Provider** in the system
administration screens (Nominatim by default). Using ORS there too means entering the same key a
second time; route planning never depends on which geocoder is active.

### Local geocoding (optional)

The **Local** geocoding provider resolves addresses inside PostgreSQL from reference tables in the
`geocoding` schema — no external call per lookup. The data covers Colombia and Costa Rica.

1. Set `LOCAL_GEOCODING=true` in `.env` and deploy. While the tables are empty, `db-init`
   downloads the sources (Overture Maps — latest release unless `GEOCODING_OVERTURE_RELEASE` is set —
   Geofabrik OpenStreetMap, and the Medellín and Bogotá neighbourhood layers), builds and loads them.
   It takes about 30 minutes and needs internet access; a failure only warns and is retried on the
   next deploy. The build is capped at `GEOCODING_MEMORY_LIMIT` (default `2GB`) and spills to disk
   beyond it (allow about 5 GB free).
2. In system administration → Geocoding Providers, add a provider of type **Local** and activate
   it. Lookups are cheap: raise `requestsPerSecond` (e.g. 100) and the enrichment budget in
   `configurationJson` (e.g. `{"enrichmentBudget": 500}`).

Refresh with newer data (add `-f docker-compose.backend.yml` on a split deployment); the new tables
are built aside and swapped in, so lookups keep working:

```bash
docker compose run --rm --no-deps --entrypoint /app/import-geocoding.sh db-init --refresh
```

`backup-database.sh` skips the geocoding table data (about 500 MB); after a restore the next deploy
downloads it again.

### Toll catalog (empty by design)

TrackHub ships with no toll stations, tariffs or vehicle classes — there is no external toll API
behind the feature. Each deployment enters its catalog through the administration screens or the
CSV import. Until then toll estimates are **null**, never zero, and the `trip-toll-cost` report
shows the gap.

**Colombia.** `scripts/fetch-toll-catalog.mjs` turns the INVÍAS open-data publication
([datos.gov.co, *Peajes*](https://www.datos.gov.co/Transporte/Peajes/68qj-5xux)) into one SQL
script — 7 vehicle classes, about 173 stations and 1 026 tariffs:

```bash
node scripts/fetch-toll-catalog.mjs --effective-from 2026-01-01 --out toll-catalog.sql
psql -h DB_HOST -U postgres -d TrackHub -f toll-catalog.sql
```

- Start from an empty catalog (`SELECT count(*) FROM trip.toll_stations;` is 0). The catalog is
  platform-wide, so test stations are visible to every account.
- The script is one transaction and not idempotent: a second run fails on the unique indexes and
  rolls back, which prevents a duplicated station from being charged twice.
- Make later rate changes through the screens or the CSV import, which close the open tariff and
  insert a new one, so past estimates stay reproducible.
- The source's scalar latitude/longitude columns are corrupt for many records; the script reads the
  GeoJSON point instead and rejects anything outside Colombia. Check the figures against the
  resolution in force — categories VI and VII are left marked `CONFIRMAR`.

---

## Configuration

### How it flows

`.env` is the single source. `deploy.sh` runs `generate-appsettings.sh` before every backend or full
deploy, writing `generated/appsettings.<service>.json`; the compose files bind-mount each one
read-only over the container's `/app/appsettings.json`. **Environment variables set in the compose
files take precedence over the mounted file.**

```bash
./scripts/sync-config.sh validate                      # required keys present and not placeholders
./scripts/sync-config.sh show                          # current values
./scripts/sync-config.sh generate                      # all backend appsettings into generated/
./scripts/sync-config.sh frontend                      # the portal .env only
./scripts/generate-appsettings.sh --service manager    # preview one service on stdout
```

`config/appsettings.template.json` shows every templated value and the variable it comes from.

### Keys

`.env.example` (full stack), `.env.backend.example` and `.env.frontend.example` are the annotated
reference for every key. The rules that matter:

- **Required, no defaults** (`scripts/required-config.sh`): `ALLOWED_CORS_ORIGINS`, `AUTHORITY_URL`,
  `DB_CONNECTION_SECURITY`, `DB_CONNECTION_MANAGER`, `DB_CONNECTION_TELEMETRY`,
  `DB_CONNECTION_LOGGING`, `CERTIFICATE_PASSWORD`, `ENCRYPTION_KEY`, and the seven service-client
  secrets `SYNCWORKER_`, `ROUTER_`, `SECURITY_`, `GEOFENCE_`, `TRIP_`, `REPORTING_` and
  `MANAGER_CLIENT_SECRET`. `deploy.sh` and `sync-config.sh validate` refuse to continue while any
  is empty or a placeholder.
- **Connection strings use `user id=`, with the space.** `init-databases.sh` and
  `backup-database.sh` parse that exact key. Telemetry and the log sink use the same `TrackHub`
  database as Manager.
- **Service-client secrets live only in `.env`.** `db-init` passes each `<CLIENT_ID>_SECRET` to the
  client seeder, which uses it instead of whatever `clients.json` holds; the services authenticate
  with the same variables.
- **`ENCRYPTION_KEY` encrypts stored GPS provider credentials.** Changing it makes every stored
  credential unreadable.
- **`REACT_APP_*` values are baked into the portal at build time.** Changing one means
  `./scripts/deploy.sh portal` (or `update-service.sh frontend`), not a restart.
- **`GRAPHQL_*_SERVICE` are Docker-network addresses** (`http://manager:8080/graphql/`); leave them
  unless you rename compose services.
- **`OPENIDDICT_SCOPES`** must list the scopes in `clients.json`:
  `mobile_scope,staff_mobile_scope,web_scope,service_scope`.
- **`CERTIFICATE_PATH` / `CERTIFICATE_THUMBPRINT`** only affect appsettings generated for a host
  (non-Docker) deployment; containers always load `/app/certificates/certificate.pfx`.
- **Forwarded headers are trusted only from private networks** (loopback, `10/8`, `172.16/12`,
  `192.168/16`, IPv6 ULA). A reverse proxy elsewhere must be listed in
  `AppSettings:KnownProxyNetworks` (CIDR array), or client IPs in rate limits and audit rows are the
  proxy's.

### Notifications

Email and WhatsApp stay disabled until configured (`SMTP_*`, `WHATSAPP_*`, `PORTAL_BASE_URL` for
the links in messages); see `.env.backend.example`. WhatsApp needs a verified Meta Business account,
a WhatsApp Business Account and an approved utility template.

### Document storage

Documents are stored on the `manager-documents` volume by default
(`DOCUMENT_STORAGE_PROVIDER=LocalFileSystem`), capped at 50 MB per upload by nginx. For `S3` or
`AzureBlob`, Manager refuses to start without the required keys:

| Provider | Required | Optional |
|---|---|---|
| `S3` | `DOCUMENT_S3_BUCKET_NAME` | `DOCUMENT_S3_REGION`, `DOCUMENT_S3_SERVICE_URL` (MinIO / S3-compatible), `DOCUMENT_S3_ACCESS_KEY` + `DOCUMENT_S3_SECRET_KEY` (omit both for the AWS credential chain), `DOCUMENT_S3_FORCE_PATH_STYLE`, `DOCUMENT_S3_PRESIGNED_EXPIRY_MINUTES` |
| `AzureBlob` | `DOCUMENT_AZURE_CONTAINER_NAME`, `DOCUMENT_AZURE_CONNECTION_STRING` | `DOCUMENT_AZURE_SAS_EXPIRY_MINUTES` |

Switching provider does not migrate existing files — move them yourself.

---

## Certificates

Two certificates with different jobs: the **TLS** certificate nginx serves, and the **OpenIddict**
certificate (`certificates/certificate.pfx`) every service uses to sign and validate tokens.

### TLS (Let's Encrypt)

```bash
sudo ./scripts/generate-certs.sh [domain] [email] [certificate-password] [--rotate-openiddict]
```

Arguments default to `DOMAIN`, `LETSENCRYPT_EMAIL` and `CERTIFICATE_PASSWORD` from `.env`. The
script installs certbot if needed, obtains the certificate (webroot when nginx is running,
standalone otherwise), copies `fullchain.pem` / `privkey.pem` into `certificates/`, creates the
OpenIddict certificate when none exists, and installs the deploy hook
`/etc/letsencrypt/renewal-hooks/deploy/trackhub-nginx.sh`. certbot's timer renews; the hook copies the
renewed files and reloads nginx. Without the hook, nginx keeps serving the old copy until it expires.

```bash
./scripts/renew-ssl.sh                                           # manual renewal check
ls -l /etc/letsencrypt/renewal-hooks/deploy/trackhub-nginx.sh
sudo certbot renew --dry-run
```

With a certificate obtained another way, place `fullchain.pem` and `privkey.pem` in `certificates/`
and still run `generate-certs.sh` once, for the hook and the `.pfx`. It needs a public domain — it
always goes through Let's Encrypt.

### OpenIddict signing certificate

`generate-certs.sh` keeps an existing `certificate.pfx` (checking that `CERTIFICATE_PASSWORD` opens
it), because a new signing key invalidates every token and signs every user out.
`--rotate-openiddict` replaces it on purpose. To create one by hand:

```bash
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes -subj "/CN=your-domain.com"
openssl pkcs12 -export -out certificates/certificate.pfx -inkey key.pem -in cert.pem -password pass:your-certificate-password
rm key.pem cert.pem
```

For a CA-issued certificate, add `-certfile your-ca-bundle.pem` to the `pkcs12` step. Containers load
it from the read-only `/app/certificates/` mount (`OpenIddict__LoadCertFromFile=true`,
`OpenIddict__Path`, `OpenIddict__Password`). A Windows host deployment can instead use the
certificate store: `LoadCertFromFile=false` and `Thumbprint`.

---

## Database

### Migrations create the schema; db-init only seeds

| Step | Does | Does not |
|---|---|---|
| EF migrations (`dotnet ef database update`) | Create and alter every table and schema | Seed anything |
| `db-init` (every `deploy.sh full` / `backend`) | Upsert OAuth scopes and clients, seed roles, resources, permissions and service-client grants, seed master data and the report catalog, pin database time zones to UTC, install the retention and geocoding SQL | Create or alter tables |

### Applying migrations

Five projects own migrations; Telemetry's tables come from Manager's. The commands, with explicit
connection strings and no command timeout, are in [QUICKSTART.md step 5](QUICKSTART.md#5-create-the-schema-ef-migrations).

| Project | Database | Schemas |
|---|---|---|
| `TrackHubSecurity/src/Infrastructure/SecurityDB` | `TrackHubSecurity` | `security` |
| `TrackHub.AuthorityServer/src/Infrastructure` (`--context AuthorityDbContext`, after Security) | `TrackHubSecurity` | OpenIddict tables |
| `TrackHub.Manager/src/Infrastructure/ManagerDB` | `TrackHub` | `app`, `map`, `telemetry` |
| `TrackHub.Geofencing/src/Infrastructure/ManagerDB` | `TrackHub` | `geofencing` (PostGIS) |
| `TrackHub.TripManagement/src/Infrastructure/TripDB` | `TrackHub` | `trip` (PostGIS) |

**No EF tooling near production?** Generate SQL on a workstation and run it as the application role:
`dotnet ef migrations script --idempotent` per project (add `--context AuthorityDbContext` for the
AuthorityServer). Keep the scripts with your operator files.

### Running the seeders by hand

They assume the schema exists. Each reads its connection string from its own `appsettings.json`
(`ConnectionStrings:Security`, or `DefaultConnection` for Manager), overridable with
`ConnectionStrings__...` variables:

```bash
cd /opt/trackhub
dotnet run --project TrackHub.AuthorityServer/src/ClientSeeder   # reads clients.json beside it
dotnet run --project TrackHubSecurity/src/DBInitializer
dotnet run --project TrackHub.Manager/src/DBInitializer
```

### Server tuning

`scripts/sql/tune-postgresql.sql` sets what the workload depends on (SSD cost model, WAL sizing,
autovacuum, slow-query logging, the `accountid` statistics target). Run it once per server as a
superuser:

```bash
psql -h DB_HOST -U postgres -d postgres -f scripts/sql/tune-postgresql.sql
```

Memory settings and `max_connections` are listed, commented, at the end of the file.
`max_connections` must cover the sum of every service process's pool (`Database:MaxPoolSize`,
20 by default) plus headroom — Npgsql pools per process.

### Time zones

Every timestamp is `timestamp with time zone`, so instants survive a restore on a host in another
zone. Services pin their sessions to UTC, images set `TZ=UTC`, and `db-init` runs
`ALTER DATABASE ... SET timezone TO 'UTC'` on every deploy — which needs the database owner or a
superuser; otherwise it warns and you run it once by hand. A dump does not carry the setting, so
re-run `db-init` after a restore. Calendar questions use each account's IANA zone.

An appsettings file generated before the log sink moved into Common still names
`Serilog.Sinks.PostgreSQL.Configuration` and fails at startup; regenerate it.

### Partitioning the position history (optional)

`telemetry.transporter_position_history` is the one table whose growth justifies partitioning
(about 66 GB in 30 days at 3 000 devices on a one-minute provider). Partitioned, retention drops
whole months instead of deleting rows. Everything works unpartitioned; Telemetry detects the layout.

After the migrations are applied:

```bash
psql -h DB_HOST -U postgres -d TrackHub -f scripts/sql/partition-position-history.sql
```

The script renames the table, creates the partitioned one (a month per partition, covering the data
plus three months ahead), copies the rows and drops the old table — budget for the copy.
`ops.maintain_position_partitions()`, part of the retention run, then keeps creating months ahead
and dropping aged ones. With partitions, history deduplication is enforced per month by the
database and for two days by the ingest probe, so a provider replaying older batches stores them
twice.

---

## Split deployment

Frontend and backend on separate servers, each with its own `.env` and certificate.

```bash
# backend server
cp .env.backend.example .env && nano .env
./scripts/deploy.sh backend

# frontend server
cp .env.frontend.example .env && nano .env
./scripts/deploy.sh frontend
```

The frontend server only serves the static bundle; the browser calls the backend directly through
the `REACT_APP_*` URLs, so those point at the backend host. On the backend, `ALLOWED_CORS_ORIGINS`
and the `web_client` URIs in `clients.json` use the **frontend** domain. `deploy.sh` refuses
`frontend` or `backend` on a host that already runs the full stack. Pass
`docker-compose.backend.yml` / `docker-compose.frontend.yml` as the compose-file argument to
`update-service.sh`, `rollback.sh` and `health-check.sh`.

---

## Updating

For a release with no new migrations, OAuth clients or configuration keys. Builds use the Docker
layer cache and detect source changes, containers are always recreated, and the portal refreshes its
static assets on every start — no `--no-cache`, repeated runs or volume cleanup needed. Every image
an update replaces is kept as `:previous` first (see [Rollback](#rollback)).

```bash
cd /opt/trackhub && git pull
cd TrackHub.Deployment
./scripts/deploy.sh full
```

| Command | Effect |
|---|---|
| `./scripts/deploy.sh full` | Every image, `db-init`, then the whole stack (default) |
| `./scripts/deploy.sh portal` | Only the portal on a full-stack server, plus nginx |
| `./scripts/deploy.sh backend` / `frontend` | One half of a split deployment |
| `./scripts/update-service.sh <service> [compose-file] [--no-cache]` | One service, no `db-init` |

Options for `deploy.sh`: `--no-cache` (full rebuild), `--skip-init` (no `db-init`),
`--skip-git-check` (deploy although the checkout is behind its upstream — by default it refuses,
because the compose files themselves are deployment inputs). `--build` is accepted and has no effect.

`update-service.sh` services: `frontend`, `authority`, `security`, `manager`, `router`,
`geofencing`, `tripmanagement` (alias `trip`), `telemetry`, `reporting`, `syncworker`, `nginx`. The
SyncWorker builds from `TrackHubRouter/`, so a Router change means updating both `router` and
`syncworker`.

### Zero-downtime order

Publish a maintenance announcement first (Platform Status → Manage announcements,
SuperAdministrator). Announcements are stored and served by Manager, so the banner disappears while
Manager restarts; one scheduled in advance with *Starts at* / *Ends at* is what users see. Then
update one service at a time, frontend last:

```bash
for s in authority security manager router geofencing tripmanagement telemetry reporting syncworker frontend nginx; do
  ./scripts/update-service.sh "$s"
done
```

`update-service.sh` never runs migrations or `db-init`; a release that adds either goes through
[Upgrading](#upgrading-from-a-previous-version).

---

## Upgrading from a previous version

A release can add migrations, OAuth clients, scopes and configuration keys. `.env` and
`config/clients.json` are your files — `git pull` never touches them — so merge new keys in by hand,
in this order.

**1. Back up** the databases, configuration and documents ([Backups](#backups)).

**2. Pull** — `cd /opt/trackhub && git pull`. An installation from before the single-repository
layout (one clone per service under `/opt/trackhub`) cannot be converted in place: clone into a new
directory, copy `TrackHub.Deployment/.env`, `config/clients.json`, `certificates/` and `generated/`
across, and switch over.

**3. Reconcile `.env`** — list the keys the template has and yours lacks:

```bash
comm -23 <(grep -oE '^[A-Z0-9_]+=' .env.example | sort -u) <(grep -oE '^[A-Z0-9_]+=' .env | sort -u)
```

Then `./scripts/sync-config.sh validate`.

**4. Reconcile `config/clients.json`** with `config/clients.json.example`: the four scopes
(`web_scope`, `mobile_scope`, `staff_mobile_scope`, `service_scope`, all with resource
`trackhub_api`), the `mobile_client` with `"passwordGrant": true`, `staff_mobile_client`, every service
client in the example, and `web_client.postLogoutUri` = `https://<domain>/`.

**5. Apply the migrations** — the five commands in [QUICKSTART.md step 5](QUICKSTART.md#5-create-the-schema-ef-migrations),
before starting the new images. A Security or Manager image against a schema that lacks its columns
fails at request time while the container reports healthy.

> An installation created before the AuthorityServer had a migration has the OpenIddict tables but
> no history row, so its `database update` fails with *relation "OpenIddictApplications" already
> exists*. Baseline it once in `TrackHubSecurity`:
>
> ```sql
> INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
> SELECT '20260722030751_InitialCreate', '10.0.10'
> WHERE NOT EXISTS (SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260722030751_InitialCreate');
> ```

**6. Deploy** — `./scripts/deploy.sh full`. Its `db-init` registers new clients, scopes, resources
and grants. **Skipping it is the most common post-upgrade failure:** a new module's calls all
return `FORBIDDEN` while its service reports healthy.

**7. Verify** — `./scripts/health-check.sh <domain>`, then `/status`: every tile *Working* and the
*Portal build* line showing the new build (hard-reload).

---

## Moving to a new server

Keeping the existing database:

1. Back up the database and the old server's `TrackHub.Deployment/.env`, `config/clients.json`,
   `certificates/` (including `certificate.pfx` — a new one signs every user out) and the documents
   volume (`./scripts/backup.sh` collects all but the database).
2. Install Docker and clone on the new server ([QUICKSTART.md](QUICKSTART.md) steps 1–2), restore
   those files, and update `.env` for the new host and domain.
3. Deploy. `db-init` is idempotent and safe against the existing data; use
   `./scripts/deploy.sh full --skip-init` only while the old server still writes to the same
   database.
4. Sign in, check `/status`, and exercise the APIs before retiring the old server.

---

## Operations

### Retention (schedule this)

Nothing inside the services deletes aged rows. `db-init` installs `scripts/sql/purge-functions.sql`
on every deploy; scheduling it — cron, pgAgent, Task Scheduler — is yours. Daily is enough:

```bash
psql -h DB_HOST -U trackhub -d TrackHub -c "CALL ops.run_purge();"
```

`ops.run_purge()` runs each task in its own transaction and logs a failing one as a warning;
`ops.purge_all()` does the same in one transaction and returns a row per task. Each task is also
callable alone with its window, e.g. `SELECT ops.purge_audit_events(now(), 365);`.

| Data | Default retention |
|---|---|
| Position history, logs, operator health checks | 30 days (position history per account: the `gps.positionHistory` entitlement's `retentionDays`) |
| Background job runs, notification deliveries, operator sync runs | 90 days |
| Resolved alert events | 180 days |
| API usage hours | 400 days |
| Audit events, trip events | 730 days |

Each task keeps what is still needed: failed job runs and the newest run per job, alert events a
delivery references, events of unfinished trips, the newest sync run and health check per operator.
On a partitioned history, run it at least monthly even with nothing to purge, or new rows land in
the default partition. Document bytes are the exception: Manager reclaims superseded versions itself
after `DOCUMENT_RETENTION_DAYS`.

### Logs and health

```bash
./scripts/logs.sh                         # all services, following
./scripts/logs.sh manager -n 50 --no-follow
./scripts/health-check.sh your-domain.com # exit code 1 if anything is unhealthy
curl -k https://your-domain.com/health/manager
docker compose ps
docker stats
```

`/health/<authority|security|manager|router|geofencing|trip|telemetry|reporting>` are anonymous.
Authority, Security, Manager, Telemetry, Geofencing and TripManagement probe their database; Router
and Reporting are liveness-only. The SyncWorker has no listener — its activity shows on `/status`.
`/status` needs no sign-in and is the check for users who cannot log in.

`/status`, the `/health*` probes and `GET /Manager/api/PlatformStatus/announcements` are anonymous
by design (the latter rate-limited, cached 60 s, and returning only visible announcements). Do not
put them behind authentication or an IP allowlist without giving up the status page.

### Backups

```bash
./scripts/backup.sh                                        # .env, config/, certificates/, nginx/, generated/, local documents
./scripts/backup-database.sh backup                        # both databases into one backups/database/*.tar.gz
./scripts/backup-database.sh list
./scripts/backup-database.sh restore backups/database/<file>.tar.gz
./scripts/backup-database.sh cleanup 7                     # keep 7 days (default 30)
```

Archives are readable by their owner only; they hold every credential and every tenant's data.
Uploaded documents are **not** in PostgreSQL. On `LocalFileSystem` storage `backup.sh` includes
them; to archive the volume yourself:

```bash
docker volume ls | grep manager-documents
docker run --rm -v <volume>:/data -v "$PWD/backups:/backup" alpine tar czf /backup/documents-$(date +%F).tar.gz -C /data .
```

**Never run `docker compose down -v`** — it deletes the documents volume. Use
`docker compose down --remove-orphans`.

### Which build is deployed?

The portal shows its version and build time in the footer and on `/status` (*Portal build*). That
is the only proof of what a browser loaded — hard-reload, since `index.html` is cached. Backend
services are identified by their image tags: `./scripts/rollback.sh list` and `docker compose ps`.

### Rollback

Rollback is image-level: it re-points a service's image at an older tag and recreates the container.

```bash
./scripts/rollback.sh list [compose-file]
./scripts/rollback.sh history <service>
./scripts/rollback.sh tag <service> v1.0.0            # name the running image before an update
./scripts/rollback.sh rollback <service> previous     # undo the last update
./scripts/rollback.sh rollback <service> v1.0.0
```

`deploy.sh` and `update-service.sh` tag every image they replace as `:previous`, which holds only the
last deployment; tag named versions you want to keep. `rollback` asks for confirmation and saves
the image it replaces as `:pre-rollback-<timestamp>`. Tagging also saves the service's
`generated/appsettings.<service>.json` under `generated/releases/`, and rolling back restores it.

Not rolled back: the **database** (migrations are forward-only — restore a backup if the schema
itself must go back), `.env`, and seed data. Rolling the portal back reaches nginx without a
restart, because the frontend container re-copies its build into the shared volume on start.

---

## Troubleshooting

| Symptom | Cause and fix |
|---|---|
| `db-init` fails, *relation does not exist* | Migrations not applied — run them ([QUICKSTART step 5](QUICKSTART.md#5-create-the-schema-ef-migrations)) and deploy again |
| `db-init` fails on the first deploy, user parses empty | Connection string written `userid=`; use `user id=` |
| Every call to one module is `FORBIDDEN`, service healthy | `db-init` did not run after the release that added it, or its service client is missing from `.env` / `clients.json` |
| Reports fail with `Setting 'ClientId' not found` | `REPORTING_CLIENT_ID` / `REPORTING_CLIENT_SECRET` missing |
| Every API answers 404 through nginx but works directly | Containers restarted by hand got new IPs; nginx resolves upstreams once. `docker compose restart nginx` |
| Build dies with *failed to execute bake: signal: killed* | Out of memory during the build — add swap and re-run; built images are cached |
| OpenIddict *ID2083: This server only accepts HTTPS requests* | The request did not arrive through the proxy with `X-Forwarded-Proto`, or the proxy is outside `AppSettings:KnownProxyNetworks` |
| CORS errors | `ALLOWED_CORS_ORIGINS` must match the portal origin exactly, including `https://` |
| A `/status` tile reads *Unknown* | That service's `REACT_APP_*` URL is wrong; fix `.env` and `./scripts/deploy.sh portal` |
| Everyone signed out after a deploy | `certificate.pfx` was replaced; keep the original unless rotating on purpose |
| A service runs old code | Check `git -C /opt/trackhub log -1 --oneline`, redeploy; `--no-cache` only as a last resort |
| `COPY ... /TrackHubCommon/...: not found` | Incomplete checkout; `./scripts/clone-repos.sh` fast-forwards it and names any missing directory |
| 502 Bad Gateway | `docker compose ps`, then `docker logs trackhub-<service>` |
| Database unreachable | `pg_hba.conf` / `listen_addresses`; inside containers the variables are `ConnectionStrings__*` (`docker exec trackhub-manager env \| grep ConnectionStrings`) — `DB_CONNECTION_*` exist only on `db-init` |

For more detail, add `Logging__LogLevel__Default=Debug` to a service's `environment` in the compose
file and recreate it.

---

## Security checklist

The repository ships generic development credentials on purpose — seeded client secrets, sample
appsettings, `.env` examples, the seeded administrator — so a development environment starts
without secret plumbing. A deployment replaces all of them:

1. Generate every `*_CLIENT_SECRET`, `ENCRYPTION_KEY`, `CERTIFICATE_PASSWORD` and the database
   password fresh; never commit `.env`.
2. Change the seeded administrator's password at first sign-in.
3. Use Let's Encrypt or a CA certificate, open only ports 80 and 443 (and SSH).
4. Keep Docker and the OS patched, back up configuration and databases, and watch the logs.
