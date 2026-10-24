# Membership revenue, private vault and collaborative workspaces

## Delivered value and boundaries

The `creator-monthly` plan provides a private, server-side JSON project vault and collaborative workspaces. A free member can own three active items across both services; an active paid member can own one hundred. Vault files hold up to 65,536 UTF-8 bytes of JSON. Workspaces hold up to 65,536 bytes for free owners or 524,288 bytes for paid owners, with two or twenty invited collaborators and ten or one hundred stored revisions respectively. Existing items remain readable after a downgrade; new items, restores and workspace saves use the owner's current limits. Source code and public examples are not sold as private or exclusive content. See [Workspace contracts](Workspaces.md) before implementing a client.

Stripe and Lemon Squeezy are alternative checkout adapters for the same entitlement. One valid provider subscription is sufficient. Refund or expiry in one provider does not remove another valid subscription. The default configured plan is USD 5 per month; `CREATOR_MONTHLY_CENTS` must match the actual provider price. No payment provider is available until all its required configuration is present. A success return URL never grants access.

This module does not claim a completed live payment, merchant approval, actual subscribers, or revenue. Signed test fixtures validate local integration behavior. A provider account, an approved payment arrangement where applicable, real keys, deployment configuration, and a provider checkout are required to verify actual payment collection.

## Source ownership

- `functions/api/commerce/[[path]].js`: Cloudflare Pages Functions route, authentication, private vault, checkout, customer portal, signed webhook processing, and entitlement reconciliation.
- `schema/commerce.sql`: member, session, checkout reference, provider customer, subscription, revocation, webhook event, lock, private project, and rate-limit tables. Apply to the same D1 database bound as `DB`.
- `scripts/Configure-Revenue.ps1`: owner-invoked configuration; it is not scheduled or invoked by visitors. It reads secrets from process environment variables or masked prompts, provisions provider configuration, and writes production Pages bindings. Run after deploying this backend, and again after a key, price, portal, or webhook change.
- The membership interface is maintained separately by the root agent at `/uyelik/`.

## HTTP contract

Every response is JSON, uses `Cache-Control: no-store`, and has no cross-origin access policy. Authenticated requests use the `binsite_member` cookie. Mutating browser requests must include the site's exact `Origin`. The cookie is independent of the anonymous social visitor cookie. All times are Unix milliseconds.

| Method and path under `/api/commerce/` | Body | Success |
| --- | --- | --- |
| `GET status` | None | `{account, providers, projects, plans, configurationVersion, providerModes, paymentMode}` |
| `POST account` | `{}` | HTTP 201, `{recoveryKey}` and a session cookie |
| `POST recover` | `{recoveryKey}` | `{ok:true}` and a new session cookie; earlier sessions are revoked |
| `POST logout` | `{}` | `{ok:true}` and an expired cookie |
| `POST checkout` | `{provider:"stripe" or "lemon", plan:"creator-monthly"}` | `{url}` for provider-hosted checkout |
| `POST portal` | `{provider:"stripe" or "lemon"}` | `{url}` for provider-hosted subscription management |
| `GET projects` | None | `{projects:[{id,title,kind,updatedAt}]}` |
| `POST projects` | `{title,kind,body}` | HTTP 201, `{id}`; `body` must be a JSON object |
| `GET projects/:id` | None | `{project:{id,title,kind,body,updatedAt}}` for the owning member |
| `POST projects/:id` | `{title,kind,body}` | `{id}`; updates an owned project |
| `POST projects/:id/delete` | `{}` | `{ok:true}` for the owning member |
| `POST webhooks/stripe` | Provider raw body and `Stripe-Signature` | `{received:true}`; duplicates add `duplicate:true` |
| `POST webhooks/lemon` | Provider raw body and `X-Signature` | `{received:true}`; duplicates add `duplicate:true` |

An anonymous status response includes `account:null`, the available-provider booleans, an empty project list, and the plan description. A signed-in account includes `id`, `premium`, `projectLimit`, `usedProjects`, `plan`, `expiresAt`, and a `subscriptions` array with provider, status, current entitlement, period end, and cancellation-at-period-end state. `usedProjects` combines vault files and unarchived owned workspaces. `providerModes` identifies test/live mode separately; `paymentMode` is a coarse compatibility field. A provider availability boolean indicates complete configuration, not a claimed successful checkout.

Example free-account status fields:

```json
{
  "account": {
    "id": "d2306f87-7d9c-46c4-a82e-3abf4943e010",
    "premium": false,
    "projectLimit": 3,
    "usedProjects": 0,
    "plan": "free",
    "expiresAt": null,
    "subscriptions": []
  },
  "providers": { "stripe": false, "lemon": false },
  "projects": [],
  "plans": [{
    "id": "creator-monthly",
    "name": "Creator Membership",
    "priceLabel": "5.00 USD / month",
    "features": [
      "100 private cloud projects and workspaces",
      "64 KB vault files; up to 512 KB per collaborative workspace",
      "Access across devices with your recovery key",
      "Read and delete existing projects after membership ends"
    ]
  }],
  "configurationVersion": null,
  "providerModes": { "stripe": null, "lemon": null },
  "paymentMode": "test"
}
```

Errors return `{error:"English explanation",code:"stable_code"}` with HTTP 400, 401, 403, 404, 409, 413, 415, 429, 500, 502, or 503 as appropriate. Important interface codes are `authentication`, `project_limit`, `provider_unconfigured`, `already_subscribed`, `checkout_pending`, and `processing`. Recovery keys are returned once on account creation and are never returned by status or recovery.

## Authentication and storage

The server creates 256-bit random recovery keys and session tokens. Only SHA-256 hashes are stored. The session cookie is `HttpOnly; Secure; SameSite=Strict`, scoped to `/api/commerce`, with a thirty-day lifetime. Recovering an account revokes its older sessions and creates a new one. Recovery keys are bearer credentials; the interface must tell the member to preserve the once-shown value.

Every vault query constrains `member_id`; knowing a project UUID does not grant access. Creation checks the three/one-hundred limit inside the SQL insert, preventing simultaneous requests from bypassing the quota. JSON body size is measured in UTF-8 bytes. Browser requests are bounded at 70,000 bytes, provider notifications at 262,144 bytes, and SQL values are bound parameters. Requests are rate-limited in D1 using a salted IP digest. Technical error output contains an error class only, without request bodies, raw recovery keys, provider keys, or card data.

## Payment verification and lifecycle

Checkout references are created server-side and bound to the signed-in member, selected provider, and plan. Stripe receives the reference both on Checkout and subscription metadata; Lemon receives it as checkout custom data. An authenticated webhook alone does not select an arbitrary member: the reference must match a stored checkout, and a previously mapped subscription/customer cannot move to another member.

Stripe requests pin API version `2025-02-24.acacia`; the code also handles newer nested invoice subscription and item period fields. Notification signatures use HMAC-SHA-256 over the raw body, with a five-minute Stripe timestamp tolerance. Lemon's raw-body HMAC is compared against `X-Signature`.

Stripe event IDs and Lemon payload digests identify duplicates. Event leases protect concurrent deliveries. A second lock serializes each subscription's reconciliation, which retrieves the current subscription and invoice from the provider API before writing entitlement state. Event arrival order and `created` timestamps are not used as authoritative subscription state. A request that cannot acquire its lock returns a retryable response; failed processing releases the event lease.

An active, paid period grants access until its end. A request to cancel at period end keeps the remainder of the paid period. Actual subscription termination, an expired paid period, or a fully refunded/charged-back current invoice removes that period's entitlement. Partial refunds do not remove the entire paid period. A period revocation is stored separately, so a later old notification cannot resurrect it; a newly paid renewal invoice can supply a new valid period. An earlier paid period can remain valid while a subsequent invoice is past due, but an unpaid new invoice does not extend access.

The member may always open the provider's hosted management portal to review invoices, update payment details, or stop renewal. Cancellation is configured for period end in the Stripe portal. Card details are entered at the payment provider and never accepted by this API.

## Configuration and activation

Common binding: `COMMERCE_RATE_SALT` (secret), `CREATOR_MONTHLY_CENTS` (plain), `COMMERCE_CONFIG_VERSION` (plain marker).

Stripe bindings: `STRIPE_SECRET_KEY`, `STRIPE_WEBHOOK_SECRET` (secret); `STRIPE_PRICE_ID`, `STRIPE_PORTAL_CONFIGURATION_ID`, `STRIPE_WEBHOOK_ID` (plain).

Lemon bindings: `LEMON_API_KEY`, `LEMON_WEBHOOK_SECRET` (secret); `LEMON_STORE_ID`, `LEMON_VARIANT_ID`, `LEMON_TEST_MODE`, `LEMON_WEBHOOK_ID` (plain).

The configuration script reads account ID, project name, and the verified site origin from `docs/hosting.json`; explicit parameters can override them. Its default provider is Lemon, and Stripe or Both can be selected when the owner's provider account supports that choice. It requires `CLOUDFLARE_API_TOKEN` or a masked prompt. Stripe keys must match the selected Test or Live mode. Stripe creates/reuses the monthly price, creates/configures its customer portal, and registers a fresh signed webhook. Lemon verifies an existing store and monthly variant, then registers a signed webhook. Existing Pages variables and the `DB` binding are preserved. No secret is written to a source, report, temporary file, or console output.

With the process environment holding the Cloudflare token, the ordinary owner command is:

```powershell
pwsh -File C:\E\Claude\WebSiteleri\BinSite\scripts\Configure-Revenue.ps1 -Provider Lemon -Mode Test
```

It prompts for the selected provider's missing API key with hidden input. `-NoPrompt` requires the values to already exist in process environment variables. Lemon store and variant IDs are selected automatically when unambiguous; otherwise their explicit override parameters select the existing approved subscription product. No jurisdiction or provider-account eligibility is inferred from the site owner's language.

New environment bindings apply to a new deployment. Earlier webhook endpoints remain until that deployment is active. After deployment, invoke the script with `-FinalizeWebhooks`: it compares `/status`'s active configuration marker with Pages production configuration before retiring earlier endpoints. Stripe endpoints are disabled, and earlier matching Lemon endpoints are removed. This avoids switching the sender's signing secret before the receiver is updated. The script does not create a paid hosting subscription, purchase a domain, create a bank account, or report an unverified payment.

## Validation evidence

The installed Node.js parser accepted the commerce module. SQLite executed the production schema in memory. Request-level checks passed for account creation, hashed credentials, session recovery/revocation, free-quota concurrency, private project ownership, same-origin mutation rules, body-size limits, and unconfigured-provider refusal. Exactly one hundred premium projects were accepted, the next was rejected, and downgrade preserved reads of existing projects while refusing creation above the free quota.

Signed controlled provider fixtures and the actual SQL statements passed paid entitlement grant, duplicate suppression, paid cancellation grace, canonical reconciliation after out-of-order events, permanent full-refund blocking for the refunded period, a newly paid renewal, Lemon partial/full-refund distinction, invalid signatures, and preservation of an independent provider's valid entitlement. Provider network responses were test fixtures. No actual card charge, live merchant approval, Cloudflare secret update, or real provider portal operation was performed in these checks. The PowerShell parser accepted the complete configuration script.

## Official references

- [Stripe Checkout creation](https://docs.stripe.com/api/checkout/sessions/create), [subscription retrieval](https://docs.stripe.com/api/subscriptions/retrieve), [hosted customer portal](https://docs.stripe.com/api/customer_portal/sessions/create).
- [Stripe webhook signatures, duplicate delivery, and ordering](https://docs.stripe.com/webhooks), [webhook endpoint registration](https://docs.stripe.com/api/webhook_endpoints/create).
- [Lemon checkout creation](https://docs.lemonsqueezy.com/api/checkouts/create-checkout), [raw-body signatures](https://docs.lemonsqueezy.com/help/webhooks/signing-requests), [subscription lifecycle](https://docs.lemonsqueezy.com/api/subscriptions/the-subscription-object).
- [Lemon invoice ordering](https://docs.lemonsqueezy.com/api/subscription-invoices/list-all-subscription-invoices), [refund amount fields](https://docs.lemonsqueezy.com/api/subscription-invoices/the-subscription-invoice-object), [webhook registration](https://docs.lemonsqueezy.com/api/webhooks/create-webhook).
- [Cloudflare Pages project configuration](https://developers.cloudflare.com/api/resources/pages/subresources/projects/methods/edit/).
