# Independent applications at one origin

`integrations/mounted-products.json` pins each upstream repository to an exact commit. Its explicit backend file list preserves relative imports. The permanent upstream checkouts remain separate; generated copies under `integrations/mounted/` and compiled assets under the four public product folders are ignored.

The main publication fetches those exact sources, invokes their publishers with an application base and destination, merges their scoped hosting metadata, and then builds the directory. The local four-product publication has completed successfully. Main-origin HTTPS acceptance is recorded separately after deployment.

| Product | Primary path | Backend |
| --- | --- | --- |
| Retry Assurance | `/retry-assurance/` | Existing retry-assurance D1 through `RETRY_DB` |
| Three Beat Freight | `/three-beat-freight/` | Existing freight D1 through `FREIGHT_DB` |
| StageFold | `/stagefold/` | Existing stagefold D1 through `STAGEFOLD_DB` |
| Interlock Expeditions | `/interlock-expeditions/` | Authenticated relay to its existing service and D1 |

## Build and routing

For existing clean sibling checkouts at their pinned revisions:

```powershell
./scripts/Prepare-MountedProducts.ps1 -SourcesRoot C:/E/Claude/WebSiteleri -NoRestore
```

Omit `-NoRestore` when packages have not been restored. CI fetches into `.mounted-sources` first and rejects dirty or mismatched sources. Each real application has its own base element, assets, framework files and navigation. Opening a product from the directory loads its document rather than placing it inside the main Blazor router.

The root `_routes.json` invokes Functions only for `/api/*` and each mounted `/api/*` prefix. Explicit page/parameter rewrites serve the correct application's entry document; no catchall rewrite captures scripts, downloads or framework assets. Root `_headers` and `_redirects` contain marked, replaceable generated blocks. Nested metadata is merged because Pages applies these files at the deployment root. Identical inherited headers are included once. Where a product's `DENY` would conflict with the global `SAMEORIGIN` frame header, the product retains its frame restriction through `frame-ancestors 'none'` instead of emitting an invalid combined header.

These routing rules follow the [Pages routing](https://developers.cloudflare.com/pages/functions/routing/), [redirect](https://developers.cloudflare.com/pages/configuration/redirects/) and [header](https://developers.cloudflare.com/pages/configuration/headers/) contracts. Functions return their own API headers. Database bindings are declared in `wrangler.toml` using the existing IDs; see [Pages configuration](https://developers.cloudflare.com/pages/functions/wrangler-configuration/).

## Data and environment boundaries

The three direct adapters preserve the original Request URL and Origin. Their handlers remove their own prefix and enforce existing authorization. `server/mounted-environment.mjs` passes the matching D1 object and an explicit product-specific environment whitelist. Another product's payment keys are never inherited.

Retry and StageFold account/capability checks use existing stored hashes. Their rate keys can be explicitly configured, or are separately derived with HMAC-SHA256 from the existing main commerce rate secret and the product identity. That derivative is not used for Interlock's original invitation/player keys. Existing browser sessions and local drafts remain at the old origins; recovery keys and portable project files provide the existing continuation paths. No database or browser storage is deleted by mounting.

Each product retains its own appearance and revenue UI. The shared appearance/revenue preparers skip catalog entries marked `runtime: "mounted"`; they do not inject another theme button, account footer or product shell.

## Interlock's authenticated relay

Interlock's original `PRIVATE_SALT` derives existing invitation and player capabilities. Its original value is retained at the existing service. The main adapter copies only the pinned imports-free `server/mount-gateway.mjs` and passes `INTERLOCK_GATEWAY_SECRET`. The existing service receives the matching `TRUSTED_GATEWAY_SECRET`.

The fixed transport binds method, mounted path/query, unchanged browser Origin, body digest, timestamp, forwarded authorization and a pseudonymous rate identity. It rejects external/preview origins, unsupported routes, browser-supplied transport credentials, oversized bodies and invalid/expired signatures. The existing service constructs the logical main-origin Request only after verification. App cookies must already have the mounted scope. The transport does not relay provider webhooks or automatically retry writes.

`deployment/ensure-interlock-gateway.mjs` and its manually invoked workflow inserted only the two new secret keys. Successful setup is recorded as workflow `40135698206`; it did not read or replace the original private salt. The API patch method preserves redacted keys as described in [Cloudflare secrets](Cloudflare-Secrets.md). Never store those values in source or execution records.

## Live acceptance and evidence boundaries

Main source `3389080523234122ce1d39bf466e70c985336dc1` was published successfully by [workflow 40137439534](https://github.com/muhammetozeski/BinSite/actions/runs/40137439534). All 77 catalog targets passed the delivery, document-language, dark-default and appearance-dependency check. These include the two previously uncatalogued existing applications, not two newly created products.

The main-origin Retry, StageFold and authenticated Interlock health endpoints returned JSON with HTTP 200. Freight's public endpoint is `/three-beat-freight/api/status`, not `/api/health`; it returned HTTP 200 with anonymous multiplayer availability. The example-client ZIP returned HTTP 200 as `application/zip`. Product deep links loaded the correct base document. Live frame headers contained one `SAMEORIGIN` value and the intended scoped CSP.

Read-only browser observation showed all four actual interfaces loaded at their new main-origin paths, with each product's layout and appearance. No graphical input, user signup, gameplay mutation or paid transaction was performed in this acceptance pass.

The coordinated local publication compiled all four pinned applications and emitted their real mounted assets, routes and backend copies. Their build metadata matched the pinned source revisions. The environment consumer check covered database/provider isolation, separate stable rate derivation, explicit overrides and missing configuration. Upstream mount records describe their narrower SQL, runner, gateway and artifact checks. Publication and live main-origin observations belong in the dated Walkthrough/release record; local success alone does not establish deployed operation, revenue or audience size.
