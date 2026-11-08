<#
.SYNOPSIS
    One-time setup for running the TrackHub backend on local Docker Desktop (Windows).

.DESCRIPTION
    Prepares everything docker-compose.local.yml needs. Safe to re-run: every step is
    idempotent and skips work that is already done.

    Touches NOTHING production uses. IIS keeps ports 80 and 443 throughout - the
    TrackHub proxy takes 8080/8443.

    Steps that do NOT need elevation (the default):
      1. Verify Docker Desktop is running.
      2. Check .env.local is present. It is committed to the repo with working local
         values - there is no template to fill in and no secrets to chase down.
      3. Generate the trackhub.local TLS certificate (via a throwaway openssl
         container - no openssl needed on the host). Its SAN covers trackhub.local,
         localhost and 127.0.0.1, so ONE certificate serves both nginx on :8443 and
         the Vite dev server on :3000, and one trust import covers both.
      3b. Install that same certificate into TrackHub.Portal as cert.crt/cert.key.
      4. Copy the OpenIddict signing certificate.pfx from C:\Certificates,
         so tokens minted in Docker validate the same way everywhere.
      5. Build local/ca-bundle.crt = the stock Debian CA bundle + the trackhub.local
         cert, mounted into every container as SSL_CERT_FILE. That is what lets a
         container trust https://trackhub.local:8443 without editing any Dockerfile,
         while still trusting the real public CAs for outbound calls.
      6. Write local/clients.json (needed only as a mount target - db-init is disabled).
      7. Generate local/generated/appsettings.*.json with the production generator.

    Steps that DO need elevation (-AdminTasks):
      8. Add "127.0.0.1 trackhub.local" to the hosts file.
      9. Import the trackhub.local certificate into LocalMachine\Root so the browser
         and any host-run .NET service trust it.

.PARAMETER AdminTasks
    Run ONLY the two elevated steps. Launch an elevated PowerShell and re-run with
    this switch; the script prints the exact command when the steps are outstanding.

.PARAMETER PfxSource
    Path to the existing OpenIddict signing certificate. Defaults to
    C:\Certificates\certificate.pfx.

.PARAMETER CertFrom
    Reuse an existing trackhub.local certificate instead of generating a new one: the path to
    a TrackHub.Deployment directory that already has certificates/trackhub-local.crt + .key.
    Useful when one is already imported into LocalMachine\Root - from an earlier install, a
    backup, or another clone on this machine - since keeping it means step 9 of -AdminTasks
    has nothing left to import and the browser needs no new trust prompt.

.PARAMETER Force
    Regenerate the TLS certificate and CA bundle even if they already exist.

.EXAMPLE
    .\Setup-TrackHubLocal.ps1
    .\Setup-TrackHubLocal.ps1 -AdminTasks     # from an elevated shell
#>
[CmdletBinding()]
param(
    [switch] $AdminTasks,
    [string] $PfxSource = 'C:\Certificates\certificate.pfx',
    [string] $CertFrom,
    [switch] $Force
)

$ErrorActionPreference = 'Stop'

$HostName    = 'trackhub.local'
$ProjectDir  = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$CertDir     = Join-Path $ProjectDir 'certificates'
$LocalDir    = Join-Path $ProjectDir 'local'
$CrtPath     = Join-Path $CertDir 'trackhub-local.crt'
$KeyPath     = Join-Path $CertDir 'trackhub-local.key'
$BundlePath  = Join-Path $LocalDir 'ca-bundle.crt'
$EnvPath     = Join-Path $ProjectDir '.env.local'
$HostsPath   = "$env:SystemRoot\System32\staff\etc\hosts"

# Docker wants forward slashes in -v arguments.
$ProjectDirU = $ProjectDir -replace '\\', '/'

function Write-Step  { param($m) Write-Host "==> $m" -ForegroundColor Cyan }
function Write-Ok    { param($m) Write-Host "    OK   $m" -ForegroundColor Green }
function Write-Skip  { param($m) Write-Host "    skip $m" -ForegroundColor DarkGray }
function Write-Warn2 { param($m) Write-Host "    WARN $m" -ForegroundColor Yellow }

function Test-Admin {
    $id = [Security.Principal.WindowsIdentity]::GetCurrent()
    (New-Object Security.Principal.WindowsPrincipal($id)).IsInRole(
        [Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Test-HostsEntry {
    if (-not (Test-Path $HostsPath)) { return $false }
    $pattern = '^\s*127\.0\.0\.1\s+.*\b' + [regex]::Escape($HostName) + '\b'
    (Get-Content $HostsPath) -match $pattern | Select-Object -First 1 | ForEach-Object { $true }
}

function Test-CertTrusted {
    if (-not (Test-Path $CrtPath)) { return $false }
    try {
        $c = [System.Security.Cryptography.X509Certificates.X509Certificate2]::CreateFromCertFile($CrtPath)
        $tp = $c.GetCertHashString()
    } catch { return $false }
    [bool](Get-ChildItem Cert:\LocalMachine\Root -ErrorAction SilentlyContinue |
           Where-Object Thumbprint -eq $tp)
}

# =============================================================================
# Elevated-only tasks
# =============================================================================
if ($AdminTasks) {
    if (-not (Test-Admin)) {
        throw "-AdminTasks needs an elevated PowerShell (Run as administrator)."
    }

    Write-Step "Hosts entry for $HostName"
    if (Test-HostsEntry) {
        Write-Skip "already present in $HostsPath"
    } else {
        # Explicit newline first: the hosts file often has no trailing newline, and
        # appending without one would silently merge into the previous entry.
        Add-Content -Path $HostsPath -Value "`r`n127.0.0.1`t$HostName`t# TrackHub local Docker stack"
        Write-Ok "added 127.0.0.1 $HostName"
    }

    Write-Step "Trust the $HostName certificate"
    if (-not (Test-Path $CrtPath)) {
        Write-Warn2 "$CrtPath not found - run the script without -AdminTasks first."
    } elseif (Test-CertTrusted) {
        Write-Skip "already trusted in LocalMachine\Root"
    } else {
        Import-Certificate -FilePath $CrtPath -CertStoreLocation Cert:\LocalMachine\Root | Out-Null
        Write-Ok "imported into LocalMachine\Root"
    }

    Write-Host ""
    Write-Host "Elevated setup complete." -ForegroundColor Green
    Write-Host "Restart the browser so it picks up the new root certificate." -ForegroundColor Yellow
    return
}

# =============================================================================
# 1. Docker
# =============================================================================
Write-Step "Docker Desktop"
try {
    $v = (docker version --format '{{.Server.Version}}' 2>$null)
    if (-not $v) { throw }
    Write-Ok "engine $v"
} catch {
    throw "Docker Desktop is not running. Start it and re-run this script."
}

# =============================================================================
# 2. .env.local
# =============================================================================
Write-Step ".env.local"
if (Test-Path $EnvPath) {
    Write-Ok "present (tracked in the repo - no template to fill in)"
} else {
    throw ".env.local is missing. It is committed to the repo; restore it with: git checkout -- .env.local"
}

# =============================================================================
# 3. TLS certificate for nginx
# =============================================================================
Write-Step "TLS certificate for $HostName"
New-Item -ItemType Directory -Force -Path $CertDir | Out-Null
if ((Test-Path $CrtPath) -and (Test-Path $KeyPath) -and -not $Force) {
    Write-Skip "certificates/trackhub-local.crt already exists (use -Force to regenerate)"
} elseif ($CertFrom) {
    # Reuse rather than mint. The certificate is referenced from three places at once - nginx
    # serves it, the containers trust it through ca-bundle.crt, and LocalMachine\Root trusts
    # it - so replacing a working one costs a new trust import everywhere it is already known.
    $srcCrt = Join-Path $CertFrom 'certificates\trackhub-local.crt'
    $srcKey = Join-Path $CertFrom 'certificates\trackhub-local.key'
    if (-not (Test-Path $srcCrt) -or -not (Test-Path $srcKey)) {
        throw "-CertFrom '$CertFrom' has no certificates\trackhub-local.crt (+ .key). Point it at a TrackHub.Deployment directory that has them, or drop -CertFrom to generate a new certificate."
    }
    Copy-Item $srcCrt $CrtPath -Force
    Copy-Item $srcKey $KeyPath -Force
    Write-Ok "copied the shared certificate from $CertFrom (already trusted - no -AdminTasks import needed)"
} else {
    docker run --rm -v "${ProjectDirU}/certificates:/out" alpine/openssl `
        req -x509 -nodes -newkey rsa:2048 -days 825 `
        -subj "/CN=$HostName" `
        -addext "subjectAltName=DNS:$HostName,DNS:localhost,IP:127.0.0.1" `
        -addext "basicConstraints=critical,CA:TRUE" `
        -addext "keyUsage=critical,digitalSignature,keyCertSign" `
        -addext "extendedKeyUsage=serverAuth" `
        -keyout /out/trackhub-local.key -out /out/trackhub-local.crt 2>&1 | Out-Null
    if (-not (Test-Path $CrtPath)) { throw "Certificate generation failed." }
    Write-Ok "generated certificates/trackhub-local.crt (+ .key)"
}

# =============================================================================
# 3b. Portal dev-server certificate
# =============================================================================
# The SAME certificate serves the Vite dev server on https://localhost:3000 and nginx
# on https://trackhub.local:8443 - its SAN carries trackhub.local, localhost AND
# 127.0.0.1. One certificate means one trust import covers both origins.
#
# Without cert.crt/cert.key present, Vite silently falls back to HTTP and the OAuth
# callback dies with ERR_SSL_PROTOCOL_ERROR.
Write-Step "Portal dev-server certificate"
$portalDir = Join-Path (Split-Path $ProjectDir -Parent) 'TrackHub.Portal'
$portalCrt = Join-Path $portalDir 'cert.crt'
$portalKey = Join-Path $portalDir 'cert.key'
if (-not (Test-Path $portalDir)) {
    Write-Warn2 "TrackHub.Portal not found next to TrackHub.Deployment - skipped."
} elseif ((Test-Path $portalCrt) -and -not $Force -and
          ((Get-FileHash $portalCrt).Hash -ne (Get-FileHash $CrtPath).Hash)) {
    # Never clobber a certificate somebody deliberately put there.
    Write-Warn2 "TrackHub.Portal\cert.crt exists and differs from the local certificate."
    Write-Warn2 "Leaving it alone. It must cover 'localhost' for the Vite dev server;"
    Write-Warn2 "use -Force to replace it with the shared trackhub.local certificate."
} elseif ((Test-Path $portalCrt) -and (Test-Path $portalKey) -and -not $Force -and
          ((Get-FileHash $portalCrt).Hash -eq (Get-FileHash $CrtPath).Hash)) {
    Write-Skip "portal already uses the shared certificate"
} else {
    Copy-Item $CrtPath $portalCrt -Force
    Copy-Item $KeyPath $portalKey -Force
    Write-Ok "installed into TrackHub.Portal (cert.crt / cert.key, both gitignored)"
}

# =============================================================================
# 4. OpenIddict signing certificate
# =============================================================================
Write-Step "OpenIddict signing certificate"
$pfxDest = Join-Path $CertDir 'certificate.pfx'
if (Test-Path $pfxDest) {
    Write-Skip "certificates/certificate.pfx already present"
} elseif (Test-Path $PfxSource) {
    Copy-Item $PfxSource $pfxDest
    Write-Ok "copied from $PfxSource"
} else {
    Write-Warn2 "$PfxSource not found. Services validate tokens against this certificate;"
    Write-Warn2 "copy your OpenIddict signing certificate into certificates\certificate.pfx."
}

# =============================================================================
# 5. CA bundle for the containers
# =============================================================================
Write-Step "Container CA bundle"
New-Item -ItemType Directory -Force -Path $LocalDir | Out-Null
if ((Test-Path $BundlePath) -and -not $Force) {
    Write-Skip "local/ca-bundle.crt already exists (use -Force to rebuild)"
} else {
    docker run --rm `
        -v "${ProjectDirU}/certificates:/certs:ro" `
        -v "${ProjectDirU}/local:/out" `
        mcr.microsoft.com/dotnet/aspnet:10.0 `
        sh -c "cat /etc/ssl/certs/ca-certificates.crt /certs/trackhub-local.crt > /out/ca-bundle.crt" 2>&1 | Out-Null
    if (-not (Test-Path $BundlePath)) { throw "CA bundle build failed." }
    $n = (Select-String -Path $BundlePath -Pattern 'BEGIN CERTIFICATE' -AllMatches).Count
    Write-Ok "local/ca-bundle.crt built ($n certificates)"
}

# =============================================================================
# 6. clients.json (db-init seeds from this file)
# =============================================================================
# The committed dev seed, NOT config/clients.json.example: the example carries
# <<GENERATE_A_SECURE_SECRET>> / <<YOUR_DOMAIN>> placeholders, and the seeder
# updates an existing client from whatever it is given.
Write-Step "local/clients.json"
$clientsPath = Join-Path $LocalDir 'clients.json'
$devClients = Join-Path $ProjectDir '..\TrackHub.AuthorityServer\src\ClientSeeder\clients.json'
if ((Test-Path $clientsPath) -and -not $Force) {
    Write-Skip "already exists"
} else {
    Copy-Item $devClients $clientsPath -Force
    Write-Ok "created from TrackHub.AuthorityServer/src/ClientSeeder/clients.json"
}

# =============================================================================
# 7. Generated appsettings
# =============================================================================
Write-Step "Generated appsettings"
$gitBash = 'C:\Program Files\Git\bin\bash.exe'
if (-not (Test-Path $gitBash)) {
    # NB: "bash" on PATH is usually WSL bash, which sees a different filesystem.
    $gitBash = (Get-Command 'bash.exe' -ErrorAction SilentlyContinue |
                Where-Object Source -notlike "$env:SystemRoot*" |
                Select-Object -First 1 -ExpandProperty Source)
}
if (-not $gitBash) {
    Write-Warn2 "Git Bash not found - generate the appsettings manually with:"
    Write-Warn2 "  bash scripts/generate-appsettings.sh --env-file .env.local --output-dir local/generated"
} else {
    New-Item -ItemType Directory -Force -Path (Join-Path $LocalDir 'generated') | Out-Null
    Push-Location $ProjectDir
    try {
        & $gitBash './scripts/generate-appsettings.sh' '--env-file' '.env.local' '--output-dir' 'local/generated' 2>&1 |
            Where-Object { $_ -match 'Generated|ERROR|error' } | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkGray }
    } finally { Pop-Location }
    $count = (Get-ChildItem (Join-Path $LocalDir 'generated') -Filter 'appsettings.*.json' -ErrorAction SilentlyContinue).Count
    if ($count -lt 1) { throw "No appsettings were generated - check .env.local." }
    Write-Ok "$count appsettings files in local/generated"
}

# =============================================================================
# 8/9. Report on the elevated steps
# =============================================================================
Write-Host ""
Write-Step "Steps that need an elevated shell"
$needHosts = -not (Test-HostsEntry)
$needTrust = -not (Test-CertTrusted)

if ($needHosts) { Write-Warn2 "hosts entry '127.0.0.1 $HostName' is MISSING" } else { Write-Ok "hosts entry present" }
if ($needTrust) { Write-Warn2 "$HostName certificate is NOT trusted"        } else { Write-Ok "certificate trusted" }

Write-Host ""
if ($needHosts -or $needTrust) {
    Write-Host "Run this in an ELEVATED PowerShell to finish:" -ForegroundColor Yellow
    Write-Host "  & '$PSCommandPath' -AdminTasks" -ForegroundColor White
} else {
    Write-Host "Setup complete. Start the stack with:" -ForegroundColor Green
    Write-Host "  & '$PSScriptRoot\Start-TrackHubLocal.ps1'" -ForegroundColor White
}
