#!/bin/bash
# =============================================================================
# TrackHub Source Checkout
# =============================================================================
# The whole product is one repository, and these scripts live inside it — so by
# the time you can run this, the source is already checked out. This script now
# fast-forwards that checkout (and still clones it outright in the unusual case
# of being run from outside one), then verifies every directory the image builds
# need is present.
# Usage: ./clone-repos.sh
# Configure GITHUB_OWNER / GITHUB_REPO / GITHUB_BRANCH and, for a private
# repository, GITHUB_USER / GITHUB_PASSWORD in .env
# =============================================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
WORKSPACE_DIR="$(dirname "$PROJECT_DIR")"

RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m'

print_success() { echo -e "${GREEN}✓ $1${NC}"; }
print_error()   { echo -e "${RED}✗ $1${NC}"; }
print_info()    { echo -e "${BLUE}ℹ $1${NC}"; }

source "$SCRIPT_DIR/repo-config.sh"

print_info "Source: $(monorepo_url_clean) (branch ${GITHUB_BRANCH})"
print_info "Target: $WORKSPACE_DIR"
if [ -n "$GITHUB_USER" ]; then
    print_info "Authenticating as ${GITHUB_USER}"
fi
echo

if [ -d "$WORKSPACE_DIR/.git" ]; then
    printf "%-26s updating... " "$GITHUB_REPO"
else
    printf "%-26s cloning...  " "$GITHUB_REPO"
fi

if monorepo_clone_or_update "$WORKSPACE_DIR" >/dev/null 2>&1; then
    echo "ok"
else
    echo "FAILED"
    print_error "Could not update $(monorepo_url_clean)"
    print_info "For a private repository set GITHUB_USER and GITHUB_PASSWORD in .env."
    print_info "GITHUB_PASSWORD must be a Personal Access Token, not your account password."
    exit 1
fi

echo
# A partial checkout fails much later and far less clearly, deep inside
# "docker compose build", as:
#   failed to compute cache key: "/TrackHub.<Service>/src": not found
missing="$(missing_source_dirs "$WORKSPACE_DIR")"
if [ -n "$missing" ]; then
    print_error "Checkout is incomplete — these source directories are missing:"
    echo "$missing" | sed 's/^/    /'
    print_info "Expected them under $WORKSPACE_DIR (the docker build context)."
    exit 1
fi

print_success "Source ready in $WORKSPACE_DIR"
