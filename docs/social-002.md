# Collaborative community products

This note describes the implemented contracts of five C# Blazor products in `src/BinSite.Social/Pages`: `SharedCanvas`, `DebateStage`, `TimeMarket`, `SharedMuseum` and `PasswordTeam`. Each owns a separate route and isolated stylesheet. The server bridge is `functions/api/community/[[path]].js`; tables are created by `schema/community.sql` in the existing D1 `DB` binding. The earlier story/poll/capsule API remains separate.

## Identity and request boundary

The module uses the same `binsite_visitor` anonymous HttpOnly, SameSite=Strict cookie as the earlier social API. It hashes the same `visitor:` prefix, so identity remains consistent across the modules. This cookie is not an authenticated member account or person verification. Clearing it can create another identity, lose author authority, lose access to a private conversation or room role, and bypass identity-level one-per-browser assumptions. Network-hash rate ceilings provide an additional automated limit, not person verification.

Mutations require an exact same-origin Origin, JSON content type, a streamed body under 18,000 bytes, bounded fields and parameterized SQL. User-supplied evidence links permit only HTTP/HTTPS without URL credentials. Public text is normal encoded Razor content, never raw user markup. The API returns no private identity hashes or raw network addresses.

`ReportBox` gained an optional `Endpoint` parameter; it defaults to `reports` for the original products and uses `community/reports` for these five. Targets are selected from a fixed server table mapping. Reports are unique per target and hashed network address; three distinct network reports automatically hide a public record. This is automated reporting, not human review.

## Ortak Tuval: changed cells and revision control

`GET canvases` lists actual public canvas headers; `POST canvases` creates an empty 24×16 grid. `GET canvases/:id` returns one bounded snapshot. `POST canvases/:id/pixels` accepts 1–32 distinct cells and an expected revision. Coordinates and the twelve-color palette are validated server-side.

The paint batch first conditionally increments the revision and stores a fresh operation marker. Each subsequent cell upsert is conditional on that exact operation marker. D1 executes the batch transactionally. A stale revision changes neither pixels nor revision and returns 409. This avoids a stale writer silently overwriting another person's publication. The UI retains pending cells after conflicts and refreshes, allowing inspection before an explicit retry. It submits only changed cells, never continuously writes a full image or polls the database every second.

The interface offers twelve colors, keyboard-focusable cells and eight original editorial group-drawing constraints. There are no fabricated shared drawings. The grid is rendered in C# as accessible buttons; no extra canvas JavaScript interop is required.

## Münazara Sahnesi: argument relationships and review rubric

`GET/POST debates`, `GET debates/:id`, `POST debates/:id/arguments` and `POST arguments/:id/reviews` implement a proposition with explicit context, support/opposition, optional counterargument parent, reasoning and optional evidence URL. A counterargument parent must belong to the same debate and the opposite side. The server bounds a debate to 150 arguments.

The two review axes are clarity and evidence quality, each 1–3. An author cannot review their own argument. A different cookie identity can review it once, enforced by a primary key. The server computes actual averages and counts. These are peer opinions, not a popularity winner or a claim of automated factual verification. Twelve original editorial prompts fill unpublished proposition drafts rather than creating fake sessions.

## Zaman Pazarı: private proposals and two-party completion

Public `GET/POST exchanges` offers skill sharing or requests with a bounded 15–240 minute session. There are no contact-detail fields or payment fields. Sixteen original editorial skill exercises only fill a skill draft; they do not claim that a visitor has a skill.

`POST exchanges/:id/proposals` creates a private conversation between the listing owner and one responding cookie identity. A unique `(exchange_id, requester)` key prevents duplicate proposal threads. `GET my-threads`, `GET threads/:id` and message mutations authorize the two participants on the server before returning private text. The operator can still read stored database content; no end-to-end encryption is represented.

Allowed states are:

- Listing: `open → matched → completed`, or `open → closed`.
- Proposal: `proposed → accepted`, `declined` or `withdrawn`.
- Accepted proposal: stays `accepted` until both `owner_done` and `requester_done` are recorded, then becomes `completed` together with its listing.

Only an owner can accept or decline; only the respondent can withdraw. Acceptance conditionally claims one open listing inside a D1 batch. Other proposals are declined only when that exact thread became the matched thread. This prevents two proposals both winning a concurrent acceptance race. Only an open listing can be owner-closed. Closed, declined, withdrawn and completed conversations stop accepting new messages. A conversation is limited to 200 messages.

## Ortak Müze: private submissions and curator ordering

`GET/POST museums`, `GET museums/:id`, `POST museums/:id/objects`, `POST objects/:id/decision` and `POST museums/:id/order` implement themed collections of object stories. The creator's cookie identity is the curator. A contributor's submission begins pending and is returned only to that author and the curator. The curator's own submissions are immediately approved. Public visitors receive approved, non-hidden objects only.

The decision endpoint verifies the curator on the server. Reordering requires the complete approved set, with every ID appearing exactly once, and an expected collection revision. An operation marker guards all position updates. Stale ordering returns 409 rather than partially changing a sequence. Collections are bounded to 120 objects.

Three clearly labelled fictional editorial exhibitions contain eighteen original object narratives. They are static authored examples, not user posts, historical findings or real personal memories. Selecting an example theme fills a draft; publication remains an explicit action.

## Parola Ekibi: private information and generated solution

`POST rooms` creates a random four-role puzzle and claims role zero for the creator. There is no public room directory. `GET rooms/:id` returns the envelope and role occupancy; before joining it returns no private clue and no team conversation. `POST rooms/:id/join` assigns a remaining role with server constraints: at most four roles and one role per cookie identity. The API does not accept a caller-selected role number.

Private cards provide four different necessary pieces:

1. A random permutation of four symbols.
2. Two symbol-to-digit mappings.
3. The remaining two symbol-to-digit mappings.
4. A one-to-three-position left rotation of the decoded sequence.

The digits are distinct, and every symbol appears exactly once. The full mapping and a fixed rotation determine exactly one four-digit answer, including a possible leading zero. The server stores a room-specific SHA-256 solution hash and separate clue rows. It returns only the requesting identity's assigned clue, never the other clue rows or the solution hash. Knowing the generator's algorithm does not supply an individual room's random values.

`POST rooms/:id/messages` is participant-authorized. `POST rooms/:id/answer` requires all four roles to be occupied, validates a four-digit string, applies attempt limits and checks the server-side solution hash. A successful answer persists `solved`. Rooms stop accepting mutations after 24 hours. Existing room text is not automatically deleted merely because that play window expires. Six original fictional mission settings explain the same cooperative mechanism; they are content within one product, not six counted sites.

## Verification evidence

The five-page C# project compiled in Release with zero warnings and zero errors on 2026-10-24. The locally available .NET 10.0.11 package environment was reused. This establishes C# and Razor compilation, not rendered-browser interaction or production publication.

The actual server handler ran against real Node SQLite through a D1-compatible adapter in `C:\C\SocialBackendTests\community-api-test.mjs`. Six integration groups passed:

- Canvas cell persistence, stale revision rejection, and absence of stale pixel overwrite.
- Structured debate persistence, author self-review rejection, unique peer review, aggregates and rejection of a JavaScript evidence URL.
- Exchange conversation access restricted to participants, owner-only acceptance, message persistence and completion only after both parties confirm.
- Pending museum objects withheld from unrelated readers, curator-only approval, and stale ordering revision rejection.
- Private room clues withheld before joining and filtered by role, four-role capacity, incomplete-team answer rejection, and a generated puzzle solved by combining its four cards with the room status persisted.
- Foreign-origin mutation rejection and report deduplication with hiding only after three distinct networks.

These tests used an in-memory database and did not fabricate production activity. Production checks and deployment evidence belong to the root integration record.

## English-only package update

Packaged editorial stories, prompts, teaching drafts, fictional exhibitions, default draft text, API user-facing messages and social catalog copy now use English. Existing database contributions were not translated or rewritten. Product styles use flat surfaces, strong colors and functional selection indicators; decorative shadows and rounded cards were removed. The three social API modules retained their state and authorization checks and passed all eighteen existing SQLite integration groups after the copy update.
