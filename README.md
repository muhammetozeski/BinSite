# BinSite

A growing collection of independent English-language web products. The target is 1,000 working projects, increasing in scope. Only implemented and verified projects count as delivered. Revenue and audience size are unverified until observed.

## Structure

- `public/<slug>/index.html`: independent web product; no shared page shell is required.
- `catalog/*.json`: implemented product metadata, one file per work batch.
- `public/index.html`: searchable directory generated from the catalog.
- `scripts/`: local build, checks, and deployment tools.
- `docs/`: architecture, product decisions, and deployment evidence.

Use standard HTML, CSS, and JavaScript when sufficient. Every product must provide a useful or entertaining working interaction; changing a title, color, dataset, or prompt alone does not create another delivered product. Do not add fake users, fabricated engagement, unsupported revenue claims, or pretend network features.

Product catalog entries use `id`, `name`, `slug`, `category`, `stage`, `description`, `features`, and `businessHypothesis`. Business hypotheses are proposals, not observed outcomes. Save no secrets in source or generated files. New products remain in this permanent directory. Source is public at https://github.com/muhammetozeski/BinSite, as requested by the user.

Products have distinct paths on the main website. The four larger applications retain their permanent source repositories and are built from exact revisions in `integrations/mounted-products.json`; their layouts are independent. Existing services and old addresses remain available for compatibility, while the directory's primary links use the main origin. See [mounted products](docs/Mounted-Products.md) for publication, routing, data and environment boundaries. The delivery ledger records verified URLs, so changing an address requires a new check.

## Local use

Individual static products can be opened directly. A local or deployed HTTP origin is required for shared server features.

The public directory is https://muhammetozeski-binsite.pages.dev/ . Pushes to `main` compile the C# browser client, build the directory from completed catalog entries, check sources, and deploy with GitHub Actions. Run `scripts/Verify-Live.ps1` after a successful deployment to update the HTTP delivery ledger; interaction checks are recorded separately.

Product interfaces and bundled content are English-only. The latest brief replaces hard colors with calm, readable interfaces, concise task descriptions and prominent working controls. Do not display language badges or implementation notes such as framework names and shared-server storage. The purpose should be apparent from the controls and an immediately usable example. Tools and games are separate directory groups. Games need meaningful graphics. Prioritize useful workflows with existing service APIs over more small concepts. Advertising is based on publisher networks such as AdSense; the direct-sponsorship workshop has been removed.

## Project knowledge

Start at [the documentation index](docs/İçindekiler.md) for architecture and verified implementation knowledge. Apply [the writing guide](docs/Documentation-Guide.md) before changing knowledge documents. Read [the experience handover guide](Walkthrough/Walkthrough-Guide.md) and choose dated records by subject before resuming an interrupted task.
