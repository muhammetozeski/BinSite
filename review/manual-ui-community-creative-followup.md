# Community and creative UI follow-up

## Scope and method

The current README, AGENTS and `review/manual-ui-main-apps.md` were read before this pass. The sources below were then read in full, including markup, styles, application scripts and event handlers. The earlier report did not substitute for reading the current implementation. No automated keyword search was used. Findings describe source behavior, not live browser observations.

Ranges below identify the files as read before the edits. Paths are relative to the repository root.

## Full source reading record

The first eight entries are under `src/BinSite.Social/Pages/`. Both the Razor file and its isolated CSS were read in full.

| Page | Razor lines | Isolated CSS lines |
| --- | --- | --- |
| StoryTree | 1–174 | 1–5 |
| IdeaDuel | 1–49 | 1–3 |
| TimeCapsule | 1–45 | 1–2 |
| SharedCanvas | 1–44 | 1–3 |
| DebateStage | 1–62 | 1–1 |
| TimeMarket | 1–51 | 1–1 |
| SharedMuseum | 1–44 | 1–1 |
| PasswordTeam | 1–47 | 1–1 |

Supporting behavior was read in full: `src/BinSite.Social/Shared/ProductComponent.cs` (1–64), `src/BinSite.Social/Shared/CommunityComponent.cs` (1–16), and `src/BinSite.Social/Services/UiText.cs` (1–33). These helpers and all eight CSS files were unchanged by this pass.

Each static entry below is `public/<folder>/index.html`. Its complete authored HTML, inline CSS and JavaScript were read, including initialization, storage, sharing and export behavior.

| Folder | Full lines read | Result |
| --- | --- | --- |
| kural-atolyesi | 1–62 | No change |
| gizli-harita | 1–67 | Updated |
| ses-atolyesi | 1–30 | Updated |
| sesli-kartpostal | 1–30 | Updated |
| isik-resmi | 1–28 | Updated |
| kaleydoskop | 1–28 | Updated |
| zincir-fizik | 1–35 | Updated |
| harf-afis | 1–28 | Updated |
| doseme-ustasi | 1–28 | Updated |
| yuruyen-avatar | 1–28 | Updated |
| dokuma-tezgahi | 1–28 | Updated |
| ritim-cemberi | 1–28 | No change |

## Changes made

The eight Razor changes are included in commit `2da6c97`.

- **Story Tree:** shortened the introduction and publishing confirmation, removed a repeated example explanation, and hid the new-story action while its form is already open.
- **Idea Duel:** replaced two refresh controls with one that refreshes both the list and current question; removed a redundant choice badge and shortened voting/reporting copy while retaining voting limitations.
- **Time Capsule:** shortened reporting/privacy copy and guarded the new-letter action while another operation is pending. Opening dates, operator access and lost-link consequences remain explicit.
- **Shared Canvas:** one refresh updates the list and open canvas while keeping unsaved pixels. Conflict guidance now explains what the user should do.
- **Debate Stage:** one refresh updates the list and current discussion. Opening a discussion exits the compose view only after loading succeeds. Argument instructions and counterargument controls remain.
- **Time Market:** removed overlapping compose/proposal views and repeated implementation explanations. Public/private information warnings and both-party completion behavior remain.
- **Shared Museum:** one refresh updates the directory and current exhibition; composing no longer overlaps the old gallery. Examples remain identified as fictional, and curator controls remain available.
- **Password Team:** removed a repeated role heading and example disclaimer, shortened action labels and answer instructions, and retained the role, clue, expiry and leading-zero requirements.

The static changes affect only the ten `index.html` files marked Updated above.

- **Hidden Map:** removed the second control calling the same preview action. The active mode and the already-open introduction cannot be selected again unnecessarily. Sharing confirmation is shorter; the warning that answers are inspectable remains.
- **Audio Workshop / Audio Postcard:** disabled actions with no source, clips or active playback. Shortened sample/playback messages and clarified that audio stays on the device and should be downloaded before closing.
- **Light Drawing / Kaleidoscope:** disabled empty undo, clear, replay or rotation actions as applicable. Clarified exposure persistence and made the drawing instruction valid for both symmetry modes.
- **Chain Physics:** renamed sharing to Copy layout link. The 45-second stop now directs the user to reset or edit instead of implying successful completion. Empty controls are disabled, and Help says to pause before editing.
- **Type Poster:** removed a redundant live-preview instruction and shortened Download vector SVG to Download SVG.
- **Tile Workshop / Weaving Loom:** distinguished PNG output from saving editable work on this device and restoring that saved work. Weave-grid CSV still explains that it excludes colors.
- **Walking Avatar:** replaced interpolation/rig wording with pose-setting steps and animation-link labels; removed a repeated initial instruction.

**No-change decisions:** Rule Workshop's B/S rules, boundary behavior and pattern instructions explain the experiment; its controls have distinct effects. Rhythm Circles' pulse, cycle and sound controls are distinct, and Copy ratio link accurately describes a link that does not include sound-toggle state. No unnecessary copy or duplicate action was identified in either source.

Different outputs such as PNG, SVG, editable project files and weave CSV were retained. Header return navigation, useful task steps, storage limits and privacy consequences were preserved. Shared appearance and revenue assets were outside this edit scope.

## Verification boundary

All ten changed static application scripts passed JavaScript syntax compilation with Node's `vm.Script`. This check parses the actual inline scripts without executing the interface. No browser playback, download, clipboard, storage or interaction result is claimed by this pass. No separate shared C# build was performed by this agent; integration and publication remain coordinated by the root agent.
