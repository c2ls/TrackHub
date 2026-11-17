<#
.SYNOPSIS
    Point one TrackHub service at the Visual Studio debugger on the host, while the
    rest of the stack keeps running in Docker.

.DESCRIPTION
    Rewrites nginx/local.d/upstreams/<service>.conf so the reverse proxy sends that
    path to host.docker.internal:<port> instead of the container, stops the now
    redundant container, and reloads nginx.

    The result: the portal at https://localhost:3000 keeps calling exactly the same
    URLs, but requests for the one service under test land on your breakpoint.

        Browser -> https://trackhub.local:8443/Router/graphql
                -> nginx -> host.docker.internal:5003 -> Visual Studio (breakpoint)

    The other half - your host-run service calling its Docker peers - is covered by
    the published host ports. Use -ShowConfig to print the settings that service needs.

    Reverse it with -Reset, which restores the container upstream and starts it again.

.PARAMETER Service
    Compose service name: authority, security, manager, router, geofencing,
    tripmanagement, telemetry, reporting.

.PARAMETER Port
    The HTTP port your service listens on when debugging. Defaults to the http URL in
    that project's launchSettings.json. Use the HTTP port, not the HTTPS one - nginx
    talks to it over plain HTTP, which avoids a second certificate to trust.

.PARAMETER Reset
    Restore the service to its Docker container.

.PARAMETER ShowConfig
    Print the configuration the host-run service needs to reach its Docker peers.

.EXAMPLE
    .\Set-DebugTarget.ps1 -Service router -ShowConfig
    .\Set-DebugTarget.ps1 -Service router -Reset
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('authority','security','manager','router','geofencing','tripmanagement','telemetry','reporting')]
    [string] $Service,

    [int]    $Port,
    [switch] $Reset,
    [switch] $ShowConfig
)

$ErrorActionPreference = 'Stop'

$ProjectDir = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$UpstreamFile = Join-Path $ProjectDir "nginx\local.d\upstreams\$Service.conf"

# upstream name in nginx.local.conf, published host port, default VS http port
$Map = @{
    authority     = @{ Upstream = 'authority_server';  HostPort = 6001; DebugPort = 5291 }
    security      = @{ Upstream = 'security_api';      HostPort = 6002; DebugPort = 5000 }
    manager       = @{ Upstream = 'manager_api';       HostPort = 6003; DebugPort = 5005 }
    router        = @{ Upstream = 'router_api';        HostPort = 6004; DebugPort = 5003 }
    geofencing    = @{ Upstream = 'geofencing_api';    HostPort = 6005; DebugPort = 5011 }
    tripmanagement= @{ Upstream = 'trip_api';          HostPort = 6006; DebugPort = 5007 }
    telemetry     = @{ Upstream = 'telemetry_api';     HostPort = 6007; DebugPort = 5009 }
    reporting     = @{ Upstream = 'reporting_api';     HostPort = 6008; DebugPort = 5013 }
}
$info     = $Map[$Service]
$upstream = $info.Upstream

$composeArgs = @(
    'compose', '--env-file', '.env.local'
    '-f', 'docker-compose.backend.yml'
    '-f', 'docker-compose.local.yml'
)

function Invoke-NginxReload {
    Push-Location $ProjectDir
    try {
        # Validate before reloading: a bad include would otherwise take the proxy down.
        docker @($composeArgs + @('exec', '-T', 'nginx-local', 'nginx', '-t')) 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            docker @($composeArgs + @('exec', '-T', 'nginx-local', 'nginx', '-t'))
            throw "nginx rejected the configuration - upstream file left in place for inspection."
        }
        docker @($composeArgs + @('exec', '-T', 'nginx-local', 'nginx', '-s', 'reload')) | Out-Null
    } finally { Pop-Location }
}

# =============================================================================
# Reset
# =============================================================================
if ($Reset) {
    @"
# TrackHub local upstream: $upstream
# DEFAULT: routed to the Docker container "$Service".
# Set-DebugTarget.ps1 rewrites this file to point at a service running in Visual Studio.
upstream $upstream {
    zone $Service 64k;
    server ${Service}:8080 resolve;
}
"@ | Set-Content -Path $UpstreamFile -Encoding ascii

    Push-Location $ProjectDir
    try { docker @($composeArgs + @('up', '-d', $Service)) | Out-Null } finally { Pop-Location }
    Invoke-NginxReload

    Write-Host "$Service restored to its Docker container." -ForegroundColor Green
    return
}

# =============================================================================
# Redirect to the host
# =============================================================================
if (-not $Port) { $Port = $info.DebugPort }

@"
# TrackHub local upstream: $upstream
# DEBUG OVERRIDE: routed to $Service running on the HOST (Visual Studio), port $Port.
# Restore with:  Set-DebugTarget.ps1 -Service $Service -Reset
upstream $upstream {
    server host.docker.internal:$Port;
}
"@ | Set-Content -Path $UpstreamFile -Encoding ascii

Push-Location $ProjectDir
try {
    # Stop the container so there is exactly one instance answering.
    docker @($composeArgs + @('stop', $Service)) 2>&1 | Out-Null
} finally { Pop-Location }

Invoke-NginxReload

Write-Host ""
Write-Host "$Service is now served from the HOST on port $Port." -ForegroundColor Green
Write-Host "  https://trackhub.local:8443/... -> host.docker.internal:$Port" -ForegroundColor DarkGray
Write-Host "  The '$Service' container has been stopped." -ForegroundColor DarkGray
Write-Host ""
Write-Host "Start the project in Visual Studio on http://localhost:$Port and set your breakpoints." -ForegroundColor White
Write-Host "Restore with: Set-DebugTarget.ps1 -Service $Service -Reset" -ForegroundColor DarkGray

if ($ShowConfig) {
    Write-Host ""
    Write-Host "=== Settings for the host-run $Service ===" -ForegroundColor Cyan
    Write-Host "Its Docker peers are reachable on published host ports:" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host '  "ConnectionStrings": {'
    Write-Host '    "DefaultConnection": "server=localhost;user id=postgres;password=super;database=TrackHub;port=5432",'
    Write-Host '    "Logging":           "server=localhost;user id=postgres;password=super;database=TrackHub;port=5432"'
    Write-Host '  },'
    Write-Host '  "AuthorityServer": {'
    Write-Host '    "Authority": "https://trackhub.local:8443/Identity"'
    Write-Host '  },'
    Write-Host '  "AppSettings": {'
    Write-Host '    "GraphQLIdentityService":       "http://localhost:6002/graphql/",'
    Write-Host '    "GraphQLSecurityService":       "http://localhost:6002/graphql/",'
    Write-Host '    "GraphQLManagerService":        "http://localhost:6003/graphql/",'
    Write-Host '    "GraphQLRouterService":         "http://localhost:6004/graphql/",'
    Write-Host '    "GraphQLGeofenceService":       "http://localhost:6005/graphql/",'
    Write-Host '    "GraphQLTripManagementService": "http://localhost:6006/graphql/",'
    Write-Host '    "GraphQLTelemetryService":      "http://localhost:6007/graphql/",'
    Write-Host '  }'
    Write-Host ""
    Write-Host "Keep only the keys that service actually uses." -ForegroundColor DarkGray
    Write-Host "Put them in a NEW launchSettings.json profile (additive, nothing existing changes)," -ForegroundColor DarkGray
    Write-Host "or in dotnet user-secrets, rather than editing the committed appsettings.json." -ForegroundColor DarkGray
}
