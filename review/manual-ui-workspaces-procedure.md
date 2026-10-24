# Manual UI review: Procedure Exchange

This review reads the full UI, browser bridge, domain/content/export/client sources and live API implementation as source. It follows the first screen through editing, local runs, public versions, reported outcomes and private copies. No automatic text search, regular-expression audit scan, keyword report or UI-word inventory was used. Existing application regular expressions were read as implementation code. Source reading and isolated execution are separate evidence.

## Complete reading record

Paths are relative to `C:\E\Claude\WebSiteleri\BinSite`. Ranges refer to the original source displayed before these edits.

| File | Fully displayed range |
| --- | --- |
| `src/BinSite.Social/Pages/ProcedureExchange.razor` | 1–82 |
| `src/BinSite.Social/Pages/ProcedureExchange.razor.cs` | 1–75 and 76–140 |
| `src/BinSite.Social/Pages/ProcedureExchange.razor.css` | Whole single-line file, 6,050 bytes, displayed with `Get-Content -Raw` |
| `src/BinSite.Social/Models/ProcedureExchangeModels.cs` | 1–109 |
| `src/BinSite.Social/Services/ProcedureExchangeClient.cs` | 1–43; both public and vault adapters |
| `src/BinSite.Social/Services/ProcedureExchangeContent.cs` | 1–45 and 46–85; every action/check of all twelve authored methods |
| `src/BinSite.Social/Services/ProcedureExchangeExports.cs` | 1–40 |
| `src/BinSite.Social/Services/ProcedureExchangeRules.cs` | 1–145 |
| `src/BinSite.Social/wwwroot/procedure-exchange.js` | 1–26 |
| `functions/api/commerce/procedures/[[path]].js` | 1–90 and 91–156 |

Also read completely: `docs/Procedure-Exchange.md` 1–47 and `docs/Procedure-Service.md` 1–31. The unchanged shared `SocialApi.cs` 1–57 and commerce handler 1–353 had just been read completely for the Signal/Shift review; their ownership, request/error and private-project contracts were reused rather than read again. No displayed output in this group was truncated.

## First screen and file controls

- The title and “Write a method, follow its steps and share improvements.” already explain the task. Kept them; removed the extra All products link from the product header.
- The first screen repeated publication/private-state boundaries in a full banner, file paragraph, author publication section and footer. The banner now keeps the immediate consequence: “Download your project before closing; this tab does not save automatically.” The file controls state only that project JSON includes private runs while method exports contain instructions and attribution.
- Removed “Local files can be edited by their holder; they are not proof of execution or ownership.” from everyday file instructions. File restoration still clears publication overwrite targets and validates the full document; the actual protection did not depend on this sentence.
- Shortened the tabs and file actions to Runs, Published methods, Private copies, New method, Download project JSON, Open JSON, Method JSON and Printable method. Their original handlers and distinct export payloads remain.
- Starting methods remain a separate authored corpus rather than fabricated public records. Removed “12 original starting methods” / “Examples you can use or adapt.” repetition; all twelve complete source methods remain unchanged.
- Replacing a truly blank project no longer asks to preserve nonexistent work. Any draft text, added steps, source provenance or run still triggers the replacement confirmation.

## Editor and source comparison

- Retained title, summary, category, tags, required inputs, output criteria, actionable steps, observable done checks, timing, reordering and removal. The domain still preserves stable IDs, but raw generated step IDs are no longer displayed beside every step.
- Removed “Estimates are planning hints, not promises. Stable step IDs allow meaningful comparison after a fork.” The concise local instruction is “Write a Done when check that someone can inspect.” Actual duration values, source comparison and identities are unchanged.
- Shortened update-target and fork controls while retaining the expected revision and the fact that publishing creates a new version. Source attribution still carries its exact identity/revision; imported files do not become ownership proof.
- The comparison is explicitly the last comparison. “No saved instruction differences in the latest comparison.” now says “Last comparison: no differences. Refresh after editing.” The Refresh comparison control and readable source instructions remain; stale comparison data is not labelled a current equality check.
- Local Check method now rejects tags equal after trimming/case-insensitive comparison, matching the actual publish handler's distinct-tag rule. Previously a method with `Review` and `review` passed the local ready check and then failed publication. Incomplete private drafts still retain repairable duplicate tags.
- Publishing still requires an account, publisher alias and explicit copying/export/fork permission. Removed repeated “self-supplied / not a verified real name” narration and the long protocol prose. One sentence at publication says the method becomes public and private work belongs in Runs. The client still posts only the canonical public document and source pin.

## Local runs and portable outputs

- Earlier runs retain their own immutable instruction snapshots. Kept that useful fact and removed the repeated “IMMUTABLE INSTRUCTIONS · PRIVATE EXECUTION RECORDS” banner and cryptographic snapshot explanation from the working screen. Full source attribution and the actual instructions remain visible; hashes and strict validation remain in the file/domain format.
- “Save local run fields” becomes Check entries: its actual function validates/copies the in-memory fields, not durable browser saving. The banner already explains how to persist them.
- The finishing instruction now matches the real rule: every result needs a note; Completed additionally requires all checks and an artifact. Partial/blocked still represent explicit local results, and finished records still require Reopen before edits.
- Retained actual inputs, observed checks, private notes/evidence, artifact/file references and result note. Shortened redundant declarations and success messages without fabricating work or approval.
- The private-report checkbox remains explicit and disabled during busy actions. Its captured pre-await choice still controls both the exported HTML and completion message. The report defaults to excluding private execution text.
- Retained method JSON, printable method, complete private project, run HTML and actual result-text outputs. Removed generic report footers such as “This report is a portable local record. It does not submit a public outcome or authenticate another person's approval.” The run badge/state still identify self-reported checks; privacy inclusion/omission stays explicit. Exported source, notes, evidence and recorded data are unchanged.
- Run selection and private-copy detachment now guard Busy in their methods as well as disabling their buttons, preserving the targeted record during asynchronous actions. Navigation tabs also disable during an active action.

## Public methods, outcomes and private copies

- Public reads still use real search results, exact versions, source pins and server-owned visibility. There are no authored examples substituted into public search and no fabricated participation counts.
- Shortened “Run this exact published revision locally”, “Fork into my editable draft” and the owner-update actions. Exact revision semantics remain in the handler and visible revision label.
- Removed repeated source/alias disclaimers. Retained actual author alias, version, timestamps, source attribution, withdrawn/hidden state and the distinction between updating an owned publication versus making a new fork.
- The revision input now exposes the actual server range 1–100. Next-page disables at the real offset bound and tells the user to narrow the search rather than presenting another request that the service must refuse.
- Outcome counts remain exact account self-reports per selected revision. The long “not verified executions, people or a measured success rate” paragraph is replaced with the actual participation rule: one self-report per account/revision, replaced when changed. The note is clearly public, requires sign-in and is never filled from private runs.
- Publish, outcome submission and withdrawal now preserve the mutation acknowledgement if the following detail GET fails. No write is automatically retried. Publication keeps the acknowledged new update target; an acknowledged withdrawal immediately disables public actions. Retained detail data is marked potentially stale until an explicit revision read succeeds. A failed write is still an error and never receives a success message.
- Private copies retain the real sign-in path, owner-only save/load behavior, separate saved-copy name, 64 KiB body limit, account capacity and explicit overwrite confirmation. Removed “actual owner vault”, “complete private project JSON” and repeated full-title/outer-label implementation explanations. The selected-copy warning still states that replacement overwrites contents without history or conflict protection.
- The browser file input now resets in `finally` after oversized or failed reads, allowing the same file to be selected again after repair. The bridge still performs no network fetch or automatic local storage.

## API matching and boundaries

The read backend matches the UI/client: 20-item filtered pages; immutable revision reads; account-authorized canonical publication; expected-revision updates with 409; pinned fork sources; one replaceable outcome per account/revision; explicit report and owner withdrawal. Public methods include only whitelisted document/step fields. Private projects use the existing owner-only commerce vault, not collaborative workspaces. Neither backend nor shared transport was edited.

The service does not provide a publication-creation idempotency key. This review separates an acknowledged successful mutation from its failed refresh; it does not claim exactly-once recovery after a write response itself is lost. No mutation retry was added.

## Executed checks and manifest

- Disposable consumer: `C:\C\ProcedureUiReview\Review.csproj`. It compiled the current verbatim Razor/code-behind and actual models/content/rules/exports/client/HTTP adapter in an isolated .NET 10 project. Page wrapper, HTTP and browser bridge were upper-layer substitutes; no production test exports/hooks were added.
- Twelve assertions passed: repairable private duplicate tags versus public rejection; publication acknowledgement and one mutation; outcome acknowledgement with stale-detail state; withdrawal acknowledgement with immediate visibility flags; failed write never succeeding; blank replacement avoiding an empty confirmation; busy selection/detachment; both paused-download privacy directions preserving exact HTML and matching status.
- `C:\C\ProcedureUiReview\bridge.mjs` passed three actual-module checks: successful, oversized and rejected file reads all reset selection; success returns the exact file text.
- These results are source/isolated component and HTTP-consumer evidence. They are not a claim of browser layout, live publication/account operations, native file dialogs or another deployment acceptance. No shared application build was started.
- Changed production sources: `Pages/ProcedureExchange.razor`, `Pages/ProcedureExchange.razor.cs`, `Services/ProcedureExchangeRules.cs`, `Services/ProcedureExchangeExports.cs`, `wwwroot/procedure-exchange.js`, all under `src/BinSite.Social`. CSS, model, public/vault adapter, authored content and backend were read without changes.
- The five source files and this record remain unstaged for the parent's integration order. No commit or push was performed for this group.
