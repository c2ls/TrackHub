# TrackHub Deployment

Docker-based deployment for the whole TrackHub application stack.

---

## Overview

This repository holds the compose files, Dockerfiles, nginx configuration, certificate tooling and operational scripts that stand up TrackHub — frontend, all backend services, the background worker and the database seeder — behind a single nginx reverse proxy.

- **Complete stack orchestration** with one command, or frontend-only / backend-only
- **Automated SSL** — certificate generation and Let's Encrypt auto-renewal
- **Centralized configuration** — every service's `appsettings.json` generated from one `.env`
- **Centralized database logging** — a shared PostgreSQL Serilog sink for APIs and background services
- **Database backup and restore** with versioned restore
- **Health monitoring** — built-in health checks, plus a **public status page** at `/status` that works without signing in, and platform-wide maintenance announcements
- **Version management** — tag, list and roll back per service
- **Document storage** — a local volume by default, or S3 / Azure Blob

**Deployment topology, ports, routes, configuration keys, schema ownership and the background job catalog are documented in the wiki: [Deployment and Operations](https://github.com/shernandezp/TrackHub/wiki/Deployment-and-Operations).** This README and the two guides below cover the procedures.

---

## Guides

| Document | Use it for |
|---|---|
| [QUICKSTART.md](QUICKSTART.md) | A first installation, end to end — databases, PostGIS, migrations and OAuth clients in order |
| [INSTALL.md](INSTALL.md) | The full guide: detailed installation, [configuration reference](INSTALL.md#configuration-reference), [upgrades](INSTALL.md#upgrading-from-a-previous-version), SSL, monitoring, [troubleshooting](INSTALL.md#troubleshooting) |

New installs should follow [QUICKSTART.md](QUICKSTART.md) end to end.

---

## Quick start

```bash
# 1. Configure environment
cp .env.example .env
nano .env

# 2. Configure OAuth clients
cp config/clients.json.example config/clients.json
nano config/clients.json

# 3. Create the database schema — REQUIRED. db-init only SEEDS data; it does not
#    create tables. See QUICKSTART.md Step 5 for the four `dotnet ef database update`
#    commands (Security, Manager, Geofencing, TripManagement). Skipping this makes db-init fail.

# 4. Generate certificates
sudo ./scripts/generate-certs.sh your-domain.com admin@your-domain.com

# 5. Deploy
./scripts/deploy.sh full --build

# 6. Check health
./scripts/health-check.sh your-domain.com
```

---

## Deployment options

| Command | Description |
|---------|-------------|
| `./scripts/deploy.sh full` | Frontend + all backend services (default) |
| `./scripts/deploy.sh frontend` | Frontend only |
| `./scripts/deploy.sh backend` | Backend services only |

Builds use the Docker layer cache and reliably detect source changes, so updated code is always deployed without needing `--no-cache` or repeated runs. Containers are always started with `--force-recreate` so the freshly built images take effect, and the frontend refreshes its static assets on every start.

```bash
./scripts/deploy.sh full --build            # normal, cached, deterministic
./scripts/deploy.sh full --build --no-cache # optional full rebuild
```

See [INSTALL.md → Updating Services](INSTALL.md#updating-services) for the authoritative update procedure and how deterministic rebuilds work.

---

## Service management

```bash
# Update a single service
./scripts/update-service.sh manager

# View logs
./scripts/logs.sh
./scripts/logs.sh manager -n 50

# Health check
./scripts/health-check.sh your-domain.com

# Backup configuration
./scripts/backup.sh

# Database backup and restore (takes NO database argument — dumps both into one archive)
./scripts/backup-database.sh backup                                    # Back up both DBs
./scripts/backup-database.sh list                                      # List backups
./scripts/backup-database.sh restore backups/database/<file>.tar.gz    # Restore
./scripts/backup-database.sh cleanup 7                                 # Prune old backups

# Version management and rollback (both take a SERVICE name)
./scripts/rollback.sh tag manager v1.0.0        # Tag a service's current image
./scripts/rollback.sh list                      # List versions
./scripts/rollback.sh history manager           # Show a service's image history
./scripts/rollback.sh rollback manager previous # Undo the last update (:previous is
                                                # tagged automatically by every update)
./scripts/rollback.sh rollback manager v1.0.0   # Roll a service back to a named tag
```

The portal reports its own build in the footer and on the public `/status` page — the only
way to confirm which frontend a browser actually loaded. Backend services are identified by
their image tags (`rollback.sh list`). Full procedure: `INSTALL.md` →
*Version Management & Rollback*.

---

## Configuration management

All backend services share similar `appsettings.json` configurations, generated from the central `.env` into `generated/` and bind-mounted read-only over each container's `/app/appsettings.json` by the compose files. `deploy.sh` regenerates them from `.env` before every backend or full deploy; **environment variables set in compose still take precedence over the mounted file.**

```bash
# Preview appsettings.json for all services (stdout)
./scripts/generate-appsettings.sh

# Generate all backend configs into generated/
./scripts/generate-appsettings.sh --output-dir ./generated

# Generate for a specific service
./scripts/generate-appsettings.sh --service manager --output-dir ./generated

# Generate all backend configs (same as generate-appsettings.sh --output-dir generated)
./scripts/sync-config.sh generate

# Validate configuration
./scripts/sync-config.sh validate

# Show current configuration
./scripts/sync-config.sh show
```

The full key-by-key reference is in [INSTALL.md → Configuration Reference](INSTALL.md#configuration-reference).

---

## Gotchas

### db-init seeds data — it never creates schema

TrackHub uses EF Core migrations as the source of truth for schema. The `db-init` container **seeds data only**, so migrations must be applied — for new installations **and** updates — with your EF migration process, **before** deploying the updated services.

| Migration project | Database | Schemas |
|---------|----------|--------|
| TrackHubSecurity | `TrackHubSecurity` | `security` (+ OpenIddict) |
| TrackHub.Manager | `TrackHub` | `app`, `map`, **and `telemetry`** |
| TrackHub.Geofencing | `TrackHub` | `geofencing` (PostGIS) |
| TrackHub.TripManagement | `TrackHub` | `trip` (PostGIS) |

> **Telemetry has no migrations of its own** — its `telemetry`-schema tables are created by the Manager migrations, so `DB_CONNECTION_TELEMETRY` must point at the same `TrackHub` database.

> **PostgreSQL must have PostGIS enabled** for the Geofencing and TripManagement schemas. One `CREATE EXTENSION` covers both — they share the `TrackHub` database.

> **The migration host needs the .NET SDK and `dotnet-ef`.** The `TrackHubCommon.*` packages are not on nuget.org, so pack them from the `TrackHubCommon/` source into a local feed and register it before running `dotnet ef`. Docker image builds pack them automatically in a `common` stage. Full commands: [QUICKSTART.md Step 5](QUICKSTART.md) / [INSTALL.md → Applying Migrations](INSTALL.md#applying-migrations).

### Re-run db-init after adding a service

A migration creates the schema but seeds nothing. **After adding TripManagement to an existing deployment, re-run `db-init`** — without the `trip_client` registration and the `Trips` / `TripTracking` / `TollCatalog` resource and role seeding, **every trip call returns `FORBIDDEN` while the service reports healthy.** The same applies to any new module.

See [INSTALL.md → Upgrading From a Previous Version](INSTALL.md#upgrading-from-a-previous-version).

### Uploaded documents are not in PostgreSQL

They live on the `manager-documents` volume (or in S3 / Azure Blob), capped at **50 MB** per upload by nginx. This volume is the only stateful data outside PostgreSQL.

- `backup-database.sh` does **not** cover it — back it up separately, see [INSTALL.md](INSTALL.md#backing-up-uploaded-documents).
- **Never run `docker compose down -v`** — it deletes them.

### Publish maintenance announcements before taking Manager down

Announcements are stored *and served* by Manager, so a Manager outage removes the status-page banner while the service tiles keep working.

### TripManagement needs an OpenRouteService key

See [INSTALL.md → OpenRouteService provisioning](INSTALL.md#openrouteservice-provisioning-required).

### Centralized logging

Requires a `TrackHub` database and the `DB_CONNECTION_LOGGING` environment variable. The Serilog sink auto-creates the `logs` table on first write.

---

## Project structure

```
TrackHub.Deployment/
├── docker-compose.yml           # Full stack deployment
├── docker-compose.frontend.yml  # Frontend-only deployment
├── docker-compose.backend.yml   # Backend-only deployment
├── .env.example                 # Environment template (full stack)
├── .env.frontend.example        # Environment template (frontend)
├── .env.backend.example         # Environment template (backend)
├── INSTALL.md                   # Detailed installation guide
├── QUICKSTART.md                # Simplified guide for beginners
├── README.md                    # This file
├── certificates/                # SSL and OpenIddict certificates
├── config/
│   ├── clients.json.example     # OAuth clients configuration
│   └── appsettings.template.json # Master config template
├── docker/
│   ├── Dockerfile.frontend      # React frontend
│   ├── Dockerfile.authority     # Authority Server
│   ├── Dockerfile.security      # Security API
│   ├── Dockerfile.manager       # Manager API
│   ├── Dockerfile.router        # Router API
│   ├── Dockerfile.geofencing    # Geofencing API
│   ├── Dockerfile.tripmanagement # Trip Management API
│   ├── Dockerfile.reporting     # Reporting API
│   ├── Dockerfile.telemetry     # Telemetry API
│   ├── Dockerfile.syncworker    # SyncWorker background service
│   ├── Dockerfile.db-init       # Database initialization
│   ├── Dockerfile.*.dockerignore # Per-Dockerfile ignore files (exclude bin/obj/node_modules)
│   └── frontend-entrypoint.sh   # Refreshes frontend assets in the shared volume on start
├── nginx/
│   ├── nginx.conf               # Full stack nginx config
│   ├── nginx.frontend.conf      # Frontend-only nginx config
│   └── nginx.backend.conf       # Backend-only nginx config
├── nuget-packages/              # NuGet feed config for TrackHubCommon
│   └── nuget.config             # NuGet source configuration (packages packed in-container)
└── scripts/
    ├── deploy.sh                # Main deployment script
    ├── update-service.sh        # Update individual services
    ├── health-check.sh          # Health check script
    ├── logs.sh                  # Log viewer
    ├── backup.sh                # Configuration backup
    ├── backup-database.sh       # PostgreSQL backup/restore
    ├── rollback.sh              # Version rollback utility
    ├── generate-certs.sh        # Certificate generation
    ├── renew-ssl.sh             # SSL auto-renewal (Let's Encrypt)
    ├── generate-appsettings.sh  # Generate appsettings.json files
    ├── sync-config.sh           # Sync all configuration
    ├── sync-user-account-ids.sh # Sync User/Account IDs between DBs
    └── init-databases.sh        # Database initialization
```

---

## Requirements

- Docker 24.0+
- Docker Compose v2.20+
- PostgreSQL 14+ (external, with PostGIS)
- SSL certificate
- Domain name
- An OpenRouteService API key, if TripManagement is deployed

---

## Documentation

- **Technical** — the [TrackHub wiki](https://github.com/shernandezp/TrackHub/wiki): [Deployment and Operations](https://github.com/shernandezp/TrackHub/wiki/Deployment-and-Operations), [Database](https://github.com/shernandezp/TrackHub/wiki/Database), [Security and Identity](https://github.com/shernandezp/TrackHub/wiki/Security-and-Identity), [Technology](https://github.com/shernandezp/TrackHub/wiki/Technology)
- **Procedures** — [QUICKSTART.md](QUICKSTART.md) and [INSTALL.md](INSTALL.md) in this repository
- **User** — in the app: the Help button or **F1** on any screen

For issues: [GitHub Issues](https://github.com/shernandezp/TrackHub/issues)

---

## License

Apache License 2.0.
