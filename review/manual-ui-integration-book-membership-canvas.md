# Integration review: Book Companion, Membership and Shared Canvas

This is an independent source review of the root's current three-component snapshot. No automatic keyword/text scan, new test project, repeated syntax check or live account mutation was used. The three full component sources were displayed and read: `Pages/BookCompanion.razor` 1–35, `Pages/Membership.razor` 1–85 and 86–155, and `Pages/SharedCanvas.razor` 1–44, under `src/BinSite.Social`. Root authorized the narrow fixes below after receiving their exact source locations.

## Related contracts read

- `Shared/ProductComponent.cs` 1–64, `CommunityComponent.cs` 1–16 and `CollaborationComponent.cs` 1–27: busy boundary, error state, typed requests, revision attachment and history replacement.
- `Models/CollaborationModels.cs` 1–62 and `CommunityModels.cs` 1–62: complete typed response definitions.
- `Services/CollaborationEditorial.cs` 1–32: all four stories and their three chapters, plus the remaining content in that file.
- `wwwroot/social.js` 1–21: clipboard fallback, history replacement and actual project Blob download helper.
- The relevant `functions/api/collaboration/[[path]].js` creation/reading branch and request/revision exposure at 1–45 and 80–93; the relevant `functions/api/community/[[path]].js` canvas reads, bounds and conditional batch write at 1–49. Unrelated product branches were outside this bounded integration task.
- The unchanged shared commerce handler had already been completely read at 1–353 in the immediately preceding Signal/Shift review. Its status/provider/portal/owner-only vault contracts were reused rather than reread.

## Findings and fixes

### Shared Canvas: pending edits during save

At original line 37, the save request materialized the current Pending cells before awaiting the HTTP response, then cleared the whole Pending dictionary. Original Paint line 33 and ClearPending line 39 lacked a Busy guard, and the grid/palette buttons at line 9 remained active. A new cell painted during the await could therefore be absent from the submitted request but erased by the response's Pending.Clear.

Fixed only this race: Paint/ClearPending return while Busy, and grid/palette buttons disable while Busy. The chosen color, twelve palette values, `data-appearance-preserve` markers, pending-cell preview, 32-cell limit, optimistic revision, refresh-retains-pending behavior and existing open/create unsaved guards remain unchanged. The backend accepts the same 24×16 coordinates, color indices 0–11 and 1–32-cell revision-checked batch.

### Membership: stale private UI after account change

Original Recover line 116 and Logout line 118 awaited a successful identity-changing POST and then read status without first clearing the old Status object. If the follow-up GET failed, the old account and private project titles remained visible. Logout's comment promised clearing private UI, but Status still referenced it.

After acknowledged recover/logout, Status now clears before the follow-up GET. Recovery/upload/title fields also clear at that account boundary. The file input is keyed by account ID, so switching to a different account cannot retain the previous account's selected-file control. Recovery-key and project-title inputs disable during an action so a newly typed value cannot be erased by the earlier action's completion.

Vault download matches its actual helper: the authenticated read returns an owned JSON object; the component serializes that body and calls `binSiteSocial.downloadProject(title, contents)`, which creates a JSON Blob with a sanitized `.json` filename. The provider/plan controls match the status contract and portal/checkout endpoints. No additional confirmed download/provider binding issue was found in this bounded source pass.

### Book Companion: comment draft during submit

All four authored books have three chapters. The new no-club preview exposes every chapter. The club read button checks the current reader's server progress against the selected chapter; the server uses the same monotonically increasing 0–3 progress value. No chapter-array mismatch was found.

The comment textarea remained editable while its request awaited, and original CommentAsync line 34 cleared Comment unconditionally afterward. Additional text entered during that request could be erased. The relevant create/join/comment inputs now disable while Busy. CommentAsync also captures the submitted body/chapter and clears only if the draft still equals the submitted body, preserving a changed draft even if an already queued event updates it.

## Delivery boundary

Changed files: `src/BinSite.Social/Pages/BookCompanion.razor`, `Membership.razor`, `SharedCanvas.razor`, plus this review record. No backend, shared helper, palette, CSS, stage, commit or push change was made. This records source-level sequencing and contract evidence; the next combined build belongs to root. No additional infrastructure or verification loop was started.

## Privacy/help wording follow-up

The follow-up manually revisited the already read Shared Canvas/Book views and the full two-line `Shared/CollaborationRules.razor`, then the five community products' privacy/help views. The combined community output clipped part of Time Market and Shared Museum; their complete UI ranges were then displayed separately: Time Market 1–17 and 18–20, Shared Museum 1–13. Debate Stage 1–62 and Password Team 1–47 were fully visible in the first output. The clipped output was not accepted for the missing ranges.

Removed the infrastructure sentences about network hashes, cookie-based identities, report thresholds and automatic hiding from the rendered help/footers. They were deleted, not moved under Help. Public/private visibility, curator/participant access, operator access to messages, sharing permission, payment boundaries and the absence of human report review remain. Clearing site data is explained only by the actual lost-access consequence. Book's own source needed no further change; it receives the shorter shared CollaborationRules.

Exact follow-up source manifest: `Pages/SharedCanvas.razor`, `Pages/DebateStage.razor`, `Pages/TimeMarket.razor`, `Pages/SharedMuseum.razor`, `Pages/PasswordTeam.razor`, `Shared/CollaborationRules.razor`, all under `src/BinSite.Social`, plus this review record. No behavior change, build, test project, automatic text search, stage or push was added in this follow-up.
