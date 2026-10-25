# Directory and shared UI cleanup

This follows the complete [page-by-page review](manual-ui-index.md). Findings came from reading authored UI and its handlers, not a keyword scan. Project sources remain in their permanent directories. Publishing and native-browser observations are separate evidence.

## Directory

Read the complete directory generator, stylesheet and search script, the generated directory, and the catalog descriptions. Fifty-four descriptions were individually rewritten around the visitor's task. Framework/language feature labels were also removed from the four affected social catalog entries.

The directory now has one opening link per product, shorter rows, a single search field, and separate tools/community and games sections. The overlapping category selector, repeated introductory heading and extra per-product guide links were removed. Guides remain inside their tools and in the sitemap. Search accepts all entered words, hides empty groups and their navigation links, and uses one empty-result message.

## Shared navigation

Read `public/assets/revenue.js`, `appearance.js`, `scripts/Prepare-Revenue.mjs`, the policy pages and both legacy redirect documents in full. Account, privacy and home links are added only when absent from visible page navigation; links to the current page are omitted. Deferred application rendering and responsive visibility are handled. A failed advertising configuration request no longer removes basic navigation. Existing static header returns remain available.

The shared navigation uses its own inline container instead of inheriting product-specific menu layouts. Advertising eligibility and configuration checks remain intact. The existing contrast guard remains active; authored media is not recolored.

## Additional source reading

- Complete HTML, CSS and scripts: Pixel Loop, Rhythm Desk, Comic Workshop, Daily Planner and Recipe Book.
- Complete three product guides, shared guide stylesheet and two legacy redirect documents.
- Complete shared LanguageSwitch, PageSurface and ReportBox markup; shared rules/styles and browser bootstrap were also read.
- The account cleanup was staged independently of unpublished Release Deck work. The latter remains preserved locally.

Removed Pixel Loop's unnecessary dimensions badge; Daily Planner's character counter, generic planning advice, duplicate backup footer and empty task recap; Recipe Book's unused measurement footer and repeated storage wording. Distinct exports, recovery/privacy information and actual instructions remain. The bootstrap now uses one loading status.

## Full product follow-through

- [36 static games and creative tools](manual-ui-static-followup.md): complete reading, exact changes and unchanged pages.
- [Eight community applications and twelve creative tools](manual-ui-community-creative-followup.md): complete markup/style/handler reading and resulting changes.
- [Other data applications](manual-ui-data-followup.md): account, collaboration, review, catalog, procedure and reasoning-game screens.
- [Four mounted applications](manual-ui-independent-projects.md): prior complete UI/service review, retained at the exact revisions in the mounted-source manifest. Their current primary locations are recorded in [mounted hosting](../docs/Mounted-Products.md).

These completed groups and the five additional static pages map to the 77 catalog products. The four mounted applications retain their previous complete source review at the pinned revisions; the other 73 products received the new source pass. Account, guides and policy pages are extra screens, not extra products.

## Verification

The actual shared navigation and directory scripts passed 21 controlled DOM consumer assertions, including delayed navigation, hidden menus, closed/open/nested disclosures and summary links, cross-origin links, configuration-independent navigation, multiword search, no results and clearing the search. The three edited additional-page scripts parsed. These checks do not represent native interactions. Product-specific checks and reading ranges are in the linked records. Coordinated publication evidence is appended after deployment.

## Published result

The main cleanup source `27ef6774334b24a1df71db38e91432ddc1ea2fee` passed [workflow 40143810261](https://github.com/muhammetozeski/BinSite/actions/runs/40143810261). The final two live findings were included in `cc8355428cc4aaf53f3fe63826aa612f5eacad9e`, published successfully by [workflow 40144362519](https://github.com/muhammetozeski/BinSite/actions/runs/40144362519).

After the main cleanup, the existing delivery verifier checked all 77 catalog URLs: HTTP 200, English document metadata, dark defaults and appearance dependencies. Directory CSS/search and shared navigation assets matched source. After the final release, the two changed resources—`assets/revenue.js` and `fabrika-akisi/index.html`—again returned HTTP 200 and matched the final source. Unchanged delivery checks were not repeated merely to create another count.

Read-only production observation showed the compact directory with one opening link per entry, Story Tree's story/example/choice screen, Catalog Studio's actual editable example catalog, and Factory Flow's working counters. Factory's zero-stock sale buttons were visibly disabled while positive-stock sales remained available. Story Tree retained visible footer privacy access with its help closed and did not duplicate its header Home link. The initial native observation exposed the closed-disclosure geometry issue; the final observation confirmed its correction.

[Published directory screenshot](homepage-cleanup.png). Browser navigation and observation did not exercise forms, file dialogs, clipboard, payments or account mutations. This evidence is narrower than claiming every state and interaction was graphically tested.

The later [Release Deck follow-through](manual-ui-release-deck.md) extends this reading record to the 78th catalog product, records its remaining visible repetitions and corrects the earlier interpretation of deep-link delivery checks. Its first publication and subsequent corrections are separate from the 77-product cleanup above.
