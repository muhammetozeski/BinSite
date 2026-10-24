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

## Field Work Pack and Shift & Cover Desk follow-up

Both products were subsequently reviewed in full, including their current Razor markup, event handlers, styles, model, examples, validation, persistence bridge and exports. This is a source review; no browser session or provider/account action is claimed. No automated keyword search was used.

All paths below are under `src/BinSite.Social/`. Line ranges identify the source as read before these edits.

| File | Full lines read | Result |
| --- | --- | --- |
| Pages/FieldWorkPack.razor | 1–562 | Updated |
| Pages/FieldWorkPack.razor.css | 1–4 | Unchanged |
| Models/FieldWorkModels.cs | 1–98 | Unchanged |
| Services/FieldWorkCloud.cs | 1–86 | Unchanged |
| Services/FieldWorkExamples.cs | 1–77 | Unchanged |
| Services/FieldWorkReport.cs | 1–63 | Updated |
| Services/FieldWorkRules.cs | 1–215 | Updated user-facing messages only |
| wwwroot/field-work.js | 1–153 | Updated error messages only |
| wwwroot/field-work-sw.js | 1–38 | Updated error messages only |
| Pages/ShiftCoverDesk.razor | 1–335 | Updated |
| Pages/ShiftCoverDesk.razor.css | 1–1 | Unchanged |
| Models/ShiftCoverModels.cs | 1–87 | Unchanged |
| Services/ShiftCoverExamples.cs | 1–78 | Shortened visible example copy |
| Services/ShiftCoverExports.cs | 1–125 | Updated user-facing labels and explanations |
| Services/ShiftCoverPlanner.cs | 1–62 | Unchanged |
| Services/ShiftCoverRules.cs | 1–232 | Unchanged |
| Services/ShiftCoverVault.cs | 1–34 | Updated validation messages only |
| wwwroot/shift-cover.js | 1–62 | Updated error messages only |

### Field Work Pack

- The invitation message said "Open Private workspace", but the visible section is Sharing. It now names Sharing and explains joining.
- Both clicking the invitation field and the Copy button invoked the same copy action. Copying now uses the dedicated button; the field remains selectable.
- Raw source-image hashes were displayed in the checklist and report without a user task. Those displays were removed. Fingerprints remain in saved data and still enforce original-photo matching.
- Active section buttons, empty-title order creation, first/last field moves and adding beyond the field limit no longer invite ineffective actions. The current shared project's list entry does not duplicate its dedicated Reload control.
- "Committed changes", "photo bytes", storage-engine names and offline-worker setup explanations were replaced with editing, saving, photo and reconnect instructions. Error and history messages describe the affected task.
- Required-item checks, order status locks, template snapshots, photo decoding/matching and replacement confirmations remain. Project files/reports include photos; online saves exclude them. That distinction, original-photo relinking, the self-reported nature of completion and the genuine offline setup step remain visible.
- Save order file, save whole project, download report and print report were retained: they produce different scopes or delivery actions.

### Shift & Cover Desk

- "Choose the planning period" had no period-selection control. The heading now says Planning limits. Fixed-offset and daylight-saving consequences remain explained.
- "Actual availability", "entered capabilities" and similar qualifiers were shortened to the fields users edit. Project-file controls and download confirmations now describe the output directly.
- New person/New shift actually cleared the form. They now say Clear form or Cancel edit and are disabled when clearing would change nothing. Active tabs, empty snapshot names and assigning a seat to its existing occupant are also guarded.
- Open-seat reasons exposed internal codes such as `shift-hours` and algorithm language such as "bounded search" and "infeasibility proof". They now name the missing role, availability, shift hours, total hours, overlapping shifts or rest time, and explain what can be changed. They still refer only to the current assignments, not a proof that no solution exists.
- Snapshot sign-in/recovery instructions, example notes and export messages were shortened. The first snapshot view now tells users to refresh their saved-roster list.
- Full-project files, all-seat/person CSV, calendars and print reports were retained as distinct outputs. Current conflict checks, valid locks, swaps, fixed-offset conversion, previous calendar-import behavior, fictional-example labels, snapshot limits and staff-acceptance limitations remain. No planner, assignment rule, server API or persistence algorithm was changed.

The three edited browser scripts (`field-work.js`, `field-work-sw.js`, `shift-cover.js`) passed Node's module syntax check. This does not establish browser storage, image decoding, offline caching, printing, calendar import or account behavior. No shared C# build was started by this agent; the root agent coordinates the combined build. CSS and layout were preserved. No staging or publication was performed by this agent.
