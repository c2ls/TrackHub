#!/bin/bash
# =============================================================================
# TrackHub Database Initialization Script
# =============================================================================
# Runs on every deploy (new installations AND updates). It performs:
#   1. ClientSeeder            - OpenIddict scopes + OAuth clients (idempotent upsert)
#   2. Security DBInitializer  - security resources/roles/service-client seed (idempotent)
#   3. Manager DBInitializer   - master data seed: reports, transporter types, etc. (idempotent)
#
# Every step is safe to re-run: the seeders upsert / guard every insert with an
# existence check, so on updates they simply add anything new (e.g. newly
# introduced OAuth clients or permission resources) and leave existing data
# untouched. The seeded administrator and master account carry the well-known
# PlatformBootstrap ids in both databases, so no cross-database id sync exists.
#
# NOTE: EF schema migrations ("DB updates") are applied separately from this
# script (the seeders assume the schema already exists, and ClientSeeder stops if
# the OpenIddict tables are missing). See QUICKSTART.md -> "Create the schema".
# =============================================================================

set -e

# Colors / logging helpers
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_success() { echo -e "${GREEN}✓ $1${NC}"; }
print_warning() { echo -e "${YELLOW}⚠ $1${NC}"; }
print_error()   { echo -e "${RED}✗ $1${NC}"; }
print_info()    { echo -e "${BLUE}ℹ $1${NC}"; }

echo "=========================================="
echo "Starting TrackHub Database Initialization"
echo "=========================================="

# Wait for database to be ready
wait_for_db() {
    local connection_string=$1
    local db_name=$2

    echo "Waiting for $db_name database to be ready..."

    # Extract host and port from connection string
    local host
    local port
    host=$(echo "$connection_string" | grep -oP 'server=\K[^;]+')
    port=$(echo "$connection_string" | grep -oP 'port=\K[^;]+')
    port=${port:-5432}

    local max_attempts=30
    local attempt=1

    while [ $attempt -le $max_attempts ]; do
        if pg_isready -h "$host" -p "$port" > /dev/null 2>&1; then
            echo "$db_name database is ready!"
            return 0
        fi
        echo "Attempt $attempt/$max_attempts: $db_name database not ready yet..."
        sleep 5
        attempt=$((attempt + 1))
    done

    echo "ERROR: $db_name database did not become ready in time"
    return 1
}

# Wait for database
wait_for_db "$DB_CONNECTION_SECURITY" "Security"

# -----------------------------------------------------------------------------
# Step 1: ClientSeeder (idempotent - runs every deploy)
# -----------------------------------------------------------------------------
echo ""
echo "=========================================="
echo "Step 1: Running ClientSeeder"
echo "=========================================="
cd /app/client-seeder

# Update connection string in appsettings
cat > appsettings.json << EOF
{
  "ConnectionStrings": {
    "Security": "$DB_CONNECTION_SECURITY"
  }
}
EOF

# The image's own clients.json is the development set (localhost redirects): seeding
# it into a real installation would silently repoint web_client, so a mounted file is required.
if [ ! -f "/app/clients.json" ]; then
    print_error "No clients.json mounted at /app/clients.json; refusing to seed the development clients."
    exit 1
fi
cp /app/clients.json ./clients.json
echo "Using provided clients.json"

dotnet TrackHub.AuthorityServer.ClientSeeder.dll || { print_error "ClientSeeder failed"; exit 1; }
print_success "ClientSeeder completed successfully!"

# -----------------------------------------------------------------------------
# Step 2: Security DBInitializer (idempotent - runs every deploy)
# -----------------------------------------------------------------------------
echo ""
echo "=========================================="
echo "Step 2: Running Security DBInitializer"
echo "=========================================="
cd /app/security-init

cat > appsettings.json << EOF
{
  "ConnectionStrings": {
    "Security": "$DB_CONNECTION_SECURITY"
  }
}
EOF

dotnet TrackHub.Security.DBInitializer.dll || { print_error "Security DBInitializer failed"; exit 1; }
print_success "Security DBInitializer completed successfully!"

# -----------------------------------------------------------------------------
# Step 3: Manager DBInitializer (idempotent - runs every deploy)
# -----------------------------------------------------------------------------
echo ""
echo "=========================================="
echo "Step 3: Running Manager DBInitializer"
echo "=========================================="
cd /app/manager-init

cat > appsettings.json << EOF
{
  "ConnectionStrings": {
    "DefaultConnection": "$DB_CONNECTION_MANAGER"
  }
}
EOF

dotnet TrackHub.Manager.DBInitializer.dll || { print_error "Manager DBInitializer failed"; exit 1; }
print_success "Manager DBInitializer completed successfully!"

echo ""
echo "=========================================="
echo "Seeding Complete!"
echo "=========================================="
echo ""

# -----------------------------------------------------------------------------
# Step 3b: UTC session time zone on every database (idempotent - runs every deploy)
# -----------------------------------------------------------------------------
# The services pin their own sessions to UTC; this covers psql, cron and every hand-run script so
# no result ever depends on the host's zone.
pin_utc_timezone() {
    local connection_string=$1
    local host port user pass db

    host=$(echo "$connection_string" | grep -oP 'server=\K[^;]+')
    port=$(echo "$connection_string" | grep -oP 'port=\K[^;]+'); port=${port:-5432}
    user=$(echo "$connection_string" | grep -oP 'user id=\K[^;]+')
    pass=$(echo "$connection_string" | grep -oP 'password=\K[^;]+')
    db=$(echo "$connection_string" | grep -oP 'database=\K[^;]+')
    [ -z "$db" ] && return 0

    if PGPASSWORD="$pass" psql -h "$host" -p "$port" -U "$user" -d "$db" -q \
        -c "ALTER DATABASE \"$db\" SET timezone TO 'UTC'"; then
        print_success "Database $db: session time zone pinned to UTC."
    else
        print_warning "Could not pin the time zone of $db (needs its owner or a superuser); sessions keep the server default."
    fi
}

echo ""
echo "=========================================="
echo "Step 3b: Pinning database time zones to UTC"
echo "=========================================="
for conn in "$DB_CONNECTION_SECURITY" "$DB_CONNECTION_MANAGER" "${DB_CONNECTION_TELEMETRY:-}"; do
    [ -n "$conn" ] && pin_utc_timezone "$conn"
done

# -----------------------------------------------------------------------------
# Step 4: retention functions (idempotent - runs every deploy)
# -----------------------------------------------------------------------------
# Installed here so a fresh install has them; scheduling `CALL ops.run_purge()` stays the
# operator's job. Function bodies are checked when they first run, not now: on a fresh install the
# module tables they reference are created later, when each service applies its migrations.
install_sql() {
    local connection_string=$1
    local file=$2
    local host port user pass db

    host=$(echo "$connection_string" | grep -oP 'server=\K[^;]+')
    port=$(echo "$connection_string" | grep -oP 'port=\K[^;]+'); port=${port:-5432}
    user=$(echo "$connection_string" | grep -oP 'user id=\K[^;]+')
    pass=$(echo "$connection_string" | grep -oP 'password=\K[^;]+')
    db=$(echo "$connection_string" | grep -oP 'database=\K[^;]+')

    if PGPASSWORD="$pass" PGOPTIONS='-c check_function_bodies=off' \
        psql -h "$host" -p "$port" -U "$user" -d "$db" -q -v ON_ERROR_STOP=1 -f "$file"; then
        print_success "Installed $(basename "$file") into $db."
    else
        print_error "Could not install $(basename "$file") into $db."
        exit 1
    fi
}

echo ""
echo "=========================================="
echo "Step 4: Installing the retention functions"
echo "=========================================="
install_sql "$DB_CONNECTION_MANAGER" /app/sql/purge-functions.sql

echo ""
echo "=========================================="
echo "Step 5: Local geocoding schema and data"
echo "=========================================="
install_sql "$DB_CONNECTION_MANAGER" /app/sql/geocoding-schema.sql
if [ "${LOCAL_GEOCODING:-false}" = true ]; then
    /app/import-geocoding.sh || print_warning "Local geocoding import failed; the next deploy retries it."
fi

echo ""
print_success "Initialization complete."
