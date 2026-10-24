# Updating Pages variables without replacing redacted secrets

A real production Pages secret was configured for the commerce rate-limit salt. The authorized project GET returned its `type: secret_text` and an empty `value`, not the original secret. Treat that value as redacted; copying the returned map into a new PATCH could submit empty strings for existing secrets.

Cloudflare's own Wrangler Pages secret implementation sends an environment PATCH containing only the keys being inserted or changed. Its delete operation sends only a single key with `null`. The project update therefore uses key-level patches rather than requiring the complete variable map. BinSite's advertising and revenue setup scripts follow the same source-backed method and never round-trip redacted secrets.

Primary implementation inspected: `packages/wrangler/src/pages/secret/index.ts` in https://github.com/cloudflare/workers-sdk/blob/main/packages/wrangler/src/pages/secret/index.ts . The put and bulk handlers use partial `deployment_configs[env].env_vars` objects; the delete handler uses a null value for the selected key. The secret read behavior was observed on the BinSite account on 24 October 2026; partial-update semantics are supported by the official client source inspection. No payment-provider secret was created or exposed during this observation.
