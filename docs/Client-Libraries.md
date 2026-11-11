# Client-side libraries and external requests

Inventory of the browser libraries the application ships under `RPA.MTSInspections/wwwroot`, what changed on
4 November 2026, and what remains. NuGet auditing does not scan these files.

## Changes made

| Library | Before | After | Reason |
|---|---|---|---|
| jQuery | 3.3.1 | **3.7.1** | 3.3.1 predates fixes for prototype pollution (CVE-2019-11358, fixed 3.4.0) and the HTML-handling XSS issues CVE-2020-11022 and CVE-2020-11023 (fixed 3.5.0). |
| Bootstrap (JS, CSS, theme, glyphicon fonts) | 3.3.7 | **3.4.1** | 3.3.7 predates the XSS fixes in 3.4.0 and 3.4.1 (for example CVE-2018-14040, CVE-2018-14041, CVE-2018-14042, CVE-2019-8331). 3.4.1 also ships the `woff2` glyph font that `bootstrap.css` referenced but the project did not contain. |
| jquery-validation | 1.17.0 | **1.19.5** | 1.17.0 predates the ReDoS fix in 1.19.3 (CVE-2021-21252). |
| bootstrap-datepicker (JS and CSS) | 1.7.1 | **1.10.0** | Moves to the latest 1.x release. XSS issues have been reported against older 1.x versions; confirm the exact advisory against your vulnerability database. |
| Font Awesome 4.7 (cdnjs link) | loaded from a CDN, **unused** | removed | No view uses a `fa` class. Removes a third-party request. |
| Hind (Google Fonts link) | loaded from `fonts.googleapis.com` | self-hosted, Latin 400 (`fonts/hind-400-latin.woff2`) | Removes a third-party request and the need for outbound access to a font service. Licence: SIL Open Font Licence 1.1. |

All of the old files were unmodified upstream copies except one local edit to `bootstrap.css`
(`color: white` on `.glyphicon-home:before`). That rule now lives in `Site.css`, so the vendored files stay pristine and can
be replaced cleanly. The layout now references `jquery-3.7.1.min.js`; `jquery-3.3.1*` was deleted.

The advisory identifiers above are from the maintainers' public release notes as recalled when this was written;
confirm them against your own vulnerability database before quoting them in a risk acceptance.

## Verification

- `StaticAssetTests` (in the test project) requests every script, stylesheet and font the layout and the stylesheets reference, over HTTP, and fails on any missing file or any external host. On a Linux container it also catches wrong letter case.
- A headless-DOM smoke test (jsdom) loaded the scripts in the layout's order and checked: jQuery and Bootstrap 3.4.1 plugins initialise; unobtrusive validation blocks an empty required field and shows its message, then accepts a filled one; the datepicker reads `31/01/2020` as day, month, year; the Bootstrap collapse opens from its data attributes. All passed.
- **Not done:** a visual check in a real browser. Bootstrap 3.4.1 and jQuery 3.7.1 are compatible (Bootstrap 3 supports jQuery 1.9.1 to 3.x), and no custom page code was changed, but open each main screen (plant list, plant details with the scheme forms and date pickers, admin pages, export and import) before release.

## What remains

| Item | Note |
|---|---|
| `modernizr-2.8.3.js`, `respond.js` | Unchanged and old. They are legacy support for old browsers; confirm which browsers the service must support and remove them if none need them. |
| Unused files in `wwwroot` | Several unminified copies, source maps, a slim jQuery (removed with 3.3.1) and `bootstrap-datepicker.standalone*` are shipped but not referenced. They cost bytes only. |
| `jquery.validate.unobtrusive.js` | The Microsoft adapter; it was already in the project and worked with 1.19.5 in the smoke test. |
| Content security policy | None is set. A policy limited to `'self'` is now feasible because no external hosts remain, but the pages contain inline scripts, so a nonce or hash scheme is needed first. |
| Total size | About 4 MB in 130 files before the upgrade; the static content finding (`Scale.0001`) stays accepted. |
