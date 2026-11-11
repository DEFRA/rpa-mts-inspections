# Operator guide: RPA MTS Inspections on Azure Container Apps

For the teams that deploy and support the application as a Linux container on Azure Container Apps (ACA). The
[README](../README.md) covers configuration, build and local running; the [infra README](../infra/README.md) covers
the Bicep templates; [MTS_Inspections_ACA_Remediation_Summary.md](./MTS_Inspections_ACA_Remediation_Summary.md)
records what was changed and what was verified.

**Status:** a runbook for the receiving team's deployment and acceptance. **Nothing here has been run against Azure.**
The commands are for the receiving team to run with approved values; check each against your own environment.

## 1. Who owns what

| Owner | Responsibilities |
|---|---|
| Application team | Image, application settings contract, regression tests, business smoke tests. |
| Platform and operations | ACA, registry, networking and private DNS, deployment permissions, monitoring and alerts, image retention, runtime identity attachment. |
| Identity team | Entra app registration, app roles, user and group assignments, the audit claim, callbacks, client-secret lifecycle. |
| DBA | Schema and `__MigrationHistory` delivery, the runtime identity's database user and grants, backup and restore, compatibility. |
| Security and service owner | Role and permission approval, access reviews, telemetry privacy, incident handling, RTO and RPO, release acceptance. |

Agree named owners, rollback authority, RTO and RPO before go-live. Nothing in the repository sets them.

## 2. How the pieces fit

| Concern | Mechanism | Owner |
|---|---|---|
| Who the user is | Entra sign-in (authorization code with PKCE). HTTPS-only `__Host-Mts.Auth` cookie, 30-minute sliding. | Identity |
| What the user may do | Entra **app roles** in the `roles` claim: `MTS.BasicAccess`, `MTS.SupportTeam`, `MTS.Inspector`, `MTS.Admin` (configurable). Flat: assign every role a person needs. | Identity and service owner |
| Who is recorded in audit rows | One configured claim (`Identity__AuditNameClaimType`), lower-cased, `earth\` removed, `m0` to `m`. | Identity |
| How the app reaches Azure | One user-assigned managed identity (UAMI): SQL, Key Vault key wrapping, Blob key ring, telemetry ingestion, registry pull. No passwords or keys. | Platform |
| Cookie and antiforgery keys | Shared Data Protection key ring in Blob Storage, wrapped by a Key Vault key. | Platform |
| Schema | Never changed by the application. Released by the DBA or a pipeline run. | DBA |

Keep three permissions apart: an **app role** lets a person use the application; **Azure RBAC and database grants** let
the runtime identity reach resources; **Entra administrator rights** let someone manage the registration. Owning an
Azure subscription does not grant any of the others.

## 3. First deployment

Follow the [infra README](../infra/README.md) for commands and parameter validation. The order matters:

1. **Foundation** (`infra/main.bicep`): network, registry, identity, Key Vault, key-ring storage, SQL, monitoring, ACA environment. No application starts.
2. **Private access:** confirm operators and the workload can resolve and reach SQL, Blob and Key Vault. A private endpoint is not reachable from a laptop or Cloud Shell by default.
3. **Key bootstrap:** create the wrapping key through the private path with `infra/Initialize-DataProtectionKey.ps1`.
4. **Entra registration:** the identity team runs `infra/entra/New-MtsEntraRegistration.ps1` (plan mode first, then `-Apply`), then creates a client secret and stores its **value** in the Key Vault secret named by `entraClientSecretName`.
5. **Database:** the DBA delivers the schema (section 5) and runs `infra/Grant-DatabaseAccess.sql` for the runtime identity.
6. **Image:** build and push (the pipeline, or `docker build` and `az acr` by hand) and note the immutable `sha256:` digest.
7. **Activation** (`infra/app.bicep`): deploy with the digest. Verify image pull, secret resolution and role propagation rather than trusting a fixed delay.
8. **Entra assignments:** assign users and groups to the roles (section 4), then run the acceptance checklist (section 10).

The application does not create schema, database users or the wrapping key.

## 4. Entra setup, roles and audit claim

### 4.1 Registration

`infra/entra/New-MtsEntraRegistration.ps1` creates a single-tenant web registration with the four roles, the
`/signin-oidc` redirect and the logout URL, with implicit grant disabled. Confirm in **Entra ID, App registrations**
that the tenant and client ID match `AzureAd__TenantId` and `AzureAd__ClientId`. The script does not create a secret
and does not assign anyone to a role.

The role **values** (`MTS.Admin` and so on) must match `Authorization__Roles__*`, including case. Spaces belong in a
display name, never in a value. Do not use role GUIDs, group names or the policy names used in code.

### 4.2 Assigning people

Under **Enterprise applications, your application, Users and groups, Add user/group**, choose the people or security
group and the role. Creating a role does not assign it, and **Default Access** gives no role. Group assignment needs
suitable Entra licensing and nested groups do not cascade. The application reads the `roles` claim, so no `groups`
claim is needed.

| Role | Grants |
|---|---|
| `MTS.BasicAccess` | Home, plant list and details; SCC scheme edits. |
| `MTS.SupportTeam` | Plant administration, audit history, CSV export; read-only scheme view. |
| `MTS.Inspector` | BLS, BCC and PCG edits, except the inspection-history fields; SCC edits. |
| `MTS.Admin` | Criteria, option and weighting administration; bulk import; recalculating all risk scores; all scheme fields including the last inspection date; plus everything above. |

The IDT role hierarchy is not carried over. A person who needs Support and Inspector abilities needs both roles.
The permission matrix is awaiting business-owner approval; do not widen or narrow a role in Entra without it.

### 4.3 The audit claim

`Identity__AuditNameClaimType` names a claim in the **ID token**. It must be present exactly once with a non-blank value
for every permitted user, or sign-in is rejected even when the role is correct. With `preferred_username` a user is
audited as `operator@example.invalid`, not as their old account name. If audit continuity with historical rows is
required, the identity team must supply the legacy value through an approved claim (the registration script's
`-AddUpnClaim` adds `upn`; custom attributes need a separate approved claims-mapping policy) and verify real samples.
The claim identifies the actor and grants no access. Check collisions against existing audit rows before go-live.

### 4.4 Verifying and changing access

- After any change to assignments or claims, the user must **sign out and sign in again**. Renewing a cookie does not fetch new roles.
- With approved test users: each role reaches its pages; a user with no role gets the Access denied page (403); a missing audit claim rejects sign-in.
- Never paste live tokens into public decoders, logs, tickets or screenshots.
- Warning event 1001 carries a failure code: `AUDIT_CONTRACT_REJECTED` (audit claim), `TENANT_CONTRACT_REJECTED` (tenant), `OIDC_REMOTE_FAILURE` (the Entra round trip). A 403 is a role question, not a sign-in failure.

### 4.5 Removing access, and urgent revocation

Remove the user or group assignment. **Existing cookies keep their roles for up to 30 minutes of inactivity, and
longer with activity**, because cookies renew while in use and there is no absolute session cap. Removal is therefore
not instant. For an urgent case the service owner must agree a procedure in advance. Options to evaluate, none tested
here:

- Wait out the cookie lifetime for a low-risk case.
- Invalidate every session by changing `DataProtection__ApplicationName` (all users, antiforgery tokens and TempData are affected and everyone must sign in again).
- Remove the key ring blob so new keys are generated (same effect; the old keys stay in blob versions and soft-delete).

## 5. Releasing the schema

The application never changes the schema, and refuses `ApplyMigrationsOnStartup=true` outside Development. The DBA owns it.

| Task | How |
|---|---|
| Review the SQL | `dotnet RPA.MTSInspections.dll --script schema.sql` with `ConnectionStrings__MTSInspectionsContext` pointing at an **empty scratch** database. It writes the SQL of every migration. A copy of that script was checked against the migrations on SQL Server LocalDB in the tests. |
| Apply pending migrations | `dotnet RPA.MTSInspections.dll --migrate` with a connection string whose identity may change the schema (never the runtime identity). Repeating it applies nothing. The pipeline exposes this as the `runSchemaMigration` parameter. |
| Existing database | Confirm `__MigrationHistory` matches before trusting `--migrate`; it applies only what history says is missing. The orphan migration `201803161004159_plantnamereq` is excluded from the build. |
| Seed data | `SCC-Seed-Data.sql` in the repository is not run by anything. The DBA decides whether and how to load it. |

Take a backup or confirm point-in-time restore before a schema release.

## 6. Configuration, secrets and keys

All settings are in the [README](../README.md#configuration). The application refuses to start on missing or invalid
required values; read the console log for the setting name (it never prints secret values).

| Item | Maintenance |
|---|---|
| Entra client secret | Rotate in Entra, store the new value as a new version of the Key Vault secret, then restart or create a new revision. ACA resolves the versionless reference, so confirm the revision picked up the new version. Remove the old secret afterwards. Record its expiry date and alert before it. |
| Data Protection wrapping key | Rotation is a policy of the Key Vault key. Keep all previous versions, because the key ring still needs them to decrypt. The application uses a versionless URI so new keys use the current version. Do not purge old versions. |
| Key-ring blob | Versioning and soft delete are on. Do not delete or edit `keys.xml`; losing it signs everyone out and invalidates antiforgery tokens. |
| `DataProtection__ApplicationName` | Must stay identical within an environment and differ between environments. Changing it signs everyone out. |
| Runtime identity | It is a separate resource. If it is deleted and recreated, its client ID changes: update SQL grants, Blob and Key Vault roles, and every setting that names it. |

## 7. Health, logs and diagnosis

| Signal | Meaning and limit |
|---|---|
| `/health/live` | HTTP 200: the process responds. Checks nothing else, by design, so a database outage cannot cause a restart loop. |
| `/health/ready` | Bounded SQL `SELECT 1` (3-second timeouts): 200 or 503. Does not check tables, grants or Entra. |
| ACA system logs | Image pull, revision provisioning, probes, secret resolution. |
| Console logs | Start-up validation errors, authentication rejections (event 1001), readiness failures (event 1002). |
| Application Insights | Requests, dependencies, exceptions, traces, subject to sampling. Missing telemetry is not proof of health. |

Probes call HTTP 8080 with an allowed `Host` header, every 10 seconds, three failures to act: liveness 3-second timeout,
readiness 5. Keep these consistent with the SQL check's own 3-second limits if you change any of them.

| Symptom | Likely cause and first check |
|---|---|
| Revision will not start | Console log: a named required setting is missing or invalid. Also a `ConnectionStrings__IDTSecurityConnection` setting left over from the old design is rejected. |
| Everyone redirected to sign-in repeatedly, or antiforgery errors across replicas | Key ring not shared or not reachable: check `DataProtection__*` and the identity's Blob and Key Vault roles. |
| Sign-in fails with the "Sign-in failed" page | Event 1001 code; check callbacks match the real origin and proxy path, the tenant, and the audit claim. |
| Signed in but "Access denied" | The user lacks the role, or signed in before it was assigned. Sign out and in again. |
| `/health/ready` is 503 | Database unreachable or the identity has no grant: check private DNS, firewall, the contained user (`Grant-DatabaseAccess.sql`) and the SQL console log entry (no details are exposed by the endpoint). |
| Redirect loops or `http` links | The proxy is not in `ReverseProxy__*`. Check the real ACA peer lies in the configured range; never set it to trust everything. |
| Cannot reach the app from an unexpected host name | `AllowedHosts` lists only the approved hosts. |

## 8. Bulk import and export

- The Admin-only import reads a workbook with **three worksheets named after the schemes** (BCC, PCG and BLS; SCC is not part of the import). It rejects an unreadable file, a file over `Upload__MaxFileBytes`, a worksheet over `Upload__MaxRowsPerSheet` rows, or fewer than three sheets, with an error row, before applying anything.
- Rows are saved one at a time. A row that fails validation is reported and the rest still load; **there is no rollback of rows already saved.** After a partial failure, correct the reported rows and load only those (re-loading the whole file is safe for unchanged values but writes audit rows only for changes).
- Nothing is written to disk, so a replica restart during an upload loses only that request. Re-upload.
- The 16 CSV exports keep their column order, quoting and Windows line endings. Known issue: **PCG exports 11 and 12 have their active filters swapped relative to their labels** (existing behaviour, awaiting owner approval).

## 9. Rollback, backup and recovery

| Situation | Action |
|---|---|
| Bad release | Deploy the previous image digest with `infra/app.bicep` (or activate the previous revision). Keep the last known-good digest and settings. |
| Bad schema release | Restore from point-in-time backup or the DBA's rollback script; the application code does not undo migrations. Agree the approach before the release. |
| Lost key ring | Restore the blob from versions or soft delete. If it is unrecoverable, new keys are generated and users must sign in again. |
| Lost wrapping key | Restore from the vault's soft delete or purge protection. Without it the key ring cannot be decrypted (everyone signs in again), and nothing in the database is affected. |
| Secret exposure | Rotate the Entra client secret, then follow the urgent revocation section if sessions may be compromised. |

RTO, RPO, backup retention and geo-redundancy are inputs to the Bicep templates and have not been chosen.

## 10. Acceptance checklist (deployed environment)

Use approved test data and users. Do not treat successful provisioning or a healthy readiness probe as business acceptance.

- [ ] Image digest, Production environment, resource size and replica bounds are the intended ones.
- [ ] Hostnames, `AllowedHosts`, HTTPS origin and Entra callbacks match; the real proxy is inside the trusted range; untrusted forwarded headers change nothing.
- [ ] `/health/live` and `/health/ready` pass; stopping SQL access turns readiness unhealthy while liveness stays healthy, then recovers without restart loops.
- [ ] Real Entra sign-in and sign-out work for each role and for a user with no role (403 page).
- [ ] The audit claim is present and the audit actor stored matches what the identity owner agreed.
- [ ] Each role reaches exactly its pages; an Inspector cannot change the inspection-history fields or the last inspection date by crafting a request (see the tests for the cases).
- [ ] The runtime identity reaches only SQL, Blob, Key Vault and telemetry; it has `db_datareader` and `db_datawriter` and no more.
- [ ] Cookies survive restart, a second replica and a revision replacement; antiforgery tokens issued by one replica validate on another.
- [ ] Bulk import: valid workbook, malformed file, wrong sheet count, oversize file; error rows are shown.
- [ ] All 16 exports against approved data match what downstream consumers expect (see the golden files in the tests); the date format is day/month/year.
- [ ] The largest approved import, export and "Calculate all risk scores" finish within the limits the owners agree. **No limits have been supplied; none are assumed.**
- [ ] Telemetry arrives without unacceptable personal data; alerts reach the on-call team; secret and key expiry alerts exist.
- [ ] Rollback to the last known-good digest and recovery of the key ring and database have been rehearsed.
- [ ] Re-run AppCAT for the `ACA` target on the released code and record a disposition for every remaining incident.

## 11. Known limits

- Roles refresh only at sign-in; sessions are not revoked instantly (section 4.5).
- There is no absolute session cap. The 30-minute lifetime is sliding.
- Plant and audit lists load full result sets before paging; measure with production-sized data.
- Old vendored front-end libraries are listed in [Client-Libraries.md](./Client-Libraries.md).
- Live Entra sign-in, Azure SQL with a managed identity, the Blob and Key Vault key ring and telemetry ingestion have **not** been exercised by the project; the acceptance checklist is how they are verified.
