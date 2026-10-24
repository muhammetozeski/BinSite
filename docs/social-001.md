# Shared social products, batch 001

## Implementation

Three independent products use one standalone .NET 10 / C# 14 Blazor WebAssembly runtime. The user interfaces and state transitions live in `src/BinSite.Social/Pages/StoryTree.razor`, `IdeaDuel.razor`, and `TimeCapsule.razor`. Each has a separate isolated stylesheet and visual structure. There is no imposed parent product layout. `wwwroot/social.js` only provides clipboard, language preference storage, and same-origin history helpers. The deployment copies the compiled runtime to the public root and its bootstrap document to each product route. The directory homepage remains separate.

Shared persistence runs in `functions/api/[[path]].js` on Cloudflare Pages Functions with a D1 SQLite binding named `DB`. This is a JavaScript serverless API bridge, not a C# server or MAUI native package. `schema/social.sql` creates the data tables. The C# `SocialApi` service calls that API over the current origin.

Browser-compatible equivalents implement the relevant CSharpProgram components: scoped language preference in browser storage, central HTTP logging that excludes user content and secret link identifiers, typed models, and one final UI action boundary. Desktop filesystem settings, log files, executable signing and Windows launcher distribution do not apply to this browser output. Mutations are not automatically retried because the first write might have succeeded. A failed action keeps draft text and exposes a manual retry. Turkish and English interface strings are selected by `UiText`; editorial and user content retain their authored language.

The authored content contains 15 original story openings, 15 two-option creative poll prompts, and five letter-writing prompts in each interface language. Suggestions are explicitly labelled editorial, fill an unpublished draft, and never create a fabricated user post, vote or account.

## API contract

All successful responses use JSON with status 200. Error responses use `{ "error": "public explanation" }` and an appropriate 4xx or 5xx status. Responses have `Cache-Control: no-store, private`. Mutating requests require a same-origin `Origin` and JSON content type. The streamed request body is capped at 12,000 bytes.

| Route | Method | Behaviour |
| --- | --- | --- |
| `/api/stories?q=` | GET | Latest 30 visible title matches, opening and visible paragraph count |
| `/api/stories` | POST | Create title and opening paragraph |
| `/api/stories/:id` | GET | Visible story metadata and up to 300 nodes; hidden bodies are null |
| `/api/stories/:id/nodes` | POST | Add a paragraph under an existing visible parent |
| `/api/polls` | GET / POST | Latest 24 visible polls / create a two-option poll |
| `/api/polls/:id` | GET | Poll, aggregate vote counts and this cookie identity's optional choice |
| `/api/polls/:id/votes` | POST | Record choice 0 or 1; reject a repeated identity |
| `/api/capsules` | POST | Create an envelope with future unlock instant |
| `/api/capsules/:id` | GET | Envelope and server lock status; message omitted until unlock |
| `/api/reports` | POST | Record a reason for a story, node, poll or capsule |

There is no GET capsule directory. A capsule is identified by a random UUID. It is unlisted, not encrypted: anyone with the link may read the title and opening time, and read the body after unlock. The database operator can access stored text. The site does not claim end-to-end encryption, guaranteed permanent storage, identity verification or email delivery.

## Server boundaries

- A random anonymous cookie is `HttpOnly`, `SameSite=Strict`, and `Secure` on HTTPS. Its SHA-256 hash is stored as a visitor identifier. It is not a signed-in account or person identity.
- Poll votes have a database primary key on `(poll_id, visitor_hash)`. Clearing a cookie or changing browser can create another identity. IP-based rate limiting reduces abuse but does not establish one vote per human.
- Fixed-window rate counters use both visitor and hashed network address. Read limits are 120 per visitor per minute and 360 per network per minute. Hourly visitor write limits are six new stories, eighteen paragraphs, six new polls, forty votes, five capsules and ten reports; network ceilings are three times these limits. Expired counters are removed during subsequent requests.
- Story continuations are limited to depth 24 and 300 total stored nodes per story. An insertion-side SQL predicate enforces the node count.
- Capsule access uses a server-time SQL `CASE` expression, so the locked message is not returned to the browser. A client clock change does not unlock content.
- SQL parameters contain all user input. The report table name is chosen from a fixed server mapping. Search wildcards are escaped with a literal `!` SQL escape character.
- Content is rendered as normal Razor text, never raw markup. No user HTML is executed.
- Reports are unique per target and hashed network address. Three distinct network reports hide content automatically. Hidden node text is withheld while its existing children remain navigable. This is automated community reporting; no human-review service is represented.
- Submitted text and capsule links are excluded from HTTP logs. The API never returns stored identity hashes or raw network addresses. Anonymous identity hashes and reports are still user-related data and require the operator's retention policy as the service grows.

## Verification completed

`dotnet build src/BinSite.Social/BinSite.Social.csproj --no-restore -c Release` completed with zero warnings and zero errors on 2026-10-24 before membership integration. The .NET 10.0.11 packages restored successfully from the available environment.

The server handler was executed against actual SQLite through a small D1-compatible test adapter using Node 26.12.1. Six integration groups passed: story creation/branch relation and literal search including an injection string; persistent vote totals and duplicate rejection; absent capsule list and before/after unlock body gating; foreign-origin, oversized-body and invalid-input rejection; network-deduplicated reports with three-network hiding; and mutation rate limits plus protected-cookie/no-store response headers. Tests used an in-memory database, not fabricated production engagement. The harness was stored in `C:\C\SocialBackendTests\social-api-test.mjs`.

This batch does not claim rendered-browser interaction verification or live endpoint verification; deployment evidence belongs in the root deployment record.

## Growth discussion and measurable hypotheses

The social agent proposed Story Tree as the strongest current social candidate: an incoming reader can add one paragraph and share that new branch, creating another entry point. Discussion with the creative agent identified the same consumer-to-creator loop in user-authored escape rooms. Discussion with the games agent kept freeform collaborative fiction separate from authored choice-based narrative games. A later adaptation of a public story branch into a game is a proposal, not implemented functionality.

Useful future measurements are shared-link visit → paragraph read → continuation published → branch link copied. For Idea Duel they are shared-link visit → valid vote → new poll created. For Time Capsule they are creation → link saved → revisit after unlock. These are proposed measurement definitions, not collected metrics, revenue evidence or user counts.

Any future payment entitlement must be granted server-side only after a verified payment-provider webhook, with an idempotent event ledger and a durable order-to-customer binding. A browser success redirect must never grant access. Paid sponsorship must be clearly labelled and must not alter public poll results. Commerce implementation and account recovery are owned by the root integration, outside these anonymous content identities.
