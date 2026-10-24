# Public procedure versions and explicit reported outcomes

The Procedure Exchange server route is `functions/api/commerce/procedures/[[path]].js`. Its public read path and account-authorized writes are under `/api/commerce/procedures`, inside the existing membership cookie scope. `schema/procedures.sql` creates only additional tables and indexes; its eight statements were successfully applied to production before deploying the route. No existing workspace kind or table constraint was changed.

## Publication boundary

Only canonical procedure text can be published: format version, title, summary, one supported category, at most six tags, twenty input descriptions, an output-artifact description and one to forty actionable steps. Every step has a stable ID, title, action, observable completion check and integer estimated minutes. Public text is limited to 48 KiB of UTF-8. Unknown document or step fields are rejected, including private run records. The client must explicitly project public text rather than sending its entire local workspace.

Publishing requires a real account session, the exact request Origin, JSON and explicit permission for public copying, forking and attributed exports. The public author alias is supplied by that account; it is not a verified real name or expert credential. Public responses do not expose account IDs, report authors, session tokens or recovery credentials. The browser may read without an account. Results have `Cache-Control: no-store` and no cross-origin access policy.

## Version and participation contracts

`GET /` lists twenty items with search/category filters, offset pagination and `hasMore`; `mine=true` requires an account and includes its withdrawn/moderated records. `GET /:id?revision=n` retrieves an exact immutable published version, its real aggregate outcomes and only the current account's own outcome note. Omitting revision selects the current edition.

`POST /` creates a procedure from `{document,authorAlias,allowReuse:true,parent}`. An optional parent pins the exact available source ID and revision. `POST /:id` publishes the owner's next version from `{revision,document,authorAlias,allowReuse:true}`. The old version is retained. The conditional update and generated mutation identifier bind exactly one successful update to its version insertion. A stale save returns 409, leaving local work to be deliberately reloaded or forked.

`POST /:id/outcomes` accepts `{revision,kind,note}` where kind is `completed`, `partial` or `blocked`. This is an explicit self-report, not an inferred result, verified person or success measurement. The primary key is procedure/revision/member: changing one report updates its record rather than inflating participation. Outcomes on one revision are not carried into another revision's counts. Local checkboxes and artifacts stay local unless the visitor explicitly submits a bounded public outcome note.

`POST /:id/withdraw` lets the author hide the procedure reversibly in storage; it remains readable to that account. Public reads, forks and new outcomes no longer expose it. `POST /:id/report` records `spam`, `unsafe`, `rights` or `privacy` with an optional note. Reports are deduplicated per account/procedure and do not automatically remove content. Owner administration is provided by `scripts/Moderate-Procedures.ps1`: List, Hide, Unhide and Resolve affect a selected procedure. The script requires the owner's process-held hosting token and never deletes published text. A resolved report does not automatically unhide a procedure, and unhide does not reverse an author's withdrawal.

Reads allow 120 requests per minute per account or salted IP digest. Writes allow twenty per member and sixty per IP per minute; publishing allows ten per member and forty per IP per day. Each member may own fifty active public procedures, and each procedure may publish one hundred immutable versions. Limits are enforced by the server, with the active-item check inside the insert SQL. Stored values use bound parameters; wildcard search escapes the SQL LIKE escape character separately.

## Private copies

Private Procedure Exchange drafts use the existing owner-only `/api/commerce/projects` vault with kind `procedure`. They share the real three/free or one-hundred/paid item allowance with the other private products. The vault's outer label is limited to one hundred characters and its JSON body to 64 KiB; the complete local procedure title and runs remain in the body when they fit that allowance. A private vault copy does not grant collaboration roles or immutable public publication history. The older workspace table continues accepting its original catalog/review/field kinds.

## Executed evidence

The production module parsed successfully. An isolated in-memory SQLite harness executed the actual route, commerce account helpers, production SQL and combined schemas. It passed 107 request assertions plus explicit checks of private-field rejection, public identity boundaries, immutable revisions, pinned forks, old-version reads, one-report outcome updates, revision-specific real counts, report deduplication, withdrawal, SQL wildcard escaping, size limits, simultaneous expected-revision conflicts, fifty-item allowance and session recovery. Controlled time advancement exercised the real publishing route up to its item limit. No database or network provider was mocked as a successful paid subscription.

After deployment, actual production requests passed public creation/read, immutable first/second editions, stale-save rejection, a fork pinned to the first edition, one-account outcome updates with revision-specific counts, reporting, the real owner List/Hide/Unhide/Resolve tools, author withdrawal and foreign-Origin refusal. Exact fixture procedures, outcomes, reports and accounts were removed afterward. This verifies the deployed service and owner tool; the Procedure Exchange browser client is still being implemented and is not counted as a published product.
