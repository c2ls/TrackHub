# TrackHub Quick Start

A first installation, end to end, on one Linux server. The steps depend on each other — follow them
in order. Everything else (configuration reference, split servers, upgrades, backups, rollback,
troubleshooting) is in [INSTALL.md](INSTALL.md).

## What you need

| | |
|---|---|
| **Server** | Ubuntu 24.04 LTS, 4+ CPU cores, 8 GB RAM plus 4 GB swap, 50 GB SSD |
| **Database** | PostgreSQL 14+ reachable from the server, with the PostGIS extension available |
| **Domain** | A DNS name pointing at the server, ports 80 and 443 open |
| **Route planning** | An OpenRouteService API key — free, same day, at <https://openrouteservice.org/dev/#/signup> |
| **Migration host** | The .NET 10 SDK and `dotnet-ef`, on any machine that can reach PostgreSQL (this server is fine) |

## 1. Install Docker

```bash
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER
```

Log out and back in, then check `docker --version` and `docker compose version`. On a small host,
add swap before the first build (image builds spike above 4 GB):

```bash
sudo fallocate -l 4G /swapfile && sudo chmod 600 /swapfile && sudo mkswap /swapfile && sudo swapon /swapfile
echo "/swapfile none swap sw 0 0" | sudo tee -a /etc/fstab
```

## 2. Clone the repository

```bash
sudo mkdir -p /opt/trackhub && sudo chown $USER:$USER /opt/trackhub
git clone https://github.com/shernandezp/TrackHub.git /opt/trackhub
cd /opt/trackhub/TrackHub.Deployment
```

The checkout root is the Docker build context: every image builds from the service directories
beside `TrackHub.Deployment/`. The SyncWorker builds from `TrackHubRouter/`.

## 3. Prepare the databases

As a PostgreSQL **superuser** (`CREATE EXTENSION` needs one):

```sql
CREATE DATABASE "TrackHubSecurity";
CREATE DATABASE "TrackHub";
CREATE USER trackhub WITH PASSWORD 'YourStrongPassword';
GRANT ALL PRIVILEGES ON DATABASE "TrackHubSecurity" TO trackhub;
GRANT ALL PRIVILEGES ON DATABASE "TrackHub" TO trackhub;

-- PostgreSQL 15+: the role also needs the public schema, where the log sink creates its table
\c TrackHubSecurity
GRANT ALL ON SCHEMA public TO trackhub;
\c TrackHub
GRANT ALL ON SCHEMA public TO trackhub;
CREATE EXTENSION IF NOT EXISTS postgis;
```

## 4. Configure `.env`

```bash
cp .env.example .env
nano .env
```

`.env.example` documents every key. `deploy.sh` refuses to run while any of these is empty or still
a placeholder:

| Key | Value |
|---|---|
| `DOMAIN`, `LETSENCRYPT_EMAIL` | Your domain and the Let's Encrypt contact |
| `ALLOWED_CORS_ORIGINS` | `https://<domain>` |
| `AUTHORITY_URL` | `https://<domain>/Identity` |
| `DB_CONNECTION_SECURITY` | `server=DB_HOST;port=5432;database=TrackHubSecurity;user id=trackhub;password=...` |
| `DB_CONNECTION_MANAGER`, `DB_CONNECTION_TELEMETRY`, `DB_CONNECTION_LOGGING` | The same string with `database=TrackHub` — all three use the `TrackHub` database |
| `CERTIFICATE_PASSWORD` | A strong password for the token-signing certificate |
| `ENCRYPTION_KEY` | A new GUID (`uuidgen`) |
| `SYNCWORKER_`, `ROUTER_`, `SECURITY_`, `GEOFENCE_`, `TRIP_`, `REPORTING_`, `MANAGER_CLIENT_SECRET` | One strong random secret each, e.g. `openssl rand -hex 32` |

Also set `ORS_API_KEY` (without it every route plan fails with `ROUTING_NOT_CONFIGURED`) and replace
`your-domain.com` in every `REACT_APP_*` URL.

> Write the user as **`user id=`**, with the space. The scripts parse that exact key; `userid=`
> makes `db-init` fail on the first deploy.

## 5. Create the schema (EF migrations)

`db-init`, which runs during every deploy, **only seeds data** — it never creates tables. Against
empty databases it fails. Five projects own migrations; Telemetry's tables come from Manager's.

```bash
dotnet tool install --global dotnet-ef
cd /opt/trackhub

export SECURITY_CONN="server=DB_HOST;port=5432;database=TrackHubSecurity;user id=trackhub;password=YourStrongPassword"
export TRACKHUB_CONN="server=DB_HOST;port=5432;database=TrackHub;user id=trackhub;password=YourStrongPassword"
export Database__CommandTimeoutSeconds=0    # index builds outlast the services' 30 s command timeout

ConnectionStrings__Security="$SECURITY_CONN" dotnet ef database update \
  --project TrackHubSecurity/src/Infrastructure/SecurityDB --startup-project TrackHubSecurity/src/Web

# OpenIddict tables, same database, after Security
ConnectionStrings__Security="$SECURITY_CONN" dotnet ef database update \
  --project TrackHub.AuthorityServer/src/Infrastructure --startup-project TrackHub.AuthorityServer/src/Web \
  --context AuthorityDbContext

# app, map and telemetry
ConnectionStrings__DefaultConnection="$TRACKHUB_CONN" dotnet ef database update \
  --project TrackHub.Manager/src/Infrastructure/ManagerDB --startup-project TrackHub.Manager/src/Web

ConnectionStrings__DefaultConnection="$TRACKHUB_CONN" dotnet ef database update \
  --project TrackHub.Geofencing/src/Infrastructure/ManagerDB --startup-project TrackHub.Geofencing/src/Web

ConnectionStrings__DefaultConnection="$TRACKHUB_CONN" dotnet ef database update \
  --project TrackHub.TripManagement/src/Infrastructure/TripDB --startup-project TrackHub.TripManagement/src/Web

cd /opt/trackhub/TrackHub.Deployment
```

Always pass the connection strings: the committed `appsettings.json` files point at a local
development database, which EF would otherwise use silently. Re-run the same five commands on every
upgrade — EF applies only what is pending.

## 6. Certificates

```bash
sudo ./scripts/generate-certs.sh
```

It reads `DOMAIN`, `LETSENCRYPT_EMAIL` and `CERTIFICATE_PASSWORD` from `.env`, obtains the Let's
Encrypt certificate, creates the OpenIddict token-signing `certificates/certificate.pfx` if none
exists, and installs the renewal hook that copies renewed files into `certificates/` and reloads
nginx.

## 7. OAuth clients

```bash
cp config/clients.json.example config/clients.json
sed -i 's/<<YOUR_DOMAIN>>/your-domain.com/g' config/clients.json
```

Only the `web_client` URIs need your domain. Leave the `<<GENERATE_A_SECURE_SECRET>>` placeholders:
the seeder takes every service client's secret from the matching `*_CLIENT_SECRET` in `.env`.

## 8. Deploy

```bash
./scripts/deploy.sh full
```

The first run builds every image one at a time and takes a while. Before stopping anything it runs
`db-init` (OAuth clients, roles and permissions, master data), so a failed seed leaves nothing
half-deployed.

## 9. Verify

```bash
./scripts/health-check.sh your-domain.com
```

Open `https://your-domain.com` for the login page and `https://your-domain.com/status` — the status
page needs no sign-in and shows one tile per service. Every tile should read *Working*; an *Unknown*
tile usually means that service's `REACT_APP_*` URL is wrong in `.env`.

Sign in with the administrator the Security seeder creates on an empty database —
`email@mail.com` / `12345678` — and **change that password immediately** (Profile → password).

**Next:** schedule the retention job and set up backups — [INSTALL.md → Operations](INSTALL.md#operations).
