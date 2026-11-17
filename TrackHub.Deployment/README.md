# TrackHub Deployment

Compose files, Dockerfiles, nginx configuration and operational scripts that run the whole stack —
portal, the eight backend services, the SyncWorker and the `db-init` seeder — behind one nginx
reverse proxy, against an external PostgreSQL with PostGIS.

| Document | Use it for |
|---|---|
| [QUICKSTART.md](QUICKSTART.md) | A first installation on one server, step by step |
| [INSTALL.md](INSTALL.md) | Requirements, configuration, certificates, database, split servers, updates, upgrades, backups, rollback, retention, troubleshooting |
| [LOCAL-DOCKER.md](LOCAL-DOCKER.md) | The development stack on Docker Desktop (Windows) |
| [Deployment and Operations](https://github.com/shernandezp/TrackHub/wiki/Deployment-and-Operations) (wiki) | Topology, routes, configuration groups, background jobs |

## Scripts

All run from this folder.

| Script | Purpose |
|---|---|
| `scripts/deploy.sh [full\|portal\|frontend\|backend] [--no-cache] [--skip-init] [--skip-git-check]` | Build, seed (`db-init`) and start a deployment |
| `scripts/update-service.sh <service> [compose-file] [--no-cache]` | Rebuild and recreate one service |
| `scripts/health-check.sh [domain] [protocol] [compose-file]` | Probe every deployed service; non-zero exit on failure |
| `scripts/logs.sh [service] [-n N] [--no-follow]` | Container logs |
| `scripts/rollback.sh list\|history\|tag\|rollback ...` | Image tags and rollback |
| `scripts/backup.sh` | Archive configuration, certificates and locally stored documents |
| `scripts/backup-database.sh backup\|list\|restore <file>\|cleanup [days]` | Both databases in one archive |
| `scripts/generate-certs.sh [domain] [email] [password] [--rotate-openiddict]` | Let's Encrypt certificate, OpenIddict certificate, renewal hook |
| `scripts/renew-ssl.sh` | Manual renewal check |
| `scripts/sync-config.sh generate\|frontend\|validate\|show` | Generate and check configuration from `.env` |
| `scripts/generate-appsettings.sh [--service s] [--output-dir d] [--env-file f]` | Render backend `appsettings.json` files |
| `scripts/clone-repos.sh` | Fast-forward the checkout and verify every source directory is present |
| `scripts/init-databases.sh`, `scripts/import-geocoding.sh` | Run inside the `db-init` image |
| `scripts/fetch-toll-catalog.mjs` | Build a toll catalog SQL script from Colombian open data |
| `scripts/sql/*.sql` | PostgreSQL tuning, history partitioning, retention and geocoding functions |
| `scripts/local/*.ps1` | The local Docker Desktop stack |

## Layout

```
TrackHub.Deployment/
├── docker-compose.yml            full stack
├── docker-compose.backend.yml    backend only (split deployment; also the base of the local stack)
├── docker-compose.frontend.yml   frontend only (split deployment)
├── docker-compose.local.yml      local Docker Desktop overlay
├── .env.example, .env.backend.example, .env.frontend.example   annotated configuration templates
├── .env.local                    local stack values (committed development settings)
├── config/                       clients.json.example, appsettings.template.json
├── certificates/                 TLS and OpenIddict certificates (gitignored)
├── docker/                       one Dockerfile + .dockerignore per image, frontend entrypoint
├── nginx/                        nginx configs for full, backend, frontend and local
└── scripts/
```

Images build with the repository root as context (`context: ..`), so this folder only works inside a
full checkout.

## Before you deploy

- **`db-init` seeds data, never schema.** Apply the EF migrations first, on every install and every
  upgrade that adds them.
- **Re-run `db-init` (any `deploy.sh full`) after a release that adds a module or client**, or that
  module answers `FORBIDDEN` while reporting healthy.
- **Uploaded documents live on the `manager-documents` volume**, not in PostgreSQL. Back it up, and
  never run `docker compose down -v`.
- **Schedule `CALL ops.run_purge();`** — nothing else deletes aged rows.

Requirements: Docker 24+, Compose v2.20+, PostgreSQL 14+ with PostGIS, a domain, and an
OpenRouteService API key.
