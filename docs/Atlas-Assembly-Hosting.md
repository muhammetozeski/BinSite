# Atlas Assembly at the main origin

The permanent source is [AtlasAssembly](https://github.com/muhammetozeski/AtlasAssembly). Its [engine contract](https://github.com/muhammetozeski/AtlasAssembly/blob/main/docs/Engine-Contract.md), [room API](https://github.com/muhammetozeski/AtlasAssembly/blob/main/docs/Room-Api.md) and [artwork preparation](https://github.com/muhammetozeski/AtlasAssembly/blob/main/docs/Artwork-Content.md) remain the product-specific sources of truth. Source work and publication are separate; current delivered counts belong in publication.json and the dated Walkthrough.

## Routing and source boundaries

The application is mounted at `/atlas-assembly/` with explicit room deep links. The main adapter at `functions/api/atlas-assembly/[[path]].js` forwards the original Request to the pinned handler at `/api/atlas-assembly/`, supplying only the main `DB` and canonical `SITE_ORIGIN`. It forwards no other product's account/provider settings. Room identity is a product capability, not the main commerce account.

The manifest must include the five server modules, `shared/engine.mjs`, `content/artworks.json` and `schema/atlas-assembly.sql`. The publisher supplies the same canonical engine to local solo play and copies the three actual bundled JPEGs and metadata. Independent assets/styles remain inside the mount; the main directory supplies one opening link. The mounted appearance markers are `data-atlas-theme` and `data-atlas-appearance`, with the public `AtlasAssemblyAppearance.audit()` helper.

## Additive storage and expiry

`deployment/prepare-atlas-assembly.mjs` applies the pinned additive schema to the existing main D1 through its documented query API. It requires top-level success and a nonempty list of successful statement results; an incomplete result stops publication. Only numeric provider error codes are reported. No environment secret is created for gameplay, and the Cloudflare deployment token remains in the existing Actions secret.

The schema adds `atlas_rooms`, `atlas_players`, `atlas_receipts`, `atlas_registrations` and `atlas_rate_limits` with their indexes. Foreign keys cascade expired room deletion to player/receipt records. Registration signatures remain until their separate tombstone expiry. Gameplay requests independently refuse expired rooms, but that is not a physical cleanup guarantee.

Registration admission uses a minute-specific product hash of the edge connection address rather than one global room-entry counter. The service trusts `CF-Connecting-IP` only at the deployed Cloudflare boundary described by the [provider header contract](https://developers.cloudflare.com/fundamentals/reference/http-headers/); controlled requests and local fallback are not proof of that external boundary. Changing a browser registration proof does not reset an address's quota. Raw addresses do not enter the rate rows, room snapshots or application logs. Thirteen targeted actual-rate-SQL consumers verified isolation and renewal; the main-host adapter needs no new environment key.

The workflow applies the Atlas schema before deploying the existing hourly `binsite-maintenance` Worker. That Worker invokes both the Release Deck cleanup and Atlas cleanup with `Promise.allSettled`, allowing either product's failure to be reported after the other's work finishes. It logs only product identity and statement change counts, then fails the scheduled event if either cleanup failed. No HTTP route or public Worker URL is added. Stored schedule confirmation remains part of the coordinated publication; a confirmed cron is not proof that a future invocation has already executed.

## Commercial and verification scope

Optional publisher advertising consumes the main public configuration and its approved consent gate. Placement is controlled inside the game application; the main preparers skip mounted UI. It stays empty when unavailable, is removed on entering gameplay, and requests at most one unit per document. A placement boundary does not unload already executed provider JavaScript or establish a separate-origin sandbox. Actual filled ads and revenue still require approved provider configuration and observation.

The main host sources passed 58 controlled module assertions: actual additive SQL forwarding, missing credentials, provider failures, incomplete statement results, environment isolation, both cleanup outcomes including one failure, and the maintenance HTTP 404. Filesystem/provider/cleanup boundaries were controlled; no external configuration request was made by those checks. Product-specific engine, actual-schema SQLite and UI checks are documented upstream, with their evidence boundaries. Live acceptance must record the pinned build, direct paths, actual artwork delivery, API admission and a controlled multi-client room separately from human use or audience growth.
