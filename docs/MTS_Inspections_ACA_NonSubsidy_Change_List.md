# RPA MTS Inspections: ACA change list (CPH Non-Subsidy pattern)

**Page 2 of 2.** The ordered list of changes to implement one by one. The scan findings are on page 1: [MTS_Inspections_ACA_AppCAT_Issues.md](./MTS_Inspections_ACA_AppCAT_Issues.md). The consolidated report (evidence, design, decisions) is [MTS_Inspections_ACA_Readiness_Assessment.md](./MTS_Inspections_ACA_Readiness_Assessment.md). Task IDs (T00 to T23) are the same across all three documents.

**Status:** Code steps implemented and tested on 4 November 2026, and the remaining repository items (templates, pipeline, scripts, runbooks, characterization tests, front-end hardening) added in a second pass; see [MTS_Inspections_ACA_Remediation_Summary.md](./MTS_Inspections_ACA_Remediation_Summary.md). **Not deployed, and nothing was run against Azure or an Entra tenant.** The plan below is kept as written; the status table that follows records what is done.

| Step | ID | Status |
|---:|---|---|
| 0a | T00 | Done: baseline evidence kept; golden files freeze all 16 CSV exports; risk-score behaviour recorded for all four schemes (including the SCC weighting anomaly); scheme-permission, upload and CSV tests |
| 0b | T01 | Decisions **assumed**, not approved (summary section 5) |
| 1 | T02 | Script written (`infra/entra/New-MtsEntraRegistration.ps1`, plan mode verified). **Not applied**: registration, secret, role assignments and IDT migration are for the identity team |
| 2 | T03 | Done |
| 3 | T04 | Done (simulated sign-in only; no live Entra) |
| 4 | T05 | Done |
| 5 | T06 | Done |
| 6 | T09 | Done |
| 7 | T08 | Done |
| 8 | T07 | Done |
| 9 | T10 | Done with EF6 on Microsoft.Data.SqlClient (decision D-04); `--script` and `--migrate` utilities and `infra/Grant-DatabaseAccess.sql` added; verified on LocalDB, not Azure SQL |
| 10 | T15 | Done |
| 11 | T16 | Done: CRLF pinned, all 16 exports frozen, en-GB dates checked, Linux asset case fixed. Found a pre-existing PCG 11/12 label swap, recorded and **not** changed. Downstream consumers not consulted |
| 12 | T11 | Done |
| 13 | T12 | Done |
| 14 | T13 | Done (ingestion not tested) |
| 15 | T14 | Done (not tested against real Blob and Key Vault) |
| 16 | T17 | Done: Dockerfile and `.dockerignore`; `docker build` blocked here by NuGet TLS, equivalent steps verified |
| 17 | T18 | Templates written and validated offline (`infra/`: foundation, application, key bootstrap, tests). **Not deployed, no what-if**; parameters need the D-08 decisions |
| 18 | T19 | Pipeline written (`azure-pipelines.yml`); YAML parses. **Not run**; service connections, variable group and environment are placeholders |
| 19 | T20 | Done for code: 276 tests (265 also pass on Linux) |
| 20 | T21 | Partly: AppCAT re-run and Linux checks done; deployed acceptance not done (checklist in `docs/operator-guide.md`) |
| 21 | T22 | Written: `docs/operator-guide.md`, README, `docs/Client-Libraries.md`; named owners, RTO and RPO not set; procedures not rehearsed |
| 22 | T23 | Mostly done: patched jQuery, Bootstrap, jquery-validation and datepicker; external font and icon requests removed; leftover folder removed. Not done: legacy shims (`modernizr`, `respond`), paging and memory measurement, content security policy |

## 1. How to use this list

- Implement the steps in the order shown. Each step lists its prerequisites, the MTS files it touches, the Non-Subsidy code it follows, and its done criteria. Do not start a step before its prerequisites are done and its decisions are approved.
- **Source column:** *Non-Subsidy* means the change follows code or documentation in the `rpa-cph-non-subsidy` repo. *MTS-specific* means the reference has no equivalent and the change comes from reading MTS itself. *Platform* means work owned outside the application code.
- **Clears** shows the AppCAT rule (page 1) that should clear when the step is complete.
- Priority: P0 = required before deployment; P1 = required before production acceptance; P2 = optional. Size (S, M, L) is a relative judgement, not a delivery estimate.
- After every code step: build Release with 0 warnings and 0 errors, run the full test suite, and keep the 42 baseline tests passing.

## 2. Implementation sequence

| Step | ID | Change | Source | Pri | Size | Clears | Depends on |
|---:|---|---|---|---|---|---|---|
| 0a | T00 | Freeze baseline and business contracts | MTS-specific | P0 | S | | none |
| 0b | T01 | Approve identity, role and permission decisions | Non-Subsidy / owners | P0 | S | | T00 |
| 1 | T02 | Entra app registration, roles and assignments | Non-Subsidy | P0 | M | | T01 |
| 2 | T03 | Fail-closed external configuration | Non-Subsidy | P0 | M | `Configuration.0003`, part of `Connection.0001` | T01 |
| 3 | T04 | Entra sign-in and cookie session | Non-Subsidy | P0 | M | `Identity.0002` | T02, T03 |
| 4 | T05 | Role policies and constants | Non-Subsidy (extended) | P0 | M | | T01, T04 |
| 5 | T06 | Scoped audit identity | Non-Subsidy | P0 | S | | T04 |
| 6 | T09 | Antiforgery and safe verbs | MTS-specific | P0 | M | | T05 |
| 7 | T08 | Field-level scheme permissions | MTS-specific | P0 | L | | T01, T05 |
| 8 | T07 | Retire IDT role lookup | Non-Subsidy | P0 | S | `Cache.0001`, `Database.0003` | T05, T06 |
| 9 | T10 | SQL managed identity and schema ownership | Non-Subsidy / decision D-04 | P0 | L | `Connection.0001` | T00, T01 |
| 10 | T15 | Upload processing for non-root Linux | MTS-specific | P0 | M | `Local.0003` | T00, T03 |
| 11 | T16 | Export and culture characterization | MTS-specific | P1 | S | | T00 |
| 12 | T11 | Proxy, HTTPS and host handling | Non-Subsidy | P1 | S | | T03, T04 |
| 13 | T12 | Liveness and readiness probes | Non-Subsidy | P1 | S | | T10 |
| 14 | T13 | Telemetry | Non-Subsidy (verify) | P1 | S | | T03 |
| 15 | T14 | Shared Data Protection keys | Non-Subsidy | P0 | M | | T03, T04 |
| 16 | T17 | Linux container image | Non-Subsidy | P0 | M | | T03 to T16 |
| 17 | T18 | Platform foundation and activation | Non-Subsidy / Platform | P0 | M | | T01, T10 to T14 |
| 18 | T19 | Delivery pipeline | Platform | P1 | M | | T17, T18 |
| 19 | T20 | Automated test suites | Non-Subsidy / MTS-specific | P0 | L | | T04 to T16 |
| 20 | T21 | Deployed acceptance and AppCAT re-run | Non-Subsidy / Platform | P0 | M | | T18 to T20 |
| 21 | T22 | Operations handover | Non-Subsidy | P1 | M | | T21 |
| 22 | T23 | Optional hardening | MTS-specific | P2 | M | `Scale.0001` (accepted) | T00 |

The order is chosen so that authorization is enforced (steps 4 to 7) before the shared role source is removed (step 8), and so that data access, runtime and hosting follow identity. Steps 10 and 11 can run in parallel with 9 once T00 and T03 are done.

## 3. Decisions to close before Step 1

| ID | Decision | Owner | Blocks |
|---|---|---|---|
| D-01 | Retire SQL IDT roles in favour of Entra app roles, and accept that role changes apply at next sign-in (no immediate revocation) | Service, identity, security | T02, T05, T07 |
| D-02 | Role values, flat versus implied roles, and migration of existing assignments | Identity + Security DB owner | T02, T05 |
| D-03 | Permission matrix (section 4), especially Support versus Admin and SCC editing | Business + security | T05, T08 |
| D-04 | EF Core conversion (the Non-Subsidy path) versus retaining EF6 with a new provider | App + DBA | T10 |
| D-05 | Audit claim and legacy-name continuity | Identity + audit owner | T06 |
| D-06 | Session lifetime and urgent-revocation procedure | Security | T04, T22 |
| D-07 | Upload limits, partial-success contract, CSV downstream compatibility | Business + operations | T15, T16 |
| D-08 | Ingress, environment/region, sizing, pipeline ownership, RTO/RPO, retention | Platform + service owner | T18, T19 |

## 4. Reference: roles and permission matrix to approve

### 4.1 Proposed Entra role values (proposals only, not approved or created)

Role values must be token-safe and may not contain spaces; the current labels contain spaces and an en dash, so they cannot be copied directly.

| Current IDT role | Proposed Entra value |
|---|---|
| `MTS Risk Analysis: Basic Access` | `MTS.BasicAccess` |
| `MTS Risk Analysis : General User – Support Team` | `MTS.SupportTeam` |
| `MTS Risk Analysis : General User – Inspector` | `MTS.Inspector` |
| `MTS Risk Analysis : Admin User` | `MTS.Admin` |

Entra roles are flat, and Non-Subsidy uses one role. Any implied-role behaviour the IDT database provides today must be defined explicitly (D-02).

### 4.2 Permission matrix

| Surface | Current intent (evidence) | Required |
|---|---|---|
| Home, plant list/details | Home requires Basic Access; plant actions only authenticated | Approve baseline read policy; enforce on direct URLs |
| Plant/criteria/option/weighting admin | Support and Admin see different links; actions unrestricted beyond authentication | Approve Support versus Admin per action; enforce on every action |
| BLS/BCC/PCG field edits | Admin, Support, Inspector see different enabled/disabled fields | Server-side allowlist per scheme and role; ignore forged fields |
| SCC edits | Partial lets Basic, Support, Inspector or Admin edit | Confirm this broader behaviour explicitly |
| Last-inspection date | Explicit Admin check in the BLS save only | Verify per scheme and on the import path |
| Export (16 variants) | UI shows to Support or Admin | Enforce on the action for every variant |
| Bulk import | UI shows to Admin; POST has antiforgery but no role check | Enforce policy on GET and POST |
| Audit views | No explicit check | Approve who may view; enforce on list and detail |
| Risk recalculation (single / all) | Link/redirect-driven writes | Protected POST or internal call; approve role |
| Account, error, probes | Error route inherits authentication | Anonymous only where deliberate; no sign-in loop |

## 5. Step details

### Step 0a. T00: Freeze baseline and business contracts (P0, S, MTS-specific)

- **Change:** keep the 3 November AppCAT, build and test evidence. Capture synthetic characterization fixtures for the four schemes' risk scores (BLS, BCC, PCG, SCC), the 16 CSV exports, the three-sheet import with its error list, and audit rows.
- **Touches:** tests and fixtures only.
- **Done when:** fixtures run green against the unchanged code and are used as the regression baseline for every later step.

### Step 0b. T01: Approve identity, role and permission decisions (P0, S)

- **Change:** close D-01 to D-06 and D-08 in section 3.
- **Done when:** each decision has a named owner and a recorded answer. Real user or role data is handled only through owner-approved, access-controlled extracts.

### Step 1. T02: Entra app registration, roles and assignments (P0, M, Non-Subsidy)

- **Follows:** [operator guide, Entra setup](../../rpa-cph-non-subsidy/docs/operator-guide.md).
- **Change:** single-tenant web registration with exact HTTPS `/signin-oidc` and `/signout-callback-oidc` callbacks; client secret stored in Key Vault; app roles from section 4.1; ID-token claim for the audit actor; assignment of existing users or groups to roles from an approved extract.
- **Touches:** Entra tenant and Key Vault (identity team); no application code.
- **Done when:** approved test users with each role, and a user with none, are verified; no tokens appear in logs or tickets.

### Step 2. T03: Fail-closed external configuration (P0, M, Non-Subsidy)

- **Follows:** [ApplicationServices.cs, `ValidateConfiguration`](../../rpa-cph-non-subsidy/RPA.CPH.NonSubsidy.Domain/RPA.CPH.NonSubsidy.Web/Services/ApplicationServices.cs); `/run/secrets` support in [Program.cs](../../rpa-cph-non-subsidy/RPA.CPH.NonSubsidy.Domain/RPA.CPH.NonSubsidy.Web/Program.cs).
- **Change:** validate at startup the tenant GUID (not `common`/`organizations`), client ID, HTTPS authority, client secret, role values, audit claim, SQL connection string, explicit `AllowedHosts` and HTTPS port. Remove environment hosts and Integrated Security from the committed settings files. Read secrets from environment, `/run/secrets` or ACA secrets.
- **Touches:** `Program.cs`, `appsettings.json`, `appsettings.Production.json`, `appsettings.SIT.json`, `appsettings.UAT.json`, a new configuration/validation class.
- **Done when:** missing or invalid values stop the host with a clear non-secret message; no host names or credentials remain in committed files. Residual `Configuration.0003` on a retained base file is accepted, not forced to zero.

### Step 3. T04: Entra sign-in and cookie session (P0, M, Non-Subsidy)

- **Follows:** [ApplicationServices.cs, `AddIdentity`](../../rpa-cph-non-subsidy/RPA.CPH.NonSubsidy.Domain/RPA.CPH.NonSubsidy.Web/Services/ApplicationServices.cs) and the [Account pages](../../rpa-cph-non-subsidy/RPA.CPH.NonSubsidy.Domain/RPA.CPH.NonSubsidy.Web/Pages/Account/SignIn.cshtml.cs).
- **Change:** replace IIS/Negotiate with `Microsoft.Identity.Web` OIDC (authorization code, PKCE). Disable inbound claim mapping on both the OIDC options and the token handler; use `roles` as the role claim type. Require exactly one `tid` claim equal to the configured tenant. Cookie: `__Host-` prefix, HttpOnly, Secure, SameSite per decision D-06, sliding expiry. Antiforgery-protected POST sign-out. Sign-in, signed-out, access-denied and authentication-failed pages. Event-coded warnings without claim values. No Windows or anonymous production fallback.
- **Touches:** `Program.cs`, `RPA.MTSInspections.csproj` (package references), new Account controller or pages, layout sign-out control.
- **Done when:** a user with a role signs in and out; a user without one gets 403; a foreign tenant is rejected; `Identity.0002` no longer appears in a re-scan.
- **Note:** MTS is MVC, not Razor Pages. Keep MVC and implement the account endpoints as a controller.

### Step 4. T05: Role policies and constants (P0, M, Non-Subsidy, extended)

- **Follows:** the single named policy in [ApplicationServices.cs](../../rpa-cph-non-subsidy/RPA.CPH.NonSubsidy.Domain/RPA.CPH.NonSubsidy.Web/Services/ApplicationServices.cs). MTS needs several roles, so the reference is extended, not copied.
- **Change:** define role values and capability policies in one place. Add `[Authorize(Policy = ...)]` to every controller and action per section 4.2. Replace the label strings in `HomeController`, the `User.IsInRole` call in `SchemeController`, and the 18 `User.IsInRole` checks in views (`Home/Index`, `Export/Index`, and the four scheme partials) with policy-based checks from the same source.
- **Done when:** every action has an explicit policy or a documented anonymous exception; tests cover no-role, wrong-role and permitted-role requests for each row of 4.2.

### Step 5. T06: Scoped audit identity (P0, S, Non-Subsidy)

- **Follows:** [AuditIdentity.cs](../../rpa-cph-non-subsidy/RPA.CPH.NonSubsidy.Domain/RPA.CPH.NonSubsidy.Web/Services/AuditIdentity.cs) and the scoped [User](../../rpa-cph-non-subsidy/RPA.CPH.NonSubsidy.Domain/RPA.CPH.NonSubsidy.Web/Models/User.cs) service.
- **Change:** replace the static `UserHelper` (which reads `Identity.Name` and has a null-name path) with a scoped user service over the configured claim: exactly one non-blank value or the request fails, no fallback to `Identity.Name`, email or a posted field. Keep audit timestamps in UTC (Non-Subsidy uses local time and must not be copied here). Keep the lowercase, `earth\` and `m0` normalization only if D-05 confirms it.
- **Touches:** `Helpers/UserHelper.cs`, `Program.cs`, all callers (`AdminController`, `SchemeController`, `ExportService`), `AuditService`.
- **Done when:** an edit stores the agreed actor in `Audit.User` with a UTC date; a missing, duplicate or blank claim fails the request instead of storing "Unknown".

### Step 6. T09: Antiforgery and safe verbs (P0, M, MTS-specific)

- **Why:** many admin POSTs and all scheme POSTs lack `[ValidateAntiForgeryToken]` and `[HttpPost]`, and `CaclulateAllRiskScores` is a link that writes data. The reference's sign-out is the only antiforgery pattern to reuse.
- **Change:** add global antiforgery validation for unsafe verbs; mark all mutations `[HttpPost]`; replace the recalculation link with a protected POST or an internal call.
- **Touches:** `AdminController`, `SchemeController`, `ExportController`, the views that post forms or link to recalculation.
- **Done when:** GET never changes data; a missing or invalid token is rejected; invalid-post redisplay and the save-then-recalculate flow still work.

### Step 7. T08: Field-level scheme permissions (P0, L, MTS-specific)

- **Why:** the scheme edit partials hide or disable fields by role, but the server accepts whatever is posted.
- **Change:** server-side allowlist per scheme (BLS, BCC, PCG, SCC) and role from the approved matrix; reload stored values and apply only permitted fields; cover last-inspection dates and the import path.
- **Touches:** `SchemeController`, `SchemeEditViewComponent`, scheme partials, `SchemeService`, `ExportService` (import).
- **Done when:** a forged privileged field is ignored for every role and scheme; SCC behaviour matches what the owner approved.

### Step 8. T07: Retire the IDT role lookup (P0, S, Non-Subsidy)

- **Change:** remove `IdtRoleClaimsTransformation`, the `IDTSecurityConnection` setting, `AddMemoryCache` and the role-cache use, and the `System.Data.SqlClient` use in that class. Do not alter the shared Security database.
- **Precondition:** steps 4 and 5 pass, and D-01 and D-02 are approved.
- **Done when:** the application starts without the Security database; authorization depends only on Entra role claims; `Cache.0001` and `Database.0003` clear in a re-scan.

### Step 9. T10: SQL managed identity and schema ownership (P0, L, Non-Subsidy / D-04)

- **Follows:** the managed-identity connection string in [app.bicep](../../rpa-cph-non-subsidy/infra/app.bicep) and the EF Core guidance in the [developer guide](../../rpa-cph-non-subsidy/docs/developer-guide.md).
- **Option A, the Non-Subsidy path: EF Core.** Needs explicit mappings to the existing EF6-style plural tables (for example `Plants`, `Options`), explicit loading or includes because the code relies heavily on navigation properties (about 500 matches across 53 files for navigation, includes and EF6 usings), explicit tracking and modified-state handling, validation on save, and EF6 migrations excluded from execution.
- **Option B: retain EF6.** Use a Microsoft.Data.SqlClient-based provider. The reference does not demonstrate this, so it must be proven against Azure SQL with a managed identity before being chosen.
- **Both options:** no startup migration in ACA; schema and `__MigrationHistory` delivered by the DBA; contained database user for the runtime identity with minimum grants; connection string with managed identity, `Encrypt=True`, `TrustServerCertificate=False`. Keep the excluded orphan migration (`201803161004159_plantnamereq`) out of execution.
- **Touches:** `DAL/*`, `Migrations/Configuration.cs`, `Program.cs`, services, models, `RPA.MTSInspections.csproj`.
- **Done when:** business outputs match the T00 fixtures, and an approved Azure SQL environment confirms identity-based access. `Connection.0001` no longer reports committed hosts.

### Step 10. T15: Upload processing for non-root Linux (P0, M, MTS-specific)

- **Change:** parse the workbook once from a bounded stream, or use a unique temporary path outside the application directory; dispose the workbook and streams; clean up on success and failure; ignore client-supplied path parts. Validate size, sheet names, headers and row/cell bounds before applying data. Preserve three-sheet order and the current partial-success error list unless D-07 says otherwise.
- **Touches:** `SL/ExportService.cs`, `Uploads` handling in `RPA.MTSInspections.csproj`, `ExportController`.
- **Done when:** malformed, oversize and concurrent same-name uploads are handled safely, and `Local.0003` clears or is reduced to documented temporary files.

### Step 11. T16: Export and culture characterization (P1, S, MTS-specific)

- **Change:** verify all 16 exports on Linux for column order, quoting, encoding and BOM, newline, and en-GB date and number culture (the image must keep ICU). No format change without approval.
- **Done when:** outputs match the T00 fixtures and downstream consumers confirm compatibility.

### Step 12. T11: Proxy, HTTPS and host handling (P1, S, Non-Subsidy)

- **Follows:** [TrustedForwarding.cs](../../rpa-cph-non-subsidy/RPA.CPH.NonSubsidy.Domain/RPA.CPH.NonSubsidy.Web/Services/TrustedForwarding.cs) and the probe exclusion in [Program.cs](../../rpa-cph-non-subsidy/RPA.CPH.NonSubsidy.Domain/RPA.CPH.NonSubsidy.Web/Program.cs).
- **Change:** explicit proxy addresses or canonical CIDRs only (no wildcard, no `/0`), one hop, forwarding disabled when unset; forwarding before HTTPS redirect and authentication; HTTPS redirect skipped for the probe paths; explicit `AllowedHosts` and HTTPS port.
- **Done when:** the real ACA peer lies inside the configured range, external redirects stay HTTPS, and untrusted forwarded headers change nothing.

### Step 13. T12: Liveness and readiness probes (P1, S, Non-Subsidy)

- **Follows:** [SqlReadinessHealthCheck.cs](../../rpa-cph-non-subsidy/RPA.CPH.NonSubsidy.Domain/RPA.CPH.NonSubsidy.Web/Services/SqlReadinessHealthCheck.cs) and the probe settings in [app.bicep](../../rpa-cph-non-subsidy/infra/app.bicep).
- **Change:** anonymous `/health/live` with no checks; `/health/ready` runs a bounded SQL `SELECT 1` (3-second connect and command timeouts), returns 503 on failure and logs a generic warning without SQL error text.
- **Done when:** a SQL outage makes readiness unhealthy while liveness stays healthy, with no restart loop.

### Step 14. T13: Telemetry (P1, S, Non-Subsidy, verify)

- **Follows:** [ApplicationInsightsRegistration.cs](../../rpa-cph-non-subsidy/RPA.CPH.NonSubsidy.Domain/RPA.CPH.NonSubsidy.Web/Services/ApplicationInsightsRegistration.cs).
- **Change:** prefer `APPLICATIONINSIGHTS_CONNECTION_STRING`, fall back to the nested key; optional user-assigned identity for ingestion; an identity without a connection string fails startup. Keep personal data, tokens and workbook contents out of telemetry.
- **Caution:** Non-Subsidy uses `Microsoft.ApplicationInsights.AspNetCore` 2.23.0; MTS uses 3.2.0. Verify the registration and token-credential API against 3.2.0 before reusing the code.
- **Done when:** telemetry arrives in an approved environment without unacceptable sensitive data.

### Step 15. T14: Shared Data Protection keys (P0, M, Non-Subsidy)

- **Follows:** [SharedDataProtection.cs](../../rpa-cph-non-subsidy/RPA.CPH.NonSubsidy.Domain/RPA.CPH.NonSubsidy.Web/Services/SharedDataProtection.cs).
- **Change:** Blob-stored key ring wrapped by a Key Vault key through an explicit user-assigned identity; validated HTTPS URIs; stable per-environment application name; incomplete or unknown settings fail startup.
- **Done when:** cookies, antiforgery and TempData survive restart, multiple replicas and revision replacement; old wrapping-key versions stay recoverable.

### Step 16. T17: Linux container image (P0, M, Non-Subsidy)

- **Follows:** the [Dockerfile](../../rpa-cph-non-subsidy/Dockerfile).
- **Change:** multi-stage .NET 10 build with a restore layer, publish, non-root user, `ASPNETCORE_HTTP_PORTS=8080`, ICU enabled. Add `.dockerignore` excluding uploads, review artifacts, bin/obj and secrets. Check published asset paths are case-correct. Record the image digest and scan results.
- **Done when:** the image runs as non-root on Linux, serves assets, processes a test workbook and exits cleanly.

### Step 17. T18: Platform foundation and activation (P0, M, Non-Subsidy / Platform)

- **Follows:** [infra/README.md](../../rpa-cph-non-subsidy/infra/README.md) and [app.bicep](../../rpa-cph-non-subsidy/infra/app.bicep).
- **Change:** foundation (network, registry, managed identity, Key Vault, key-ring Blob, SQL, monitoring, ACA environment), key bootstrap, then application activation in single-revision mode with HTTPS ingress, digest-pinned image, secrets by Key Vault reference and probes. The receiving team supplies all parameters; no region, SKU or network range is taken from Non-Subsidy.
- **Done when:** image pull, secret resolution, SQL contained-user grants, Blob data access, key wrap/unwrap and registry pull are verified for the runtime identity, with build, deploy and runtime privileges kept separate.

### Step 18. T19: Delivery pipeline (P1, M, Platform)

- **Change:** build, test, scan and promote an immutable image digest with smoke gates and rollback. No MTS pipeline exists today; use the team's standard if one is supplied.
- **Done when:** a release and a rollback have both been exercised.

### Step 19. T20: Automated test suites (P0, L)

Preserve the 42 baseline tests and add suites modelled on the Non-Subsidy test categories:

- Startup configuration failures and the production no-bypass rule.
- Signed-token authentication: valid role, missing or wrong role, foreign or duplicate tenant, missing or duplicate audit claim, raw claim types preserved.
- Every row of section 4.2 for all four schemes, including direct requests and forged privileged fields; GET mutation rejected; missing antiforgery rejected.
- Audit actor and UTC timestamp; invalid saves do not stamp or commit.
- Trusted forwarding with allowed and untrusted peers; probes do not redirect or challenge.
- Shared key ring round trip across two hosts and a restart.
- Upload fixtures: valid, malformed, missing headers, oversize, concurrent same-name, cancellation.
- All 16 exports and the four risk calculations against the T00 fixtures.
- Data-layer tests against SQL-backed synthetic data. SQLite alone does not prove SQL Server behaviour, which Non-Subsidy also states.

### Step 20. T21: Deployed acceptance and AppCAT re-run (P0, M)

In an approved environment:

1. Real Entra sign-in and sign-out, callbacks over the real proxy path, assigned and unassigned users.
2. The runtime identity reaches only the required resources.
3. Non-root Linux image serves assets and processes the workbook and CSV contracts with en-GB behaviour.
4. SQL outage turns readiness unhealthy and keeps liveness healthy, then recovers.
5. Two replicas and a revision replacement share cookies and follow the agreed role-revocation procedure.
6. The largest approved workbook, export and all-score recalculation meet owner-agreed limits (none have been supplied).
7. Rollback, key recovery, secret rotation and import-failure recovery are rehearsed.
8. Re-run AppCAT for `ACA` and apply the criteria on page 1, section 5.

### Step 21. T22: Operations handover (P1, M, Non-Subsidy)

- **Follows:** the [operator guide](../../rpa-cph-non-subsidy/docs/operator-guide.md).
- **Change:** runbooks for role and claim management, assignment and removal, urgent revocation, secret and key rotation, import-failure recovery, RTO/RPO, rollback and alert ownership. Replace the placeholder `README.md` with verified build, test and run commands and the known limits.
- **Done when:** named owners accept the production gates.

### Step 22. T23: Optional hardening (P2, M, MTS-specific)

- **Change:** inventory the vendored browser libraries and external CDN and font links in the layout (licensing, egress, privacy); measure paging and export memory under representative load; confirm static asset placement. Any release-blocking result moves into step 20. No change to business formulas without separate approval.
- **Accepts:** `Scale.0001` with a documented rationale.

## 6. Configuration surface (to implement in steps 2, 9, 12, 14 and 15)

Names follow Non-Subsidy (`__` separates sections in environment variables). They are not existing MTS settings.

| Key | Contract |
|---|---|
| `AzureAd__Instance`, `TenantId`, `ClientId`, `ClientSecret` | HTTPS authority, specific tenant GUID, client ID, secret from Key Vault or an ACA secret reference |
| `Authorization__*` | Approved role values and capability mapping (several roles, replacing the single `BasicAccessRole`) |
| `Identity__AuditNameClaimType` | Explicit audit claim; no fallback |
| `ConnectionStrings__MTSInspectionsContext` | Azure SQL with managed identity, `Encrypt=True`, `TrustServerCertificate=False` |
| `AllowedHosts` | Explicit semicolon-separated hosts |
| `ReverseProxy__KnownProxies__n`, `ReverseProxy__KnownIPNetworks__n`, `HttpsRedirection__HttpsPort` | Platform-approved proxy trust and external HTTPS port |
| `DataProtection__Enabled`, `BlobUri`, `KeyIdentifier`, `ApplicationName`, `ManagedIdentityClientId` | Shared key-ring contract |
| `APPLICATIONINSIGHTS_CONNECTION_STRING`, `Telemetry__ManagedIdentityClientId` | Optional telemetry with identity ingestion |
| Upload limits and temporary location | Owner-approved bounds; ephemeral temp only unless retention is approved |

`ApplyMigrationsOnStartup` stays false or absent in ACA, and `IDTSecurityConnection` is removed with IDT. Keep Azure RBAC, Entra role assignments and SQL grants separate; the runtime identity holds no business roles.

## 7. Known limits of the reference

- Non-Subsidy has one role, no hierarchy and no field-level rules, so steps 4, 7 and 8 are extensions, not copies.
- Roles refresh only at the next sign-in; there is no absolute session cap or immediate revocation.
- Its Azure deployment and production parity are documented as unverified, and its tests use fake identities, signed offline tokens, TestServer and SQLite.
- Its audit timestamps use local time; MTS stores UTC and keeps it.
- Its EF Core conversion is large for MTS and is a decision (D-04), not an assumed step.
