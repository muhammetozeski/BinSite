# Private workspaces and revision conflicts

This service stores project metadata for Catalog Studio, Review Room and Field Work Pack. Their local editing and file exports do not require an account. Optional cloud storage uses the existing membership cookie under `/api/commerce`; the nested `/api/commerce/workspaces` route therefore receives that cookie. Apply `schema/commerce.sql` and `schema/workspaces.sql` to the bound D1 database before deploying the functions. The workspace schema was applied to production before the route was published.

## Shared storage and limits

`functions/api/commerce/workspaces/[[path]].js` imports the production authentication, entitlement, hashing and rate-limit helpers from the commerce route. It accepts document kinds `catalog`, `review` and `field`. A body must be a JSON object. Binary media stays in the client; fingerprints and references may be stored with the document. These services do not provide media hosting.

An owner's allowance is shared with the older vault: three active owned items for free accounts and one hundred for an active subscription. The insert or restore SQL checks both tables in the same statement. An invitation does not consume the invited member's owned-project allowance. Free workspaces allow 65,536 UTF-8 bytes, two invited members and ten revisions; paid workspaces allow 524,288 bytes, twenty invited members and one hundred revisions. Saving uses the owner's entitlement, including when an editor submits the change. Request streams are bounded before parsing, separately from the stored-document limit.

## HTTP contract

All paths below are relative to `/api/commerce/workspaces`. Responses are JSON with `Cache-Control: no-store`. All timestamps are Unix epoch milliseconds. Every request requires the member session; writes also require the exact site Origin and JSON content type.

| Method and path | Body | Result |
| --- | --- | --- |
| `GET /` | None | `{workspaces,account}` with roles, revisions, archive state and current limits |
| `POST /` | `{kind,title,body}` | HTTP 201, `{workspace}` at revision one |
| `GET /:id` | None | `{workspace}` with complete body and caller role |
| `POST /:id` | `{revision,title,body}` | `{workspace}` with incremented revision; stale revision returns 409 |
| `POST /:id/invites` | `{role:"viewer" or "editor"}` | Owner-only, HTTP 201, `{token,expiresAt}` |
| `POST /join` | `{token}` | `{workspaceId,role}` after one-use invitation claim |
| `GET /:id/members` | None | Owner-only, `{members,ownerId}` |
| `POST /:id/members/:memberId` | `{role:"viewer", "editor" or "remove"}` | Owner changes or removes an invited role |
| `GET /:id/history` | None | Owner/editor, `{versions}` with revision metadata |
| `GET /:id/history/:revision` | None | Owner/editor, `{version}` including its saved body |
| `POST /:id/archive` | `{}` | Owner archives; invited access disappears |
| `POST /:id/restore` | `{}` | Owner restores within the current allowance |

The complete body is visible to every invited viewer or editor. There is no per-field or private-comment permission. A viewer cannot save, retrieve revision history, invite members or change roles. Unrelated members receive 404 instead of document contents. Archive is reversible and preserves data.

## Conflict and invitation behavior

Clients must retain the server revision from the loaded document. An update includes that exact revision. The conditional update also rechecks active membership and archive state. A generated mutation identifier ties the successful update to the history insert, so a rejected stale save cannot append a false history snapshot. After 409, clients preserve local work and offer deliberate reload or a separate copy; silent overwrite is not a merge strategy.

Invitations use random `ws-` tokens, expire after seven days, and are stored as hashes. Claiming requires an existing account. A unique claim identifier binds member insertion to the successful claim, preventing a replayed or losing claim from restoring a previously removed member. The owner changes member roles separately. Recovery of the member account revokes earlier sessions; workspace access follows the recovered account identity.

## Evidence and boundaries

The actual production functions and SQL passed local SQLite request-level checks for owner isolation, one-use invitations, revocation, member limits, optimistic revisions, revision retention, shared vault/workspace quota concurrency, archive/restore allowance, request origin, body size and session recovery. A paid entitlement was seeded only in that local fixture to test the larger limits; this was not a payment.

A production request sequence then created owner/editor fixtures, created and reread a private workspace, denied an unrelated reader, claimed an editor invitation, saved revision two, rejected a stale owner update with 409, read the two stored versions, changed the editor to viewer, rejected its save with 403, archived and restored the workspace, and recovered viewer access. The fixture records were removed after verification. This demonstrates the deployed service contract, not complete browser interaction coverage for every client.
