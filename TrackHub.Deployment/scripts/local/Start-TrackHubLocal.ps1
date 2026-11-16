<#
.SYNOPSIS
    Start / stop / inspect the TrackHub backend on local Docker Desktop.

.DESCRIPTION
    Thin wrapper over the three-file compose stack so the long command line stays in
    one place:

      docker compose --env-file .env.local
        -f docker-compose.backend.yml
        -f docker-compose.local.yml <action>

    docker-compose.backend.yml is the production APIs-without-frontend split, and it
    is used unmodified. All local-only behaviour lives in docker-compose.local.yml.

.PARAMETER Action
    up      Build if needed and start (default).
    down    Stop and remove the containers.
    restart Restart the running containers.
    build   Rebuild images without starting.
    logs    Follow logs (all services, or just -Service).
    ps      Show status and the published ports.
    config  Print the fully merged compose file (for debugging the overlay itself).

.PARAMETER Service
    Limit the action to one compose service (manager, router, telemetry, ...).

.PARAMETER Workers
    Also start the syncworker, which is off by default.

.EXAMPLE
    .\Start-TrackHubLocal.ps1
    .\Start-TrackHubLocal.ps1 logs -Service manager
    .\Start-TrackHubLocal.ps1 down
#>
[CmdletBinding()]
param(
    [ValidateSet('up','down','restart','build','logs','ps','config')]
    [string] $Action = 'up',
    [string] $Service,
    [switch] $Workers
)

$ErrorActionPreference = 'Stop'

$ProjectDir = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$EnvFile    = Join-Path $ProjectDir '.env.local'

if (-not (Test-Path $EnvFile)) {
    throw ".env.local not found. Run Setup-TrackHubLocal.ps1 first."
}

# A missing bind-mount source makes Docker silently create a DIRECTORY at the
# container path, which surfaces much later as an unreadable appsettings.json.
foreach ($required in @('local\ca-bundle.crt', 'local\clients.json', 'certificates\trackhub-local.crt')) {
    if (-not (Test-Path (Join-Path $ProjectDir $required))) {
        throw "$required is missing. Run Setup-TrackHubLocal.ps1 first."
    }
}
if (-not (Get-ChildItem (Join-Path $ProjectDir 'local\generated') -Filter 'appsettings.*.json' -ErrorAction SilentlyContinue)) {
    throw "local/generated is empty. Run Setup-TrackHubLocal.ps1 first."
}

$composeArgs = @(
    'compose'
    '--env-file', '.env.local'
    '-f', 'docker-compose.backend.yml'
    '-f', 'docker-compose.local.yml'
)
if ($Workers) { $composeArgs += @('--profile', 'workers') }

Push-Location $ProjectDir
try {
    switch ($Action) {
        'up' {
            $a = $composeArgs + @('up', '-d')
            if ($Service) { $a += $Service }
            docker @a
            if ($LASTEXITCODE -ne 0) { throw "docker compose up failed." }

            Write-Host ""
            Write-Host "TrackHub local stack is up." -ForegroundColor Green
            Write-Host "  API gateway   https://trackhub.local:8443/" -ForegroundColor White
            Write-Host "  Portal        https://localhost:3000/        (npm start in TrackHub.Portal)" -ForegroundColor White
            Write-Host ""
            Write-Host "Direct host ports, for a service you are debugging in Visual Studio:" -ForegroundColor Cyan
            Write-Host "  authority 6001   security 6002   manager   6003   router    6004" -ForegroundColor DarkGray
            Write-Host "  geofence  6005   trip     6006   telemetry 6007   reporting 6008" -ForegroundColor DarkGray
        }
        'down'    { docker @($composeArgs + @('down', '--remove-orphans')) }
        'restart' { $a = $composeArgs + @('restart'); if ($Service) { $a += $Service }; docker @a }
        'build'   { $a = $composeArgs + @('build');   if ($Service) { $a += $Service }; docker @a }
        'logs'    { $a = $composeArgs + @('logs', '-f', '--tail', '200'); if ($Service) { $a += $Service }; docker @a }
        'ps'      { docker @($composeArgs + @('ps')) }
        'config'  { docker @($composeArgs + @('config')) }
    }
} finally { Pop-Location }
