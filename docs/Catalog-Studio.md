# Catalog Studio

Catalog Studio is a .NET 10 Blazor browser product at /catalog-studio/. It is one product with a complete local catalog workflow. Its identifier is 96; the catalog declares runtime blazor, category Workspace, stage 7 and adEligible false. The root publication script owns its generated public entry point.

## Source responsibilities

- src/BinSite.Social/Pages/CatalogStudio.razor and its isolated stylesheet own the plain English workflow, row repair, filter/pagination controls and optional account panel.
- Models/CatalogStudioModels.cs defines portable rows, local image copies, source-line issues, header mapping and typed workspace responses.
- Services/CatalogStudioCsv.cs parses and maps delimited input without dropping invalid business values.
- Services/CatalogStudioWorkspace.cs validates products and image payloads, matches images, applies atomic bulk changes and serializes portable or metadata-only state.
- Services/CatalogStudioSamples.cs contains twenty original fictional products and twenty small, distinct SVG illustrations embedded in the assembly. No remote image is fetched.
- Services/CatalogStudioExporter.cs generates actual standalone search/category filtering and print output with encoded author text.
- Services/CatalogStudioCloudClient.cs uses the existing SocialApi and real Membership cookie for metadata workspaces.
- wwwroot/catalog-studio.js is limited to local file reading, raster resizing/fingerprinting, downloads and opening a print preview. It does not parse CSV or compute catalog business rules.

## Local workflow

1. Open or paste CSV and analyze it. The detector considers unquoted separators in the first logical record; comma, semicolon or tab can also be selected explicitly. Revisit each header mapping. SKU, name and price are required.
2. Import the mapped rows. This replaces rows while preserving the local image bank and one Undo snapshot. Quoted delimiters, doubled quotes and quoted newlines are retained. Error rows refer to the physical line where each CSV record started.
3. Attach PNG, JPEG or WebP photos. A precise image-filename hint wins; otherwise an exact case-insensitive filename stem matches the SKU. Ambiguous matches remain unassigned. The row detail editor offers an explicit manual image choice.
4. Search or filter products. Selection can span all matching rows, not only the visible page. Bulk category, stock and percentage changes are explicit fields. Invalid selected prices prevent any bulk change. Excluded rows remain in JSON but do not block or appear in an export.
5. Repair errors or exclude invalid rows. Duplicate included SKUs, missing names, invalid amounts and invalid stock are reported with source rows. Missing photos are warnings and produce honest placeholders.
6. Save a portable JSON project before closing or reloading. The local session is held in tab memory; automatic durable browser saving is not claimed. A JSON project includes resized local image copies.
7. Download searchable HTML or open the print preview. The file contains its images and filtering script and needs neither an account nor a hosted API. Printing uses currently visible results and waits for their images to decode. A failed decode is reported instead of silently printing missing photos. A blocked popup falls back to a printable HTML download.

Prices use the explicitly chosen dot or comma decimal separator with no grouping separators. Currency is an author-selected three-letter label; the tool performs no currency conversion, checkout, tax calculation or live inventory synchronization. All sample prices, specifications and quantities are explicitly fictional.

## Limits and data boundaries

CSV is limited to five million characters, 2,000 products, 40 columns and 5,000 characters per source cell. Product validation adds tighter practical field limits. Each image batch permits 80 files of up to 8 MB; browser rasterization reduces the maximum edge to 1,200 pixels. The bank permits 200 images and 40 MB of image data. Portable JSON is bounded to 45 MB.

Uploaded raster files are decoded and resized locally. C# accepts their matching raster data signatures; arbitrary SVG payloads are rejected. Only the exact application-authored demo SVG bytes are allowed. A short signature check is not a full image decode; browser decoding is a separate operation.

All author text is HTML-encoded for content and quoted attributes. User data is not interpolated into executable JavaScript. Exported documents restrict network access through a self-contained content policy. The component logs counts and error types rather than CSV text, photos, private identifiers or invitation tokens.

## Optional private metadata workspace

The existing SocialApi communicates with /api/commerce/workspaces. Create and save strip every image data URL before submitting the body; only product data and filename/fingerprint references are shared. On opening, remote image bytes are discarded, then references may reattach only matching images already held on the device or original bundled demo assets. Reattaching an actual photo requires selecting the local file.

Real Membership accounts and server authorization are required for cloud actions. The server controls project/body/member limits and owner/editor/viewer roles. Viewers can edit a local copy but cannot save over the shared server workspace. Saves submit the known revision with no automatic mutation retry. A conflict preserves local edits.

The private server workspace label is an explicit 1–100-character field separate from the full catalog document title. Create and update validate that label before sending; the full document title stays unchanged inside the metadata body.

Invitations are owner-created viewer/editor tokens with expiry in Unix milliseconds. They are displayed for the user to copy; nothing is automatically sent to another person. Opening a remote catalog retains one Undo snapshot. Restoring JSON, loading the demo or undoing a workspace detaches the previous cloud binding to avoid silently targeting a different server project.

## Verification evidence and limits

The C# browser project built without warnings or errors after the local workflow and cloud adapter were added. A disposable test project was created with NewTemp at C:\C\CatalogStudioChecks; it uses installed framework references and an empty NuGet source list, with no package download.

Twenty-three checks exercised quoted/multiline CSV and source lines, separator/decimal rules, malformed structure and mapping, row errors, excluded duplicates, atomic bulk and image overflow, local project round trips, ambiguous image matching, arbitrary SVG rejection, encoded HTML, metadata stripping/rebinding, the real API path/payload, stale-revision propagation without duplicate retries and client viewer-write rejection. Adapter account/API transport is captured in memory, not a browser-to-production account session. The root owner separately verified the real workspace service.

The final Release build also completed with zero warnings and zero errors. JavaScript file-bridge checks covered text size bounds, unsupported/oversized image rejection, input resets, filename sanitization and popup fallback. The script extracted from the actual generated HTML was executed against a mocked document: multi-term search, category/no-result states, decoding visible images before printing and refusing to print after a failed decode all passed. This does not establish native file-picker or printer behavior.

Independent source review found that deduplicated category options used case-insensitive C# comparison while exported filtering originally compared case-sensitively. The export now case-folds both category values with the same English locale. A focused script check with Tools, tools and TOOLS preserved both matching products; a distinct Garden option still selected only its own product.

A captured production adapter request reproduced the local-title/server-label length mismatch: a valid 120-character catalog title was originally sent to the API's 100-character label field. The separate label now preserves the entire document title. Three additional checks verified create separation, oversized-label rejection before transport, and save separation, bringing the isolated C# total to twenty-six. These final fixes do not change the shared API's limits; the root owner performs the next combined UI build.

Native file-picker interaction, perceptual image fidelity, popup permission behavior and actual printer output are not established merely by a successful C# build.
