# Manual UI review — unpublished existing drafts

These are preserved source drafts. They are not reported as published, complete, or additional counted products. This review does not add a backend, complete a new product or publish anything. No automatic text search, regular-expression scan or keyword report is used. Source/UI completeness and missing connections are recorded separately.

## Static drafts — full source read

Each file below was read in full in tool output, lines 1–6: authored markup/styles, the entire long script line, exports and import validation. No output was truncated.

### Explanation Studio — public/aciklama-studyo/index.html

- First task: the real water-cycle diagram and step viewer are useful, but the introductory sentence explains “nodes”, “focus” and “standalone” export before describing the simple task.
- Exact unnecessary/unclear text: “Build a process from nodes and choose the focus of each step”, “Download standalone explanation HTML”, two “Delete” buttons, and “Connect or disconnect”.
- Useful behavior: dragging moves elements; selected steps highlight chosen elements; arrows toggle between selected endpoints; project files retain the whole diagram; exported HTML contains an actual step viewer. This is local file work, with no network sharing service.
- Applied: a short purpose, named Remove element/Remove step actions, Add / remove arrow and Save interactive explanation. Previous/Next now also selects the displayed step in the editor, rather than leaving the editor on another step. Diagram text chooses black or white against the actual composited element color; the standalone viewer uses the same rendering function. Exported viewer data remains JSON-escaped. Editor/export controls use calm dark surfaces while the diagram artwork remains unchanged.
- Draft boundary: no persistent account/project service, public publishing or multi-user collaboration is implemented by this file. Step/form edits still require their explicit Update/Save actions.

### Collage Workshop — public/kolaj-isigi/index.html

- First task: the canvas contains a real three-layer example. Add images and the layer controls perform the advertised work.
- Exact unnecessary/unclear text: “actual blend modes”, “Add local images”, and “Original graphic examples”. Arrow symbols obscure whether the control moves a layer in the list or in front of another layer.
- Useful behavior: masks, opacity, grayscale, rotation and compositing really affect image pixels; project files include resized image data. Moving a layer in the array changes front/back compositing order. Empty controls previously appeared usable without a layer.
- Applied: “Combine photos, shapes and layers into a collage”, Add images, Front/Back and Save project file. Selection-only controls are disabled without a layer. Image limits are in Help. Loading the example or replacing a collage with a validated file asks before overwriting the open collage.
- Draft boundary: images and projects are local; no shared/server project exists. Async multi-file preparation has no whole-action busy lock and is not reported as a completed collaboration workflow.

### Draw with Commands — public/komutla-ciz/index.html

- First task: the source and rendered drawing are real inputs/outputs, but the full language reference dominated the initial editor.
- Exact unnecessary/unclear text: “Your text is interpreted rather than executed as JavaScript”, “Interpret program”, “Complete language reference”, and the status inventory of interpreter steps/units. The animation toggle did not show whether it would stop.
- Useful behavior: the bounded drawing language supports loops, position stack, pen state and colors. The progress slider reveals a prefix of the drawing, while SVG exports the complete successful drawing. Syntax errors carry line information.
- Applied: Write commands and loops to draw; Draw; a closed Commands and limits reference; Drawing progress; Save complete drawing (SVG). The status shows the actual number of lines. Animation changes its button label while active. A failed program disables SVG instead of offering an old successful drawing as if it came from the current invalid program.
- Draft boundary: this is a bounded local interpreter with file import/export, not arbitrary program execution or a hosted development service.

### Mini Game Engine — public/mini-oyun-motoru/index.html

- First task: the editable level, visible tile selector and playable character are the actual output. The opening claim about “actual movement and collision physics” adds development-process wording.
- Exact unnecessary/unclear text: “test its actual movement and collision physics”, “Download standalone playable HTML”, and touch movement controls visible while editing, when movement cannot occur.
- Useful behavior: painting sets tiles; the start marker is unique; stars and the door control winning, spikes/falling restart the player, and the exported HTML includes the same engine and touch/keyboard controls.
- Applied: a short paint/play/share purpose, Play level, Save playable game and Save level file. Touch movement is shown only in play mode; the exported game keeps its controls. Example/blank replacement asks before discarding a current level.
- Draft boundary: start/door checks are present, but they do not prove a hand-authored level is solvable. There is no online level repository or multiplayer service.

### Sound Effects Lab — public/ses-efektcisi/index.html

- First task: the real layer list and sample waveform are useful; the purpose and repeated “actual” output claims make a simple sound-making task read like implementation proof.
- Exact unnecessary/unclear text: “sample by sample”, “actual WAV”, “Material changes partial ratios and damping”, “Synthesize again” and the constant sample-rate/seed status inventory.
- Useful behavior: Add and Apply render actual sample data; the waveform, listening and WAV use that data. Seed reproduces the same variation. Material/hit controls do not affect every sound type. Opening a saved design previously left old form values visible, and deleting an earlier layer could point selection at another layer.
- Applied: Layer sounds, listen, and save your effect; Pitch, Hits or steps, Variation number, Apply layer changes and Save sound. Removed the redundant Synthesize again control: it re-rendered stored layers without applying the visible form, while Add/Apply already render. Selection now updates the form after import/delete, and the selected row is marked. Irrelevant type controls are disabled. Changing rendered sound stops the old player. Example/file replacement asks before overwriting the current design.
- Draft boundary: the sound is synthesized locally; no uploaded recording, online remix service or shared revision history exists.

All five changed script bodies passed Node `vm.Script` syntax compilation. This is not a claim of native audio, downloads, file-dialog use or visual browser interaction.

## Social draft source — complete reading

Original line ranges read in full before edits, with no truncated output:

| File under `src/BinSite.Social/` unless otherwise stated | Lines |
| --- | --- |
| `Pages/CrowdRiddle.razor` | 1–23 |
| `Pages/CrowdRiddle.razor.css` | Complete line 1 |
| `Pages/LetterChain.razor` | 1–26 |
| `Pages/LetterChain.razor.css` | Complete line 1 |
| `Pages/RecipeLaboratory.razor` | 1–27 |
| `Pages/RecipeLaboratory.razor.css` | Complete line 1 |
| `Pages/SoundExchange.razor` | 1–29 |
| `Pages/SoundExchange.razor.css` | Complete line 1 |
| `Pages/WorldNotebook.razor` | 1–25 |
| `Pages/WorldNotebook.razor.css` | Complete line 1 |
| `wwwroot/sound-patches.js` | 1–16 |
| `public/sound-patches.js` | 1–16 |
| `Pages/PaperTrailBureau.razor` | 1–101 |
| `Models/PaperTrailModels.cs` | 1–94 |
| `Services/PaperTrailCases.cs` | 1–41 |
| `Services/PaperTrailCasesIntroductory.cs` | 1–74 |
| `Services/PaperTrailCasesIntermediate.cs` | 1–94 |
| `Services/PaperTrailCasesAdvanced.cs` | 1–99 |
| `Services/PaperTrailEngine.cs` | 1–221, in two contiguous outputs |
| `Services/PaperTrailExports.cs` | 1–60 |

The file inventory has no `PaperTrailBureau.razor.css` or matching code-behind file. Neither was invented for this review.

### Crowd Riddle

- First task: create a group riddle, reveal clues and enter a three-digit code. The opening “LET CURIOSITY BE COLLECTIVE” slogan and explanation that locked text is not sent to the browser obscure that task.
- Unnecessary exact copy: “Locked clue text is not sent to the browser before its threshold”, “This clue remains on the server”, and “the correct solution was recorded on the server”. The initial fictional scene also explained that sample users/votes were not fabricated.
- Useful behavior: clues require one, then two, then three participant votes; they open in order. The answer remains a string so a leading zero is retained. The public scene is a starting example, not a pre-existing user group. Vote identity is browser/cookie based, not proof of separate people.
- Applied: short reveal/solve purpose, Create riddle, Invite group and Check code; plain waiting/result text; labelled example setting; voting/identity detail in Help. The New riddle form closes when a riddle is open. Existing vote counts, gates, nickname entry, refresh and reporting remain.

### Letter Chain

- First task: invite a small group and take turns adding fictional letters. “A FICTION TABLE, NOT AN EMAIL SERVICE” is an unnecessary comparison; the purpose paragraph then explains server storage instead of writing.
- Unnecessary exact copy: “The server stores who writes next”, “Every writing turn was checked by the server”, and the database/encryption inventory in the footer.
- Useful behavior: link holders may join only before writing starts; the owner starts with at least two writers, closing joining. Only the current writer adds the next letter. Completed stories remain readable to the authorized group. Refresh is necessary to see a new turn; automatic email delivery is not implemented.
- Applied: a short shared-fiction purpose; Opening ideas; direct nickname/join wording; Start writing and close joining; concise refresh/result messages. Privacy/joining details retain that group members and the site operator can read the story and that personal secrets should not be used. All opening texts, saved letters, turn/member limits and reporting are preserved.

### Recipe Laboratory

- First task: compare recipe variations and plan group ingredients. “let the server combine” and “Server-combined group ingredients” describe the mechanism unnecessarily.
- Unnecessary exact copy: “actual participant selections”, “This initial text is an editorial example” inside the recipe instructions, and “instruction texts remain stored separately”. A full new-recipe form stayed visible next to a branch form using the same bound fields.
- Useful behavior: each version keeps its parent; ingredient differences use each version's stated serving count, not normalized portions. Group totals use actual chosen versions/servings and keep different units separate. Ingredient text uses quantity, unit and name separated by a vertical bar; the parser error should explain that format.
- Applied: clear purpose and Group ingredients heading; short example/unit notice; explicit serving-count comparison note; actionable malformed-line message. New recipe creation closes while a recipe is open, reducing duplicate forms without removing creation. Recipe values, version differences, instructions, plans and reporting remain.

### Sound Exchange and both sound-patches modules

- First task: make a sound, listen, then share/remix its settings. “THE RECIPE OF A SOUND, NOT A FILE”, “Synthesize actual browser audio” and “No binary audio is uploaded” are repeated process descriptions.
- Useful behavior: local presets play without creating a shared patch. A published remix retains its parent ID. The visible note bars reflect the actual note steps. Browser synthesis uses the shared wave/frequency/duration/gain/sequence, with a bounded volume. Both module copies were read individually and have the same implementation.
- Applied: short make/listen/remix purpose, Example sounds and Volume; plain note-entry instructions and a nickname field only before membership is established. The draft now rejects invalid note counts/values and out-of-range pitch, duration or volume before preview/publication, rather than silently previewing the bridge's clamped values and then failing on publication. This does not change server bounds. Audio-unavailable errors are plain language in both JS copies.
- Draft limitation retained: the browser audio module uses one mutable context. Stop can overlap an awaited resume because the Stop control remains available; this review does not claim a verified concurrent native-audio session. No new audio service, file-upload feature or public publication was added.

### World Notebook

- First task: write places on a fictional grid. “SHARED FICTION, NOT REAL LOCATION” and the long revision/server footer repeat the scope before the user edits a place.
- Useful behavior: cells are fictional coordinates, populated cells open place text, members edit, and the last four previous texts remain. Concurrent edits require a refresh rather than silent overwrite. The place ideas fill draft fields; before creation/joining those fields are not visible.
- Applied: a short map-writing purpose, New world in a closed section when a world is open, Place ideas and Save changes. Choosing an idea now gives a state-specific message: create a world, join it, or edit the selected cell. The editing/history explanation is in Help. Existing ideas, cells, previous text, coordinate bounds and reporting remain.

### Existing backend boundary for these five drafts

The root agent independently read the entire current `functions/api/collaboration/[[path]].js` and confirmed registry support; this document does not pretend that I independently reread that backend file. Crowd Riddle has create/join/hint/answer with hidden locked clue text, vote thresholds/order and answer gating. Letter Chain has private member-only text, open joining, owner start and current-writer letter transitions. Recipe Laboratory, Sound Exchange and World Notebook kinds also exist in that same registry. The draft pages call those existing `CollaborationComponent` routes. No backend was written or extended here. A source-connected API is not a deployment or live UI verification.

## Paper Trail Bureau — useful domain content, incomplete UI binding

- First task: the draft describes reading fictional documents, testing a theory and closing a case with cited answers. The long “Eighteen records. No hidden answer sheet” explanation presents the solver implementation before any case is chosen.
- Unnecessary exact copy: “No account or API key”, “actual worlds survive”, “Check actual solvability”, “Download checked challenge JSON”, and the closing explanation of the server/offline installation boundary. The exported private report also used investigator-identity and engine/proof terminology where a simple private-notes warning is enough.
- Useful implemented domain behavior: cases declare finite choices and typed documentary facts. The engine enumerates consistent scenarios, handles complete OR alternatives, distinguishes supported/possible/contradicted hypotheses, and checks both a finding's full possibility set and its exact supporting citations. Deliberately ambiguous cases must retain every possible answer. Case/run files use strict validation and content identity; private notes belong only in explicit private-run/report exports. Challenge HTML and private findings HTML are distinct outputs.
- Content reading: all eighteen complete case definitions, explanations, domains and document excerpts were read. The six introductory, six intermediate and six advanced cases remain intact. In particular, the ambiguous archive owner, moth handler/carrier and cliff operator/lens evidence was preserved rather than forcing a single answer. Document prose is gameplay content, not filler to remove merely because it is long.
- Applied presentation edits: Choose a case; a one-line reading/testing task with longer rules in How to play; Test a theory; short file/report actions; ordinary scenario/solution wording. The standalone report keeps its private-notes warning and uses a calm dark screen with white print styling. No puzzle facts, answer sets, model fields or solver logic were altered.
- Missing connection: the assigned Razor file contains markup only. It has neither `@code` nor an inherited product component, and the actual matching file inventory contains no code-behind or isolated CSS. It references `View`, `Busy`, `Run`, `Desk`, `Module` and numerous handlers, but these UI/storage/export bindings are absent from the inspected product source. The existing domain engine does not make those buttons operational. The UI component was therefore not included in the successful five-draft compile, is not reported as playable/ready, and was not completed during this review.

## Verification boundary and handoff

- Five static script bodies passed syntax compilation. The two sound-patches modules passed Node syntax checking.
- The five collaboration Razor files were copied verbatim into the existing disposable `C:\C\BinSiteStaticReadability\WorkspaceChecks` consumer project and compiled against existing model/helper types with zero warnings and zero errors. No shared lower layer was changed for this compilation. This establishes source compilation, not actual account/game/audio interactions.
- All seven Paper Trail model/domain/export source files also compiled independently in `C:\C\BinSiteStaticReadability\PaperDomainCompile` with zero warnings or errors after the report-copy change. No solver behavior test was repeated for unchanged logic. Their incomplete page was not presented as a successful UI build. No new backend, bootstrap, catalog record or publication was created.
- No source/data file was deleted or moved. This group's current edits are unstaged and uncommitted. The published-page review records remain separate from this draft review.
