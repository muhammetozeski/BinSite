# Independent products and shared services

Each counted product owns a distinct `public/<slug>/index.html` entry and user interaction. The catalog builder reads JSON arrays in `catalog/`, rejects duplicate IDs and slugs, and requires each product entry point to exist. `scripts/check.mjs` parses JavaScript sources and verifies paths; this does not prove browser interactions.

The portfolio homepage is a directory, not a parent application shell. Static games and editors control their own interface and can be moved independently. Shared code or hosting does not qualify a theme or dataset variation as a separate product.

Data-backed products use a .NET 10 Blazor web client under `src/BinSite.Social`; network persistence is provided by Pages Functions under `functions/api` with the D1 binding `DB`. The browser client is compiled C#; the serverless bridge is JavaScript. Cloudflare's documented Pages deployment model supports Blazor WebAssembly, not an ASP.NET server process: https://developers.cloudflare.com/pages/framework-guides/deploy-a-blazor-site/ . This source is a platform constraint, not proof that a specific build has been published.

Source is public at https://github.com/muhammetozeski/BinSite. Provider credentials are stored in GitHub Actions or hosting secrets; `.gitignore` excludes local environment files, private keys, user data, caches and build outputs. Merely adding a tracked secret to `.gitignore` would not remove it from history, so credentials must never enter a commit.

The social schema uses parameterized statements, server-side limits and independent records for contributions, votes, locked messages and abuse reports. Source-level contracts and execution evidence for those APIs belong in the social implementation note after its checks complete.
