# Field Work Pack

## Product boundary

Field Work Pack is one English-only Blazor product at `/field-work-pack/`, catalog ID 98. Its eight templates are content within that product, not eight sites. The main workflow is local and does not require an account, a third-party API key, or a network after the application is loaded. Its interface is plain, uses dark text and sharp controls, and has no decorative three-dimensional scene or advertising placement.

The editable examples cover equipment handover, studio preparation, exhibition setup, office moving, welcome-desk preparation, rental return, print-order pickup, and pop-up stock delivery. They describe administrative records rather than medical, technical, or hazardous-work instructions. No completed jobs or customer activity are fabricated. Customer/site and reviewer labels are optional user-entered text; personal addresses, email addresses, and attendee details are not required.

## Files and responsibilities

| File under `src/BinSite.Social/` | Responsibility |
| --- | --- |
| `Pages/FieldWorkPack.razor` | Checklist editor, work-order execution, unresolved items, explicit workflow actions, local imports/exports, optional real cloud panel |
| `Pages/FieldWorkPack.razor.css` | Scoped, functional high-contrast layout |
| `Models/FieldWorkModels.cs` | Portable workspace, checklist snapshots, typed answers, actual photo data, lifecycle and local history |
| `Services/FieldWorkRules.cs` | Definition/answer validation, bounded project serialization, lifecycle transitions and photo-byte checks |
| `Services/FieldWorkExamples.cs` | Eight distinct editable starting definitions, with no fake completed work |
| `Services/FieldWorkReport.cs` | Escaped self-contained HTML/print report containing supplied answers, actual images, outstanding items and local history |
| `Services/FieldWorkCloud.cs` | Metadata-only server transport, actual local photo rebinding and typed first-party API responses |
| `wwwroot/field-work.js` | IndexedDB/localStorage fallback, actual image decoding/resizing, file downloads, print window, clipboard and offline preparation |
| `wwwroot/field-work-sw.js` | Narrow product-scoped offline asset cache, excluding APIs and foreign origins |

The catalog entry is `catalog/business-field.json`. The existing `SocialApi` handles authenticated same-origin requests and content-free diagnostic metadata. No shared helper, server API, account service, or advertising module was changed for this implementation. The root publication step produces the actual public bootstrap and copies the JS/worker assets.

## Work-order contract

A checklist contains 1–40 confirmation, numeric measurement, or photo fields. Each has a unique identity, a label, instructions, and a required flag. Measurements may have inclusive minimum/maximum values and a unit. An inverted range cannot be saved as a valid checklist. A work order receives an independent snapshot of its source checklist revision; editing the library does not change an order already created.

| Current state | Explicit action | Gate |
| --- | --- | --- |
| Plan | Start work | A valid definition and a nonempty order title |
| In progress | Request review | Required confirmations, parseable/in-range numbers, and actual required local photo bytes |
| Ready for review | Complete reviewed order | The complete evidence check runs again |
| Ready for review / Complete | Reopen for changes | Answers remain; completion time is cleared and review must be repeated |

Numbers use an English decimal dot. Empty required measurements are missing; a valid numeric zero is not treated as empty. An optional measurement may be omitted, but an entered invalid or out-of-range value remains a validation issue. Reviewed/complete input controls are locked until explicitly reopened. The domain `Touch` operation also invalidates completion if a completed order is changed programmatically through normal model use.

## Actual photo evidence

Selection reads an actual local file, not its filename alone. The JS module decodes JPEG/PNG/WebP pixels, rejects sources over 8 MB or forty million pixels, resizes to a maximum side of 1,600 pixels, and creates a bounded JPEG representation. The original file bytes supply the SHA-256 source fingerprint. The resized data URL, actual dimensions and byte count are saved in the local project.

C# checks a supported raster prefix, bounded decodable base64, actual nonempty bytes, MIME-specific signatures, dimensions, byte count and fingerprint format. The browser additionally decodes every selected, imported or restored local image and compares its dimensions. The C# signature check is not claimed to be a full image codec. A metadata reference with no data URL never satisfies a required photo field. The fingerprint supports matching a reattached original file; it does not establish capture time, location, ownership, or an authenticated inspection.

Photo preparation captures its workspace, order, requirement, answer and existing photo before reading or decoding asynchronously. Selection controls are disabled while the operation is busy, and the selection method also refuses a busy change. Before committing, the captured order must still belong to the same workspace, remain In progress at the captured revision, and contain the same requirement, answer and prior photo. A changed target, decoder failure or invalid image leaves the previous evidence unchanged. A prepared photo is never redirected to whichever order happens to be selected when decoding finishes.

## Persistence and reports

Committed field changes save the complete local project to IndexedDB. If that browser storage path fails, a small-project localStorage fallback is attempted and verified. Failed writes are reported as failures while data remains in the open page; they do not become false “saved” messages. When both stores have data, their project timestamps select the newer candidate. The workspace supports at most thirty work orders, fifty templates and twenty-five megabytes of serialized UTF-8 JSON. A selected order can be exported separately with its checklist snapshot and images.

Imports are parsed, structurally validated, and all image payloads decoded before replacing the current project. A nonempty existing workspace requires an explicit replacement confirmation. New workspaces and local imports detach from a previously selected cloud document, so a later cloud save cannot silently target the prior project.

HTML reports are standalone and include actual attached data URLs, all field values, evidence notes, unresolved requirements and local workflow history. Supplied text is HTML-encoded. Reports with outstanding items, or without a completed local review state, are explicitly labeled Draft. Printing uses the same HTML and waits for embedded images before opening the print dialog. A local reviewer label is not an authenticated signature or professional certification.

## Optional private workspace integration

The cloud panel uses the existing `/api/commerce/workspaces` API and real member cookie. Opening it, refreshing, or saving is explicit; local startup does not depend on that API. Account creation/recovery links to the actual `/uyelik/` page. The panel reads real allowances, roles and revisions, then supports create/read/update, single-use viewer/editor invitation creation and claiming, version listing, and opening a historical metadata snapshot locally. Saving requires the server revision that was edited. A conflict is surfaced without automatic overwrite. Viewer access cannot update the shared server document; local-copy editing remains distinct from an authorized server save.

Every invitee can see the whole shared metadata body, including its labels and notes. There is no claim of field-by-field privacy. Photo pixels are stripped from every cloud transport, retaining filename/dimensions/source fingerprint metadata. A cloud copy with missing required photo bytes is returned to In progress rather than falsely counted as reviewed/complete. On opening metadata, actual images already present locally may be rebound by source fingerprint; otherwise the original file must be selected again. The full local JSON/HTML exports remain the photo backup.

The panel displays the account's actual combined project allowance and metadata/member/history limits. Current API limits are enforced by the server; local preflight checks do not grant premium rights. The root owner independently reported live D1 checks of ownership, invitations, role enforcement, revision conflicts, history and archive/restore. This product's C# transport/metadata checks do not claim a separately completed browser collaboration session.

## Offline boundary

Once loaded, the editor, local evidence operations, workflow validation, JSON export and report generation do not require the server. The explicit Prepare offline reopening action registers `/field-work-sw.js` only for `/field-work-pack/`, then caches the clean published entry, local module, exact `/social.css`, `/social.js`, `/BinSite.Social.styles.css` bootstrap dependencies, the Blazor startup script, and observed framework resources. Preparation requires the named dependencies and an observed WASM resource, fetches them successfully, and checks that each selected response is actually retained before returning success. API, cross-origin, invitation-query navigation, and optional advertising/revenue requests are excluded. The offline navigation claim applies only to the clean `/field-work-pack/` path. A failed cache setup reports the failure rather than pretending the app is installed. Actual installed-worker offline reload must be checked after the real public bootstrap is deployed; no such browser installation is claimed here.

## Execution evidence

The existing .NET 10 Release project built with `--no-restore`, zero warnings and zero errors after adding the full component and cloud panel. No package or tool was downloaded.

Forty-five checks compiled and executed the production model/rules/examples/report/cloud source files in a disposable .NET console harness. They cover eight valid distinct examples, frozen checklist snapshots, real workflow gates, invalid and inclusive-bound numeric values, required-photo metadata rejection, actual embedded PNG byte round trips, separate review/completion, reopening, malformed/unknown/duplicate imports, HTML escaping, truthful draft reports, stripping cloud image bytes without modifying local originals, missing remote evidence, source-fingerprint local reattachment, ordinary free-size metadata, and typed server revision/role deserialization.

Node.js parsed both JS files. Module checks executed the fallback storage write/read, full-storage failure, missing photo-data rejection, byte-count mismatch and unsupported MIME rejection. Worker routing checks accepted only intended local application assets and rejected API/cross-origin URLs. These checks used controlled browser-interface fixtures. They do not claim a real IndexedDB browser transaction, photo codec acceptance, completed browser download/print dialog, service-worker installation, or full browser invitation session.

A subsequent independent audit reproduced a cross-order photo race by pausing the actual component's `preparePhoto` call, selecting order B while order A's image was pending, and completing the image preparation. The updated Razor component was then compiled separately in a disposable project against the unchanged production model/service assembly, without rebuilding the shared application. Seven targeted checks passed: busy selection refusal; the same A/B scenario attaching only to A; forced selection changes retaining A as the captured target; a newly locked target retaining its old photo/revision; a replaced workspace refusing the detached target; decoder rejection preserving evidence; and an invalid prepared image preserving evidence. The exact A/B output was `Order A received its photo: True` and `Order B received Order A photo: False`. These controlled asynchronous component checks do not claim an exercised browser image codec or real browser interaction.
