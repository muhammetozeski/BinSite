# BinSite

A growing collection of independent Turkish web products. The target is 1,000 working projects, increasing in scope. Only implemented and verified projects count as delivered. Revenue and audience size are unverified until observed.

## Structure

- `public/<slug>/index.html`: independent web product; no shared page shell is required.
- `catalog/*.json`: implemented product metadata, one file per work batch.
- `public/index.html`: searchable directory generated from the catalog.
- `scripts/`: local build, checks, and deployment tools.
- `docs/`: architecture, product decisions, and deployment evidence.

Use standard HTML, CSS, and JavaScript when sufficient. Every product must provide a useful or entertaining working interaction; changing a title, color, dataset, or prompt alone does not create another delivered product. Do not add fake users, fabricated engagement, unsupported revenue claims, or pretend network features.

Product catalog entries use `id`, `name`, `slug`, `category`, `stage`, `description`, `features`, and `businessHypothesis`. Business hypotheses are proposals, not observed outcomes. Save no secrets in source or generated files. New products remain in this permanent directory.

## Local use

Individual static products can be opened directly. A local or deployed HTTP origin is required for shared server features. Publication details will be added after actual deployment.
