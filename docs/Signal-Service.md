# Signal Foundry's finite language and server-owned attempts

`server/signal-engine.js` defines `sf-v1`: nine row-major cells with values 0–3 and four A–D stamps mapped injectively to eight operations. Those operations are clockwise rotation, half rotation, left/right reflection, main-diagonal reflection, wrapped right shift, wrapped down shift, color cycling and red/blue exchange. Programs contain one to eight stamps and execute left to right. The 1,680 possible mappings are enumerated lexicographically. These contracts are shared with the C# engine; changing their meaning requires a new engine version.

## Evidence rather than guessed certainty

`content/signal-missions.json` contains twenty-four original editorial definitions with worked experiments, a target, bounded extra-query budget, hint and explanation. It is imported by the server bundle, not compiled into the C# browser client. The public repository still contains the authored source, so this is a practice/sharing game rather than a secure prize competition. Custom published mappings live in D1 and are excluded from public challenge responses.

Worked and queried outputs are always recomputed by the deterministic server engine. Each observation removes only inconsistent mappings. A finish response is `deduced` when the remaining mappings agree on the answer, `consistent` when multiple outputs remain and the submitted answer matches one, or `inconsistent` when no surviving language produces it. `actualMatch` separately describes the author's chosen mapping. A logically consistent alternative is not labelled incorrect just because it differs from the author's hidden choice.

The final response includes the exact submitted `answer`, actual output, intended mapping and authored explanation. Storing the submitted answer prevents a restored result from being displayed beside an unrelated local draft. Before finish, public and attempt responses omit the mapping, explanation and held-out output.

## API and identity

`functions/api/commerce/signal/[[path]].js` serves `/api/commerce/signal`. `GET challenges` lists editorial missions; `GET challenges/:id` opens an immutable shared challenge. `POST challenges` publishes an actual account's explicit reusable text and mapping, with a UUID request identifier. A fork inherits the exact parent's mapping. A supplied mapping that differs from the parent is rejected instead of silently changing the author's reviewed preview. Changed rules require an independent challenge.

`POST challenges/:id/attempts` starts a guest attempt. `GET attempts/:id` restores it. `POST attempts/:id/query` sends an experiment and `POST attempts/:id/finish` commits a reconstruction. Every mutation carries `requestId`; replaying the same semantic action returns the saved response without spending another query. Reusing an identifier with different input returns 409. The server revision condition and mutation-bound action insert make simultaneous different requests spend at most the available budget. Action sequence numbers preserve experiment order even when timestamps are equal.

The guest cookie is random, HttpOnly, Secure, SameSite Strict and scoped to this API. Simultaneous first requests from separate fresh tabs can replace one another's cookie. Each created/authorized attempt response therefore also supplies a per-attempt recovery capability. It is an HMAC of the attempt ID under the private production salt, never stored in plaintext in replay records. Clients keep it with their local attempt and send it only through `X-Signal-Attempt-Key`. It is never part of a challenge URL, public export or log. A correct cookie or capability grants that attempt's access; an invalid capability does not bypass ownership. Rotating the underlying salt invalidates old capabilities; a still-valid owner cookie can obtain the current one.

Attempts expire after seven days and include `expiresAt`. The private snapshot preserves their original mission, examples and target. Account recovery is separate from this guest capability. Owner administration can prune expired attempts and replay records in bounded batches using `scripts/Moderate-Signal.ps1`; publication does not claim that this script has been scheduled automatically. Challenge source and live published definitions are not removed by pruning.

Successful creation replay is checked before looking up a now-withdrawn source or parent. This preserves recovery after a successful response was lost while still refusing new requests against that withdrawn source. Requests use exact Origin checks, bounded JSON, parameterized SQL and both salted-IP and identity rate limits. Public author aliases are user text, not verified people or expert credentials.

## Moderation and limits

Account-authorized `report` records spam, unsafe, rights or privacy reports. It does not turn a report into automatic removal. The author can withdraw a custom challenge; owner administration can list/resolve reports and hide/unhide editorial or custom challenge IDs. New public reads and starts refuse hidden/withdrawn sources. Existing authorized attempt snapshots remain private records of their original play session.

Publishing allows ten challenges per member and forty per IP per day, with at most fifty active owned public challenges. Starting permits thirty attempts per guest and one hundred per IP per day. Per-minute request limits apply independently to the IP and identity. There are no invented players, public score counts or payment-based rights in this service.

## Executed evidence

The actual module imports and parses the full editorial corpus. Canonical checks passed all eight operation vectors, noncommuting AB/BA order and the 1,680-family enumeration. An isolated request-level harness executed the actual commerce helpers, signal module, production SQL and SQLite schemas. Fifty-five HTTP assertions plus all twenty-four mission consistency checks passed, including secret-field omission, immutable inputs, genuine ambiguity, exact experiment ordering, query exhaustion, simultaneous same/different nonce behavior, author permissions, output recomputation, inherited fork rules, withdrawal replay recovery and seven-day expiry.

Independent review identified the creation replay ordering and first-cookie race; both were corrected with specific reproductions. The capability reproduction replaces the original guest cookie with a second tab's cookie: the correct per-attempt key restores and finishes the first attempt, a wrong key returns 404, and the second attempt remains distinct. The eleven additive schema statements were successfully applied to production before deployment.

After the clean publication workflow succeeded, the actual service passed twenty-two production HTTP assertions covering the twenty-four-mission corpus, secret-field omission, protected attempts, cookie-replacement recovery, real query accounting, nonce replay, final-answer restoration, account publication, reporting, owner moderation and withdrawal. Exact integration fixtures and the account were removed. A read-only browser observation rendered the actual English game controls and loaded the mission list; this is separate from a complete interactive browser play-through.
