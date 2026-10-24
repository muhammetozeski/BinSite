# Manual UI review — main site and data applications

Review requested 24 October 2026. Read authored UI files in full, following their actions into the code that implements them. No automated text search, keyword scanning or pattern-generated findings is used. Source-reading findings are separate from live interaction evidence. Other reviewers cover static products and independent projects in sibling records.

## Main directory
- Read: scripts/build.mjs, public/assets/catalog.css, public/assets/catalog.js in full; catalog names/descriptions read individually in the preceding route inventory.
- First task: finding a tool or game is clear; the previous product-name link and Open link duplicated the same destination.
- Removed the second same-destination link; the product name with arrow is now the single launch link. Distinct guide links remain useful for the three practice workflows.
- Removed the generic introductory sentence and internal feature metadata from search strings. Tools and games remain separate, with search/category controls that filter actual entries.
- Technical feature lists, project target/count and storage architecture do not belong in the customer directory. The build no longer renders them.

## Shared report form
- Read in full: Shared/ReportBox.razor and .razor.css; Shared/ProductComponent.cs and Services/SocialApi.cs.
- Found: “The reason is stored on the server. Do not include personal information. Reports from the same network address count once.”
- Removed storage and network-count explanation from the form. Kept the practical instruction to describe the problem without personal information.
- The actual action sends one report and clears the reason on success. The form is closed by default and does not promise human review.

## Shared content and appearance
- Read in full: Shared/CollaborationComponent.cs, CommunityComponent.cs, CollaborationRules.razor/.css, LanguageSwitch.razor, PageSurface.razor/.css; wwwroot/social.css; public/assets/appearance.js/.css and revenue.js.
- The language component retains preference initialization without a visible language badge. The appearance layer repairs text; it does not establish that a page's purpose is understandable.
- Privacy information stays in a closed disclosure. Reports are not reviewed by a person; this remains stated rather than implying a staffed service.
- The footer was shortened. Theme placement recognizes its current label, and disabled controls trigger a fresh readability check.

## Story Tree — /hikaye-agaci/
- Read StoryTree.razor and .razor.css in full, including local example handlers, published story/branch reads, write handlers, share/history behavior, limits and field validation. Read the publication generator that creates the initial loading view.
- Previous first screen showed framework/language/storage notes. It now offers a short branching story with two concrete choices and a separate start-writing action.
- Removed “Stories allow up to 300 paragraphs and paths up to 24 continuations long” and network-address thresholds from the help copy. These are implementation bounds, not reasons to use the story.
- The example is labeled Example; it is not represented as community activity. Starting from it fills a draft rather than publishing automatically.
- Fixed misleading generic invalid-input feedback. Field-specific validation shows beside the field; the controlled API response is preserved.
- Preserved drafts on branch navigation and successful write identities during result-loading retries. The latter prevents a second publication on that retry path.
- Live read-only browser observation of the first released revision showed the purpose, example and two choice buttons, with readable light text and no old technical labels. No GUI click path was performed.

## Time Capsule — /zaman-kapsulu/
- Read TimeCapsule.razor and .razor.css in full, including form, existing-link parser, countdown, share behavior, prompts and creation/opening states.
- The purpose is clear: write a letter that opens later. Retained date constraints beside the date field because they affect the user's choice; removed redundant “Optional writing ideas.”
- Found “Save / share the link”, although its handler only copies a link. Renamed to “Copy letter link”. “New capsule” became “Write another letter”.
- Existing-link input previously threw internal exceptions for foreign or incomplete links, which became a connection error. It now explains which link is needed.
- Operator access, public-link access and the need to keep the link remain relevant. The interface does not promise email delivery or indefinite preservation.
- Countdown derives from the last fetch rather than updating every second; Refresh is present and explicitly updates it.

The remaining application pages and policy/guide sources were subsequently read in full; their findings and follow-through appear below.

## Idea Duel — /fikir-duellosu/
- Read IdeaDuel.razor/.css completely, then the entire functions/api/[[path]].js including text checks, vote uniqueness, public totals and error paths.
- The original setup used “Create a duel”, “Open arena”, “Latest duels”, “The vote count starts at zero”, “Real votes, explicit limits”, “actual votes” and a success message ending “on the server”.
- Changed actions to Ask a question, Publish question and Copy voting link. Removed the count/realness/architecture claims. Results use normal English percentage order.
- An empty directory now starts with the two-choice question form. While composing, the duplicate empty reading arena is hidden.
- Vote totals are aggregate public results; the backend does not expose individual voting identities. Publishing and voting remain separate deliberate actions.

## Shared Canvas — /ortak-tuval/
- Read SharedCanvas.razor/.css completely, CommunityModels.cs, shared transport and the entire community API (including separately recovered output ranges that were truncated in the first read).
- Found visible Revision counters and “Publish changes (n/32)”; users need to draw/save, not understand revisioning or batch bounds. Removed the revision counters, renamed saving and copying, and made the paint-boundary message actionable.
- Found actual data loss: OpenAsync cleared unsaved pixels before loading another canvas. Opening or creating another canvas now requires saving or discarding pending changes first. Refresh retains pending edits.
- A selected challenge was invisible before a canvas opened. It is now shown in the workspace immediately. Discard is disabled when nothing is pending.
- The interface is click-to-paint and manually refreshed; no live-collaboration claim is added.

## Debate Stage — /munazara-sahnesi/
- Read DebateStage.razor/.css completely, the community models and API.
- Replaced proposition/session/map jargon with discussion/question actions and a truthful copy-link action.
- Removed the repeated “No evidence link provided” row. Replaced naked review numbers plus a paragraph of definitions with self-explanatory choices.
- Found that replying only set state in a form below all arguments. The reply action now requests focus on the argument field after that form has rendered.
- An empty directory starts with the question form; unrelated reading content is hidden while composing.
- Peer review still rates clarity/evidence rather than claiming factual truth; the interface retains that distinction.

## Time Market — /zaman-pazari/
- Read TimeMarket.razor/.css completely, community models and the full conversation/proposal/acceptance/completion backend.
- A reply button only set Proposing while its form remained below the entire listing directory. The action now focuses the proposal field after rendering it.
- Renamed the action to Propose an exchange, shortened jargon in labels and removed the numeric exercise count from the ideas disclosure.
- Preserved useful boundaries: public listings, participant-only conversation access, cookie-loss access warning and the requirement for both sides to confirm completion.
- The service does not match schedules automatically or verify skills; no such claim is introduced.

## Shared Museum — /ortak-muze/
- Read SharedMuseum.razor/.css, CommunityEditorial.cs and community models/API completely.
- Findings still awaiting edits: no empty-directory explanation, “Publish empty exhibition” as the first creation action, duplicate example headings, redundant per-object approval badges for already-public objects, and Move earlier/later buttons remaining enabled at either end even though the handler returns without acting.
- Useful implementation behavior: curator stories appear immediately; visitor stories wait for curator approval; only approved stories are reorderable. These distinctions should remain clear near the actual actions.

## Password Team — /parola-ekibi/
- Read PasswordTeam.razor/.css fully; a truncated setup/mission range was read separately. Read all community room creation/join/clue/message/answer actions.
- Findings still awaiting edits: six setting choices change only narrative wording, not puzzle mechanics; share actually copies a link; the chat talks about identities; malformed room links become generic connection errors; several sections restate the same four-card explanation.
- Useful behavior to preserve: separate private cards, role occupancy, four-person requirement, leading zero support, join/expiry states, and manual message refresh.

## Mini Tournament — /mini-turnuva/
- Read MiniTournament.razor/.css completely; collaboration backend still pending.
- Findings awaiting edits: “Capacity” is an internal-sounding player-count label, winner buttons say Top/Bottom rather than naming the player, and the organizer guide describes product storage instead of the steps the organizer follows.
- The bracket and participant roster are real UI output; do not remove them as decorative copy. Confirm backend winner-correction rules before changing that workflow.

## Book Companion — /kitap-arkadasi/
- Read BookCompanion.razor completely; stylesheet, editorial story source and collaboration backend still pending.
- Findings awaiting edits: Story selector is labeled “Original fictional story” with a redundant fiction note; share actually copies a link; the “Readers mark their own progress” caption repeats the obvious action.
- All chapters already exist in the delivered story collection, but preview UI shows only the first chapter. Reading previews can expose chapter navigation without requiring creation of a public club. Club discussion and progress actions remain separate.

## Micro Academy — /mikro-akademi/
- Read MicroAcademy.razor/.css, CollaborationEditorial.cs, CollaborationModels.cs and all academy backend actions in full.
- Removed “enrolled identities”, the /12 capacity badge, the redundant qualifications/infrastructure footer, and internal author-draft wording.
- Lesson ideas only appear when their click changes a visible example or editable author draft. Adding a lesson clears the form after success, preventing accidental duplicate additions.
- The backend requires the prior lesson to be completed before checking the next quiz. The UI now disables a locked quiz action and explains what to do first, rather than inviting a rejection.
- Correct-answer choices are shown as answer text for the author. The service withholds answer keys from learners; no claim of private course text is made because published lesson text is readable.

## Knowledge Relay — /birlikte-bil/
- Read KnowledgeRelay.razor/.css, all generated-question/handoff/backend states, models and editorial source in full.
- Replaced flag jargon in setup with Create team and the misleading Invite teammates action with Copy invite link. Removed a duplicate How to play heading and the generic missing-service footer.
- Added Play again in the finished state; previously the page invited a new room without offering that action.
- Correcting the final question now reports completion instead of telling the player to hand off a nonexistent next turn.
- Malformed invitation links receive a specific instruction instead of a connection error. The public progress/holder cards remain useful; the underlying legacy quiz still uses text questions, not a graphical game scene.

## Review follow-through on previously read pages
- Shared Museum: opened examples when the directory is empty, shortened the creation/copy actions, removed duplicate example headings and unnecessary published badges, disabled reorder buttons at either end.
- Password Team: removed the setup setting selector that only changes narrative, retained existing room content, clarified Copy room link and Team chat, and corrected malformed-link feedback.
- Mini Tournament: full backend confirms that results cannot be changed after recording. Winner actions now name the player; the organizer guide describes the workflow and this actual limitation.
- Book Companion: read the stylesheet and entire original fiction collection. All delivered chapters are now previewable without creating a club. The already-read action is disabled and labeled accordingly; copy action and redundant captions were corrected.
- Time Market: Refresh now fetches the open conversation as well as list headers and retains drafts. Switching conversations preserves an unsent draft by asking the user to send or clear it first. Acceptance/closing copy explains the actual effect on other proposals.

## Creative Partnership — /uretim-ortakligi/
- Read CreativePartnership.razor/.css, every task idea, collaboration models and all jam backend transitions in full.
- Replaced “Start a jam”, “Open project”, “Refresh state”, “Invite collaborators” and “Volunteer handle” with direct project/task actions.
- Task ideas previously filled fields hidden inside a closed author panel, or did nothing visible for non-owners. They are now offered to owners and reveal/focus the task form.
- Prepare delivery now reveals/focuses the actual delivery field, preserves an unfinished draft when switching tasks and restores the prior submission for a requested revision.
- Claim, publish-task and submit buttons honor the displayed deadline. Owner reviews remain available afterward, matching the backend.

## Creator account — /uyelik/
- Read Membership.razor/.css fully and the entire commerce API in three contiguous chunks (1–110, 111–270, 271–end). No private keys, account contents or provider credentials were read.
- Removed duplicated allowance text and a paid-feature list that advertised cross-device access even though free accounts already have it. Unavailable subscription offers no longer consume the screen.
- Found “Open file” only displaying raw project data. Replaced it with a real project-file download that can be reopened in the original editor; removed the raw-data dump and its copy action.
- File selection now reports unsupported/oversized project files as input issues rather than connection failures.
- The recovery endpoint invalidates other sessions. The sign-in disclosure now tells users that other devices will be signed out.
- Backend membership expiration and subscription-provider ownership were omitted from the UI. The page now shows the paid-through date and only the management portal relevant to the account's subscriptions; active members are not offered another purchase button.
- Payment return URLs now produce a status message without claiming payment acceptance before verified membership is active.
- The source review and coordinated implementation are complete. No paid checkout or native download interaction has been performed.

## YouTube Link Checker — /youtube-tools/
- Read YouTubeInspector.razor/.css and models, the complete youtube-inspector.js wrapper, canonical youtube-core.mjs, and preparation source. The prepared runtime module is a byte-identical generated copy; it does not define another UI.
- Found two links to each available video (title and Open video), and two preview controls (thumbnail and Preview). Removed the duplicate controls while preserving the fallback Open video action for unavailable details.
- Output buttons now appear after there are results; an empty-input check and empty Clear action no longer invite a pointless click.
- Thumbnail selection previously changed an above-the-list preview without revealing it. It now focuses the rendered preview. Redundant Open image and repeated successful-check messages were removed.
- Replaced public-metadata and raw status-code wording with video details. Original failed input remains visible so users can identify the problem; successful rows do not repeat a long raw input URL.
- Clipboard refusal now points to the existing table export instead of exposing a browser exception. Network/provider duration or view statistics are not invented.

## Main navigation and policies
- Read public/assets/revenue.js, public/privacy/index.html, public/advertising/index.html, and both legacy redirect HTML documents in full.
- Removed the sitewide Advertising link and links back to the current page. Removed the directory's duplicate privacy footer and self-linking brand label; global Home/Account/Privacy navigation remains available where useful.
- Advertising information remains reachable at its existing route. Removed the duplicate statement that clicking ads is optional.
- Privacy text now describes participant access and operator access directly, replacing browser-identity and end-to-end-vault jargon. Checkout text refers users to account status rather than explaining verification architecture.
- Read App.razor and Program.cs in full: not-found and load-failure UI contain a direct retry/home path, not framework notes. No behavior change was needed there.
- Directory descriptions already read individually were rewritten where they described implementation rather than task: examples include binary lift matrices, WebGL compilation, actual WAV assemblies and parameterized volumes. File formats remain when needed to choose or use an output.

## Catalog Studio — /catalog-studio/
- Root read CatalogStudio.razor 1–493 and .razor.css in full; the companion service review covers every model/parser/workspace/sample/exporter/cloud-client/JS source and the generated catalog UI.
- The actual initial sample notice survives module initialization; the earlier provisional inference that initialization erased it was corrected after reading this component's own action boundary.
- Removed the long CSV-grammar paragraph, provenance/SVG claims, fingerprint/revision/raw-byte descriptions and the empty validation-issues panel. Retained practical input formats, row repairs and image-matching instructions.
- Paste/separator options and optional photo attachment now use disclosures. Empty selection/matching/bulk actions are disabled instead of offering no-op clicks.
- Details and Find row now reveal the selected editor/row. Input rows and export filters are preserved; no spreadsheet behavior was removed.
- Undo labels identify the actual captured operation rather than implying that individual cell edits are the last undo step.
- Online-save actions now name catalog/user tasks, invitation codes and saved copies. The quota is shown as saved-project usage, not database bytes. The page states the meaningful limitation that image files are not included in online copies.
- Removed the blanket conflict diagnosis: an account quota conflict must not be mislabeled as a revision conflict.
- Save-before-closing guidance remains visible because current unsaved work lives in the tab. No autosave behavior was invented.

## Combined release check

The coordinated tracked-source Release publication completed on 25 October 2026 with exit code 0. It included the reviewed application markup, services, browser bridges and the three independently checked Book Companion, Membership and Shared Canvas fixes. Unfinished untracked drafts were not part of the release source. Ten changed JavaScript modules/handlers also passed Node syntax checks, and the directory generator produced 75 entries.

The independent follow-through for account-switch, drawing-save and comment-draft preservation is recorded in [manual-ui-integration-book-membership-canvas.md](manual-ui-integration-book-membership-canvas.md). These are source/build results; they are not evidence of paid checkout, live account mutations or native file-dialog interaction. Deployment evidence is appended only after the publication workflow succeeds.
