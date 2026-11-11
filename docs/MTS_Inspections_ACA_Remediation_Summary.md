# RPA MTS Inspections: ACA remediation summary

**Date:** 4 November 2026  
**Scope:** the P0 and P1 findings on [page 1](./MTS_Inspections_ACA_AppCAT_Issues.md) and the ACA-readiness gaps AppCAT cannot see, following the [Non-Subsidy change list](./MTS_Inspections_ACA_NonSubsidy_Change_List.md).  
**Status:** code remediation done and tested, and in the second pass (section 9) the Bicep templates, pipeline definition, Entra and grant scripts and the operator guide were added. **Not deployed to Azure, and nothing was run against a tenant or subscription.** Approval of the assumed decisions, the platform parameters, the Entra work and live validation remain (section 7).

## 1. Result at a glance

| Measure | Before | After |
|---|---|---|
| AppCAT (ACA), production project: rules / incidents / points | 7 / 27 / 69 | **4 / 9 / 17** after the first pass; **6 / 11 / 21** after the second (two accepted incidents in the schema utility, section 9) |
| AppCAT mandatory findings, production project | 2 (`Identity.0002`) | **0** |
| Release build | 0 warnings, 0 errors | 0 warnings, 0 errors |
| Tests | 42 | **276** (265 on Windows and Linux, plus 11 against SQL Server LocalDB on Windows) |
| Vulnerable NuGet packages (direct and transitive) | none reported | none reported (two transitive advisories surfaced after adding packages and were pinned) |

The test project adds 32 AppCAT incidents of its own (see section 3); they are test fixtures, not product code.

## 2. What was fixed

### AppCAT findings

| Rule | Before | After | What changed |
|---|---|---|---|
| `Identity.0002` Windows authentication (mandatory, P0) | 2 incidents | **0** | IIS/Negotiate replaced by Entra browser sign-in (authorization code with PKCE, `__Host-` HttpOnly Secure cookie, tenant check, sign-out POST, access-denied and failure pages). |
| `Connection.0001` (P1) | 8 | 1 | Hosts and Integrated Security removed from committed settings; SIT, UAT and Production files deleted; connection string supplied externally and validated at start-up. The one remaining incident is the LocalDB string in `appsettings.Development.json`. |
| `Local.0003` local I/O (P1) | 4 | **0** | Workbook read from the request stream; nothing written to the application folder; client file name never used. |
| `Configuration.0003` (P1) | 6 | 5 | Settings are environment-driven. Remaining incidents are the retained `appsettings.json` and `appsettings.Development.json`, accepted. |
| `Cache.0001` (P1) | 1 | **0** | In-memory role cache removed with IDT. |
| `Database.0003` (P2) | 5 | **0** | IDT SQL access removed. EF6 itself moved to the Microsoft.Data.SqlClient provider (see decision D-04). |
| `Scale.0001` (P2) | 1 | 1 | Accepted: static files served from the container. |
| `Security.0003` (P2, new) | 0 | 2 | **False positives** in `Program.cs` and `ApplicationServices.cs`: the strings are the names of settings (`AzureAd:ClientSecret`) and the `/run/secrets` mount path, not secrets. |

### Gaps AppCAT cannot see

| Gap | Resolution |
|---|---|
| Role enforcement only in views (T05) | Named policies on every action; one `RoleAccess` source shared by controllers and views (18 view checks replaced). |
| Field-level permissions (T08) | Server ignores posted inspection-history fields for Inspectors and the last inspection date for non-Admins, for BLS, BCC and PCG. |
| Antiforgery and unsafe verbs (T09) | Global antiforgery validation; all scheme actions `[HttpPost]`; recalculation moved from GET links to POST. |
| Audit actor from the Windows name (T06) | Scoped `IUser` over a configured, validated claim; no fallback; UTC kept. |
| IDT role lookup (T07) | Removed with its connection string; the obsolete setting is now rejected at start-up. |
| SQL access (T10) | EF6 retained on the Microsoft.Data.SqlClient provider with Azure SQL retry and managed-identity connection strings; implicit schema creation disabled; start-up migrations refused outside Development. |
| Shared cookie keys (T14) | Blob and Key Vault key ring through one user-assigned managed identity, fully validated. |
| Forwarded headers, hosts, HTTPS (T11) | Explicit proxy trust only, one hop, explicit `AllowedHosts`, probes exempt from HTTPS redirect. |
| Probes (T12) | `/health/live` (no checks) and `/health/ready` (bounded SQL `SELECT 1`). |
| Telemetry (T13) | Standard and nested connection-string settings; optional ingestion identity; verified against Application Insights 3.2.0 (`Credential`). |
| Upload safety (T15) | Size, row and sheet-count limits; unreadable files reported as an error row; workbook parsed once and fully before any row is applied. |
| Linux case-sensitivity (T16/T17) | `Site.css` referenced `../images/` but the folder is `Images`; fixed. CSV line endings pinned to CRLF as before. |
| Container image (T17) | Multi-stage Dockerfile (non-root, port 8080, ICU on) and `.dockerignore`. |

## 3. AppCAT detail after remediation

Production project: **4 rules, 9 incidents, 17 points**, none mandatory.

| Rule | Incidents | Disposition |
|---|---:|---|
| `Configuration.0003` | 5 | Accepted: retained settings files |
| `Connection.0001` | 1 | Accepted: development-only LocalDB string |
| `Scale.0001` | 1 | Accepted |
| `Security.0003` | 2 | False positive (setting names) |

Test project (final scan, 4 November 2026, after the SQL Server tests were added): 6 rules, 32 incidents, 56 points, all inside test code. The four `Identity.0002` hits are connection strings the tests use to prove Integrated Security is **rejected** or to reach LocalDB; the rest are literal URLs, temp paths and test credentials. They do not ship. The whole solution therefore reports 4 mandatory incidents, all in the test project.

## 4. Verification

| Check | Result |
|---|---|
| Clean Release build | 0 warnings, 0 errors |
| Test suite, Windows | 209 passed |
| Test suite, Linux (.NET 10 SDK container, restored from the local package cache) | 205 passed, including workbook upload, the HTTP pipeline and case-sensitive file paths |
| EF6 provider and the 15 existing migrations, real SQL Server (LocalDB) | Applies with nothing pending, orphan migration excluded, data round-trips, a missing database is never created |
| Runtime image smoke test (`aspnet:10.0.14-noble`, published output, user `1654`) | Refuses to start without configuration; with configuration: `/health/live` 200, `/health/ready` 503 when SQL is unreachable, HTTP redirected to HTTPS, unlisted host 400 |
| AppCAT re-run (ACA) | Section 3 |
| NuGet vulnerability query | None |

### Not verified

- **`docker build` itself.** The base images pulled, but the restore step failed in this environment because the container's TLS handshake to nuget.org is blocked (`NU1301`). The same restore, publish and run steps were executed in equivalent containers using the local package cache. Use `--build-arg NUGET_SOURCE=<feed>` where nuget.org is blocked.
- **A live Entra sign-in**, role-claim issuance, and the audit claim for real users.
- **Azure SQL with a managed identity**; LocalDB proves the provider, not the identity.
- **The shared key ring** against real Blob Storage and Key Vault, **telemetry ingestion**, replicas and revision overlap.
- **Performance and memory** of large workbooks, lists and exports (the lists still materialise full results before paging).

## 5. Decisions taken in this pass

You asked for the fixes without waiting for the decision list, so these were chosen and need owner confirmation:

| ID | Decision taken | Why it matters |
|---|---|---|
| D-01 | Entra app roles replace SQL IDT roles; role changes apply at next sign-in | Removes the shared Security database dependency; no immediate revocation. |
| D-02 | Four flat role values (`MTS.BasicAccess`, `MTS.SupportTeam`, `MTS.Inspector`, `MTS.Admin`), configurable | IDT hierarchy is not carried over: assign every role a person needs. Existing assignments still have to be migrated. |
| D-03 | Permission matrix derived from the existing screens (README table) | See behaviour changes below. |
| D-04 | **EF6 retained** on Microsoft.Data.SqlClient, not converted to EF Core | Avoids a large rewrite (about 500 EF usages across 53 files). The Azure SQL managed-identity path is not yet proven. |
| D-05 | Audit claim configurable; legacy name formatting kept | A claim such as `preferred_username` gives an email-style actor, not the old account name. |
| D-06 | 30-minute sliding cookie, no absolute cap | Matches the Non-Subsidy reference. |
| D-07 | Upload limits 10 MB and 10,000 rows per sheet (configurable); CRLF CSV | Placeholder limits; no owner-supplied values exist. |

### Behaviour changes to review

- **Home** needs any one MTS role.
- **BCC and PCG last inspection date:** now Admin-only on the server. Previously the screen made it read-only for non-Admins but the server accepted any posted value (BLS already enforced this).
- **Inspectors** can no longer change the five inspection-history fields by crafting a request; screens are unchanged.
- **Recalculating all risk scores** is a POST button, Admin only. Previously a link anyone signed in could follow.
- **Saving a scheme** recalculates its score in the same request instead of redirecting to a second request.
- **Bulk import:** structural problems (unreadable file, fewer than three sheets, too large, too many rows) now produce an error row instead of an unhandled error. All three sheets are read before any row is applied.
- **Removed:** SIT, UAT and Production settings files, the IDT role provider and the static `UserHelper`. On-premises IIS with Windows sign-in is no longer supported.

## 6. Open issues found and deliberately not changed

- Plant edit audit labels the County and Post Code changes as "Address Line Two" (`AdminController`).
- Plant create and edit bind the posted entity directly and mark it modified (over-posting risk).
- The SCC risk calculation multiplies the enforcement and penalty scores by the weighting of a different criterion; it looks unintended and needs an owner decision. A characterization test now records the current behaviour.
- **PCG CSV exports 11 and 12 have their active-plant filters swapped** relative to their labels (found in the second pass; present in the original code). A test records it. Fixing it changes what users download, so it needs owner approval.
- Lists and exports materialise full result sets before paging (needs measurement against production-sized data).
- The remaining legacy browser shims (`modernizr-2.8.3.js`, `respond.js`) are unchanged; see [Client-Libraries.md](./Client-Libraries.md).

## 7. Remaining work (not code in this repository)

The second pass (section 9) added the Bicep templates, pipeline, Entra script, grant script and operator guide. What is
left needs people, accounts or an Azure environment:

| Task | Owner | Note |
|---|---|---|
| T01 approve the assumed decisions (section 5) and the permission matrix | Service, identity, security owners | Everything in section 5 is an assumption until signed off. |
| T02 run the Entra script, create the secret, assign users and groups, migrate IDT assignments | Identity | `infra/entra/New-MtsEntraRegistration.ps1` (plan mode verified, `-Apply` not run). Includes the audit claim decision (D-05). |
| T10 SQL: schema and migration-history delivery, run `infra/Grant-DatabaseAccess.sql` | DBA | `--script` and `--migrate` exist; Azure SQL with a managed identity not yet exercised. |
| T18 foundation and activation parameters, deployment, key bootstrap | Platform | Templates compile and their offline tests pass; **nothing was deployed and no what-if was run**. Needs region, SKU, network and ingress decisions (D-08). |
| T19 pipeline: create the service connections, variable group and environment, then validate | Platform | `azure-pipelines.yml` parses but has **not been run** in any Azure DevOps organisation. |
| T21 deployed acceptance and AppCAT re-run in an approved environment | QA and owners | Checklist in the [operator guide](./operator-guide.md#10-acceptance-checklist-deployed-environment). |
| T22 named owners accept the runbooks, RTO, RPO and alert ownership | Operations | [operator-guide.md](./operator-guide.md) is written; the targets are not chosen. |
| Visual check of the main screens after the front-end library upgrade | QA | Headless checks passed; no real-browser check. |

## 8. Files changed (first pass)

Added (25): `Dockerfile`, `.dockerignore`, `appsettings.Development.json`, `Controllers/AccountController.cs`, `Services/*` (7 files), `Views/Account/*` (4 views), and 9 test files.  
Removed (5): `appsettings.SIT.json`, `appsettings.UAT.json`, `appsettings.Production.json`, `Helpers/IdtRoleClaimsTransformation.cs`, `Helpers/UserHelper.cs`.  
Modified (29): `README.md`, `Program.cs`, both project files, `appsettings.json`, the six controllers (attributes and audit user only for most; `SchemeController` and `ExportController` carry the substantive changes), the EF6 context, provider configuration and migration configuration, `AuditService` and `ExportService`, the layout, home, export and admin views, the four scheme partials, `Site.css`, and three existing test files updated for the new constructors.

## 9. Second pass: remaining change-list items (4 November 2026)

| Item | What was done | Verified how |
|---|---|---|
| T00/T16 characterization | Frozen output of all 16 CSV exports (`Golden/*.csv`, byte for byte, en-GB, CRLF); SCC, BLS and missing-criteria risk-score behaviour recorded, including the SCC weighting anomaly; the PCG 11/12 swap recorded as a known issue. `.gitattributes` stops line-ending conversion of the golden files. | 276 tests pass on Windows; 265 pass on Linux (without the 11 SQL tests). |
| T10/T13 schema release | `RPA.MTSInspections.dll --script <file>` writes the SQL of every migration; `--migrate` applies pending ones and is repeatable. Both run instead of the web host. | LocalDB: the generated script builds the same columns as the migrations, `--migrate` is repeatable, bad arguments are rejected. |
| T10 grants | `infra/Grant-DatabaseAccess.sql`: contained user for the managed identity with `db_datareader` and `db_datawriter` only. | Reviewed only; it needs Azure SQL and the identity name. **Not run.** |
| T18 infrastructure | `infra/` adapted from the Non-Subsidy templates: `main.bicep` (foundation), `app.bicep` (four role parameters, `MTSInspectionsContext`, `OTEL_SERVICE_NAME`), private-endpoint module, key bootstrap script, example parameter files, README. | Both entry points compile with Bicep 0.45.15 (Azure Verified Modules restored); `Test-Templates.ps1` passed 20 scenarios including duplicate role values; `Test-KeyBootstrap.ps1` passed 27 offline tests. **No what-if, no deployment.** |
| T02 Entra | `infra/entra/New-MtsEntraRegistration.ps1`: single-tenant web registration, four app roles, callbacks, optional `upn` claim; plan-only unless `-Apply`; creates no secret and assigns no one. | Plan mode and parameter validation run. **`-Apply` not run.** |
| T19 pipeline | `azure-pipelines.yml`: build with warnings as errors, NuGet vulnerability gate, tests, SQL tests on a Windows agent, image build, Trivy scan before push, digest capture, approval-gated deploy with optional schema step and a smoke test. | YAML parses. **Not run in any organisation**; service connections, variable group and environment names are placeholders. |
| T22 operations | `docs/operator-guide.md`: ownership, deployment order, Entra and roles, audit claim, urgent revocation options, schema release, rotation, diagnosis table, import and export behaviour, rollback, acceptance checklist. | Reviewed against the code; the procedures are not rehearsed. |
| T23 hardening | jQuery 3.3.1 to 3.7.1, Bootstrap 3.3.7 to 3.4.1 (adds the missing `woff2`), jquery-validation 1.17.0 to 1.19.5, bootstrap-datepicker 1.7.1 to 1.10.0; unused Font Awesome link removed; Hind self-hosted; the one local Bootstrap edit moved to `Site.css`; leftover `Uploads/` folder removed. See [Client-Libraries.md](./Client-Libraries.md). | New tests fetch every asset and fail on missing files or external hosts; a jsdom smoke test of validation, datepicker and collapse passed. No real-browser check. |

### AppCAT after the second pass

Production project: 6 rules, 11 incidents, 21 points, none mandatory. Two incidents were added by the new schema
utility (`Local.0003` and `Perf.0001` in `Services/MigrationCommands.cs`: it writes the script file the DBA names), which
are accepted for a one-off command-line tool, the same call made for the Bank Holidays utility. The other nine are
unchanged from section 3. The test project now reports 6 rules, 46 incidents, 86 points (more tests, more literals), including the
same four `Identity.0002` fixtures. Raw report: `appcat-report-final2.json`.

### Verification in the second pass

| Check | Result |
|---|---|
| Release build | 0 warnings, 0 errors |
| Tests on Windows (including 11 SQL Server tests on LocalDB) | 276 passed |
| Tests on Linux in the .NET 10 SDK container (SQL tests excluded) | 265 passed, including the asset and golden-export checks on a case-sensitive file system |
| NuGet vulnerability query | None |
| Bicep entry points and template tests | Pass (compile and offline checks only) |

### Still not verified

Everything listed under "Not verified" in section 4 still applies: a live Entra sign-in, Azure SQL with a managed
identity, the real Blob and Key Vault key ring, telemetry ingestion, a real `docker build` with nuget.org reachable, and a
real-browser check of the screens.
