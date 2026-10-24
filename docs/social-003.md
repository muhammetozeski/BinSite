# Typed collaborative state transitions

The stage-six social batch uses the isolated module `functions/api/collaboration/[[path]].js` and `schema/collaboration.sql`. C# frontends live in `src/BinSite.Social/Pages`, use `Models/CollaborationModels.cs` for filtered server views, and call the new `Shared/CollaborationComponent.cs` transport adapter. Each product has its own Razor component, interface flow and isolated stylesheet. `Services/CollaborationEditorial.cs` contains authored fiction and teaching drafts, not fabricated users or activity.

## Atomic state and visibility

`collab_spaces` stores a type discriminator, owner hash, JSON domain state and revision. Clients cannot write arbitrary state: each registered product has a distinct creation validator, domain action reducer and view filter. Every mutation requires the client's expected revision and ends with a conditional `UPDATE ... WHERE revision=?`. A concurrent winner advances the revision; the stale writer receives 409 without replacing newer state. All input values are SQL parameters.

An anonymous HttpOnly SameSite=Strict cookie uses the established visitor-hash contract. The server checks ownership and participant identity before reducers execute. Public view filters remove cookie hashes, secret solutions, future relay stages and student answer keys. These identities do not authenticate a human, survive cookie deletion or provide account recovery. Mutations require same-origin Origin and streamed JSON at most 20,000 bytes. Per-identity and network-hash rates, field limits, workspace byte limits and three-distinct-network report hiding apply.

The shared route shape is `GET/POST /api/collaboration/<kind>`, `GET /<kind>/<id>` and `POST /<kind>/<id>/<action>`. Sharing transport and persistence does not make the workflows interchangeable; reducers enforce different invariants below. Room products marked unlisted return an empty directory. Course drafts are visible and discoverable only to their author until publication.

## Products 61–65

### Mini Turnuva

Four or eight actual participants join while entry is open. An organizer can start only a full roster. The server shuffles the roster, builds all elimination matches, and accepts only an organizer-confirmed winner who belongs to that ready match. A confirmed match cannot be silently changed. Its actual winner fills the correct parent slot; the final winner persists as champion. The site stores pairings and organizer decisions, not refereeing or gameplay results inferred from another service.

### Kitap Arkadaşı

Four original editorial fictional stories each contain three chapters. A real club selects one story; members store self-reported monotonic reading progress and publish comments scoped to a chapter. Progress is not represented as verified reading. Comments require membership and remain tied to the selected chapter. The C# interface can show editorial text before a club exists; no fake club is created for a demonstration.

### Mikro Akademi

The author creates a private draft, adds at most twelve sections with original teaching text, four distinct choices, one correct answer and an explanation, then publishes. Publication freezes the lesson content so completion refers to a stable question. Non-authors cannot read a draft or its directory entry. Student views omit answer keys and omit explanations until the corresponding lesson is completed. A student must finish the previous section before answering the next. The server checks the selected choice and records completion only for a correct answer; wrong answers do not advance progress. Three original editorial teaching drafts are offered separately from user courses. No credential or formal accreditation is promised.

### Birlikte Bil

An unlisted team room generates multiplication, clued English word and coordinate-walk questions. The view returns only the current question, never its answer or future question records. Two to six identities can join. Only the current flag holder may answer; a correct answer advances the stage into a handoff state. The same person cannot continue until they assign the flag to another joined member. The server persists holder, stage and completion history, allowing asynchronous return rather than requiring simultaneous presence. A correct final answer persists the finished state.

### Üretim Ortaklığı

An owner creates a one-to-fourteen-day creative project and up to forty tasks with explicit deliverables. Joined volunteers claim open tasks. Only the assignee can submit before the server deadline. Only the owner can review a submitted task; approval finishes it, while a request for revision returns it to the same assignee. New tasks, claims and submissions close at the deadline; owner review may continue. Eight original editorial task drafts do not create fabricated published tasks. Work is public; the platform does not arrange employment, payment or rights transfers.

## Local execution evidence

The first five new Razor products compiled in Release with zero warnings and zero errors on 2026-10-24. The backend executed against real Node SQLite through a D1-compatible test adapter in `C:\C\SocialBackendTests\collaboration-api-test.mjs`.

Six meaningful integration groups passed: full tournament roster, owner-only results and actual champion advancement; chapter comments, monotonic progress and stale-write rejection; course draft privacy, answer-key withholding, prerequisites and correct-only completion; relay minimum membership, private future stages, holder authorization and mandatory different-member handoff; jam assignee-only delivery plus owner-only rejection/resubmission/approval; and origin protection with network-deduplicated report hiding. These tests use an in-memory database and do not create production engagement. Rendered browser behaviour and production publication remain separate evidence owned by root integration.

## English-only package update

Packaged editorial stories, prompts, teaching drafts, fictional exhibitions, default draft text, API user-facing messages and social catalog copy now use English. Existing database contributions were not translated or rewritten. Product styles use flat surfaces, strong colors and functional selection indicators; decorative shadows and rounded cards were removed. The three social API modules retained their state and authorization checks and passed all eighteen existing SQLite integration groups after the copy update.
