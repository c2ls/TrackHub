#!/bin/bash
# =============================================================================
# TrackHub AppSettings Generator
# =============================================================================
# Generates appsettings.json files for all services from a central configuration
# Usage: ./generate-appsettings.sh [--output-dir <dir>] [--env-file <file>]
# =============================================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

# Everything this script writes contains service secrets and database credentials.
umask 077

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_success() { echo -e "${GREEN}✓ $1${NC}"; }
print_warning() { echo -e "${YELLOW}⚠ $1${NC}"; }
print_error() { echo -e "${RED}✗ $1${NC}"; }
print_info() { echo -e "${BLUE}ℹ $1${NC}"; }

# Default values
OUTPUT_DIR=""
ENV_FILE="$PROJECT_DIR/.env"

usage() {
    echo "Usage: $0 [options]"
    echo ""
    echo "Options:"
    echo "  --output-dir <dir>   Output directory for generated files (default: prints to stdout)"
    echo "  --env-file <file>    Environment file to load (default: ../.env)"
    echo "  --service <name>     Generate only for specific service"
    echo "  --list-services      List available services"
    echo "  --help               Show this help"
    echo ""
    echo "Examples:"
    echo "  $0 --output-dir ./generated"
    echo "  $0 --service manager --output-dir ./generated"
}

# Parse arguments
SERVICE_FILTER=""
while [[ $# -gt 0 ]]; do
    case $1 in
        --output-dir)
            OUTPUT_DIR="$2"
            shift 2
            ;;
        --env-file)
            ENV_FILE="$2"
            shift 2
            ;;
        --service)
            SERVICE_FILTER="$2"
            shift 2
            ;;
        --list-services)
            echo "Available services:"
            echo "  authority  - TrackHub.AuthorityServer"
            echo "  security   - TrackHubSecurity"
            echo "  manager    - TrackHub.Manager"
            echo "  router     - TrackHubRouter"
            echo "  geofencing - TrackHub.Geofencing"
            echo "  tripmanagement - TrackHub.TripManagement"
            echo "  telemetry  - TrackHub.Telemetry"
            echo "  reporting  - TrackHub.Reporting"
            echo "  syncworker - TrackHubRouter (SyncWorker)"
            exit 0
            ;;
        --help|-h)
            usage
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            usage
            exit 1
            ;;
    esac
done

# Load environment variables
if [ -f "$ENV_FILE" ]; then
    print_info "Loading environment from: $ENV_FILE"
    set -a
    source "$ENV_FILE"
    set +a
else
    print_warning "Environment file not found: $ENV_FILE"
    print_info "Using system environment variables"
fi

# Set defaults for variables that might not be set
CERTIFICATE_PATH=${CERTIFICATE_PATH:-"/app/certificates/certificate.pfx"}
CERTIFICATE_THUMBPRINT=${CERTIFICATE_THUMBPRINT:-""}
OPENIDDICT_SCOPES=${OPENIDDICT_SCOPES:-"mobile_scope,staff_mobile_scope,web_scope,service_scope"}
# Audience the APIs validate access tokens against (matches every service appsettings.json)
VALID_AUDIENCE=${VALID_AUDIENCE:-"trackhub_api"}

# config/appsettings.template.json documents every configurable value and its environment
# variable mapping; the generators below are the single source of truth for what is written
# (keep them in sync with the template when adding a setting).

# Every value is written into a JSON string: a secret carrying a quote, a backslash or a line break
# must not end that string early.
json_escape() {
    local value=$1 bs='\'
    value=${value//"$bs"/"$bs$bs"}
    value=${value//'"'/"$bs\""}
    value=${value//$'\n'/"${bs}n"}
    value=${value//$'\r'/"${bs}r"}
    value=${value//$'\t'/"${bs}t"}
    printf '%s' "$value"
}

# A service that cannot parse its appsettings.json crash-loops; refuse to write one instead.
validate_json() {
    local file=$1
    if python3 -c "" > /dev/null 2>&1; then
        python3 -c 'import json, sys; json.load(open(sys.argv[1], encoding="utf-8"))' "$file" \
            || { print_error "Generated $file is not valid JSON."; exit 1; }
    elif jq --version > /dev/null 2>&1; then
        jq empty "$file" || { print_error "Generated $file is not valid JSON."; exit 1; }
    else
        print_warning "Neither python3 nor jq is installed; $file was not validated."
    fi
}

# Serilog + Columns blocks, kept in sync with config/appsettings.template.json.
# The TrackHubPostgreSQL sink (Common.Infrastructure) resolves "connectionString" as a
# connection string NAME ("Logging") and owns its column set, so no "Columns" block is needed.
# It also carries "restrictedToMinimumLevel": "Warning" so production
# stops flooding the logs table with Information; Console keeps the "MinimumLevel"
# default (Information). This note cannot live inside the heredoc: a "#" there would be
# emitted verbatim and break the JSON.
serilog_section() {
    cat << EOF
  "Serilog": {
    "Using": [
      "Common.Infrastructure"
    ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "OpenIddict": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "TrackHubPostgreSQL",
        "Args": {
          "restrictedToMinimumLevel": "Warning",
          "connectionString": "Logging",
          "batchSizeLimit": 50,
          "period": "00:00:02"
        }
      }
    ]
  }
EOF
}

# Generate appsettings for Authority Server
generate_authority() {
    cat << EOF
{
  "ConnectionStrings": {
    "Security": "$(json_escape "${DB_CONNECTION_SECURITY}")",
    "Logging": "$(json_escape "${DB_CONNECTION_LOGGING}")"
  },
$(serilog_section),
  "OpenIddict": {
    "LoadCertFromFile": true,
    "Path": "$(json_escape "${CERTIFICATE_PATH}")",
    "Password": "$(json_escape "${CERTIFICATE_PASSWORD}")",
    "Thumbprint": "$(json_escape "${CERTIFICATE_THUMBPRINT}")",
    "Scopes": "$(json_escape "${OPENIDDICT_SCOPES}")"
  },
  "AllowedHosts": "*",
  "AllowedCorsOrigins": "$(json_escape "${ALLOWED_CORS_ORIGINS}")"
}
EOF
}

# Generate appsettings for Security API
generate_security() {
    cat << EOF
{
  "ConnectionStrings": {
    "Security": "$(json_escape "${DB_CONNECTION_SECURITY}")",
    "Logging": "$(json_escape "${DB_CONNECTION_LOGGING}")"
  },
$(serilog_section),
  "AuthorityServer": {
    "Authority": "$(json_escape "${AUTHORITY_URL}")",
    "ValidateAudience": true,
    "ValidAudience": "$(json_escape "${VALID_AUDIENCE}")",
    "ValidateIssuer": true,
    "ValidateIssuerSigningKey": true,
    "ClientId": "$(json_escape "${SECURITY_CLIENT_ID}")",
    "ClientSecret": "$(json_escape "${SECURITY_CLIENT_SECRET}")",
    "Scope": "service_scope"
  },
  "OpenIddict": {
    "LoadCertFromFile": true,
    "Path": "$(json_escape "${CERTIFICATE_PATH}")",
    "Password": "$(json_escape "${CERTIFICATE_PASSWORD}")",
    "Thumbprint": "$(json_escape "${CERTIFICATE_THUMBPRINT}")"
  },
  "AppSettings": {
    "GraphQLManagerService": "$(json_escape "${GRAPHQL_MANAGER_SERVICE}")",
    "EncryptionKey": "$(json_escape "${ENCRYPTION_KEY}")"
  },
  "AllowedHosts": "*",
  "AllowedCorsOrigins": "$(json_escape "${ALLOWED_CORS_ORIGINS}")"
}
EOF
}

# Generate appsettings for Manager API
# DocumentStorage: Provider = LocalFileSystem (default) | S3 | AzureBlob.
# S3 REQUIRES DOCUMENT_S3_BUCKET_NAME and AzureBlob REQUIRES DOCUMENT_AZURE_CONTAINER_NAME
# + DOCUMENT_AZURE_CONNECTION_STRING, otherwise TrackHub.Manager throws at startup.
# Every value is emitted as a JSON string (as in config/appsettings.template.json): the
# .NET configuration binder converts strings to bool/int, and treats an empty string as
# "not set" (GetValue<bool?>/<int?> fall back to their defaults), so the unused provider's
# section is harmless and the JSON stays valid when the variables are empty.
# AppSettings:Smtp / AppSettings:WhatsApp power the spec-05 notification channels. They bind
# through Configure<SmtpOptions>/<WhatsAppOptions>, whose Port/UseStartTls/From*/ApiBaseUrl/
# TemplateName properties are NOT nullable - an empty string there fails to convert or wipes
# the class default, so those keys carry the same defaults as docker-compose.yml. An empty
# Smtp:Host (or WhatsApp PhoneNumberId/AccessToken) is the documented way to disable a channel.
generate_manager() {
    cat << EOF
{
  "ConnectionStrings": {
    "DefaultConnection": "$(json_escape "${DB_CONNECTION_MANAGER}")",
    "Logging": "$(json_escape "${DB_CONNECTION_LOGGING}")"
  },
$(serilog_section),
  "AuthorityServer": {
    "Authority": "$(json_escape "${AUTHORITY_URL}")",
    "ClientId": "$(json_escape "${MANAGER_CLIENT_ID:-manager_client}")",
    "ClientSecret": "$(json_escape "${MANAGER_CLIENT_SECRET}")",
    "Scope": "service_scope",
    "ValidateAudience": true,
    "ValidAudience": "$(json_escape "${VALID_AUDIENCE}")",
    "ValidateIssuer": true,
    "ValidateIssuerSigningKey": true
  },
  "AppSettings": {
    "GraphQLIdentityService": "$(json_escape "${GRAPHQL_IDENTITY_SERVICE}")",
    "GraphQLSecurityService": "$(json_escape "${GRAPHQL_SECURITY_SERVICE}")",
    "GraphQLRouterService": "$(json_escape "${GRAPHQL_ROUTER_SERVICE}")",
    "EncryptionKey": "$(json_escape "${ENCRYPTION_KEY}")",
    "PortalBaseUrl": "$(json_escape "${PORTAL_BASE_URL:-https://${DOMAIN}}")",
    "NotificationSendingReclaimMinutes": "$(json_escape "${NOTIFICATION_SENDING_RECLAIM_MINUTES:-10}")",
    "Smtp": {
      "Host": "$(json_escape "${SMTP_HOST:-}")",
      "Port": "$(json_escape "${SMTP_PORT:-25}")",
      "Username": "$(json_escape "${SMTP_USERNAME:-}")",
      "Password": "$(json_escape "${SMTP_PASSWORD:-}")",
      "UseStartTls": "$(json_escape "${SMTP_USE_STARTTLS:-false}")",
      "FromAddress": "$(json_escape "${SMTP_FROM_ADDRESS:-alerts@trackhub.local}")",
      "FromName": "$(json_escape "${SMTP_FROM_NAME:-TrackHub Alerts}")"
    },
    "WhatsApp": {
      "ApiBaseUrl": "$(json_escape "${WHATSAPP_API_BASE_URL:-https://graph.facebook.com/v21.0}")",
      "PhoneNumberId": "$(json_escape "${WHATSAPP_PHONE_NUMBER_ID:-}")",
      "AccessToken": "$(json_escape "${WHATSAPP_ACCESS_TOKEN:-}")",
      "TemplateName": "$(json_escape "${WHATSAPP_TEMPLATE_NAME:-trackhub_alert}")"
    }
  },
  "DocumentStorage": {
    "Provider": "$(json_escape "${DOCUMENT_STORAGE_PROVIDER:-LocalFileSystem}")",
    "LocalRootPath": "$(json_escape "${DOCUMENT_STORAGE_LOCAL_ROOT:-/app/documents}")",
    "RetentionDays": "$(json_escape "${DOCUMENT_RETENTION_DAYS:-1825}")",
    "S3": {
      "BucketName": "$(json_escape "${DOCUMENT_S3_BUCKET_NAME:-}")",
      "Region": "$(json_escape "${DOCUMENT_S3_REGION:-}")",
      "ServiceUrl": "$(json_escape "${DOCUMENT_S3_SERVICE_URL:-}")",
      "ForcePathStyle": "$(json_escape "${DOCUMENT_S3_FORCE_PATH_STYLE:-}")",
      "AccessKey": "$(json_escape "${DOCUMENT_S3_ACCESS_KEY:-}")",
      "SecretKey": "$(json_escape "${DOCUMENT_S3_SECRET_KEY:-}")",
      "PresignedExpiryMinutes": "$(json_escape "${DOCUMENT_S3_PRESIGNED_EXPIRY_MINUTES:-}")"
    },
    "AzureBlob": {
      "ConnectionString": "$(json_escape "${DOCUMENT_AZURE_CONNECTION_STRING:-}")",
      "ContainerName": "$(json_escape "${DOCUMENT_AZURE_CONTAINER_NAME:-}")",
      "SasExpiryMinutes": "$(json_escape "${DOCUMENT_AZURE_SAS_EXPIRY_MINUTES:-}")"
    }
  },
  "OpenIddict": {
    "LoadCertFromFile": true,
    "Path": "$(json_escape "${CERTIFICATE_PATH}")",
    "Password": "$(json_escape "${CERTIFICATE_PASSWORD}")",
    "Thumbprint": "$(json_escape "${CERTIFICATE_THUMBPRINT}")"
  },
  "AllowedHosts": "*",
  "AllowedCorsOrigins": "$(json_escape "${ALLOWED_CORS_ORIGINS}")"
}
EOF
}

# Generate appsettings for Router API
generate_router() {
    cat << EOF
{
  "ConnectionStrings": {
    "Logging": "$(json_escape "${DB_CONNECTION_LOGGING}")"
  },
$(serilog_section),
  "AuthorityServer": {
    "Authority": "$(json_escape "${AUTHORITY_URL}")",
    "ValidateAudience": true,
    "ValidAudience": "$(json_escape "${VALID_AUDIENCE}")",
    "ValidateIssuer": true,
    "ValidateIssuerSigningKey": true,
    "ClientId": "$(json_escape "${ROUTER_CLIENT_ID}")",
    "ClientSecret": "$(json_escape "${ROUTER_CLIENT_SECRET}")",
    "Scope": "service_scope"
  },
  "AppSettings": {
    "GraphQLIdentityService": "$(json_escape "${GRAPHQL_IDENTITY_SERVICE}")",
    "GraphQLManagerService": "$(json_escape "${GRAPHQL_MANAGER_SERVICE}")",
    "GraphQLTelemetryService": "$(json_escape "${GRAPHQL_TELEMETRY_SERVICE}")",
    "GraphQLGeofenceService": "$(json_escape "${GRAPHQL_GEOFENCE_SERVICE}")",
    "GraphQLTripManagementService": "$(json_escape "${GRAPHQL_TRIP_SERVICE}")",
    "EncryptionKey": "$(json_escape "${ENCRYPTION_KEY}")",
    "Protocols": [
      "CommandTrack",
      "Flespi",
      "GeoTab",
      "GpsGate",
      "Navixy",
      "Protrack",
      "Samsara",
      "Traccar",
      "Wialon"
    ]
  },
  "OpenIddict": {
    "LoadCertFromFile": true,
    "Path": "$(json_escape "${CERTIFICATE_PATH}")",
    "Password": "$(json_escape "${CERTIFICATE_PASSWORD}")",
    "Thumbprint": "$(json_escape "${CERTIFICATE_THUMBPRINT}")"
  },
  "AllowedHosts": "*",
  "AllowedCorsOrigins": "$(json_escape "${ALLOWED_CORS_ORIGINS}")"
}
EOF
}

# Generate appsettings for Geofencing API
generate_geofencing() {
    cat << EOF
{
  "ConnectionStrings": {
    "DefaultConnection": "$(json_escape "${DB_CONNECTION_MANAGER}")",
    "Logging": "$(json_escape "${DB_CONNECTION_LOGGING}")"
  },
$(serilog_section),
  "AuthorityServer": {
    "Authority": "$(json_escape "${AUTHORITY_URL}")",
    "ValidateAudience": true,
    "ValidAudience": "$(json_escape "${VALID_AUDIENCE}")",
    "ValidateIssuer": true,
    "ValidateIssuerSigningKey": true,
    "ClientId": "$(json_escape "${GEOFENCE_CLIENT_ID}")",
    "ClientSecret": "$(json_escape "${GEOFENCE_CLIENT_SECRET}")",
    "Scope": "service_scope"
  },
  "AppSettings": {
    "GraphQLIdentityService": "$(json_escape "${GRAPHQL_IDENTITY_SERVICE}")",
    "GraphQLManagerService": "$(json_escape "${GRAPHQL_MANAGER_SERVICE}")"
  },
  "OpenIddict": {
    "LoadCertFromFile": true,
    "Path": "$(json_escape "${CERTIFICATE_PATH}")",
    "Password": "$(json_escape "${CERTIFICATE_PASSWORD}")",
    "Thumbprint": "$(json_escape "${CERTIFICATE_THUMBPRINT}")"
  },
  "AllowedHosts": "*",
  "AllowedCorsOrigins": "$(json_escape "${ALLOWED_CORS_ORIGINS}")"
}
EOF
}

# Generate appsettings for Trip Management API
# The `trip` schema lives in the same TrackHub database as Manager/Geofencing
# (it needs the same postgis extension), hence DB_CONNECTION_MANAGER.
# AppSettings:Routing is the OpenRouteService config: an empty ApiKey degrades route
# planning to RoutePlan.Failed + ROUTING_NOT_CONFIGURED (trips stay usable, no route
# is produced). Point ORS_BASE_URL at a self-hosted instance to avoid the public quota.
# InteractiveTimeoutSeconds / BackgroundRequestsPerWindow / BackgroundWindowSeconds bound the
# shared request budget: an interactive caller waits at most that long for a slot, while
# background ETA refreshes are additionally capped per rolling window.
generate_tripmanagement() {
    cat << EOF
{
  "ConnectionStrings": {
    "DefaultConnection": "$(json_escape "${DB_CONNECTION_MANAGER}")",
    "Logging": "$(json_escape "${DB_CONNECTION_LOGGING}")"
  },
$(serilog_section),
  "AuthorityServer": {
    "Authority": "$(json_escape "${AUTHORITY_URL}")",
    "ValidateAudience": true,
    "ValidAudience": "$(json_escape "${VALID_AUDIENCE}")",
    "ValidateIssuer": true,
    "ValidateIssuerSigningKey": true,
    "ClientId": "$(json_escape "${TRIP_CLIENT_ID}")",
    "ClientSecret": "$(json_escape "${TRIP_CLIENT_SECRET}")",
    "Scope": "service_scope"
  },
  "AppSettings": {
    "GraphQLIdentityService": "$(json_escape "${GRAPHQL_IDENTITY_SERVICE}")",
    "GraphQLManagerService": "$(json_escape "${GRAPHQL_MANAGER_SERVICE}")",
    "GraphQLTelemetryService": "$(json_escape "${GRAPHQL_TELEMETRY_SERVICE}")",
    "Routing": {
      "Provider": "$(json_escape "${ROUTING_PROVIDER:-OpenRouteService}")",
      "BaseUrl": "$(json_escape "${ORS_BASE_URL:-https://api.openrouteservice.org}")",
      "ApiKey": "$(json_escape "${ORS_API_KEY}")",
      "Profile": "$(json_escape "${ORS_PROFILE:-driving-hgv}")",
      "RequestsPerSecond": ${ORS_REQUESTS_PER_SECOND:-2},
      "TimeoutSeconds": ${ORS_TIMEOUT_SECONDS:-30},
      "MaxWaypoints": ${ORS_MAX_WAYPOINTS:-50},
      "InteractiveTimeoutSeconds": ${ORS_INTERACTIVE_TIMEOUT_SECONDS:-15},
      "BackgroundRequestsPerWindow": ${ORS_BACKGROUND_REQUESTS_PER_WINDOW:-250},
      "BackgroundWindowSeconds": ${ORS_BACKGROUND_WINDOW_SECONDS:-300}
    }
  },
  "OpenIddict": {
    "LoadCertFromFile": true,
    "Path": "$(json_escape "${CERTIFICATE_PATH}")",
    "Password": "$(json_escape "${CERTIFICATE_PASSWORD}")",
    "Thumbprint": "$(json_escape "${CERTIFICATE_THUMBPRINT}")"
  },
  "AllowedHosts": "*",
  "AllowedCorsOrigins": "$(json_escape "${ALLOWED_CORS_ORIGINS}")"
}
EOF
}

# Generate appsettings for Reporting API
# AppSettings:Reporting binds to ReportingLimitsOptions, whose int properties reject an empty
# string, so the limits are always written with their defaults (100000 / 500 / 100).
generate_reporting() {
    cat << EOF
{
  "ConnectionStrings": {
    "Logging": "$(json_escape "${DB_CONNECTION_LOGGING}")"
  },
$(serilog_section),
  "AuthorityServer": {
    "Authority": "$(json_escape "${AUTHORITY_URL}")",
    "ValidateAudience": true,
    "ValidAudience": "$(json_escape "${VALID_AUDIENCE}")",
    "ValidateIssuer": true,
    "ValidateIssuerSigningKey": true,
    "ClientId": "$(json_escape "${REPORTING_CLIENT_ID}")",
    "ClientSecret": "$(json_escape "${REPORTING_CLIENT_SECRET}")",
    "Scope": "service_scope"
  },
  "AppSettings": {
    "GraphQLIdentityService": "$(json_escape "${GRAPHQL_IDENTITY_SERVICE}")",
    "GraphQLRouterService": "$(json_escape "${GRAPHQL_ROUTER_SERVICE}")",
    "GraphQLGeofenceService": "$(json_escape "${GRAPHQL_GEOFENCE_SERVICE}")",
    "GraphQLManagerService": "$(json_escape "${GRAPHQL_MANAGER_SERVICE}")",
    "GraphQLTelemetryService": "$(json_escape "${GRAPHQL_TELEMETRY_SERVICE}")",
    "GraphQLTripManagementService": "$(json_escape "${GRAPHQL_TRIP_SERVICE}")",
    "Reporting": {
      "MaxExportRows": ${REPORTING_MAX_EXPORT_ROWS:-100000},
      "MaxPdfRows": ${REPORTING_MAX_PDF_ROWS:-500},
      "PreviewRows": ${REPORTING_PREVIEW_ROWS:-100}
    }
  },
  "OpenIddict": {
    "LoadCertFromFile": true,
    "Path": "$(json_escape "${CERTIFICATE_PATH}")",
    "Password": "$(json_escape "${CERTIFICATE_PASSWORD}")",
    "Thumbprint": "$(json_escape "${CERTIFICATE_THUMBPRINT}")"
  },
  "AllowedHosts": "*",
  "AllowedCorsOrigins": "$(json_escape "${ALLOWED_CORS_ORIGINS}")"
}
EOF
}

# Generate appsettings for Telemetry API
generate_telemetry() {
    cat << EOF
{
  "ConnectionStrings": {
    "DefaultConnection": "$(json_escape "${DB_CONNECTION_TELEMETRY}")",
    "Logging": "$(json_escape "${DB_CONNECTION_LOGGING}")"
  },
$(serilog_section),
  "AuthorityServer": {
    "Authority": "$(json_escape "${AUTHORITY_URL}")",
    "ValidateAudience": true,
    "ValidAudience": "$(json_escape "${VALID_AUDIENCE}")",
    "ValidateIssuer": true,
    "ValidateIssuerSigningKey": true
  },
  "AppSettings": {
    "GraphQLIdentityService": "$(json_escape "${GRAPHQL_IDENTITY_SERVICE}")"
  },
  "OpenIddict": {
    "LoadCertFromFile": true,
    "Path": "$(json_escape "${CERTIFICATE_PATH}")",
    "Password": "$(json_escape "${CERTIFICATE_PASSWORD}")",
    "Thumbprint": "$(json_escape "${CERTIFICATE_THUMBPRINT}")"
  },
  "AllowedHosts": "*",
  "AllowedCorsOrigins": "$(json_escape "${ALLOWED_CORS_ORIGINS}")"
}
EOF
}

# Generate appsettings for the SyncWorker background service
#
# Emitted from this script only (like every other service above) instead of rendering
# config/appsettings.template.json: the template's "common" section carries web-API keys
# (AllowedHosts, AllowedCorsOrigins, AuthorityServer.Validate*/ValidAudience) plus a
# "_comment" documentation key, none of which belong in a deployed worker config. The
# SyncWorker has no inbound API - this matches TrackHubRouter/src/SyncWorker/appsettings.json.
generate_syncworker() {
    cat << EOF
{
  "ConnectionStrings": {
    "Logging": "$(json_escape "${DB_CONNECTION_LOGGING}")"
  },
$(serilog_section),
  "AuthorityServer": {
    "Authority": "$(json_escape "${AUTHORITY_URL}")",
    "ClientId": "$(json_escape "${SYNCWORKER_CLIENT_ID}")",
    "ClientSecret": "$(json_escape "${SYNCWORKER_CLIENT_SECRET}")",
    "IsService": true,
    "Scope": "service_scope"
  },
  "AppSettings": {
    "GraphQLIdentityService": "$(json_escape "${GRAPHQL_IDENTITY_SERVICE}")",
    "GraphQLManagerService": "$(json_escape "${GRAPHQL_MANAGER_SERVICE}")",
    "GraphQLTelemetryService": "$(json_escape "${GRAPHQL_TELEMETRY_SERVICE}")",
    "GraphQLGeofenceService": "$(json_escape "${GRAPHQL_GEOFENCE_SERVICE}")",
    "GraphQLTripManagementService": "$(json_escape "${GRAPHQL_TRIP_SERVICE}")",
    "EncryptionKey": "$(json_escape "${ENCRYPTION_KEY}")",
    "Protocols": [
      "CommandTrack",
      "Flespi",
      "GeoTab",
      "GpsGate",
      "Navixy",
      "Protrack",
      "Samsara",
      "Traccar",
      "Wialon"
    ]
  },
  "OpenIddict": {
    "LoadCertFromFile": true,
    "Path": "$(json_escape "${CERTIFICATE_PATH}")",
    "Password": "$(json_escape "${CERTIFICATE_PASSWORD}")",
    "Thumbprint": "$(json_escape "${CERTIFICATE_THUMBPRINT}")"
  }
}
EOF
}

# Generate and output/save appsettings for a service
process_service() {
    local service=$1
    local content=""
    
    case $service in
        authority)  content=$(generate_authority) ;;
        security)   content=$(generate_security) ;;
        manager)    content=$(generate_manager) ;;
        router)     content=$(generate_router) ;;
        geofencing) content=$(generate_geofencing) ;;
        tripmanagement) content=$(generate_tripmanagement) ;;
        telemetry)  content=$(generate_telemetry) ;;
        reporting)  content=$(generate_reporting) ;;
        syncworker) content=$(generate_syncworker) ;;
        *)
            print_error "Unknown service: $service"
            return 1
            ;;
    esac
    
    if [ -n "$OUTPUT_DIR" ]; then
        # These files carry every client secret, the certificate password and the database
        # credentials, and the deployment host is shared: they are owner-only from creation.
        mkdir -p "$OUTPUT_DIR"
        chmod 700 "$OUTPUT_DIR"
        echo "$content" > "$OUTPUT_DIR/appsettings.$service.json.tmp"
        chmod 600 "$OUTPUT_DIR/appsettings.$service.json.tmp"
        validate_json "$OUTPUT_DIR/appsettings.$service.json.tmp"
        mv -f "$OUTPUT_DIR/appsettings.$service.json.tmp" "$OUTPUT_DIR/appsettings.$service.json"
        print_success "Generated: $OUTPUT_DIR/appsettings.$service.json"
    else
        echo "# =============================================="
        echo "# $service - appsettings.json"
        echo "# =============================================="
        echo "$content"
        echo ""
    fi
}

# Main execution
print_info "TrackHub AppSettings Generator"
echo ""

SERVICES=("authority" "security" "manager" "router" "geofencing" "tripmanagement" "telemetry" "reporting" "syncworker")

# Optional extension point: a sibling generate-appsettings.edition.sh can append
# services and define their templates for a specific deployment. No such file ships
# with this repository.
if [ -f "$SCRIPT_DIR/generate-appsettings.edition.sh" ]; then
    source "$SCRIPT_DIR/generate-appsettings.edition.sh"
fi

if [ -n "$SERVICE_FILTER" ]; then
    process_service "$SERVICE_FILTER"
else
    for service in "${SERVICES[@]}"; do
        process_service "$service"
    done
fi

if [ -n "$OUTPUT_DIR" ]; then
    echo ""
    print_success "AppSettings generation complete!"
fi
