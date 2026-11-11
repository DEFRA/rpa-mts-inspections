<#
.SYNOPSIS
Creates the Microsoft Entra app registration for RPA MTS Inspections: single-tenant web app, the four app roles,
the sign-in callbacks and, optionally, the upn ID-token claim. Safe by default: it only prints what it would do
unless -Apply is given.

.DESCRIPTION
Run by the identity team, signed in to Azure CLI (az login) with permission to create app registrations in the
approved tenant. It does not create a client secret: create one in the portal or with the identity team's approved
tooling and store its VALUE in the Key Vault secret named by the application's entraClientSecretName parameter.
It does not assign users or groups to roles. That is a separate, access-controlled step (see docs/operator-guide.md).

.EXAMPLE
./infra/entra/New-MtsEntraRegistration.ps1 -DisplayName 'RPA MTS Inspections (UAT)' -ApplicationOrigin 'https://mts-uat-app.example.invalid'
Shows the plan.

.EXAMPLE
./infra/entra/New-MtsEntraRegistration.ps1 -DisplayName 'RPA MTS Inspections (UAT)' -ApplicationOrigin 'https://mts-uat-app.example.invalid' -Apply
Creates the registration and its service principal.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateLength(1, 120)]
    [string] $DisplayName,

    # The HTTPS origin of the application (the 'applicationOrigin' output of the foundation deployment).
    [Parameter(Mandatory)]
    [ValidatePattern('^https://[A-Za-z0-9.-]+(:[0-9]{1,5})?$')]
    [string] $ApplicationOrigin,

    # Token-safe app-role values; they must match Authorization__Roles__* in the application settings.
    [ValidatePattern('^[A-Za-z0-9][A-Za-z0-9._-]{0,119}$')]
    [string] $BasicAccessRole = 'MTS.BasicAccess',
    [ValidatePattern('^[A-Za-z0-9][A-Za-z0-9._-]{0,119}$')]
    [string] $SupportTeamRole = 'MTS.SupportTeam',
    [ValidatePattern('^[A-Za-z0-9][A-Za-z0-9._-]{0,119}$')]
    [string] $InspectorRole = 'MTS.Inspector',
    [ValidatePattern('^[A-Za-z0-9][A-Za-z0-9._-]{0,119}$')]
    [string] $AdminRole = 'MTS.Admin',

    # Adds the upn claim to the ID token, for a legacy-style audit name. preferred_username needs nothing extra.
    [switch] $AddUpnClaim,

    [switch] $Apply
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$roles = @(
    @{ Value = $BasicAccessRole; Name = 'MTS Basic Access'; Description = 'Open MTS Risk Analysis: home page, plant list and plant details, and edit SCC scheme details.' }
    @{ Value = $SupportTeamRole; Name = 'MTS Support Team'; Description = 'Plant administration, audit history and CSV export, plus the read-only scheme view.' }
    @{ Value = $InspectorRole; Name = 'MTS Inspector'; Description = 'Edit BLS, BCC and PCG scheme details, except the inspection-history fields.' }
    @{ Value = $AdminRole; Name = 'MTS Admin'; Description = 'Administer criteria, options and weightings, run bulk import, recalculate all risk scores, and edit all scheme fields.' }
)

if (@($roles.Value | ForEach-Object { $_.ToLowerInvariant() } | Select-Object -Unique).Count -ne 4) {
    throw 'The four app-role values must be different.'
}

$appRoles = $roles | ForEach-Object {
    [ordered]@{
        id = [guid]::NewGuid().ToString()
        allowedMemberTypes = @('User')
        displayName = $_.Name
        description = $_.Description
        value = $_.Value
        isEnabled = $true
    }
}

$redirectUris = @("$ApplicationOrigin/signin-oidc")
$logoutUrl = "$ApplicationOrigin/signout-callback-oidc"

$optionalClaims = if ($AddUpnClaim) {
    [ordered]@{ idToken = @([ordered]@{ name = 'upn'; essential = $false }) }
} else { $null }

Write-Output "Display name        : $DisplayName"
Write-Output 'Sign-in audience    : AzureADMyOrg (single tenant)'
Write-Output "Redirect URI        : $($redirectUris -join ', ')"
Write-Output "Front-channel logout: $logoutUrl"
Write-Output "App roles           : $($roles.Value -join ', ')"
Write-Output "Optional claims     : $(if ($AddUpnClaim) { 'upn (ID token)' } else { 'none' })"
Write-Output 'Implicit grant      : disabled (the application uses the authorization code flow)'

if (-not $Apply) {
    Write-Output ''
    Write-Output 'Plan only. Re-run with -Apply to create the registration.'
    return
}

$existing = az ad app list --display-name $DisplayName --query '[].appId' -o tsv
if ($LASTEXITCODE -ne 0) { throw 'az ad app list failed. Sign in with az login first.' }
if ($existing) { throw "An app registration named '$DisplayName' already exists ($existing). Choose another name or update it in the portal." }

$rolesFile = Join-Path ([IO.Path]::GetTempPath()) "mts-roles-$([guid]::NewGuid().ToString('N')).json"
$claimsFile = Join-Path ([IO.Path]::GetTempPath()) "mts-claims-$([guid]::NewGuid().ToString('N')).json"

try {
    $appRoles | ConvertTo-Json -Depth 5 -AsArray | Set-Content -LiteralPath $rolesFile -Encoding utf8
    $arguments = @(
        'ad', 'app', 'create',
        '--display-name', $DisplayName,
        '--sign-in-audience', 'AzureADMyOrg',
        '--web-redirect-uris', $redirectUris[0],
        '--enable-id-token-issuance', 'false',
        '--enable-access-token-issuance', 'false',
        '--app-roles', "@$rolesFile",
        '--query', 'appId', '-o', 'tsv'
    )
    if ($optionalClaims) {
        $optionalClaims | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $claimsFile -Encoding utf8
        $arguments += @('--optional-claims', "@$claimsFile")
    }

    $clientId = az @arguments
    if ($LASTEXITCODE -ne 0 -or -not $clientId) { throw 'The app registration was not created.' }

    az ad sp create --id $clientId --query id -o tsv | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "The registration $clientId was created but its service principal was not. Create it in the portal." }

    # Front-channel logout is set on the web platform after creation.
    az ad app update --id $clientId --set "web.logoutUrl=$logoutUrl" | Out-Null

    $tenantId = az account show --query tenantId -o tsv

    Write-Output ''
    Write-Output "Created. AzureAd__TenantId = $tenantId"
    Write-Output "         AzureAd__ClientId = $clientId"
    Write-Output 'Next, by the identity team: create a client secret and store its VALUE in the Key Vault secret named by'
    Write-Output 'entraClientSecretName; assign users or groups to the roles on the enterprise application; then check the'
    Write-Output 'audit claim for sample users (docs/operator-guide.md). No secret or token was printed.'
}
finally {
    foreach ($file in @($rolesFile, $claimsFile)) {
        if (Test-Path -LiteralPath $file) { Remove-Item -LiteralPath $file -Force }
    }
}
