# Manual UI review — Catalog Studio services and exported catalog

Scope: the model, five services and browser bridge listed below, followed by the coordinated editor/guide integration pass at the end of this record. The root agent owns the editor; the later pass documents the specific Razor fixes applied here. Its stylesheet was read but not changed. No automatic text search, regular-expression scan or keyword report was used for this review.

## Complete reading

All contents were printed and read without truncation:

| File under `src/BinSite.Social/` | Lines |
| --- | --- |
| `Models/CatalogStudioModels.cs` | 1–108 |
| `Services/CatalogStudioCloudClient.cs` | 1–104 |
| `Services/CatalogStudioCsv.cs` | 1–149 |
| `Services/CatalogStudioExporter.cs` | 1–61 |
| `Services/CatalogStudioSamples.cs` | 1–114 |
| `Services/CatalogStudioWorkspace.cs` | 1–232 |
| `wwwroot/catalog-studio.js` | 1–96 |

`docs/Catalog-Studio.md`, lines 1–58, was also read completely. The generated HTML and its embedded CSS/JavaScript were reviewed as a separate customer-facing interface, not merely as implementation text.

## Exported HTML catalog

- First view: the author's title/description, real product photos, prices, search, category selector and print action directly explain browsing a catalog. There is no need for a description of the software used to produce it.
- Unnecessary exact copy: “Static catalog snapshot”, “Stock in this snapshot”, and “This is a static product-information snapshot. Prices and stock are supplied by its author; this file does not take payments or update inventory.” The header repeated product count and currency, although the filter count and each price already show them. “No image attached” describes the editor's attachment process.
- Applied: removed the extra header metadata line, shortened the stock label, changed the missing-image label to “No photo”, used “Print results”, and kept one short footer identifying the catalog owner as the source of prices/stock. The exported screen has a calm dark palette; print remains black on white. Actual product illustrations and photos keep their colors.
- Useful behavior retained: all search words are required; names, codes, descriptions and categories are searched. Category comparison is case-insensitive in the same English locale used by the existing script. Filtering hides actual cards; printing includes only the visible cards and waits for their images. Text remains HTML-encoded, images remain embedded, external resources remain blocked, and no checkout or live-inventory behavior was invented.
- Confirmed narrow issue: after filtering to zero products, Print remained enabled and opened an effectively empty catalog. The generated script now disables it when no card matches, re-enables it when results return and also refuses a direct zero-result print action. An image-decoding failure still blocks printing and now says “A photo could not be loaded. Printing was stopped.”

## Editor behavior exposed by services

- CSV parsing preserves quoted separators, escaped quotes, quoted newlines and the physical starting line of each record. Automatic separator detection reads only the first logical record; explicit comma/semicolon/tab choice remains useful when a header is ambiguous. Column suggestions are editable aliases, not a successful import. SKU, name and price must be mapped to distinct source columns before importing.
- Row-value errors remain repairable records. Excluded rows stay in the project but do not appear in or block an export. Duplicate included codes, invalid prices and invalid stock do block export; missing photos are warnings and create the visible No photo state. The UI should distinguish errors from missing-photo warnings rather than treating every issue as an export failure.
- Photo matching first uses an exact case-insensitive image filename hint; otherwise it uses the filename stem matching the product code. Existing assigned image IDs are retained. Several candidate images remain unassigned, requiring a manual image choice. Attaching a file with a matching fingerprint can reconnect an earlier image reference without changing the product's image ID.
- Bulk changes calculate all proposed prices before changing any selected row. One invalid price prevents the entire price adjustment; a blank stock input can intentionally clear stock when the caller passes an empty string instead of null. Selection scope belongs to the editor, so “all matching” must be distinguished from a visible page.
- Prices use the chosen decimal separator without thousands grouping; currency is a label, not a conversion. HTML export displays the validated number to two decimal places plus that currency. This behavior needs brief input guidance, not an account of CSV grammar or parser implementation.
- The browser bridge only reads selected files, resizes images, fingerprints originals, downloads files and opens the generated print view. It has no `initStatus` or status-writing function. The earlier suggestion that it erased the sample label was corrected by the root agent after reading the component's actual status handling.
- The popup fallback is useful: if a print-preview window is blocked, the bridge downloads the same printable HTML instead. Native popup/print operation is not established by this source review.

## Original examples and preservation

- The twenty products, quantities and prices are fictional first-party content. Their descriptions and different illustrations are useful examples and were retained. The project description now says “An example collection of 20 fictional products. Prices and stock are examples.” instead of a longer demonstration/process disclaimer.
- Existing saved example SVGs are accepted by an exact byte allowlist. The SVG drawings, colors, labels, fingerprints and generated bytes were deliberately left intact; changing them for this presentation review would make older saved example projects fail validation. No model format, identifier, quota or stored product was changed.

## Shared-workspace contract

- The adapter uses `/api/commerce/workspaces`, kind `catalog`, with the existing account session. Create/save send product data and image references after stripping every image data URL. Opening a shared body discards incoming image bytes before validation, then reconnects matching images already on the device or the bundled examples.
- Viewer saves are refused; owner/editor saves send the known revision once. A stale response is propagated without an automatic retry or overwrite. Invitations require owner status, a viewer/editor role, explicit acceptance and the current account. All invited members can read the saved body; there is no per-row privacy or online photo backup.
- The outer shared project name remains limited to 100 characters independently from the full catalog title. Its validation message now simply asks for “1–100 characters for the shared project name”. No server permission, model capacity or quota was expanded.

## Verification and changed files

An isolated .NET consumer at `C:\C\BinSiteStaticReadability\CatalogExportChecks` compiled the actual model, parser, workspace, samples and exporter sources without warnings or errors. It generated the real twenty-product HTML plus the exact embedded script. Six controlled document/script checks passed: initial count/print availability; zero-result empty state; refusal to decode/print zero results; print recovery after a matching search; decoding/printing only the visible card; and retained case-insensitive category filtering. The changed file bridge passed Node syntax checking.

These checks do not claim native browser media decoding, a file-dialog interaction, an actual printed page, hosted account requests or visual browser inspection. Existing product source was not changed to support the checks.

Changed files in this scope:

- `src/BinSite.Social/Services/CatalogStudioExporter.cs`
- `src/BinSite.Social/Services/CatalogStudioSamples.cs`
- `src/BinSite.Social/Services/CatalogStudioCloudClient.cs`
- `src/BinSite.Social/wwwroot/catalog-studio.js`
- `review/manual-ui-catalog-services.md`

This group remains unstaged and uncommitted for the coordinated release.

## Later editor/guide integration pass

The final root-owned `CatalogStudio.razor`, lines 1–493, and its stylesheet, lines 1–67, were then read completely as an integration review against the service/bridge knowledge above. No output was truncated and no automatic text search was used. The stylesheet was not changed.

The following narrow editor fixes were coordinated with the root owner and applied to that one Razor file:

- Line 55: Use these products is unavailable when the selected separator no longer matches the separator used to parse the preview.
- Line 199: a `ParsedDelimiter` value accompanies `ParsedCsv`.
- Line 300: successful parsing records both input text and separator.
- Line 310: the import action itself rejects stale text/separator state, not just its button.
- Line 305: Try an example resets separator detection to automatic for the known comma-separated example. Retaining a previously selected semicolon/tab otherwise parsed that example as a single column.
- Line 124: editing the image-filename hint now refreshes issues. Without that binding, the visible issue list stayed stale while render-time export validation already used the repaired value.
- Line 366: print-preview feedback names the current generated catalog button, Print results.
- Lines 160 and 174: sharing states that invited people can read all product text/filenames, and viewer copy explains actual local editing/new-online-copy behavior. Server write roles are unchanged.

The issue row IDs, detail-description ID, focus targets, column mapping controls, outer Busy fieldset, export-selection computation, viewer-save disabling, and markup sections were checked against their bound handlers. No additional missing field/handler or closing-tag issue was identified in this snapshot. This is a source finding, not a native browser test.

The three complete guide sources were also read before rewriting: `public/catalog-studio/guide/index.html` lines 1–31; `public/field-work-pack/guide/index.html` lines 1–26; `public/review-room/guide/index.html` lines 1–26; and the complete single line of `public/guides/guide.css`. Both Catalog Studio example CSV files were read, confirming their five rows and four deliberate repair cases; their data was not changed.

Each guide now has one tool-opening action, five practical steps using current UI names, and a short section on actual file/share consequences. Framework/proof language, repeated promotion, CSV grammar lectures and defensive production claims were removed. The two catalog example downloads remain linked; Field Work Pack's handover example and Review Room's anchored-feedback example remain. The shared guide stylesheet uses a calm dark document layout. Existing appearance markers, canonical URLs and revenue inclusion remain.

No test infrastructure was added for this integration pass. The root owner performs the coordinated tracked-source build. The changed integration manifest is the one Razor file, these four guide files, and this review record. No files in this pass were staged, committed or pushed.
