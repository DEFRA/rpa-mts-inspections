# RPA MTS Inspections — Functionality-First Migration Peer Review

Detailed findings for the blocking and high-priority issues identified in the functional-equivalence
review of the legacy-to-.NET 10 / Azure migration. Legacy baseline is commit `608e36b`; migrated code is
the current working tree on top of `92e2fff`.

**Recommendation: No-Go now.** After F-01 and F-02 are fixed and F-03 to F-06 are closed, this can become
a Conditional Go for UAT.

---

## F-01 — Production, UAT and SIT fail at startup (Critical, P0)

**Root cause:** a leftover legacy connection-string key in the shipped environment config files, combined
with a deliberate fail-fast guard against it.

**The guard** — [ApplicationServices.cs](../RPA.MTSInspections/Services/ApplicationServices.cs), inside
`ValidateConfiguration` (called synchronously from `AddMtsApplication`, which runs during service
registration in `Program.cs`, before `builder.Build()`):

```csharp
if (!string.IsNullOrWhiteSpace(configuration["ConnectionStrings:IDTSecurityConnection"]))
    throw new InvalidOperationException("ConnectionStrings:IDTSecurityConnection is obsolete: roles now come from Entra app roles. Remove the setting.");
```

**The trigger** — the key is still present in every non-Development environment file:

- `appsettings.Production.json`: `"IDTSecurityConnection": "Data Source=D3VMPRWSQL003; Initial Catalog=Security;Integrated Security=True;Pooling=False;"`
- `appsettings.UAT.json`: same pattern, `D3VMPPWSQL003`
- `appsettings.SIT.json`: same pattern, `d3vmdvwdev002`

**Why it isn't neutralized at deploy time** — [app.bicep](../infra/app.bicep) only sets a container env
var for `ConnectionStrings__MTSInspectionsContext`. It never sets or blanks
`ConnectionStrings__IDTSecurityConnection`, so the value baked into `appsettings.Production.json` (which
`ASPNETCORE_ENVIRONMENT=Production` loads, per the Dockerfile) survives untouched and trips the throw on
every container start — i.e., a permanent crash loop in every deployed environment as currently
configured.

**Confirmed by running the tests**, not just by reading code: on a disposable copy of the repo,
`dotnet test` produced 67 failures with the identical message
`System.InvalidOperationException : ConnectionStrings:IDTSecurityConnection is obsolete: roles now come
from Entra app roles. Remove the setting.` — these come from every test that boots the app via
`MtsWebApplicationFactory` (which runs `Program` for real in `Production` mode), i.e. the entire
`PipelineTests` and `StaticAssetTests` suites.

**Fix:** remove `IDTSecurityConnection` from all three environment JSON files (and from any pipeline
variable group that sets it), and delete the now-dead `IdtRoleClaimsTransformation.cs` and
`UserHelper.cs` classes that still reference it, so there's no path back in.

---

## F-02 — Test suite is red and the "frozen" CSV baseline doesn't exist (High, P1)

I ran `dotnet test RPA.MTSInspections.sln -c Debug --filter "Category!=SqlServer"` on a throwaway copy:
**265 run, 182 passed, 83 failed.**

**Failure breakdown by cause:**

| Cause | Count | Detail |
|---|---|---|
| F-01 boot failure (`IDTSecurityConnection is obsolete`) | 67 | Every `PipelineTests`/`StaticAssetTests` case that uses `MtsWebApplicationFactory` — includes `Each_page_requires_its_role` (17), `A_post_without_an_antiforgery_token_is_rejected` (9), `A_post_without_the_required_role_is_forbidden_before_anything_else` (8), `Every_file_a_stylesheet_points_to_exists_and_is_local` (4), `Existing_forms_carry_an_antiforgery_token` (4), `Anonymous_requests_are_challenged` (4), plus ~20 single-case tests (sign-out, HTTPS redirect, probes, host filtering, forwarded headers, etc.) |
| Missing golden CSV files | 16 | `ExportCharacterizationTests.Export_matches_the_frozen_output(1)` through `(16)`, all failing with `Missing golden file for export N` |

**Why the golden files are missing:**
[RPA.MTSInspections.Tests.csproj](../RPA.MTSInspections.Tests/RPA.MTSInspections.Tests.csproj) declares:

```xml
<None Include="Golden\**" CopyToOutputDirectory="PreserveNewest" />
```

but `RPA.MTSInspections.Tests\Golden\` does not exist anywhere in the repo (`Test-Path` returns `False`).
[ExportCharacterizationTests.cs](../RPA.MTSInspections.Tests/SL/ExportCharacterizationTests.cs) confirms
the folder is expected (`GoldenPath` builds `.../Golden/export-{option:D2}.csv`) and even has a dedicated
regeneration test gated behind an `MTS_GOLDEN_OUT` environment variable — but nobody ran it and committed
the output.

**Why this matters beyond "a test is red":** the documentation directly contradicts what's in the repo.
[MTS_Inspections_ACA_NonSubsidy_Change_List.md](./MTS_Inspections_ACA_NonSubsidy_Change_List.md) states:

- Step 0a/T00: *"Done: baseline evidence kept; golden files freeze all 16 CSV exports"*
- Step 11/T16: *"Done: CRLF pinned, all 16 exports frozen"*
- Step 19/T20: *"Done for code: 276 tests (265 also pass on Linux)"*

None of that is true in this tree — the suite isn't green, and the CSV compatibility baseline (the thing
that is supposed to prove the 16 export formats are byte-for-byte unchanged for downstream consumers)
simply isn't there to check against. The same document separately admits *"downstream consumers not
consulted"*, so there's no fallback evidence either.

**Also structurally important:** [azure-pipelines.yml](../azure-pipelines.yml) runs `dotnet test` as a
required step in the `Build` stage before `Package`/`Deploy`. As committed, this pipeline cannot pass its
own Build stage.

**Fix:** regenerate `Golden/export-*.csv` from the **legacy** application's actual output (not the
migrated one — that would just enshrine whatever the migrated code currently produces, defeating the
point of a characterization baseline), commit them, and fix F-01 so the pipeline-dependent tests can run
at all.

---

## F-03 — Authorization tightened to four flat roles, with no provisioning done (High, P1)

**Legacy baseline** — confirmed via `git grep Authorize 608e36b`: exactly **one** `[Authorize]` attribute
in the entire legacy codebase:

```csharp
[Authorize(Roles = "MTS Risk Analysis: Basic Access")]
public ActionResult Index()   // HomeController only
```

Every other controller — `AdminController`, `PlantController`, `SchemeController`, `ExportController` —
had **no server-side role check at all**. Role names (`"MTS Risk Analysis : Admin User"`, `"...Support
Team"`, `"...Inspector"`) only drove `User.IsInRole(...)` checks inside Razor views to show/hide form
fields and links. Any Windows-authenticated user could reach any URL directly.

**Current state** — [MtsRoles.cs](../RPA.MTSInspections/Services/MtsRoles.cs) defines five policies now
enforced server-side with `[Authorize(Policy = ...)]` across every controller:

| Policy | Roles required | Applied to |
|---|---|---|
| `ReadAccess` | any of 4 | Home, Plant |
| `SupportOrAdmin` | Support, Admin | AdminController (class-level), Export Index/CSV |
| `AdminOnly` | Admin | Criteria/option/weighting admin, bulk import, recalculation |
| `SchemeEdit` | Inspector, Admin | BLS/BCC/PCG posts |
| `SccEdit` | any of 4 | SCC post |

Plus a global fallback: `options.FallbackPolicy = ...RequireAuthenticatedUser()` — meaning *any* endpoint
without an explicit policy now demands authentication, where legacy demanded nothing beyond the IIS
Windows-auth challenge.

**The gap:** [New-MtsEntraRegistration.ps1](../infra/entra/New-MtsEntraRegistration.ps1) creates the app
registration and the four app-role definitions, but its own doc comment says: *"It does not assign users
or groups to roles. That is a separate, access-controlled step"* — handed off to "the identity team" with
no script, runbook content, or mapping from the legacy IDT role membership. Until that happens, **every
existing user gets 403 on every page** regardless of what access they had before.

**Compounding open decision** — the change list itself flags this as unresolved: *"Entra roles are flat,
and Non-Subsidy uses one role. Any implied-role behaviour the IDT database provides today must be defined
explicitly (D-02)."* If the legacy SQL role provider granted any implicit hierarchy (e.g., Admin
membership implying Support rights), that won't carry over automatically — nothing in the repo confirms
whether it did or didn't.

**Fix:** export current IDT role membership from the Security database, map each account to the new Entra
app roles (resolving D-02 first), get business sign-off, and run the assignment *before* cutover — plus
add a test that checks every currently-active user maps to at least one role.

---

## F-04 — Scripted/automated consumers of export and recalculation will break (High, P1)

**Export endpoint — before:**

```csharp
// legacy, no attributes at all
public FileContentResult ExportCSV(string ExportListOption, string DropDownValue)
```

Reachable by a plain `GET` from a script or scheduled task running under a Windows service account — no
token, no role check.

**After:**

```csharp
[HttpPost]
[Authorize(Policy = MtsPolicies.SupportOrAdmin)]
public FileContentResult ExportCSV(string ExportListOption, string DropDownValue)
```

Combined with the global `AutoValidateAntiforgeryTokenAttribute` filter registered in
`AddMtsApplication`, any caller without a live browser session and a matching per-session antiforgery
token now gets rejected. This is directly confirmed by the test suite itself
(`PipelineTests.A_post_without_an_antiforgery_token_is_rejected`, case `/Export/ExportCSV`, expects
`400 Bad Request`) — i.e., the team already proved this breaks non-interactive callers; there's just no
evidence any were identified or notified.

**Recalculation endpoints — same pattern.** Legacy `AdminIndex.cshtml` rendered:

```html
@Html.ActionLink("Calculate All Risk Scores", "CaclulateAllRiskScores", "Scheme")
```

— a plain `<a>` tag, i.e. a **GET** request, reachable by any link or bookmark. The migrated view
replaced it with a `<form method="post">`, and the controller now requires
`[HttpPost][Authorize(Policy = AdminOnly)]`. `PipelineTests.Recalculation_cannot_be_triggered_by_a_get`
confirms GET now returns 404/405.

**Removed routable endpoints** (returned content in legacy, now simply don't exist as routes):

- `AdminController._OptionPartial`, `AdminController._WeightingScore`
- `PlantController._RiskScorePartial`
- `SchemeController._SchemeEditPartial`, `SchemeController._Errors`

These were ordinary public `ActionResult` methods directly reachable by URL in legacy MVC (child
actions). They're now View Components / server-rendered partials with no standalone route — a direct hit
on any of these URLs now 404s.

**What's missing:** nothing in the code, docs, or pipeline identifies or tests against any actual
external/scripted consumer. The assessment documents focus entirely on the interactive screens.

**Fix:** inventory any scheduled tasks, scripts, or integrations hitting these URLs (check IIS logs /
Application Insights history from the legacy app if available) before cutover; if any exist, they need
either a service-account OAuth flow or an explicit compatibility shim.

---

## F-05 — Audit identity changes format, and the `m0→m` rewrite is applied unmodified to a different identifier space (High, P1)

**Legacy** — [UserHelper.CurrentUser()](../RPA.MTSInspections/Helpers/UserHelper.cs) (originally, from
Windows identity):

```csharp
user = context.User.Identity.Name.ToLower().Replace("earth\\", "").Replace("m0", "m");
```

Input was always a Windows SAM name like `EARTH\m0123`, producing short audit values like `m123`.

**Current** — [AuditIdentity.GetName()](../RPA.MTSInspections/Services/AuditIdentity.cs) takes whatever
claim is configured via `Identity:AuditNameClaimType` (README recommends `preferred_username`, which for
Entra is normally an email address or UPN), and applies **the exact same transform, unchanged**:

```csharp
var name = claims[0].Value.ToLower().Replace("earth\\", "").Replace("m0", "m");
```

**Two distinct problems:**

1. **Unanchored substring corruption.** `Replace("m0", "m")` isn't a prefix check — it matches `"m0"`
   anywhere in the string. Applied to an email/UPN instead of a Windows SAM name, it will silently mangle
   any address that happens to contain the substring `m0` (e.g., in a surname, a numbered alias, or the
   domain part), producing an incorrect and misleading audit name with no error or logging. The legacy
   fixup was written for one specific naming convention and is now applied blindly to a structurally
   different identifier.

2. **Audit continuity break.** Existing `Audit` rows contain short legacy-format values like `m123`.
   Post-cutover rows will contain mangled email-like strings. There is no mapping table, reconciliation
   script, or decision record anywhere in the repo that ties a legacy audit identity to its Entra
   equivalent — so historical and future audit trails for the same real person won't correlate, and
   `AuditView`/`PlantAudit` screens will show two unrelated-looking identities for one user's history.

The team is aware of the risk but hasn't resolved it: the README's own operator notes say to *"check
collisions against existing audit rows before go-live"* — an instruction to manually inspect for damage,
not a fix.

**Fix:** stop applying the `m0→m` rewrite to non-legacy claim formats (gate it behind an explicit
"legacy identity" flag, or drop it entirely for Entra-sourced claims and keep it only for a one-time
historical-data remap); and add an explicit reconciliation/mapping record from legacy audit identities to
Entra identities before cutover, with a regression test asserting that a realistic Entra UPN/email
containing the substring `m0` passes through unmodified.

---

## F-06 — No reference/scheme data for a fresh database, and no legacy data migration path (High, P1)

**What the migrations actually create:**
[Migrations/Configuration.cs](../RPA.MTSInspections/Migrations/Configuration.cs) `Seed()` is empty —
comments only, no `AddOrUpdate` calls. Searching every migration file for `Sql(` or `INSERT`, there is
exactly one hit, in `202510221439027_AddSccScheme.cs`:

```csharp
Sql("INSERT INTO Schemes (SchemeId, Name, Active, Weighting) VALUES (NEWID(), 'SCC', 1, 0)");
```

That creates **one row** — the SCC scheme only. **There is no migration that creates BLS, BCC, or PCG
`Scheme` rows**, and no migration that creates any `Criteria`/`Option` reference data (the dropdown
values like "Throughput", "Operating Hours", "Trimming", etc. that `SchemeService.GetDropDownOptions` and
the risk calculators depend on) for *any* of the four schemes, including SCC itself.

**The one partial seed that exists isn't automated either:**
[SCC-Seed-Data.sql](../SCC-Seed-Data.sql) (196 lines) contains the `Criteria`/`Option` inserts for SCC —
but per [operator-guide.md](./operator-guide.md): *"`SCC-Seed-Data.sql` in the repository is not run by
anything. The DBA decides whether and how to load it."* It isn't referenced by the migrations, the
pipeline, or any startup path.

**Practical consequence on a freshly provisioned Azure SQL database:** after migrations run, `Scheme` has
one row (SCC) with no linked criteria/options, and zero rows for BLS/BCC/PCG. Any code path that does
`db.Scheme.Where(x => x.Name == "BLS").FirstOrDefault()` (used throughout `SchemeController`,
`SchemeEditViewComponent`, every Plant Details page load) gets `null`, and the subsequent
`currentScheme.SchemeId` dereference throws — i.e., the Plant Details screen (the core workflow of the
application) is non-functional out of the box for three of the four schemes, and even SCC isn't usable
until a DBA manually runs an unreferenced script.

**Separately, and more significant for a real cutover:** there is no documented or scripted path anywhere
(infra/, docs/, pipeline) for moving the *existing production data* — accumulated Plants, Audits,
PlantOptions, RiskScores — from the on-prem SQL Server into the new Azure SQL database. No BACPAC
export/import step, no Azure Database Migration Service configuration, no row-count/checksum
reconciliation script. The schema-release mechanism (`--script`/`--migrate`) only creates/updates
structure; it has nothing to do with data.

**Fix:** add migrations (or an automated, idempotent seed step run as part of release) for BLS/BCC/PCG
scheme and criteria/option reference data; wire `SCC-Seed-Data.sql` into an actual release step instead of
leaving it as a manual DBA judgment call; and produce and rehearse a real data-migration runbook
(export/import plus reconciliation) before any environment holding real legacy data is cut over.
