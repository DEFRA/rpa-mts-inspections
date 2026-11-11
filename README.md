# RPA MTS Inspections

MTS Risk Analysis portal: plant inspection records, scheme criteria and risk scores for the BLS, BCC, PCG and SCC
schemes. ASP.NET Core MVC on .NET 10 with Entity Framework 6, built to run as a non-root Linux container on Azure
Container Apps (ACA).

**Azure deployment has not been verified.** The code, container definition, Bicep templates, pipeline definition and
tests are in this repository; no Azure resources, Entra registration or production database were used. See
[docs](./docs) for the ACA assessment, the remediation summary and the [operator guide](./docs/operator-guide.md),
and the limits below.

## Build and test

Run from the repository root with the .NET 10 SDK:

```powershell
dotnet restore .\RPA.MTSInspections.sln
dotnet build .\RPA.MTSInspections.sln -c Release --no-restore
dotnet test .\RPA.MTSInspections.sln -c Release --no-build
```

The tests need no Azure, Entra or network access. Sign-in is simulated by a header-based test handler. The
`SqlServer` category (EF6 provider and the existing migrations) uses a scratch database on SQL Server LocalDB and is
skipped when LocalDB is absent: `dotnet test --filter "Category!=SqlServer"` leaves it out.

## Configuration

Settings come from environment variables (`__` separates sections), development user secrets or files mounted under
`/run/secrets`, which override matching environment values. Required values are validated at start-up and the host
refuses to start without them. Keep credentials out of Git and images.

| Setting | Requirement |
|---|---|
| `AzureAd__TenantId` | Required specific tenant GUID, not `common` or `organizations`. |
| `AzureAd__ClientId` | Required web application registration GUID. |
| `AzureAd__ClientSecret` | Required in every environment (ACA secret reference to Key Vault in production). |
| `AzureAd__Instance` | HTTPS authority; defaults to `https://login.microsoftonline.com/`. |
| `Authorization__Roles__BasicAccess`, `SupportTeam`, `Inspector`, `Admin` | Entra app-role **values**. Defaults `MTS.BasicAccess`, `MTS.SupportTeam`, `MTS.Inspector`, `MTS.Admin`; must be four different token-safe values. |
| `Identity__AuditNameClaimType` | Required claim type for the audit user name (for example `preferred_username`). No fallback. |
| `ConnectionStrings__MTSInspectionsContext` | Required SQL connection string. Outside Development it may not use Integrated Security or `TrustServerCertificate`. Use `Authentication=Active Directory Managed Identity;User Id=<client id>` with Azure SQL. |
| `AllowedHosts` | Required explicit hostnames separated by semicolons; no wildcards, schemes, ports or paths. |
| `HttpsRedirection__HttpsPort` | External HTTPS port; defaults to 443. |
| `ReverseProxy__KnownProxies__0`, `ReverseProxy__KnownIPNetworks__0` | Approved immediate proxy address or canonical CIDR; further entries use the next index. Never trust all networks. Forwarding is off when neither is set. |
| `DataProtection__Enabled`, `BlobUri`, `KeyIdentifier`, `ApplicationName`, `ManagedIdentityClientId` | Shared encrypted key ring (Blob Storage, wrapped by a Key Vault key, via one user-assigned managed identity). **Enable for any deployment with more than one replica or revision**; otherwise sign-in cookies and antiforgery tokens break across replicas and restarts. |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Optional telemetry; `ApplicationInsights__ConnectionString` also works and the former wins. |
| `Telemetry__ManagedIdentityClientId` | Optional ingestion identity; requires a connection string. |
| `OTEL_SERVICE_NAME` | Optional stable service/role name in telemetry. |
| `Upload__MaxFileBytes`, `Upload__MaxRowsPerSheet` | Bulk-load limits; defaults 10 MB and 10,000 rows per worksheet. |
| `ApplyMigrationsOnStartup` | Development only; start-up refuses `true` elsewhere. The schema is released separately. |

`ConnectionStrings__IDTSecurityConnection` is obsolete and rejected: roles no longer come from the Security database.

## Sign-in, roles and audit

Users sign in with Microsoft Entra ID (authorization code with PKCE) and receive an HTTPS-only `__Host-` cookie with a
30-minute sliding lifetime. Configure the web registration with the HTTPS redirect URIs `/signin-oidc` and
`/signout-callback-oidc`. Sign-out is an antiforgery-protected POST.

Authorization uses the Entra `roles` claim, and the screens and the server share one definition:

| Capability | Roles |
|---|---|
| Home, plant list and details | any MTS role |
| Plant administration, audit history, CSV export | Support, Admin |
| Criteria, option and weighting administration, bulk import, recalculating all risk scores | Admin |
| BLS, BCC, PCG edits | Admin, Inspector |
| SCC edits | any MTS role |

Inspectors cannot change the inspection-history fields (number of inspections failed, non-compliances, severity,
time since last inspection, scheme year), and only Admin can change the last inspection date. The server ignores any
posted value for these fields, whatever the screen sent. These rules come from the previous screens and are awaiting
business-owner approval; see the remediation summary.

Roles are flat: an Admin does not imply Basic Access, so assign every role a person needs. Role changes apply at the
next sign-in; there is no immediate revocation. Audit rows record the configured claim, lower-cased, with `earth\`
removed and `m0` replaced by `m`, and UTC timestamps.

## Run locally

```powershell
dotnet user-secrets set "AzureAd:TenantId" "<tenant guid>" --project .\RPA.MTSInspections
dotnet user-secrets set "AzureAd:ClientId" "<client guid>" --project .\RPA.MTSInspections
dotnet user-secrets set "AzureAd:ClientSecret" "<secret>" --project .\RPA.MTSInspections
dotnet user-secrets set "Identity:AuditNameClaimType" "preferred_username" --project .\RPA.MTSInspections
dotnet run --project .\RPA.MTSInspections
```

`appsettings.Development.json` supplies a LocalDB connection string and `AllowedHosts=localhost`. The database
schema must already exist: the application never creates or alters it. For a throw-away local database set
`ApplyMigrationsOnStartup=true` in Development.

## Container

```powershell
docker build -t mts-inspections:local .
```

The image is a multi-stage .NET 10 Linux build running as a non-root user on port 8080 with ICU enabled (en-GB dates
and numbers). Pass `--build-arg NUGET_SOURCE=<feed>` when the default NuGet feed is not reachable. The platform
probes use HTTP 8080 with an allowed `Host` header:

| Endpoint | Meaning |
|---|---|
| `/health/live` | Process responds; checks nothing else. |
| `/health/ready` | Bounded SQL `SELECT 1` (3-second timeouts); 503 when the database is unreachable. |

## Behaviour to know about

- **Bulk import** reads the uploaded workbook from the request stream and writes nothing to disk. It needs the three
  template worksheets, rejects unreadable, oversized or over-long files with an error row, and keeps the previous
  per-row results.
- **CSV export** keeps Windows (CRLF) line endings on every platform. The 16 exports are frozen byte for byte in
  `RPA.MTSInspections.Tests/Golden`; a change to their output fails a test and needs approval. Known issue: PCG exports 11
  and 12 have their active filters swapped relative to their labels.
- **Risk recalculation** runs in the same POST request as a scheme save. Recalculating all scores is an Admin-only POST.
- **Schema** is owned by the DBA. The application never changes it. To release it, run the app with
  `--script <file>` (writes the SQL of every migration) or `--migrate` (applies pending ones; repeatable), both instead of
  the web host and with a connection string whose identity may change the schema. The orphan migration
  `201803161004159_plantnamereq` stays excluded from the build. Grant the runtime identity data access only with
  `infra/Grant-DatabaseAccess.sql`.

## Infrastructure, pipeline and runbooks

| Item | Where |
|---|---|
| Bicep foundation and application templates, key bootstrap, template tests | [infra/](./infra) and [infra/README.md](./infra/README.md) |
| Entra registration script (plan mode by default) | `infra/entra/New-MtsEntraRegistration.ps1` |
| Delivery pipeline (Azure DevOps) | `azure-pipelines.yml` |
| Operator guide: roles, revocation, schema release, rotation, diagnosis, acceptance checklist | [docs/operator-guide.md](./docs/operator-guide.md) |
| Front-end libraries and external requests | [docs/Client-Libraries.md](./docs/Client-Libraries.md) |
| ACA assessment, change list and remediation summary | [docs/](./docs) |

The templates and pipeline are validated offline only (the Bicep compiles and its tests pass; the pipeline YAML parses).
None of it has been deployed or run, and the placeholders in the pipeline and the null example parameters need the
platform decisions in the assessment.

## Not done

A deployment, the Entra registration and role assignments, the DBA grants, and live validation of Entra sign-in,
managed-identity SQL access, the Blob and Key Vault key ring and telemetry. The permission matrix and the other
decisions in the remediation summary are assumptions until the owners approve them.
