# Manual UI review: Signal Foundry and Shift & Cover Desk

The full files below were displayed in tool output and read as source. The review follows the first-screen task, every visible control, its event handler, local persistence, portable output and the actual API contract. No automatic text search, keyword report or regular-expression audit scan was used. Existing application expressions were read as code. Source inspection and isolated consumer execution are distinguished from browser acceptance.

## Reading record

Paths in this section are relative to `C:\E\Claude\WebSiteleri\BinSite`.

| File | Complete original range |
| --- | --- |
| `src/BinSite.Social/Pages/SignalFoundry.razor` | 1–259 |
| `src/BinSite.Social/Pages/SignalFoundryBoard.razor` | 1–26 |
| `src/BinSite.Social/Pages/SignalFoundry.razor.css` | Whole single-line file, 3,491 bytes, read with `Get-Content -Raw` |
| `src/BinSite.Social/Pages/SignalFoundryBoard.razor.css` | Whole single-line file, 557 bytes, read with `Get-Content -Raw` |
| `src/BinSite.Social/Models/SignalFoundryModels.cs` | 1–100 |
| `src/BinSite.Social/Services/SignalFoundryClient.cs` | 1–70 |
| `src/BinSite.Social/Services/SignalFoundryEngine.cs` | 1–131 |
| `src/BinSite.Social/Services/SignalFoundryExports.cs` | 1–35 |
| `src/BinSite.Social/wwwroot/signal-foundry.js` | 1–26 |
| `content/signal-missions.json` | 1–27; all 24 descriptions, hints, explanations, mappings, examples and targets |
| `functions/api/commerce/signal/[[path]].js` | 1–147 |
| `server/signal-engine.js` | 1–49 |
| `src/BinSite.Social/Pages/ShiftCoverDesk.razor` | 1–170 and 171–325 |
| `src/BinSite.Social/Pages/ShiftCoverDesk.razor.css` | Whole single-line file, 3,222 bytes, read with `Get-Content -Raw` |
| `src/BinSite.Social/Models/ShiftCoverModels.cs` | 1–87 |
| `src/BinSite.Social/Services/ShiftCoverExamples.cs` | 1–78; all eight example definitions and construction branches |
| `src/BinSite.Social/Services/ShiftCoverExports.cs` | 1–125 |
| `src/BinSite.Social/Services/ShiftCoverPlanner.cs` | 1–62 |
| `src/BinSite.Social/Services/ShiftCoverRules.cs` | 1–232 |
| `src/BinSite.Social/Services/ShiftCoverVault.cs` | 1–34 |
| `src/BinSite.Social/wwwroot/shift-cover.js` | 1–62 |
| `src/BinSite.Social/Services/SocialApi.cs` | 1–57 |
| `functions/api/commerce/[[path]].js` | 1–180 and 181–353 |

Product documentation was also read completely: `docs/Signal-Foundry.md` 1–52, `docs/Signal-Service.md` 1–37 and `docs/Shift-Cover-Desk.md` 1–58. No displayed source output was truncated. An initial PowerShell read indexed a single-line CSS result as a string and displayed only its first character. Those three outputs were rejected as evidence; all three stylesheets were subsequently displayed in full using `-Raw`. Their existing layouts and colors were preserved.

## Signal Foundry

- **First screen:** the first mission opens with its authored description, worked input/output boards and Start button. That explains the task. The actual board remains a 3×3 grid, has R/B/Y/dot marks in addition to color and accessible row/column names. No palette or layout replacement was needed.
- **Repeated entry instructions:** “Choose a mission to begin.”, the empty-state “Choose a mission, or make your own in Build a challenge.” and the initial status repeated one action. Removed the extra paragraphs, shortened the Play tab and kept the local builder available if missions cannot load.
- **Unnecessary implementation wording:** removed the visible `sf-v1` identifier, “Share link · the answer key is not in this address”, “Open a mission-specific hint”, “Practice preview. No mission attempt is recorded.” and “Published a real immutable challenge.” Plain author/source attribution, Hint, Challenge link, Local preview and a publication acknowledgement remain.
- **Expiry repetition:** “Download your record before then. Keep a downloaded record.” is now one instruction alongside the actual expiry date. The repeated Privacy and safety footer was removed; actual expiration and storage failure feedback remain in the workflow.
- **Useful controls retained:** real guest start/resume, current versus latest attempt recovery, brush selection, uppercase stamp programs, copy/clear input, optional budgeted query, final answer/reveal, complete worked/query evidence, truthful ambiguity verdicts, notes, printable record, builder examples and computed outputs, mapping/source locking, browser draft save/restore, JSON import/export/undo, public publishing, report and owner withdrawal all remain bound to their existing behavior.
- **Instructions at the right step:** “1. Choose a calibration experiment” becomes “1. Test a stamp”, with input → stamps → experiment. Final reconstruction states the exact target program and that finishing reveals the key and locks the attempt. Authored strategic hints, mission fiction and post-answer explanations remain unchanged. The distinct deduced/consistent/inconsistent meanings remain clear without repeating the ambiguity explanation beside the difference count.
- **Author publication:** “Explicit public publication” and its long process paragraph repeated the heading, checkbox and button. The shorter Publish section still states exactly what becomes public, that players see the key after finishing, that attributed forks are allowed and that publishing requires sign-in. The answer-key warning remains beside author JSON; players still receive the server challenge URL.
- **Unsaved-work protection:** starting another attempt while an unfinished one is open now asks before replacing the current working investigation. A refusal leaves the attempt and working state untouched.
- **Accurate local-preview notes and report:** local previews do not call the browser attempt writer. Their notes label now tells the user to download a record instead of promising browser saving. The exported record itself says Local preview. Removed its generic “not a verified contest result” / editorial-process footer, while preserving evidence, notes, answer and result.
- **Resolved request state:** an uncertain query keeps its request identifier for an immediate retry. After a successful authoritative Resume, pending query/finish identifiers for that attempt are cleared. Otherwise an already-restored query could replay its old response and be appended again as a new displayed observation. Failed Resume still retains the identifier, so retry recovery is not discarded.
- **Backend match:** public challenge/unfinished attempt responses withhold the hidden key/result; query and finish carry per-request capability headers, not share links. The server owns query accounting, snapshots, immutable publication, nonce replay, reporting and withdrawal. The read C# engine and server engine use the same eight operations and 1,680 mappings. No backend behavior was changed by this review.

## Shift & Cover Desk

- **First screen:** setup presents title, fixed clock and roles. The ready message now states the real order: add roles, people and shifts, then suggest a roster. The 62-day period and whole-project hour scope remain visible. A short instruction beside downloads explains that each form must be saved first.
- **Repeated wording:** removed the extra Product directory link, shortened “Started from the fictional editable example … Its original names, availability and limits were invented for this exercise.” to the actual example label, and replaced the long legal/certification/eligibility lists with one concise statement that checks use entered limits and do not record staff acceptance. The examples and invented labels remain clearly fictional.
- **Clock behavior retained:** the fixed offset still applies to every date, changing it changes exported calendar instants and periods needing different daylight-saving offsets must be split. This is relevant to an actual choice and was shortened rather than hidden.
- **Duplicate actions:** the left-column “New person” and “New shift” called the same methods as the right-column “Clear person draft” and “Clear shift draft”. Each editor now has one New person/New shift reset action. Existing draft isolation and explicit form saves remain.
- **Ineffective controls:** Delete disables for a role used by a shift and exposes that reason; role/person/shift creation disables at existing limits or when required labels are empty; availability Add disables at 24 windows. Suggest disables without people or shifts. Manual assignment disables until a person is selected; swap requires two distinct selections. A person without assignments no longer offers a header-only CSV download.
- **Planner explanation:** removed “Eight deterministic greedy passes try different priorities, with at most … dynamic candidate checks.” from the main instruction and the matching diagnostic counts from success feedback. The useful consequences remain: valid locks stay, unlocked assignments can be replaced, suggestions are checked, and open seats may have another solution. No optimality or infeasibility promise was added.
- **Useful controls retained:** exact candidate blockers, unfilled-seat reasons, locks, manual replacement, validated two-sided swap, retained conflicts after input changes, per-person schedules, all-seat and per-person CSV/calendar, print report, strict JSON replacement and eight editable examples remain. The UI still blocks calendars with conflicts and explains calendar import behavior.
- **Persistence feedback:** “Saved locally using IndexedDB/localStorage fallback” becomes “Saved locally.” The actual verified fallback and failure reporting are unchanged. The manual Save locally retry remains useful if automatic persistence fails.
- **Vault wording:** “Optional owner-only cloud snapshots”, “Outer vault label”, “List my real saved snapshots”, “Open validated JSON”, “from the real account” and “acknowledged by the server” become Private snapshots, Snapshot name, Refresh snapshots, Open snapshot and concise results. The real sign-in link, account capacity, 64 KiB preflight and full owner-only semantics remain.
- **Acknowledged mutation versus list failure:** Save and Delete now retain their successful acknowledgement when the subsequent list GET fails. The message says that the snapshot was saved/deleted and that the list must be refreshed. The mutation is not automatically retried. A successfully deleted row is removed from the currently visible list before refresh; a failed mutation never receives a success message.
- **Backend match:** the vault uses `commerce/projects` with `kind: roster`; the read/create/delete shapes, ownership checks, 100-character outer name and 65,536-byte JSON body match the actual route. It does not use workspace invitations/history or claim collaboration. Main planning remains local and independent of the account service.

## Checks and edit manifest

- Disposable fixture: `C:\C\SignalShiftUiReview\Review.csproj` with `Program.cs`. It compiles the current verbatim Signal/Board/Shift Razor sources plus their actual models, rule engines, clients, exports and shared HTTP adapter in an isolated .NET 10 project. Only the page wrapper and external browser/HTTP interfaces are test substitutes; no production test hooks were added.
- Ten assertions passed: uncertain direct retry keeps its identifier; successful Resume starts a new logical experiment; failed Resume retains recovery; refused fresh attempt preserves work; exported local preview has its own accurate label; acknowledged save/delete survive failed list reads; no duplicate save is submitted; deleted rows leave the visible list; failed mutations never appear saved.
- This is source and isolated component/HTTP-consumer evidence. It does not claim browser layout, live account mutations, native storage/clipboard/file-dialog interaction or a new full deployed play-through. The shared application build was not started.
- Edited production files: `Pages/SignalFoundry.razor`, `Services/SignalFoundryClient.cs`, `Services/SignalFoundryExports.cs`, `Pages/ShiftCoverDesk.razor`, all under `src/BinSite.Social`. The other listed source/CSS/JS/content/backend files were read without edits.
- This record and the four production files remain unstaged for the parent's integration order. No push was performed.
