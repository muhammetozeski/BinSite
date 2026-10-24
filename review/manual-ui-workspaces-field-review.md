# Manual UI review — Field Work Pack and Review Room

This is a separate complete-source review following the 27 static pages. Findings come from reading the authored markup, styles, browser bridges, models and services and matching visible controls to their behavior. No automatic text search, regular-expression scan or keyword report is used. A source review is not a claim of browser interaction.

## Field Work Pack — complete source read and scoped corrections

Read in tool output without truncation:

| File under `src/BinSite.Social/` | Complete lines |
| --- | --- |
| `Pages/FieldWorkPack.razor` | 1–552 |
| `Pages/FieldWorkPack.razor.css` | 1–2 |
| `Models/FieldWorkModels.cs` | 1–98 |
| `Services/FieldWorkCloud.cs` | 1–86 |
| `Services/FieldWorkExamples.cs` | 1–77 |
| `Services/FieldWorkRules.cs` | 1–215 |
| `Services/FieldWorkReport.cs` | 1–63 |
| `wwwroot/field-work.js` | 1–153 |
| `wwwroot/field-work-sw.js` | 1–38 |

The product contract `docs/Field-Work-Pack.md`, lines 1–74, was also read completely.

- First view: the purpose is understandable, but five file/offline actions and two status blocks precede the actual Create order form. With no order, the right-hand area repeats “Start with your actual work” and “Choose a checklist” rather than showing the selected checklist.
- Unnecessary exact copy: “Download full project JSON”, “Open project JSON”, “Prepare offline reopening”, “Working locally…”, “Choose actual photo”, “Bounds are inclusive”, “Actual photo pixels were attached and saved locally”, and the repeated “I confirm this item has been checked.” These expose storage/implementation language or repeat a nearby field heading. “Open or recover a real account” and “Refresh account and list” make ordinary account actions sound like implementation proofs.
- Repeated controls and copy: outstanding issues are listed in full above the fields and repeated at every field. Order, checklist and revision identifiers appear before any answers. Every optional note opens a textarea, and several paragraphs repeatedly explain that this is not a certified inspection.
- Useful behavior mapped to the UI: each order freezes a checklist revision; later library edits do not alter it. Start work unlocks evidence entry, Request review checks required confirmations/numbers/photos, and Complete is a distinct local action. Reopen preserves answers. Numeric zero is valid; supplied out-of-range optional values still fail validation. Photos are actual decoded bytes; online sharing excludes those bytes. Reports embed photos, while shared projects need original-file reconnection. The history view is real saved history, and stale shared saves do not overwrite automatically.
- Applied presentation changes: file/backup/offline actions are in one closed Project files and offline use disclosure. The selected checklist is previewed before the first order. Create work order and the next workflow action are prominent. Optional order details and field notes are closed disclosures. The missing-item list can be opened without repeating every issue by default. Project-file, report, invite, account and shared-project actions use direct names. The whole-document sharing visibility and local-photo boundary remain visible at sharing. History and checklist versions remain accessible. Styling uses a calm dark surface with legible controls, retaining the independent two-column layout.
- Confirmed functional issue and correction: order titles accept 160 characters, but the single-order export put that title in `FieldWorkWorkspace.Title`, which accepts 120. A valid 121–160-character order therefore could not be exported. The backup now uses a separate workspace label and retains the complete order title inside the exported order.
- Confirmed preservation issue and correction: New workspace, local import and cloud replacement asked for confirmation only when orders existed. A custom checklist-only workspace could be replaced without confirmation. These replacement paths now also protect checklist content and an open draft.
- Additional draft preservation: switching to another checklist or creating a new checklist previously replaced the current unsaved definition immediately. It now compares the draft with its saved definition and asks before discarding a changed draft.

## Review Room — complete source read and scoped corrections

Read in tool output:

| File under `src/BinSite.Social/` | Complete lines |
| --- | --- |
| `Pages/ReviewRoom.razor` | 1–291 |
| `Pages/ReviewRoom.razor.css` | Complete single CSS line, then its final blank line |
| `Models/ReviewRoomModels.cs` | 1–108 |
| `Models/ReviewRoomCloudModels.cs` | 1–16 |
| `Services/ReviewRoomCloudClient.cs` | 1–55 |
| `Services/ReviewRoomExamples.cs` | 1–33 |
| `Services/ReviewRoomRules.cs` | 1–230 |
| `Services/ReviewRoomExports.cs` | 1–90 |
| `wwwroot/review-room.js` | 1–207 |

`docs/Review-Room.md`, lines 1–72, was read completely. The first CSS output accidentally indexed its single string character by character and was truncated; that attempt was not counted. A complete raw output was then read. The other source reads above were complete.

- First view: adding images/videos is the actual starting action, but an empty project showed a title/brief editing form, a reconnect-file control, two empty-state messages and an unmatched-file warning in the empty viewer. Example projects were closed and each button carried a long brief.
- Unnecessary exact copy: “Select real media to begin”, “Comment on this exact version”, “Click the image to place a normalized marker”, “The button reads the actual player time”, “Explicit review and delivery state”, “List my real workspaces”, “Create cloud copy of this JSON”, “Save expected cloud revision”, “Create real invitation”, and “Review JSON saved to the actual cloud workspace”. These explain implementation/proof rather than the user's task. Repeated notes about signatures, exact version binding and sharing appeared in several places.
- Hidden useful behavior: new assets and revised files are separate actions. Revised bytes must differ; exact originals can be reconnected even after renaming. Image comments use a clicked point, with a center alternative; video comments pause and capture the player frame. Comments never migrate between versions. Approval is blocked by unresolved blocking comments, delivery follows approval, and reopening review is needed before adding another blocking change. Reports contain notes/file identities but no media. The editing-list export contains only open comments. Invites grant whole-project visibility, not private individual notes.
- Applied presentation changes: project details and file/export controls are closed sections. Add images or videos remains visible; the three labelled original examples open by default for an empty project. Reconnect is offered only after there are saved media identities, and the viewer is hidden until a version is selected. File limits and matching details are available in a disclosure. Comment actions use ordinary point/frame language. Decision history and comparison remain distinct accessible disclosures. The editing-list action is unavailable without open work. The sharing panel uses direct save/list/invite names and opens automatically for an invitation. Its whole-project visibility/media boundary remains visible. Detailed workflow and editable-record limitations appear once in Help. The independent side list/media review layout is retained with calm dark controls.
- Confirmed comment-visibility issue and correction: selecting a resolved comment through a marker or comparison while the Open filter was active focused the saved target but still hid its text. Going to a comment now selects All comments, revealing the actual target without changing its resolution or version.
- Confirmed draft issue and correction: an otherwise empty project could contain unapplied title/brief edits, but replacement confirmation checked only the committed project and comment draft. Both title and brief drafts now participate in the replacement gate.
- Data boundaries retained: no media upload, content migration, saved-schema change, role expansion or automatic approval was added. The selected file's digest, anchors, original notes and decision history remain intact.

## Targeted verification

The current two Razor files were copied verbatim into `C:\C\BinSiteStaticReadability\WorkspaceChecks`, with their existing imports and the current five-line `PageSurface` wrapper. They compiled against the existing product model/service assembly in a disposable Razor consumer project. No shared production component, model or service was changed for the checks.

Nine checks passed using the actual component methods and controlled browser download/confirmation boundaries:

1. A single-order export retains its complete valid 160-character title.
2. That export uses a separate bounded workspace label.
3. New project asks before replacing checklist-only work.
4. Declining local import retains checklist-only work.
5. Declining shared-project replacement retains checklist-only work.
6. Declining a checklist switch retains the unsaved draft.
7. An unapplied Review Room title triggers replacement confirmation.
8. An unapplied brief triggers replacement confirmation.
9. Going to a resolved comment reveals it despite a previous Open filter.

The final isolated compile/check invocation produced no warnings or errors. `review-room.js` passed Node syntax checking. This evidence does not claim rendered browser interactions, a downloaded file on disk, a file picker, media decoding, service-worker installation, cloud requests or a real print dialog. No graphical control was clicked. This second group remains unstaged and uncommitted for the coordinated release.
