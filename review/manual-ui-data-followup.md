# Manual UI follow-up — data applications

This pass follows the current task and the earlier [main application review](manual-ui-main-apps.md). Files below were read as source, not scanned for keywords. Existing content, user records, API payloads and backend rules were preserved. Changes and source reading are not claims of a complete graphical-browser interaction or deployment.

## Mini Tournament

Read `Pages/MiniTournament.razor` and `.razor.css` in full. The collaboration API's existing tournament/review definitions were also read when checking the review-note requirement below.

- “Manage a real elimination bracket” was an unnecessary title claim; the title is now the product name.
- “Close entry and build bracket” became “Start tournament”; winner actions still name the player and now say “Name wins”.
- The four/eight-player selection, roster, bracket and irreversible winner progression remain. Refresh list and Refresh bracket are different reads and were retained.
- Replaced dark-on-dark green buttons with readable subdued colors without changing the bracket layout.

## Book Companion

Read `Pages/BookCompanion.razor`, `.razor.css` and `Services/CollaborationEditorial.cs` in full.

- “Reading table” became “Readers”; the comment prompt became “Your comment”.
- All four original three-chapter stories, chapter previews, discussion and self-reported progress remain. Story content is the product's reading material, not interface clutter.
- The book typography/layout remains; white source surfaces and dark controls were changed to readable dark defaults.

## Micro Academy

Read `Pages/MicroAcademy.razor`, `.razor.css` and the full editorial lesson source above.

- Removed “actual progress” from the title and a repeated paragraph explaining where the example could be added.
- “Create course draft”/“Add section to draft” became “Create course”/“Add section”. The instruction about published questions being fixed remains because it affects the publishing decision.
- Completed, locked, unenrolled or draft-only quiz choices no longer invite an answer interaction that cannot be submitted. Published lesson text, answer checking, prerequisites and author answer visibility remain.
- Updated the original light stylesheet to a calm dark palette.

## Knowledge Relay

Read `Pages/KnowledgeRelay.razor` and `.razor.css` in full. The existing room states, generated questions and handoff behavior are covered by the main review and the API range read in this pass.

- Replaced flag terminology in the interface with turn/teammate wording. Removed a finish paragraph that merely restated completion.
- “Use Refresh to see your team updates” now appears only when there is a room and a Refresh control.
- Two-player minimum, delayed handoff, roster/progress, manual refresh and Play again remain. Play again is disabled while an action is in progress.
- Corrected the orange-on-dark control contrast without changing the game layout.

## Creative Partnership

Read `Pages/CreativePartnership.razor`, `.razor.css`, all eight task ideas, and the existing collaboration API lines 1–70 in full.

- Shortened “Add a task with a defined deliverable” and the delivery-note prompt; removed the paragraph defending example tasks as unpublished.
- Prepare delivery is unavailable after the existing submission deadline. Existing deliveries and owner reviews remain readable/available.
- Approve and Request revision now honor the existing minimum note length. This is not a new requirement: the review handler executes `task.review=txt(body.review,'Review',10,1000)` before choosing approved/claimed. The field uses `oninput` so the controls respond immediately.
- Kept public-work, payment/rights and deadline consequences. The project/task-column layout remains, with a subdued dark palette.

## YouTube Link Checker

Read `Pages/YouTubeInspector.razor`, `.razor.css` and the complete page-owned `wwwroot/youtube-inspector.js` bridge. The canonical network module's earlier full review remains recorded in the main review.

- “Public details found” became “Details found”; the summary now uses ordinary “X of Y videos found” wording.
- Thumbnail clicks still open the focused preview, where image download remains. Removed the duplicate per-row image-download button.
- Cancel appears only for checking or image work that the bridge can abort; copy/table output no longer offers a nonfunctional Cancel action.
- Removed the generic playback disclaimer. Unavailable details still retain the original input and an Open video link; no statistics or playback success are invented.

## Creator account

Read `Pages/Membership.razor` and `.razor.css` in full.

- “My project vault” became “My saved projects”. Empty recovery/sign-in and untitled-save actions are disabled, with `oninput` binding so typing enables them immediately.
- Removed the duplicate footer Home link and used canonical `/privacy/`. The recovery-key privacy warning, sign-out consequence, subscription dates, provider choices and private-file warning remain.
- Readable dark account panels replace the white/yellow source styling.
- The root-owned `returnTo` parameter, return-path validation and conditional “Return to your release” header were preserved exactly. UI edits are outside those hunks.

## Catalog Studio

Read `Pages/CatalogStudio.razor` in contiguous ranges 1–170, 171–340 and 341–end; its entire stylesheet; `Services/CatalogStudioExporter.cs`, `CatalogStudioCloudClient.cs` and `wwwroot/catalog-studio.js` in full. The earlier [catalog service review](manual-ui-catalog-services.md) covers the unchanged parser/model/workspace/sample sources.

- Simplified the import's separator/column diagnostic sentence. “Use sample CSV” and “Open sample catalog” now distinguish staging a sheet from replacing the catalog with the complete example.
- Removed the “blocking issues/visible placeholders” explanation and repeated searchable-export claims. Actual repair counts, source-row links, file limits and the image-transfer consequence remain.
- Photo reference states and saved-catalog wording replace “reference only” and workspace jargon. The exported catalog no longer carries the unrelated “Prices and stock were supplied by the catalog owner” footer.
- CSV parsing/mapping, included/excluded products, bulk editing, Undo, photo matching and project/catalog outputs are unchanged. Dark table/form colors preserve the existing dense working layout.

## Client Review Room

Read `Pages/ReviewRoom.razor` in ranges 1–120 and 121–end; `.razor.css`; both review model files; `Services/ReviewRoomRules.cs`, `ReviewRoomExports.cs`, `ReviewRoomCloudClient.cs`, `ReviewRoomExamples.cs`; and `wwwroot/review-room.js`, all in full.

- Removed the visible SHA-256/raw-byte explanation and repeated “not customer activity/authenticated signatures” defenses. The exact file matching, separate original-media requirement and shared-note visibility remain.
- Decision buttons now honor the existing three-character note requirement in `ReviewRoomRules.Decide`; the note uses `oninput`.
- Go to comment already selected a marker/video time above the visible area. It now reveals/focuses that existing preview after the correct version is mounted.
- Reports retain all notes and decisions with dark readable styling and a short media-supplied-separately statement. Raw identities remain in project/CSV data. Bundled examples remain explicitly labeled; no saved user content was rewritten.

## Procedure Exchange

Read `Pages/ProcedureExchange.razor`, `.razor.cs`, `.razor.css`; `Services/ProcedureExchangeRules.cs`, `ProcedureExchangeClient.cs`, `ProcedureExchangeExports.cs`, `ProcedureExchangeContent.cs`; `Models/ProcedureExchangeModels.cs`; and `wwwroot/procedure-exchange.js` in full. The content output's truncated publish-check/decision-actions range was separately recovered with lines 34–55; no missing range was counted as read.

- File/open/export actions are grouped under “Save, open or export”; their distinct privacy boundaries remain. Start this method was moved above the long editor instead of duplicated at its end.
- Simplified artifact, alias, fork, source-ID and revision jargon. Exact source IDs, public version pins and immutable run checks remain in data and requests. Readonly share text no longer also invokes Copy when clicked; the copy button remains.
- Completion controls use the existing rules: a three-character result note for any final result; all checks and an output for Completed. Result/output fields, published name and private-copy name use `oninput` for their action thresholds. Empty output-text download is disabled.
- Removed raw snapshot/run identifiers and repeated execution/provenance defenses from HTML reports; kept reported-result wording and the deliberate private-data inclusion choice. Empty private note/evidence sections are omitted. The existing capture of `IncludePrivateReport` before awaiting download was preserved.
- All twelve rich starting methods, instructions, output expectations and checks remain. They are content, not interface marketing. Private replacement warnings still explain overwriting in user terms.
- The page and standalone reports have readable dark defaults; print output retains a light background.

## Delegated companion groups

Field Work Pack and Shift & Cover Desk were not reopened by this reviewer; Creative completed their new full-source pass and reported it directly to root.

Signal Foundry was divided without duplicate reads: Creative owns the full Razor/Board/CSS UI pass; Games read `Models/SignalFoundryModels.cs`, `Services/SignalFoundryEngine.cs`, `SignalFoundryClient.cs`, `SignalFoundryExports.cs` and `wwwroot/signal-foundry.js` completely with no truncated output. Games changed only empty-result wording in Client and report labels/empty notes in Exports; engine, model, session-key, restore and JS behavior were preserved. Creative's final UI status is reported separately to root.

The root owns staging, the combined build and publication. This report does not claim those later steps or replace the individual agents' source evidence with a graphical-browser test.
