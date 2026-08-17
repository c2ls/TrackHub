#!/bin/bash
# =============================================================================
# Source checkout configuration
# =============================================================================
# The whole product lives in ONE repository. This file used to list the sibling
# repositories to clone; it now describes that single repository plus the
# top-level directories the image builds expect to find inside it.
#
# Layout contract — deliberately UNCHANGED from the polyrepo layout. This is why
# the compose files and every Dockerfile needed no edits: the build context
# (`context: ..`) is still a directory containing TrackHub/, TrackHubCommon/,
# TrackHub.Manager/ and friends as siblings of TrackHub.Deployment/.
#
#   <workspace>/                 <- monorepo checkout == docker build context
#     TrackHub/                  <- portal
#     TrackHubCommon/            <- shared packages
#     TrackHub.Manager/          <- service
#     TrackHub.Deployment/       <- this directory; compose runs from here
#     ...
#
# Usage:  source "$SCRIPT_DIR/repo-config.sh"
#         monorepo_url                  -> clone URL, credentials included
#         monorepo_url_clean            -> same URL without credentials
#         monorepo_clone_or_update DIR  -> clone into DIR if missing, else fast-forward
#         missing_source_dirs [DIR]     -> names any required directory that is absent
#         "${TRACKHUB_REPOS[@]}"        -> source directories the image builds require
# =============================================================================

# Top-level directories that must be present for the images to build. These keep
# the old variable name (and the old values) because they are still exactly the
# paths the Dockerfiles COPY from — only their provenance changed.
TRACKHUB_REPOS=(
    "TrackHub.Portal"
    "TrackHub.AuthorityServer"
    "TrackHubSecurity"
    "TrackHub.Manager"
    "TrackHubRouter"
    "TrackHub.Geofencing"
    "TrackHub.Telemetry"
    "TrackHub.Reporting"
    "TrackHubCommon"
)

_repo_config_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TRACKHUB_PROJECT_DIR="$(dirname "$_repo_config_dir")"
TRACKHUB_WORKSPACE_DIR="$(dirname "$TRACKHUB_PROJECT_DIR")"

# Load GITHUB_* settings from .env when present.
_repo_config_load() {
    local env_file="$1"
    [ -f "$env_file" ] || return 0
    local key
    for key in GITHUB_OWNER GITHUB_REPO GITHUB_BRANCH GITHUB_USER GITHUB_PASSWORD; do
        # Only take the value from .env if it is not already set in the environment.
        if [ -z "${!key}" ]; then
            local value
            value="$(grep -E "^${key}=" "$env_file" | tail -1 | cut -d= -f2- | sed 's/^"//; s/"$//')"
            [ -n "$value" ] && export "$key=$value"
        fi
    done
}

_repo_config_load "${PROJECT_DIR:-$TRACKHUB_PROJECT_DIR}/.env"

# Optional extension point: a sibling repo-config.edition.sh may point this at a
# different repository and add directories to TRACKHUB_REPOS. It is sourced BEFORE
# the defaults below so it can set them, while anything already supplied via the
# environment or .env still wins. No such file ships with this repository.
if [ -f "$_repo_config_dir/repo-config.edition.sh" ]; then
    source "$_repo_config_dir/repo-config.edition.sh"
fi

# Defaults keep the public deployment working with no configuration at all.
: "${GITHUB_OWNER:=shernandezp}"
: "${GITHUB_REPO:=TrackHub}"
: "${GITHUB_BRANCH:=main}"

# Clone URL without credentials — safe to store as a git remote.
monorepo_url_clean() {
    echo "https://github.com/${GITHUB_OWNER}/${GITHUB_REPO}.git"
}

# Clone URL with credentials when they are configured. Only ever passed to git
# on the command line, never written into .git/config.
monorepo_url() {
    if [ -n "$GITHUB_USER" ] && [ -n "$GITHUB_PASSWORD" ]; then
        echo "https://${GITHUB_USER}:${GITHUB_PASSWORD}@github.com/${GITHUB_OWNER}/${GITHUB_REPO}.git"
    else
        monorepo_url_clean
    fi
}

# Clone into $1 if missing, otherwise fast-forward. Credentials stay out of the remote.
monorepo_clone_or_update() {
    local target="$1"
    local branch="$GITHUB_BRANCH"

    # An existing .env may still name a branch from the polyrepo layout, where most
    # repositories defaulted to master and only two to main. The monorepo publishes
    # main and develop, so fall back instead of failing on a stale setting.
    if ! git ls-remote --exit-code --heads "$(monorepo_url)" "$branch" >/dev/null 2>&1; then
        branch=main
    fi

    if [ -d "$target/.git" ]; then
        # Refuse to pull one branch into a checkout sitting on another. With a single
        # repository that would fast-forward the deployed branch onto an unrelated
        # one; under the old layout each repo had its own clone so it could not happen.
        local current
        current="$(git -C "$target" rev-parse --abbrev-ref HEAD 2>/dev/null)"
        if [ "$current" != "$branch" ]; then
            echo "checkout is on '$current' but GITHUB_BRANCH resolves to '$branch'" >&2
            echo "switch it first:  git -C \"$target\" checkout $branch" >&2
            return 1
        fi
        git -C "$target" pull --ff-only "$(monorepo_url)" "$branch"
    else
        git clone --branch "$branch" "$(monorepo_url)" "$target"
        git -C "$target" remote set-url origin "$(monorepo_url_clean)"
    fi
}

# Print the name of every required source directory that is absent from the
# checkout root ($1, default: the directory holding TrackHub.Deployment).
# Silence means the checkout is complete.
missing_source_dirs() {
    local root="${1:-$TRACKHUB_WORKSPACE_DIR}" d
    for d in "${TRACKHUB_REPOS[@]}"; do
        [ -d "$root/$d" ] || echo "$d"
    done
}
