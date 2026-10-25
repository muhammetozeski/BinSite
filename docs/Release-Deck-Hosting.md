# Release Deck at the main origin

Read this when changing the mounted release service, configuring Google access, or publishing its provider-data cleanup. The permanent public source is [ReleaseDeck](https://github.com/muhammetozeski/ReleaseDeck); its [API contract](https://github.com/muhammetozeski/ReleaseDeck/blob/main/docs/Api-Contract.md), [media transfer contract](https://github.com/muhammetozeski/ReleaseDeck/blob/main/docs/Media-Transfer.md) and [owner setup](https://github.com/muhammetozeski/ReleaseDeck/blob/main/docs/Owner-Setup.md) remain the product-specific sources of truth.

## Identity and publication

The main manifest pins the application and seven service modules. `functions/api/commerce/release-deck/[[path]].js` supplies the existing commerce identity, account status, hash and limit helpers; it does not introduce a second account system. The API stays under the commerce cookie path. Only this product's explicit provider settings cross the adapter boundary. Main vault/workspace creation and restore admission also count active release workspaces so moving between products cannot bypass the shared allowance.

The publisher emits an independent application document and scoped assets at `/release-deck/`. Its project JSON and release-sheet ZIP contain authored planning metadata, not the selected video bytes. Local files stay on the device until a creator explicitly chooses an authorized provider transfer. The original sample clips are bundled content, not simulated uploads or user activity.

## Additive setup and secret preservation

`deployment/prepare-release-deck.mjs` applies the pinned `schema/release-deck.sql` through the D1 query API and checks every statement result. The schema adds only release-prefixed tables and references the existing commerce member identity. D1's [query API](https://developers.cloudflare.com/api/resources/d1/subresources/database/methods/query/) accepts the statement batch; [foreign-key enforcement](https://developers.cloudflare.com/d1/sql-api/foreign-keys/) remains enabled.

The same script provisions a dedicated random 32-byte provider-token encryption key only if absent. An existing secret is preserved, while a same-named non-secret setting fails setup instead of being silently replaced. Updates use only changed environment keys, preserving unrelated redacted secrets as described in [secret updates](Cloudflare-Secrets.md).

The optional encrypted GitHub Actions secret `RELEASE_GOOGLE_WEB_CLIENT` contains a web client ID and secret as one JSON pair. `ReleaseDeck/scripts/Configure-Google.ps1` accepts an official web-client file, sends the pair through standard input to `gh secret set`, and dispatches the existing deployment workflow. Credential values are not command arguments or workflow inputs. The tool was checked with `-WhatIf` and a disposable client file; no real Google credentials were submitted. Actual channel authorization remains the creator's explicit action. The registered callback is `https://muhammetozeski-binsite.pages.dev/api/commerce/release-deck/oauth/callback`.

## Scheduled provider retention

The separate `binsite-maintenance` Worker imports the same pinned cleanup module as the product. It shares the main D1 binding, runs at `17 * * * *`, has `workers_dev = false` and `preview_urls = false`, and exposes no application route. Its HTTP handler returns 404. Cleanup removes expired provider authorization/operation records after their policy interval; it preserves user-authored release drafts. The source contract sets a 29-day provider-data interval and a ten-minute OAuth-state lifetime.

An observed first deployment uploaded the Worker but failed to install its cron with Cloudflare code 10063: this account had no workers.dev namespace. The account namespace was required even with the Worker's public URLs disabled. Setup now reads the [account subdomain](https://developers.cloudflare.com/api/resources/workers/subresources/subdomains/methods/get/), preserves an existing registration, and uses the [subdomain update endpoint](https://developers.cloudflare.com/api/resources/workers/subresources/subdomains/methods/update/) only when none exists. The scheduled Worker remains without a public workers.dev or preview URL.

After Wrangler deploys the Worker, setup reads the [script schedules endpoint](https://developers.cloudflare.com/api/resources/workers/subresources/scripts/subresources/schedules/methods/get/) and requires the exact hourly cron before setting `RELEASE_PROVIDER_MAINTENANCE_READY`. Provider operations require the Google pair, encryption and this readiness gate. See [Cron Triggers](https://developers.cloudflare.com/workers/configuration/cron-triggers/) and [Wrangler configuration](https://developers.cloudflare.com/workers/wrangler/configuration/) for provider contracts. A confirmed stored schedule is not evidence that a future hourly invocation has already executed.

## Evidence boundaries

Main workflow [40147621584](https://github.com/muhammetozeski/BinSite/actions/runs/40147621584) completed schema setup, dedicated-key provisioning, account namespace setup, maintenance deployment, schedule confirmation and Pages publication. Twelve isolated setup-consumer cases cover preserved/new keys, optional Google input, invalid configuration, statement failures, namespace registration and cron confirmation. Their provider responses were controlled, not live account mutations.

Live read-only checks observed a configured application, `googleConfigured: false`, the expected source revision, all document routes, and the API's anonymous/origin/method admission boundaries. No real channel transfer, public video publication, customer purchase, revenue or user growth is established by these checks.
