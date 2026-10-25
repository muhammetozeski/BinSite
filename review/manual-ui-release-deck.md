# Release Deck interface follow-through

This extends the [complete directory and product cleanup](manual-ui-cleanup-followup.md) to the subsequently published 78th product. It is a manual source review with separate production observations; no keyword scan was used as a substitute for reading.

## Source reading and changes

The prior complete readings of Library, WorkspacePage, TeamReview, Operations, Help, DeckLayout, DeckPage, ReleaseStore, domain exports, the stylesheet and browser bridges remain applicable to the unchanged source. The current UI changes were read with their handlers. App, Notice and the bootstrap were also checked. Genuine video-language/translation fields are product inputs, not page-language badges.

The library now names the actual task: planning and reviewing a video series, previewing clips, preparing titles/schedules and downloading the plan. Removed the extra eyebrow and two repetitions of the starting instruction. The header Account link remains the single account destination. Anonymous users no longer see an ineffective Refresh or a disabled invitation form; a failed online read still exposes retry. An invitation in the address remains retained for sign-in.

Workspace navigation appears after a local release is opened; team review and delivery appear for an online workspace. These conditions use the existing store-change subscription, so navigation follows the actual active project. Removed repeated empty-screen links to the same library/workspace destination. Reworded visible transfer errors around the existing Check remote status action rather than internal reconciliation/offset terminology. Necessary original-file, sharing, publishing and overwrite consequences remain.

The non-interactive heading no longer receives the visible rectangle from programmatic route focus. Keyboard focus styles for actual controls remain. Video previews, saved content, approval rules, provider receipts and permissions are unchanged. The final Release build completed with no warnings or errors; the edited upload module also passed the Node syntax check. Unchanged media/domain checks were not repeated for wording changes.

## Production observations and routing finding

The first production library loaded and showed the actual controls. Read-only observation exposed the duplicated account/sign-in destination, anonymous Refresh, repeated start instructions and heading focus outline above. The main directory independently showed one product link per row, separate tools/games and concise task descriptions; its generator, URL helper, entire generated document, stylesheet and search script were manually re-read without finding another change to make.

Direct navigation to Help and Workspace unexpectedly returned to the library. A first-response request established HTTP 308 to `/release-deck/`; the corresponding StageFold studio path also returned 308 to its product root. Earlier delivery checks followed those redirects and therefore did not establish correct deep routing. This is a correction to that broader interpretation of the earlier acceptance, not a new claim that HTTP 200 alone proves a functional page.

The publisher now proxies only explicit deep routes to the canonical product folder, with no folder self-proxy. The main assembler normalizes the same index target in the four older pinned applications. No wildcard captures assets or APIs. The two PowerShell sources parsed and 65 controlled metadata assertions passed; production route preservation and the final UI require the subsequent publication observation.

## Final publication

[Workflow 40148902547](https://github.com/muhammetozeski/BinSite/actions/runs/40148902547) successfully published main `e58ecbd9c6ea627d0b3beb9f2a5d0f53868e73a9` with Release Deck source `724d19e268eeeea06e523c517c0e5d9275f1a26f`. Its live metadata matched. The first-response checks now returned 200 without Location and retained the requested paths for all four Release Deck screens, Help with a trailing slash, StageFold Studio/Guide, Retry Privacy and Interlock Help. The media asset remained JavaScript; the changed upload wording matched source.

Read-only native observation showed one account link, one concrete title/description, one primary New release action, no anonymous Refresh/Join, no unopened-project navigation, and no heading outline. Release Help loaded its actual five-step content at `/release-deck/help/`; StageFold's actual Guide loaded at its deep URL. The previously visited no-slash Help address still returned the library in that existing browser, while its fresh first-response HTTP check returned 200 with no redirect. The unvisited trailing-slash route supplied the native observation above; browser history/cache was not cleared or counted as a second clean no-slash observation. [Updated release screenshot](release-deck.png). These observations do not claim that every authenticated state or provider transfer was graphically exercised.
