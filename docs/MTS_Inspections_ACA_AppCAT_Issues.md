# RPA MTS Inspections: AppCAT assessment issues (ACA target)

**Page 1 of 2.** Tool findings only. The ordered list of changes to implement is on page 2: [MTS_Inspections_ACA_NonSubsidy_Change_List.md](./MTS_Inspections_ACA_NonSubsidy_Change_List.md). The consolidated report with evidence, design and decisions is [MTS_Inspections_ACA_Readiness_Assessment.md](./MTS_Inspections_ACA_Readiness_Assessment.md).

**Status:** Assessment only. No source, database, identity or deployment changes were made.

> **Update, 4 November 2026:** the P0 and P1 findings below were remediated in code. The scan on this page is the
> **baseline**; the re-scan result (production project 7 / 27 / 69 to 4 / 9 / 17, no mandatory findings) and the
> verification are in [MTS_Inspections_ACA_Remediation_Summary.md](./MTS_Inspections_ACA_Remediation_Summary.md).

## 1. Scan details

| Field | Result |
|---|---|
| Tool | .NET AppCAT CLI `1.0.1127.58951`, report schema `1.0.0` |
| Run | `2026-11-03T12:00:41Z` to `2026-11-03T12:00:50Z` (UTC) |
| Options | Source = Solution, target = `ACA`, code analysis on, privacy mode Restricted (snippets omitted) |
| Scope | `RPA.MTSInspections.sln`: web project and test project, both `net10.0`, scanned from an isolated copy of the source. The repository was not modified. |
| Totals | 2 projects; **7 rule types, 27 incidents, 69 story points** (production project only; test project 0) |
| Severity | Mandatory 2, Potential 19, Optional 6, Information 0 |
| Category | Connection 8, Configuration 6, Database 5, Local 4, Identity 2, Cache 1, Scale 1 |
| Baseline | Release build 0 warnings / 0 errors; 42 tests passed; no vulnerable NuGet packages reported by the configured feed |

Severity and points are each incident's `targets.ACA` rating. Story points are the tool's effort estimate, not a delivery estimate: several incidents are cleared by the same change. The report metadata lists Windows and Linux as operating systems; that is not evidence that Windows containers are supported on ACA.

## 2. Findings at a glance

| # | Rule | Title | Severity | Incidents | Points | Files |
|---:|---|---|---|---:|---:|---|
| 1 | `Identity.0002` | Windows authentication detected | **Mandatory** | 2 | 6 | `Program.cs` |
| 2 | `Connection.0001` | Connection string detected | Potential | 8 | 24 | `appsettings.json` (2), `.Production` (2), `.SIT` (2), `.UAT` (2) |
| 3 | `Local.0003` | Local or network I/O detected | Potential | 4 | 12 | `SL/ExportService.cs` |
| 4 | `Configuration.0003` | Local application configuration detected | Potential | 6 | 6 | `appsettings.json` (3), `.Production` (1), `.SIT` (1), `.UAT` (1) |
| 5 | `Cache.0001` | Data caching detected | Potential | 1 | 3 | `Program.cs` |
| 6 | `Database.0003` | System.Data.SqlClient dependency detected | Optional | 5 | 15 | `Helpers/IdtRoleClaimsTransformation.cs` |
| 7 | `Scale.0001` | Static content detected | Optional | 1 | 3 | `RPA.MTSInspections.csproj` (`wwwroot`) |
| | **Total** | | | **27** | **69** | |

Mandatory findings block migration; Potential findings are likely to cause problems on ACA if unaddressed; Optional findings are best practice. All paths are under `RPA.MTSInspections/`. The raw report was produced in Restricted privacy mode, so exact setting names and code lines are not in it; the "Verified" notes below come from reading the source.

## 3. Finding detail

### 3.1 `Identity.0002`: Windows authentication detected (Mandatory, 2 incidents, 6 points)

- **AppCAT says:** Windows authentication is not supported on Azure App Service, Azure Container Apps or Azure Kubernetes Service.
- **Verified:** [Program.cs](../RPA.MTSInspections/Program.cs) registers IIS in-process authentication when `APP_POOL_ID` is set, otherwise Negotiate. The fallback policy then requires an authenticated user, so there is no anonymous path to keep.
- **Impact on ACA:** a Linux container has no IIS or domain-joined Windows identity. Negotiate against an on-premises domain is not a supported replacement.
- **Remediation:** Entra ID browser sign-in with a secure cookie (T04), fed by registered roles and claims (T02, T03). The IDT role lookup and audit identity that depend on `Identity.Name` change with it (T05 to T07).
- **Expected after remediation:** both incidents clear.

### 3.2 `Connection.0001`: Connection string detected (Potential, 8 incidents, 24 points)

- **AppCAT says:** the connection string may not be reachable from Azure; review it and consider migrating the database to Azure SQL.
- **Verified:** two connection strings (`MTSInspectionsContext`, `IDTSecurityConnection`) in each of the four settings files. All use `Integrated Security=True`; the hosts are on-premises SQL servers (`Data Source=.` in the base file, named servers in Production, SIT and UAT). `IDTSecurityConnection` also sets `Pooling=False`.
- **Impact on ACA:** Windows-integrated authentication does not work from a Linux container, and the committed hosts may not be reachable from ACA.
- **Remediation:** remove environment hosts and Integrated Security from committed files; supply the application connection string externally using a managed-identity connection with encryption on (T03, T10). The Security database connection is removed with IDT (T07). Network path and database grants are platform tasks (T18).
- **Expected after remediation:** the four incidents for `MTSInspectionsContext` and the four for `IDTSecurityConnection` no longer ship as committed hosts. A development-only connection string in a retained settings file may still be flagged by design.

### 3.3 `Local.0003`: Local or network I/O detected (Potential, 4 incidents, 12 points)

- **AppCAT says:** I/O outside the application folder might not exist in the target; use storage or deploy needed files with the app.
- **Verified:** `ExportService.BulkData` saves each uploaded workbook under `AppDomain.CurrentDomain.BaseDirectory\Uploads\<client file name>`, opens it with EPPlus, and deletes it. The `Uploads` folder is copied into the publish output. The routine is called once per worksheet (three times), re-writing the same file each time; the workbook object is not disposed.
- **Impact on ACA:** a non-root container's application directory should not be written to; replicas do not share local files; a client-supplied file name is used in the path; concurrent same-name uploads collide.
- **Remediation:** parse the workbook once from a bounded stream or a unique temporary path, dispose it, and clean up on failure (T15). Durable file storage is not required unless the business wants uploads retained.
- **Expected after remediation:** incidents clear or reduce to documented temporary-file use.

### 3.4 `Configuration.0003`: Local application configuration detected (Potential, 6 incidents, 6 points)

- **AppCAT says:** settings in local files could move to Azure App Configuration for central management and dynamic refresh.
- **Verified:** the same four settings files. The base file also carries `ApplyMigrationsOnStartup`, `AllowedHosts: "*"` and an empty `ApplicationInsights:ConnectionString`.
- **Impact on ACA:** environment-specific values should not be baked into the image.
- **Remediation:** ASP.NET Core already reads environment variables, `/run/secrets` and ACA secrets, which is how the Non-Subsidy reference is configured; Azure App Configuration is optional and not used by the reference (T03).
- **Expected after remediation:** incidents on a retained base settings file will likely remain. Accept them with the documented rationale rather than forcing zero.

### 3.5 `Cache.0001`: Data caching detected (Potential, 1 incident, 3 points)

- **AppCAT says:** local caches may not work across machines or in Azure.
- **Verified:** `AddMemoryCache()` in `Program.cs`. Its only consumer is the five-minute IDT role cache. It is replica-local and not used for session state.
- **Impact on ACA:** with several replicas each caches independently, so role changes appear at different times per replica. A distributed cache is not otherwise needed.
- **Remediation:** the registration and cache usage go away when IDT is retired (T07). Redis is not required.
- **Expected after remediation:** clears.

### 3.6 `Database.0003`: System.Data.SqlClient dependency detected (Optional, 5 incidents, 15 points)

- **AppCAT says:** prefer Microsoft.Data.SqlClient, which supports managed identity from the connection string.
- **Verified:** all five incidents are in `IdtRoleClaimsTransformation.cs`, which opens a `SqlConnection` to run `dbo.aspnet_UsersInRoles_GetRolesForUser`.
- **Not reported by AppCAT:** EF6 itself goes through `System.Data.SqlClient` (`MTSInspectionsDbConfiguration` and the migration configuration), so the application database path is also affected. This is the main open design decision (decision D-04 in the report).
- **Remediation:** the IDT incidents clear with IDT retirement (T07). The EF6/application-database provider path is handled under T10.
- **Expected after remediation:** the five incidents clear; verify the EF path manually because AppCAT does not report it.

### 3.7 `Scale.0001`: Static content detected (Optional, 1 incident, 3 points)

- **AppCAT says:** serving static content from the app may raise cost and require redeployment for content changes; consider Blob Storage and a CDN.
- **Verified:** `wwwroot` is served by the application. The Non-Subsidy reference also serves static files from the container.
- **Remediation:** accept for now. Separately review the old vendored browser libraries and the external CDN/font links in the layout (T23); NuGet audit does not scan them.
- **Expected after remediation:** the incident remains, with a documented acceptance.

## 4. Issues AppCAT cannot see

These are not AppCAT findings, but they block or weaken an ACA deployment. They are listed on page 2 with their implementation steps.

| Gap | Evidence | Page 2 item |
|---|---|---|
| Role enforcement is mostly in views | Only `Home.Index` declares a role; other actions rely on authenticated-only fallback | T05, T08 |
| State-changing actions without antiforgery / POST-only | Scheme and many admin POSTs lack token validation; risk recalculation is a link | T09 |
| Audit actor comes from the Windows name | `UserHelper` reads `Identity.Name` | T06 |
| No shared cookie keys | No Data Protection configuration | T14 |
| No probes, forwarded headers, explicit hosts | Not present; `AllowedHosts` is `*` | T11, T12 |
| Telemetry key gate | Only `ApplicationInsights:ConnectionString` is honoured | T13 |
| No container, IaC or pipeline | None in the repo | T17 to T19 |

## 5. Re-scan and acceptance criteria

After the changes on page 2 are implemented, re-run AppCAT for target `ACA` on the remediated production project and compare with this baseline (7 rules / 27 incidents / 69 points).

| Rule | Target result |
|---|---|
| `Identity.0002` | 0 incidents |
| `Database.0003` | 0 incidents (and no `System.Data.SqlClient` in the EF path, verified manually) |
| `Cache.0001` | 0 incidents, or one documented replica-local cache |
| `Connection.0001` | No Integrated Security or environment hosts in committed files; any development-only string documented |
| `Configuration.0003` | Residual incidents on retained settings files accepted with rationale |
| `Local.0003` | 0 incidents, or temporary-file use documented |
| `Scale.0001` | Accepted with rationale |

No unaccepted Mandatory finding may remain. Reaching zero incidents is not a goal in itself; each residual finding needs a recorded disposition.

**Evidence retained outside this repository:** `appcat-report.json`, `mts-baseline.trx` and `nuget-vulnerability-report.json` from the 3 November 2026 run.
