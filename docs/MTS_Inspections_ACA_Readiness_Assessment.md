# RPA MTS Inspections: ACA readiness assessment and task plan (CPH Non-Subsidy reference)

**Assessed:** 3 November 2026 (rewritten 3 November 2026)  
**Target:** Azure Container Apps (ACA), Linux container  
**Reference pattern:** `rpa-cph-non-subsidy` only. Bank Holidays and Claim Statements are no longer used as design inputs.  
**Status:** Assessment as of 3 November 2026. The P0 and P1 code items were remediated on 4 November 2026; see [MTS_Inspections_ACA_Remediation_Summary.md](./MTS_Inspections_ACA_Remediation_Summary.md) for results, verification, assumed decisions and what remains.
**Overall finding:** **Not ready to deploy to ACA as-is. Ready for a targeted remediation phase** following the Non-Subsidy pattern, with the decisions in section 10 resolved first.

**Report pages:** this document is the consolidated report. The same content is also split into two working pages, using the same T-numbers:

1. [AppCAT assessment issues](./MTS_Inspections_ACA_AppCAT_Issues.md): the 7 rules / 27 incidents / 69 points, with per-finding detail and re-scan criteria.
2. [Non-Subsidy change list](./MTS_Inspections_ACA_NonSubsidy_Change_List.md): the ordered, step-by-step implementation list. Its sequence replaces the phase grouping in section 7; if the two differ, the change list is the current plan.

## 1. Executive decision

MTS Inspections is already ASP.NET Core MVC on .NET 10, so a framework upgrade or Razor Pages rewrite is **not** required. The fresh .NET AppCAT scan reports **7 rule types, 27 incidents and 69 tool-estimated story points**, including **2 mandatory Windows-authentication incidents**. AppCAT's points are not a delivery estimate; several incidents share one fix.

Following Non-Subsidy, the recommended target is:

1. **Authentication:** Entra ID browser sign-in (authorization code + PKCE), single tenant, secure cookie, fail-closed startup. Replaces IIS/Negotiate Windows authentication.
2. **Authorization:** Entra **app roles** carried in the `roles` claim and enforced by named policies. This **retires the SQL-based IDT role lookup**, as Non-Subsidy has no Security-database role store. It needs owner approval and a migration of existing role assignments (section 6).
3. **Audit identity:** a configured, validated claim feeds a scoped user service. Legacy name formatting is kept only if the owner confirms audit continuity.
4. **Azure access:** user-assigned managed identity (UAMI) for SQL, Key Vault, Blob and telemetry; no passwords or developer-credential fallback.
5. **Hosting:** non-root Linux image on HTTP 8080, trusted forwarded headers, liveness/readiness probes, shared encrypted Data Protection keys, external configuration and ACA secrets.

Non-Subsidy does not cover MTS's bulk workbook import, 16 CSV exports, four-scheme field-level permissions, GET-driven mutations or missing antiforgery checks. Those are MTS-specific tasks below, not Non-Subsidy-derived ones.

## 2. Basis, evidence and limits

- **Non-Subsidy code and documents reviewed:** [README][C12], [developer guide][C01], [operator guide][C02], [sign-in/authorization/startup validation][C03], [audit identity][C11], [shared keys][C04], [trusted forwarding][C06], [SQL readiness][C07], [telemetry][C08], [Program.cs][C13], [Dockerfile][C10], [app.bicep][C09] and [infra handoff][C05].
- **MTS reviewed:** startup, project files, settings files, all controllers, role-bearing views, view components, services (including `ExportService`), models/risk calculations, EF6 context/configuration and migrations, and the test project.
- **Executed (previous step, isolated source copy, repository untouched):** AppCAT ACA scan, clean Release build, 42-test run, NuGet advisory query. The 281 staged files were byte-for-byte identical to the originals. The copy has since been removed; the raw AppCAT report (`appcat-report.json`), test results (`mts-baseline.trx`) and NuGet advisory output (`nuget-vulnerability-report.json`) are retained outside this repository and are not stored in `docs`.
- **Not done:** live Entra sign-in, real SQL/IDT access, role/assignment export, container build, ACA deployment, replica/revision testing. The contents of the checked-in PII-review file were not read.
- **Reference limits:** Non-Subsidy documents that Azure deployment and production parity are unverified. Its tests use fake identities, signed offline tokens, TestServer and SQLite, which do not prove a live Entra exchange or Azure SQL behaviour. Its [AppCAT report][C14] predates its remediation, so it is not a post-remediation baseline.
- **Historical MTS material:** the October export describes .NET Framework/Silverlight/PeopleContext/EF.Audit. The current source is .NET 10 with C# application services under `SL/` and no evidence of those dependencies, so no tasks are created for them.

## 3. What the Non-Subsidy reference actually implements

| Area | Verified behaviour | Source |
|---|---|---|
| Sign-in | `AddMicrosoftIdentityWebApp`; OIDC `ResponseType = Code` (PKCE per its docs); Cookie default sign-in, OIDC default challenge. Client secret required in every environment (certificate/federated credentials unsupported). | [C03][C03], [C02][C02] |
| Tenant contract | Startup requires a specific tenant GUID (not `common`/`organizations`). After sign-in, exactly one `tid` claim must equal it, else the sign-in fails with a logged `TENANT_CONTRACT_REJECTED`. | [C03][C03] |
| Claims | Inbound claim mapping disabled on both the OIDC options and the token handler; `RoleClaimType = "roles"`, `NameClaimType` = configured audit claim. | [C03][C03], [C01][C01] |
| Authorization | `BasicAccess` policy = authenticated user + exact configured app-role value; fallback policy requires authentication; protected pages use `[Authorize(Policy=...)]`. Anonymous only for account status pages, error page and probes. | [C03][C03], pages |
| Audit identity | Exactly one non-blank configured claim or the request fails; no `Identity.Name`/email fallback. Lowercase, strip `earth\`, replace `m0` with `m`. Provided through a scoped `IUser`. | [C11][C11] |
| Cookie | `__Host-Cph.Auth`, HttpOnly, Secure, SameSite=Lax, 30-minute sliding expiry. Sign-out is an antiforgery-protected POST. | [C03][C03], [C01][C01] |
| Startup validation | Fails on missing/invalid tenant, client ID, authority (HTTPS only), secret, role value, audit claim, SQL connection string, wildcard/invalid `AllowedHosts`, invalid HTTPS port. | [C03][C03] |
| Proxy trust | Explicit proxies/CIDRs only, no wildcard/`/0`; forwarding disabled when none configured; `ForwardLimit = 1`; HTTPS redirect skipped for probes. | [C06][C06], [C13][C13] |
| Probes | `/health/live` runs no checks; `/health/ready` runs a bounded SQL `SELECT 1` (3 s connect/command) and logs a generic warning, never SQL error text. | [C13][C13], [C07][C07] |
| Telemetry | Standard `APPLICATIONINSIGHTS_CONNECTION_STRING` takes precedence over the nested key; optional UAMI for ingestion; an identity without a connection string fails startup. | [C08][C08] |
| Shared keys | Blob key ring wrapped by a Key Vault key through an explicit UAMI; validated HTTPS URIs; stable application name; incomplete/unknown settings fail startup. | [C04][C04] |
| Data access | EF Core with explicit existing-schema mappings; no startup migration or schema/user creation; DBA owns schema; managed-identity connection string. | [C01][C01], [C09][C09] |
| Container/IaC | Multi-stage .NET 10 Linux image, ICU kept, port 8080, non-root. Bicep foundation + application stages, single-revision mode, external HTTPS ingress, secrets by Key Vault reference, digest-pinned image, probes. | [C10][C10], [C09][C09], [C05][C05] |

**Reference limitations to design around:** one role only (no read-only/admin split, hierarchy or field-level rules); roles are not refreshed until the next sign-in; no absolute session cap or immediate revocation; Entra application role assignment is outside the app; local `DateTime.Now` audit timestamps (MTS uses UTC and must not copy this). The guide itself says to add policies/handler checks and tests when more roles are needed.

## 4. Fresh AppCAT results (ACA target)

| Field | Result |
|---|---|
| Tool | .NET AppCAT CLI `1.0.1127.58951`, report schema `1.0.0` |
| Window | `2026-11-03T12:00:41Z` to `2026-11-03T12:00:50Z`; Solution source, code analysis, Restricted privacy |
| Totals | 2 projects; production project 7 rules / 27 incidents / 69 points; test project 0 |
| Severity | Mandatory 2, Potential 19, Optional 6, Information 0 |
| Baseline | Release build 0 warnings / 0 errors; 42 tests passed; no vulnerable NuGet packages reported |

Disposition below uses each incident's `targets.ACA` rating and the Non-Subsidy pattern. "Expected" means expected to clear or change after remediation; confirm by re-running AppCAT.

| Rule | Severity | Incidents / points | Where | Disposition under the Non-Subsidy approach |
|---|---|---:|---|---|
| `Identity.0002` Windows authentication | Mandatory | 2 / 6 | [Program.cs][M01] | Replace IIS/Negotiate with Entra OIDC (T04). Expected to clear. |
| `Connection.0001` Connection string | Potential | 8 / 24 | [appsettings.json][M03] + SIT/UAT/Production | Remove committed hosts and Integrated Security; supply the app SQL string as external configuration with managed identity (T03, T10). The IDT string goes away if IDT is retired (T07). Some incident on a local-development settings file may remain by design. |
| `Configuration.0003` Local config | Potential | 6 / 6 | same four files | Environment variables / ACA secrets as in Non-Subsidy (T03). Azure App Configuration is not used by the reference and not required. Residual incidents on a retained settings file are expected. |
| `Cache.0001` Data caching | Potential | 1 / 3 | [Program.cs][M01] | `AddMemoryCache` exists for the IDT role cache. Expected to clear when IDT is retired (T07); otherwise document as replica-local, non-authoritative. No Redis requirement. |
| `Database.0003` System.Data.SqlClient | Optional | 5 / 15 | [IdtRoleClaimsTransformation][M04] | Clears with IDT retirement (T07). EF6's own provider is a separate path (T10). |
| `Local.0003` Local/network I/O | Potential | 4 / 12 | [ExportService.BulkData][M10] | **Not covered by the reference.** MTS-specific upload redesign (T15). |
| `Scale.0001` Static content | Optional | 1 / 3 | [csproj][M02] / `wwwroot` | Accept, as Non-Subsidy also serves static files from the container. Review old vendored libraries separately (T20). |

## 5. Gap analysis: Non-Subsidy versus MTS now

| ID | Priority | Area | MTS today (evidence) | Required to match the reference / ACA |
|---|---|---|---|---|
| G-01 | P0 | Sign-in | IIS or Kestrel Negotiate ([Program.cs][M01]) | Entra OIDC, cookie, account pages, no production bypass |
| G-02 | P0 | Role source | SQL IDT roles via `dbo.aspnet_UsersInRoles_GetRolesForUser`, app name `/`, five-minute in-memory cache ([M04][M04]) | Entra app-role claims + policies; IDT retired after approval and assignment migration |
| G-03 | P0 | Endpoint authorization | Only `Home.Index` declares a role ([M05][M05]); other controllers rely on authenticated-only fallback. Views gate links by `User.IsInRole` with the role label strings. | Server-side policy on every action and component, not just visible links |
| G-04 | P0 | Field-level rules | Scheme edit partials render different editable fields for Admin / Support / Inspector; BLS save checks Admin for last-inspection ([M08][M08]) | Server-side per-scheme field allowlist from an approved matrix |
| G-05 | P0 | Audit identity | Static `UserHelper` reads `Identity.Name`, lowercases it and applies the legacy replacements ([M12][M12]); a null name would throw before the "Unknown" fallback | Scoped user service over a validated, explicit claim; fail rather than record "Unknown" |
| G-06 | P0 | Antiforgery / verbs | Many admin POSTs and all scheme POSTs lack `[ValidateAntiForgeryToken]`/`[HttpPost]`; `CaclulateAllRiskScores` is a link-driven write ([M06][M06], [M08][M08]) | POST-only mutations with token validation, via a global filter where feasible |
| G-07 | P0 | SQL access | Two Integrated-Security strings; EF6 on `System.Data.SqlClient`; startup `DbMigrator` switch ([M11][M11], [M15][M15]) | Managed-identity connection; DBA-owned schema; no startup migration (decision D-04) |
| G-08 | P0 | Cookie keys | No shared Data Protection | Blob + Key Vault key ring via UAMI |
| G-09 | P0 | Upload | Client filename under app directory; workbook not disposed; first 3 sheets, row-by-row save ([M10][M10]) | Bounded, isolated, cleaned-up processing safe for non-root Linux |
| G-10 | P0 | Container/IaC | None in the repo | Image, ACA configuration, registry, identity and grants |
| G-11 | P1 | Proxy/hosts | No forwarded-header config; `AllowedHosts: "*"` | Explicit hosts, trusted proxy CIDR, redirect/probe handling |
| G-12 | P1 | Probes | None | Liveness (no dependencies) and bounded SQL readiness |
| G-13 | P1 | Telemetry | Registered only when `ApplicationInsights:ConnectionString` is set; package `3.2.0` | Standard key support, optional ingestion identity; Non-Subsidy uses SDK `2.23.0`, so the pattern is **not drop-in** (T13 verifies the 3.x API) |
| G-14 | P1 | Tests | 42 controller/service/model tests; no identity, cookie, key-ring or HTTP-pipeline tests | Signed-token, policy, forwarding, key-ring and TestServer coverage |
| G-15 | P2 | Static/CDN | Old vendored libraries and external CDN/font links in [_Layout][M17] | Inventory, license, egress and case-sensitivity review |

## 6. Authentication and authorization design

### 6.1 Three separate boundaries (as in Non-Subsidy)

1. **User authentication:** Entra validates the person; the app issues a protected cookie.
2. **Business authorization:** app roles in the validated `roles` claim.
3. **Resource access:** the UAMI reaches SQL, Key Vault, Blob and telemetry. It is never the signed-in user or the audit actor, and has no business roles.

### 6.2 Proposed Entra roles (proposals, not approved or created)

The current labels contain spaces and an en dash. Entra role **values** must be token-safe, and Non-Subsidy rejects spaces in the value, so labels cannot be copied as-is.

| Current IDT role | Proposed Entra value |
|---|---|
| `MTS Risk Analysis: Basic Access` | `MTS.BasicAccess` |
| `MTS Risk Analysis : General User – Support Team` | `MTS.SupportTeam` |
| `MTS Risk Analysis : General User – Inspector` | `MTS.Inspector` |
| `MTS Risk Analysis : Admin User` | `MTS.Admin` |

Define one named policy per capability, not per string, and keep the role-to-capability map in one place used by controllers, views and field filters.

### 6.3 Permission matrix to approve and enforce

| Surface | Current intent (evidence) | Required |
|---|---|---|
| Home, plant list/details | Home requires Basic Access; plant actions only authenticated | Approve whether higher roles imply Basic Access (Entra roles are flat; the old IDT hierarchy is not carried over). Enforce on direct URLs. |
| Plant/criteria/option/weighting admin | Support and Admin see different links; actions unrestricted beyond authentication | Approve Support versus Admin per action; enforce on every action. |
| BLS/BCC/PCG field edits | Admin, Support and Inspector see different enabled/disabled fields | Server-side allowlist per scheme and role; ignore forged fields. Disabled HTML is not a control. |
| SCC edits | Partial lets Basic, Support, Inspector or Admin edit | Confirm this broader behaviour explicitly; do not impose another scheme's matrix by accident. |
| Last-inspection date | Explicit Admin check in the BLS save only | Verify per scheme and on the import path. |
| Export (16 variants) | UI shows to Support or Admin | Enforce on the action for every variant. |
| Bulk import | UI shows to Admin; POST has antiforgery but no role check | Enforce policy on GET and POST. |
| Audit views | No explicit check | Approve who may view; enforce on list and detail. |
| Risk recalculation (single / all) | Link/redirect-driven writes | Protected POST or internal call; approve role; define concurrent-run behaviour. |
| Account/error/probes | Error route inherits authentication | Anonymous only where deliberate; no sign-in loop on probes or errors. |

### 6.4 Audit identity

Keep `Audit.Date` in **UTC**. Use a configured claim exactly as Non-Subsidy does (exactly one non-blank value, no fallback). Preserve the lowercase / `earth\` / `m0`→`m` normalization only if the identity owner confirms it, and check collisions against existing audit rows. A claim of the form `preferred_username` produces an email-style actor, not the legacy account name; a legacy value needs an owner-approved claim. The claim identifies the actor and grants no access.

### 6.5 Role assignment, revocation and sessions

Assign roles to users or groups on the enterprise application (group-based assignment needs suitable Entra licensing; nested groups do not cascade). As in Non-Subsidy, a cookie keeps its claims, so role changes apply after a **new sign-in**; there is no immediate revocation. The owner must set the session lifetime and an incident revocation procedure. This is weaker than a database role lookup, so the decision is explicit (D-01).

### 6.6 If IDT cannot be retired

Keeping SQL-backed IDT roles is **outside the Non-Subsidy reference**. It would add a Microsoft.Data.SqlClient-based role store, managed-identity access to the Security database, revocation/cache rules and outage behaviour, none of which the reference demonstrates. Treat it as a separate design, not an extension of this plan.

## 7. Task backlog (not started)

Size is a relative assessor judgement: S, M or L. Dependencies are prerequisites for implementation or acceptance, not an instruction to execute.

### Phase A: decisions and baseline

| Task | Pri / Size / Owner | Work and done criteria | Depends on |
|---|---|---|---|
| T00 Freeze baseline | P0 / S / App | Keep this AppCAT/build/test evidence. Capture synthetic characterization fixtures for the four schemes' risk scores, 16 exports, three-sheet import and audit rows. | None |
| T01 Approve identity model | P0 / S / Service + identity + security owners | Resolve D-01 to D-03 and D-05: Entra roles replace IDT, role values, flat versus implied roles, permission matrix, audit claim, session lifetime and revocation procedure. | T00 |

### Phase B: identity and authorization

| Task | Pri / Size / Owner | Work and done criteria | Depends on |
|---|---|---|---|
| T02 Entra registration, roles, assignments | P0 / M / Identity | Single-tenant web registration with exact HTTPS callbacks, client secret in Key Vault, app roles per 6.2, ID-token claim for the audit actor, and migration of current IDT user/role assignments to Entra users/groups using owner-approved, access-controlled extracts. Verify with approved test users; no tokens in logs. | T01 |
| T03 Fail-closed configuration | P0 / M / App | Required settings validated at startup as in Non-Subsidy: tenant GUID, client ID, HTTPS authority, secret, role values, audit claim, SQL string, explicit `AllowedHosts`, HTTPS port. Read secrets from environment, `/run/secrets` or ACA secrets. Remove environment-specific hosts and Integrated Security from committed files. Missing/invalid values stop the host. | T01 |
| T04 Entra sign-in and cookie | P0 / M / App | Replace IIS/Negotiate with Microsoft.Identity.Web OIDC (code + PKCE), `roles`-claim role type, tenant-claim check, `__Host-` HttpOnly Secure cookie, antiforgery-protected POST sign-out, sign-in/signed-out/access-denied/authentication-failed pages, generic failures and event-coded logging. No Windows or anonymous production fallback. | T02, T03 |
| T05 Role policies and constants | P0 / M / App | Single role-value/policy definition. Replace the label strings in the controllers and the 18 `User.IsInRole` checks in views with policy-based checks. Add `[Authorize(Policy=...)]` to every controller/action per 6.3; decide anonymous exceptions explicitly. | T01, T04 |
| T06 Scoped audit identity | P0 / S / App | Replace static `UserHelper` with a scoped user service over the validated claim (Non-Subsidy `AuditIdentity` pattern); keep UTC; missing/duplicate/blank claim fails rather than writing "Unknown"; fix the null-name path. | T04 |
| T07 Retire IDT | P0 / S / App + Security DB | Remove the role claims transformation, `IDTSecurityConnection`, the role cache registration and `System.Data.SqlClient` usage once T05 passes. Do not alter the shared Security database. | T05, T06 |
| T08 Field-level scheme permissions | P0 / L / App + business owner | Server-side allowlist per scheme and role, reloaded from stored data so forged or disabled fields are ignored. Cover BLS, BCC, PCG and SCC, last-inspection dates and the import path. | T01, T05 |
| T09 Antiforgery and safe verbs | P0 / M / App | Global antiforgery validation for unsafe verbs; `[HttpPost]` on all mutations; replace link-driven risk recalculation with protected POST or an internal call. Missing or invalid tokens are rejected; invalid-post redisplay preserved. | T05 |

### Phase C: data, runtime and hosting

| Task | Pri / Size / Owner | Work and done criteria | Depends on |
|---|---|---|---|
| T10 SQL managed identity and schema ownership | P0 / L / App + DBA | Apply D-04. **Non-Subsidy-aligned path:** EF Core with explicit existing-schema mappings and Microsoft.Data.SqlClient. Needs explicit table mapping (existing tables are EF6-pluralized, for example `Plants`, `Options`), explicit loading or includes (the code base relies heavily on navigation properties), explicit `SetModified`/tracking behaviour, validation on save, and EF6 migrations excluded from execution. **Alternative:** retain EF6 with a Microsoft.Data.SqlClient provider, which the reference does not cover and must be proven. Either way: no startup migration in ACA, schema/history delivered by the DBA, UAMI contained database user with minimum CRUD grants, `Authentication=Active Directory Managed Identity` with encryption and certificate validation. Business outputs match T00 fixtures. | T00, T01 |
| T11 Proxy, HTTPS, hosts | P1 / S / App + platform | Reuse the Non-Subsidy trusted-forwarding rules: explicit proxy/CIDR, no wildcard, one hop, disabled when unset; forwarding before redirects/auth; HTTPS redirect excluded for probes; explicit `AllowedHosts` and HTTPS port. Verify the real ACA peer lies in the configured range. | T03, T04 |
| T12 Probes | P1 / S / App + DBA | Anonymous `/health/live` with no checks; `/health/ready` with a bounded SQL `SELECT 1` (3 s timeouts), 503 on failure, generic log, no SQL error text. | T10 |
| T13 Telemetry | P1 / S / App + ops | Prefer `APPLICATIONINSIGHTS_CONNECTION_STRING`, fall back to the nested key; optional UAMI ingestion; identity without connection string fails startup. **Verify against package 3.2.0**: Non-Subsidy uses 2.23.0 and `SetAzureTokenCredential`; do not assume the code ports unchanged. Keep personal data, tokens and workbook content out of telemetry; agree sampling and probe filtering. | T03 |
| T14 Shared Data Protection | P0 / M / App + platform | Blob key ring wrapped by a Key Vault key through the explicit UAMI; validated URIs; stable per-environment application name; incomplete settings fail startup. Cookies, antiforgery and TempData survive restarts, replicas and revisions; old wrapping-key versions stay recoverable. | T03, T04 |
| T15 Upload redesign | P0 / M / App + business owner | Parse the workbook once from a bounded stream, or use unique temp paths outside the app directory; dispose workbook/streams; clean up on success and failure; ignore client-supplied path parts. Validate size, sheet names, headers and row/cell bounds before applying data. Preserve three-sheet order and current partial-success error list unless separately approved. | T00, T03 |
| T16 Export and culture characterization | P1 / S / App | Verify all 16 exports on Linux for column order, quoting, encoding/BOM, newline, en-GB date/number culture (ICU kept in the image) and downstream consumers. No format change without approval. | T00 |

### Phase D: container, platform and delivery

| Task | Pri / Size / Owner | Work and done criteria | Depends on |
|---|---|---|---|
| T17 Container image | P0 / M / App + platform | Multi-stage .NET 10 Linux image following the Non-Subsidy Dockerfile shape: restore layer, publish, non-root user, `ASPNETCORE_HTTP_PORTS=8080`, ICU enabled. Add `.dockerignore` excluding uploads, review artifacts, bin/obj and secrets. Check published asset paths are case-correct. Record digest and scan results. | T03 to T16 |
| T18 Platform foundation and activation | P0 / M / Platform + DBA + identity | Following the Non-Subsidy Bicep split (foundation, key bootstrap, activation): network, ACR, UAMI, Key Vault, key-ring Blob, SQL, monitoring, ACA environment, single revision, ingress choice, probes, digest-pinned image, secret references. Parameters are supplied by the receiving team, with no region, SKU or network range assumed from Non-Subsidy. Grants verified: SQL contained user, Blob data access, key wrap/unwrap, ACR pull. | T01, T10 to T14 |
| T19 Delivery pipeline | P1 / M / Platform + App | Build, test, scan and promote an immutable digest with smoke gates and rollback. No pipeline exists for MTS today; reuse the team's standard if one is supplied. | T17, T18 |

### Phase E: verification and handover

| Task | Pri / Size / Owner | Work and done criteria | Depends on |
|---|---|---|---|
| T20 Automated tests | P0 / L / App + QA | See section 9. Preserve the 42 existing tests; add the identity, authorization, key-ring, forwarding, upload and pipeline suites. | T04 to T16 |
| T21 Deployed acceptance and AppCAT re-run | P0 / M / QA + owners | Execute section 9 acceptance in an approved environment; re-run AppCAT on remediated code; disposition each remaining incident; record real-tenant results. | T18 to T20 |
| T22 Operations handover | P1 / M / Ops + service owner | Role and claim runbook, assignment and removal, revocation procedure, secret and key rotation, import-failure recovery, RTO/RPO, rollback, alert ownership. Replace the placeholder README with verified commands and limitations. | T21 |
| T23 Optional hardening | P2 / M / App | Vendored library and CDN inventory/licensing/egress, paging and export memory under measured load, static asset placement. Release-blocking findings move into T21; no business formula changes. | T00 |

Parallel streams after T01: identity (T02 to T09), data (T10, T12), runtime (T11, T13, T14), MTS-specific (T08, T15, T16). Estimate delivery only after the D-decisions and representative SQL/workbook fixtures exist.

## 8. Proposed configuration surface

Names follow Non-Subsidy (`__` for `:` in environment variables). They are not existing MTS settings.

| Key | Contract |
|---|---|
| `AzureAd__Instance`, `TenantId`, `ClientId`, `ClientSecret` | HTTPS authority, specific tenant GUID, client ID, secret from Key Vault/ACA secret reference |
| `Authorization__*` | Approved role values and capability mapping (several roles, replacing the single `BasicAccessRole`) |
| `Identity__AuditNameClaimType` | Explicit audit claim; no fallback |
| `ConnectionStrings__MTSInspectionsContext` | Azure SQL, managed identity, `Encrypt=True`, `TrustServerCertificate=False` |
| `AllowedHosts` | Explicit semicolon-separated hosts |
| `ReverseProxy__KnownProxies__n` / `KnownIPNetworks__n`, `HttpsRedirection__HttpsPort` | Platform-approved proxy trust and external HTTPS port |
| `DataProtection__Enabled`, `BlobUri`, `KeyIdentifier`, `ApplicationName`, `ManagedIdentityClientId` | Shared key ring contract |
| `APPLICATIONINSIGHTS_CONNECTION_STRING`, `Telemetry__ManagedIdentityClientId` | Optional telemetry with identity ingestion |
| Upload limits / temp location | Owner-approved bounds; ephemeral temp only unless retention is approved |

`ApplyMigrationsOnStartup` stays false/absent in ACA, and `IDTSecurityConnection` is removed with IDT. Keep Azure RBAC, Entra role assignments and SQL grants separate; the UAMI holds no business roles.

## 9. Validation gates

**Baseline (done):** AppCAT 7 / 27 / 69; Release build 0 warnings, 0 errors; 42 tests passed; no vulnerable NuGet packages reported by the configured feed query (not a browser-asset or container-image assessment).

**Automated tests required (mirroring Non-Subsidy's categories):**
- Startup configuration: every missing/invalid value fails; no production auth bypass.
- Signed-token authentication: valid role; missing/wrong role (403); foreign or duplicate tenant; missing/duplicate/blank audit claim; raw claim types preserved.
- Every row of 6.3, for all four schemes, including direct requests and forged privileged fields; GET mutation rejected; missing antiforgery rejected.
- Audit identity and UTC timestamp saved correctly; invalid saves do not stamp or commit.
- Trusted forwarding with allowed and untrusted peers; probes do not redirect or challenge.
- Shared key ring: cookie/antiforgery/TempData round trip across two hosts and across restart.
- Upload: valid fixture, malformed workbook, missing headers, oversize, concurrent same-name files, cancellation; partial-success contract preserved.
- All 16 exports against approved synthetic fixtures; risk calculations unchanged.
- Data layer against the chosen provider with SQL-backed synthetic data (SQLite alone does not prove SQL Server behaviour).

**Deployed acceptance (approved environment):**
1. Real Entra sign-in/out, callbacks over the real proxy path, role-assigned and unassigned users.
2. UAMI reaches only the required resources; SQL contained-user grants work.
3. Linux non-root image serves assets and processes workbook/CSV contracts with en-GB behaviour.
4. SQL outage turns readiness unhealthy and keeps liveness healthy, then recovers without restart loops.
5. Two replicas and a revision replacement share cookies and observe the agreed role-revocation procedure.
6. Largest approved workbook, export and all-score recalculation meet owner-agreed limits (none supplied yet; none assumed).
7. Rollback, key recovery, secret rotation and import-failure recovery rehearsed; alerts reach the on-call team.
8. Re-run AppCAT; no unaccepted Mandatory finding. Residual local-settings, static-asset or telemetry incidents are documented, not forced to zero.

## 10. Decisions required before execution

| ID | Decision | Owner | Why it gates work |
|---|---|---|---|
| D-01 | Retire SQL IDT roles in favour of Entra app roles, and accept cookie-bound role refresh (no immediate revocation) | Service, identity, security | The reference has no SQL role store; this removes the shared-database dependency but changes revocation behaviour. |
| D-02 | Role values, flat versus implied roles, and migration of existing assignments | Identity + Security DB owner | IDT inheritance is resolved in the database; Entra roles are flat. |
| D-03 | Permission matrix (6.3), especially Support versus Admin and SCC editing | Business + security | Prevents accidental privilege change. |
| D-04 | EF Core conversion (Non-Subsidy path) versus retaining EF6 with a new provider | App + DBA | EF Core is a large change here (explicit table names, navigation loading); retention needs proof the EF6/provider path supports managed identity. |
| D-05 | Audit claim and legacy-name continuity | Identity + audit owner | A valid Entra login does not equal the legacy account name. |
| D-06 | Session lifetime and incident revocation procedure | Security | Reference has a 30-minute sliding cookie and no absolute cap. |
| D-07 | Upload limits, partial-success contract and CSV downstream compatibility | Business + operations | Defines safe parsing and recovery. |
| D-08 | Ingress, environment/region, sizing, pipeline ownership, RTO/RPO, retention | Platform + service owner | Not inferable from Non-Subsidy's examples. |

**Recommendation:** proceed to a separately approved remediation phase using this backlog. Do not deploy the current source to ACA, and do not treat authentication as complete because the build and 42 tests pass.

## Evidence links

Links M04 and M12 pointed to `IdtRoleClaimsTransformation.cs` and `UserHelper.cs`, which the remediation deleted; they now open the remediation summary, which records what replaced them. Other source links show the code as it is now, not as it was when assessed.

[C01]: ../../rpa-cph-non-subsidy/docs/developer-guide.md
[C02]: ../../rpa-cph-non-subsidy/docs/operator-guide.md
[C03]: ../../rpa-cph-non-subsidy/RPA.CPH.NonSubsidy.Domain/RPA.CPH.NonSubsidy.Web/Services/ApplicationServices.cs
[C04]: ../../rpa-cph-non-subsidy/RPA.CPH.NonSubsidy.Domain/RPA.CPH.NonSubsidy.Web/Services/SharedDataProtection.cs
[C05]: ../../rpa-cph-non-subsidy/infra/README.md
[C06]: ../../rpa-cph-non-subsidy/RPA.CPH.NonSubsidy.Domain/RPA.CPH.NonSubsidy.Web/Services/TrustedForwarding.cs
[C07]: ../../rpa-cph-non-subsidy/RPA.CPH.NonSubsidy.Domain/RPA.CPH.NonSubsidy.Web/Services/SqlReadinessHealthCheck.cs
[C08]: ../../rpa-cph-non-subsidy/RPA.CPH.NonSubsidy.Domain/RPA.CPH.NonSubsidy.Web/Services/ApplicationInsightsRegistration.cs
[C09]: ../../rpa-cph-non-subsidy/infra/app.bicep
[C10]: ../../rpa-cph-non-subsidy/Dockerfile
[C11]: ../../rpa-cph-non-subsidy/RPA.CPH.NonSubsidy.Domain/RPA.CPH.NonSubsidy.Web/Services/AuditIdentity.cs
[C12]: ../../rpa-cph-non-subsidy/README.md
[C13]: ../../rpa-cph-non-subsidy/RPA.CPH.NonSubsidy.Domain/RPA.CPH.NonSubsidy.Web/Program.cs
[C14]: ../../rpa-cph-non-subsidy/docs/Report_20261006101509_20261006102206.md
[M01]: ../RPA.MTSInspections/Program.cs
[M02]: ../RPA.MTSInspections/RPA.MTSInspections.csproj
[M03]: ../RPA.MTSInspections/appsettings.json
[M04]: ./MTS_Inspections_ACA_Remediation_Summary.md
[M05]: ../RPA.MTSInspections/Controllers/HomeController.cs
[M06]: ../RPA.MTSInspections/Controllers/AdminController.cs
[M08]: ../RPA.MTSInspections/Controllers/SchemeController.cs
[M10]: ../RPA.MTSInspections/SL/ExportService.cs
[M11]: ../RPA.MTSInspections/DAL/MTSInspectionsDbConfiguration.cs
[M12]: ./MTS_Inspections_ACA_Remediation_Summary.md
[M15]: ../RPA.MTSInspections/Migrations/Configuration.cs
[M17]: ../RPA.MTSInspections/Views/Shared/_Layout.cshtml
