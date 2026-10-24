# Hosting capacity for the 1,000-product goal

Observed on 2026-10-24: the authenticated Cloudflare D1 inventory returned four databases (`total_count: 4`). This is a resource count, not a product or user count. The live product directory contains 73 products; most existing entries share the BinSite deployment, while completed independent services link directly to their own origins.

Current primary documentation lists [ten databases and 500 MB per database on D1 Free](https://developers.cloudflare.com/d1/platform/limits/), and [100 Pages projects per account with 20,000 files per Free project](https://developers.cloudflare.com/pages/platform/limits/). Those documented capacities are not proof of this account's remaining request, storage or billing allowance. No paid plan was purchased during this work.

The user explicitly allows different site URLs under shared hosting provided each product controls its own interface and the parent does not restrict it. Consequently, the final goal does not require 1,000 hosting projects or databases. Independent source folders and product behavior must remain distinct even where deployment resources are shared.

Before an actual resource limit is reached, the next allocation can use bounded hosting groups and separately named application tables, or another suitable free service. That is a planning option; a shared-database migration or shared service gateway has not been implemented or tested. Existing live databases and source are preserved. Do not treat this proposal as an instruction to migrate a functioning project, upgrade a plan or weaken application access boundaries.

Actual file, request and storage measurements must determine group sizes. A design/layout template is not part of resource sharing. New products count only after their complete implemented flow is published and verified, regardless of their host or number of URLs.
