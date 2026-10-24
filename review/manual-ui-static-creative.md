# Manual UI review: static creative products

This is a fresh page-by-page reading of complete authored HTML, CSS and JavaScript. The earlier batch cleanup is not represented as a full code review. Source ranges below refer to the complete working file read before that page's edits. No automated text search, keyword scan or generated phrase report is used to select findings. Source inspection is distinguished from browser interaction; this record does not claim GUI controls were clicked.

## 1. Pixel Loop — cizgi-dongusu

- Read: `public/cizgi-dongusu/index.html`, lines1–77, including every drawing, frame, storage and export handler.
- First view: the canvas, pencil/eraser/fill, frame strip and Play communicate a frame animator. The blank canvas is an actual drawing input.
- Unnecessary or misleading copy: “32 × 32 / FRAME BY FRAME” repeats the concept; “Create locally with drawing, fill, frame management, project files and image export.” repeats visible tools. The workshop says “Duplicate it and move the drawing slightly.” although the code has no move-selection tool.
- Useful behavior that needs clearer naming: PNG export always uses the selected frame, even during playback; onion skin is excluded. Undo retains twenty editing states, including replacing the project with the bouncing-ball example.
- Applied: badge “32 × 32 pixels”; removed inventory footer; workshop now says to change a few pixels after duplicating; export says “Save selected frame as PNG”. Drawing, fill, history, import and file formats remain.

## 2. Rhythm Desk — ritim-masasi

- Read: `public/ritim-masasi/index.html`, lines1–58, including oscillator/noise generation, audio scheduler, live edits, scale, mute and link encoding.
- First view: sixteen cells per instrument plus tempo and Play explain the sequencer. The saved pattern is not an audio recording.
- Unnecessary or misleading UI: “16 steps · 6 sounds · unlimited experiments”, decorative “▂ ▆ ▄ █”, separate “■ Stop” beside a button already toggling stop, and “Audio status / Enabled by Start”. The animated `.meter` bars use a fixed CSS pulse unrelated to measured sound. “Bass and melody follow the selected scale” is inaccurate: bass uses its selected fixed pitch; scale changes the lead intervals.
- Hidden useful behavior: a letter button mutes its row; editing steps/tempo works during playback. The link includes mute state, scale, bass, tempo and swing. The empty readonly URL field adds no value before sharing.
- Applied: removed decorative header/meter and duplicate Stop/status controls together with their direct DOM writes; the remaining transport still calls the same stop function. Row actions now say Mute/Unmute. Corrected bass/scale help and percent labels; shortened Save to “Save pattern”; URL field appears only after link creation. Sound generation and scheduling are preserved.

## 3. Comic Workshop — sonsuz-cizgi-roman

- Read: `public/sonsuz-cizgi-roman/index.html`, lines1–62, including every SVG scene/character/bubble, editable field, panel mutation, validation and export path.
- First view: the illustrated sample, selected panel tabs and dialogue fields make the task clear. This is a still comic, not an animation or a print-mode implementation.
- Unnecessary or misleading copy: “Three panels from reader to storyteller”, “Create comics in your browser. Write your own dialogue and character names.” and the technical “Standalone vector drawing” counter. “Print preview” suggests a print-specific mode that the code does not provide.
- Hidden useful behavior: shared links open an editable copy; SVG stays scalable, PNG is rendered at twice the SVG dimensions, and the project file preserves future editing. The empty sharing box need not occupy the screen before a link exists.
- Applied: removed the slogan/footer; preview is “Your comic”; count is simply panels; exports distinguish “Save vector drawing”, “Save image” and “Save editable project”. Sharing input appears on demand. Kept all panel, expression, character and download capabilities.

## 4. Rule Workshop — kural-atolyesi

- Read: `public/kural-atolyesi/index.html`, lines1–62, including neighbor updates, wrapping, direct painting, pattern/reset semantics and bit-packed sharing.
- First view: a seeded cell field and Start work, but eighteen neighbor checkboxes plus a second rule-text representation overload a beginner before the first action.
- Unnecessary UI: constant “Field / 96 × 64”, repeated footer “Explore how a single rule changes the world.” and the empty link field. The legend swatches were white/gray while actual cells are green/dark green.
- Hidden useful behavior: dragging draws or erases according to the starting cell; Reset restores initial cells while retaining the current rule; a step pauses and advances exactly once; each supplied pattern selects its appropriate rule.
- Applied: manual checkbox/text rule editing is behind “Edit the rule”, while presets and wrap remain available. Corrected legend colors to match `draw()`, clarified “Restore starting cells” and “Next generation”, exposed drawing guidance in the initial status, corrected percentage order, removed the constant field/footer and deferred URL display. Preserved custom rules and all simulations.

## 5. Hidden Map — gizli-harita

- Read: `public/gizli-harita/index.html`, lines1–67, including sample puzzles, complete/draft validation, all three puzzle types, player phases, editor saving and sharing.
- First view: the original three-room adventure can be played immediately and the editor is a separate mode. However, the initial “An adventure from a friend” falsely describes a bundled example.
- Unnecessary or misleading UI: “Browser escape adventure”, footer “Write the adventure for a friend to explore. At the end, they can create their own alternative map.”, repeated sharing sentence in the room count and the always-empty link box. The final “Step into the lighthouse” is hardcoded even for user-authored worlds.
- Hidden useful behavior/problem: the header Play handler only switched mode and could play the old map after editing `draft`; only the lower preview action validated/copied the draft. Invalid-link feedback was immediately cleared by the initial `renderPlayer()`.
- Applied: labelled Example/Shared/Your adventure according to the actual source; header Play and Preview use one draft-validation path while leaving an in-progress player intact when already playing. Generic “Finish adventure” fits custom worlds. Preserved the invalid-link explanation after rendering; made sharing input on-demand; shortened the answer-visibility warning and removed repeated footer/count copy. All puzzle types, hints, editor data and map exports remain.

## 6. Audio Workshop — ses-atolyesi

- Read: `public/ses-atolyesi/index.html`, lines1–30, including decoding, sample creation, trim copying, reverse/fade, concatenation and WAV writing.
- First view: opening audio and selecting start/end times are understandable. The waveform is a preview, not a draggable selection.
- Unnecessary copy: “Source files stay unchanged. Selected clips form a new assembly.” and source metadata “channels · samples/sec”. “Assembly” is repeated across several actions without helping the listener.
- Hidden useful behavior: Reverse and Fade affect the selected clip before it is copied into the sequence; later setting changes do not rewrite clips already added. Sources can be reused, clips reordered, and mono/stereo combined.
- Applied: visible Source label; “Try sample audio”, “Add clip”, “Play sequence” and “Save audio (WAV)”; clarified Reverse selection/Fade selection edges; retained filename/duration while removing sample-rate/channel chatter. Kept trim, sequence, reverse, fades and format/length limits.

## 7. Audio Postcard — sesli-kartpostal

- Read: `public/sesli-kartpostal/index.html`, lines1–30, including deterministic melody, message rendering, oscillator playback, local restoration and link validation.
- First view: the finished card and edit fields communicate the purpose. “Listen to card” can imply spoken text, but the code only plays a twelve-note melody.
- Unnecessary copy: “Audio starts only when you press Listen.” and the long recipe/browser/voice explanation. The seed number is an advanced repeatability control, not the first task.
- Hidden useful behavior: the seed reproduces a melody; mood changes its scale. The drawn note heights come from the same pitches and are functional. Paper color is user artwork, so theme recoloring must not override it.
- Applied: “Play melody”, “Copy card link”; moved melody number into a disclosure and made the URL appear on demand. Marked the card as appearance-preserved; its chosen paper now gets a black/white text and note-stroke color based on luminance. This preserves the actual paper-color control instead of masking it with the UI theme. Kept the privacy warning about link readers.

## 8. Light Drawing — isik-resmi

- Read: `public/isik-resmi/index.html`, lines1–28, including stroke compositing, exposure, pointer capture, replay, PNG and restored stroke validation.
- First view: colored light drawing, brush and exposure are clear. The glow belongs to the drawing output rather than an unrelated interface animation.
- Unnecessary copy: “Create a light signature, cover image or abstract pattern. No camera access is needed.” repeats possible uses and implementation reassurance.
- Hidden useful behavior: color and width are captured per new stroke; exposure redraws all strokes. Only the stroke array is saved, not exposure. Replay reconstructs stroke order and PNG captures the fully redrawn image.
- Applied: removed the footer; clarified Stroke width/Color style/Clear drawing. Help now distinguishes per-stroke settings and whole-image exposure and states that exposure must be set again after reopening. No stored strokes were changed.

## 9. Kaleidoscope — kaleydoskop

- Read: `public/kaleydoskop/index.html`, lines1–28, including symmetry transforms, inverse-rotation drawing coordinates, project validation and exports.
- First view: the actual rosette and drawing controls explain the tool. Mobile column reversal placed the title and purpose after the canvas/tools.
- Unnecessary or misleading copy: “Clear the example to build your own pattern.” is an unnecessary instruction: adding strokes to the existing drawing works. Ink/width labels do not say that they affect new strokes only.
- Hidden useful behavior: sector/reflection/background settings rebuild every stored stroke; color/width apply only to the next stroke; the saved project preserves rotation and editable paths.
- Applied: “New stroke color”, “New stroke width”, “Clear drawing” and direct drawing guidance. On mobile the title/purpose precede the canvas while the rest of the settings follow it; desktop remains a sidebar and large canvas. All symmetry and project functions remain.

## 10. Chain Physics — zincir-fizik

- Read: `public/zincir-fizik/index.html`, lines1–35, including ramp collisions, ball collisions, one-shot target spawning, timed stepping and layout sharing.
- First view: ramps, balls and the target counter correspond to the physics shown. The graphic is the editable simulation itself.
- Unnecessary or unclear copy: “Target / new ball” sounds like two placement modes; “An experiment tool” is generic commentary; “Back to editing” hides the reset-to-initial-positions behavior.
- Hidden useful behavior: a target releases a new ball only on its first hit. Reset preserves the layout, while Undo removes the last placed part. Playback can pause; it stops after45 seconds.
- Applied: “Place release target”, “Start reaction”, “Reset positions” and “Clear layout”; tightened Help to actual interactions/limits and deferred the empty sharing field. Kept collision logic, one-shot triggers, all parts and sharing.

## 11. Type Poster — harf-afis

- Read: `public/harf-afis/index.html`, lines1–28, including all three SVG layouts, text escaping, PNG scaling and editable-link restoration.
- First view: the generated poster is a useful output, but mobile `aside{order:2}` put the product title after it.
- Unnecessary copy: “The poster uses a 600 × 800 canvas. Enlarge the SVG for printing.” exposes dimensions before the user edits. “Editable link” does not name the copy action.
- Hidden useful behavior: every text/style edit redraws immediately; SVG is scalable, PNG is1200×1600, and the link restores text, colors, layout, type and frame.
- Applied: mobile title/purpose now precede the poster; “Typeface”, “Border and motif”, “Copy editable link” and direct editing guidance. Link field appears after creation. Kept all layouts, artwork colors and exports.

## 12. Tile Workshop — doseme-ustasi

- Read: `public/doseme-ustasi/index.html`, lines1–28, including polygon hit-testing, painting, motif rotation, geometry generation and saved tile restoration.
- First view: the tiled surface is the input/output, with paint and rotation controls nearby.
- Unnecessary or misleading copy: “This tool creates a design rather than ordering physical goods.” is unrelated reassurance. “Single-color start” always rebuilds cream tiles, regardless of selected paint. Rotation was enabled even when motifs were hidden, producing no visible change.
- Hidden useful behavior: drag paints many tiles, whereas rotation changes only the clicked motif; changing shape discards current tiles; saved patterns include each tile's color and rotation.
- Applied: “Reset tiles”, “Shuffle pattern”, “Save pattern” and “Rotate motifs instead of painting”; rotation becomes unavailable when motifs are off, including after loading a saved pattern. Help states that changing shape clears tiles and recommends saving first. No saved pattern format changed.

## 13. Walking Avatar — yuruyen-avatar

- Read: `public/yuruyen-avatar/index.html`, lines1–28, including joint endpoints, cosine interpolation, stored poses, SVG snapshot and shared rig validation.
- First view: the character and limb controls show pose editing; mobile reversal hid the title below the graphic.
- Unnecessary or unclear copy: “From0.5 to4 seconds” shows a range rather than the selected duration; “Share rig” uses internal terminology. “Save this as pose A/B” can imply persistent storage, but these arrays are in memory.
- Hidden useful behavior: moving an angle does not store either endpoint until Set A/B; Show A/B recalls the endpoints. Duration16 means1.6 seconds. SVG exports the current pose, not an animation; the link contains both stored poses and timing.
- Applied: “Set pose A/B”, “Save current pose (SVG)”, “Copy animation link”, visible current duration and direct endpoint instructions. Mobile title precedes the stage; sharing URL appears on demand. Kept all bones, poses, examples and looping.

## 14. Weaving Loom — dokuma-tezgahi

- Read: `public/dokuma-tezgahi/index.html`, lines1–28, including weave formulas, top/left header hit areas, crossing toggles, image/grid export and saved colors/grid.
- First view: the weave is recognizable, but a paint color alone does not explain that only top/left swatches color complete threads.
- Unnecessary copy: “This is a weave draft rather than a fabric stretch or thickness simulation.” is an implementation comparison. “Download fabric PNG” is inaccurate when the over/under grid is being shown.
- Hidden useful behavior: tapping crossings swaps the top thread; top and left swatches color vertical/horizontal threads. PNG saves the current view. CSV carries only the binary grid; local Save retains colors too.
- Applied: “Show over/under grid”, “Save current view (PNG)”, “Save weave grid (CSV)” and “Save weave here”; shortened the direct hit-area instruction and clarified what each export retains. Preserved every weave and crossing.

## 15. Rhythm Circles — ritim-cemberi

- Read: `public/ritim-cemberi/index.html`, lines1–28, including ring marker timing, sound scheduling, duration, mute checkboxes and ratio sharing.
- First view: the colored rings represent the pulse counts, but Play was below the large clock. The extra Stop duplicated the same toggle handler.
- Unnecessary or misleading copy: “Enable this ring” only controls sound; it does not remove the ring. “Press Start to enable audio” explains startup plumbing rather than the listening task.
- Hidden useful behavior: rings meet at a common cycle boundary; pulse/duration changes stop playback. The share link contains counts and duration, not checkbox mute states. The moving markers are tied to the audio clock.
- Applied: moved the existing transport before the clock, retained one Play/Stop toggle, named the checkbox “Sound on” and sharing “Copy ratio link”. The URL is shown on demand. Kept the functional clock and sound synchronization.

## 16. Sound Viewer — ses-gorseli

- Read: `public/ses-gorseli/index.html`, lines1–28, including analyser data, oscillator/file sources, looping, live controls and image capture.
- First view: source controls are visible but the main playback action was below the graph. “Built-in oscillator” and several metric labels make the initial task harder than necessary.
- Unnecessary or unclear copy: “Press Start to analyze audio. Files stay local and may be up to30 MB.” mixes a startup command, storage and limit. Tone waveform/frequency controls remained enabled in file mode even though handlers intentionally ignored them.
- Hidden useful behavior: the two tone examples start playback directly; an opened file loops; listening volume is applied after the analyser and therefore is not the measured signal level. PNG preserves the current graph.
- Applied: transport before the graph; “Tone generator”, “Play and view”, clear tone examples, “Listening volume”, “Signal level” and “Frequency center”. Tone-only controls are disabled for a file and re-enabled for tone examples. Kept all analysis and export functions; file limits remain enforced.

## 17. Logic Board — mantik-tahtasi

- Read: `public/mantik-tahtasi/index.html`, lines 1–28, including node placement, input toggles, recursive evaluation, cycle detection, wiring and truth-table generation.
- First view: the gate diagram is the working circuit, but an input's body and its connection port perform different actions without a clear distinction.
- Unnecessary or unclear copy: “Drag a gate or click an input to toggle its value” does not distinguish the INPUT box from the input port. “This models combinational logic rather than electrical timing” is explanatory scope rather than an immediate task. Delete was available with no selection.
- Hidden useful behavior: clicking a left port without a selected output disconnects it; cycles evaluate to `?`. The truth table is a generated snapshot, not a live table. Its numbered columns previously did not identify the corresponding boxes.
- Applied: direct INPUT/port instructions, disabled Delete without selection, visible gate IDs matching the truth-table headings, and a snapshot caption reminding users to regenerate after edits. Sharing field appears on demand. Circuit evaluation and the four-input limit are unchanged.

## 18. Perspective Room — perspektif-oda

- Read: `public/perspektif-oda/index.html`, lines 1–28, including coordinate projection, painter ordering, item controls, rotation and shared-layout validation.
- First view: the room is the useful output, but the mobile reverse layout put the title and purpose below it. Empty-room controls suggested an item could still be edited.
- Unnecessary or unclear copy: “Furniture to add” and “Furniture to edit” are longer than necessary. The painted “420 × 320 sketch units · Perspective volume sketch” is internal measurement copy rather than help choosing an arrangement.
- Hidden useful behavior: selection changes the object controlled by the sliders; rotation changes its footprint; nearby objects appear larger. The arrangement is not a measured floor plan.
- Applied: title and purpose precede the stage on mobile; “New item” and “Selected item”; direct movement guidance; disabled selected-item controls when empty; on-demand sharing field. Kept the useful scale limitation in Help and preserved projection, rotation and layout data.

## 19. Paper Fold — kagit-katla

- Read: `public/kagit-katla/index.html`, lines 1–29, including the box net geometry, A4 fit calculation, cut/fold paths, exported dimensions and printing.
- First view: dimensions, an actual cut/fold pattern and Print make the task clear. The final size and control line are needed for the physical result.
- Unnecessary or unclear copy: “Update net” uses a specialist term; “scaled to %…” puts the percent sign before its value. “Actual dimensions” does not say these are the box dimensions.
- Hidden useful behavior: large dimensions are scaled to fit A4, so the finished box can be smaller. Download and Print previously reused an older generated pattern after fields changed, despite displaying the new values in the controls.
- Applied: “Update pattern”, “Box size” and correct percent formatting. Download and Print now regenerate from the visible inputs, refusing invalid dimensions. Kept the scale calculation, actual finished dimensions, fold/cut/glue legend and 20 mm print check.

## 20. Story Nodes — dugum-hikaye

- Read: `public/dugum-hikaye/index.html`, lines 1–28, including the original story, scene/choice editing, reachability, bounded reader navigation, JSON validation and shared-reader loading.
- First view: the writing/reading modes exist, but “Nodes” and a generic reachability count make editing harder to understand. Saving a story file had no matching way to reopen it.
- Unnecessary or unclear copy: “The example train journey has three endings” was shown even for other saved/shared stories. “In this journey: … choices made” adds a counter without helping the reader choose. The ending message sent the reader back to editing unnecessarily.
- Hidden useful behavior: the first scene is the start; disconnected scenes will not be reached. Reader links contain the story. Navigation has a 100-choice safety limit. The download contains an editable story, but import was missing.
- Applied: “Scenes”, “Scene title”, “Next scene” and “Story map”; labelled the starting scene; named disconnected scenes instead of only counting them; shortened the ending. Added Open story file using the existing validator, a size limit and assignment only after validation. Kept all scene content, choice editing, JSON format, navigation limit and sharing.

## 21. Word Composition — sozcuk-bestesi

- Read: `public/sozcuk-bestesi/index.html`, lines 1–28, including all original word banks, token placement, rearrangement, examples, poem text and editable-link restoration.
- First view: selecting words to compose is clear, but “Read an example” actually replaces the current poem and “New blank composition” clears only the poem.
- Unnecessary or unclear copy: “The challenge checks your chosen constraint rather than judging literary quality” explains what the product does not judge. “Check challenge” duplicates the always-visible word/line counters.
- Hidden useful behavior: the fixed goal is exactly 12 words across 3 nonempty lines. Shared poems carry their own bank, but the theme selector could still show a different named bank. The local empty action can keep that bank.
- Applied: “Word bank”, “Load example”, “Clear poem” and “Copy editable poem link”. Completion is checked automatically in the existing score; removed the extra check button and disclaimer. Shared banks now match a known theme or receive a “Shared word bank” option. Clearing a poem preserves its current bank; all words and composition operations remain.

## 22. Wind Art — ruzgar-sanati

- Read: `public/ruzgar-sanati/index.html`, lines 1–28, including particle motion, distance-based wind forces, trails, source positioning and parameter-link restoration.
- First view: wind arrows and the resulting trails explain the image, but source placement required an undisclosed keyboard modifier and was inaccessible through touch alone.
- Unnecessary or unclear copy: “Trail persistence” names the opposite of the slider's actual direction: a higher value fades old trails faster. “Share force field” uses a technical noun instead of naming what the recipient receives.
- Hidden useful behavior: Shift-click repositions the source. Image export leaves editing arrows out. A link restores parameters and arrows, not the identical random particle paths.
- Applied: “Trail fade speed”, a Draw wind arrows / Move particle source action selector, “Copy wind setup link” and concise export guidance. Preserved the Shift shortcut, particle engine, parameters and sharing format; the URL appears only after sharing.

## 23. Gesture Brush — jest-fircasi

- Read: `public/jest-fircasi/index.html`, lines 1–28, including gesture normalization, stamp rotation, stored brush paths, drawing strokes, sample path generation and PNG export.
- First view: the brush pad and main canvas are useful, but the action that enables recording followed the pad. Recording had no explicit cancellation action.
- Unnecessary or unclear copy: “Leaf-path example” did not say that it replaced every existing stroke. Repeatedly choosing the same leaf preset filled the saved brush library with duplicates.
- Hidden useful behavior: recording creates a brush shape used as repeated stamps along the main drawing; Undo removes the last whole drawing stroke. The library has a 20-brush limit and the drawing an 80-stroke limit.
- Applied: moved “Draw a new brush” before the pad and made it toggle to Cancel new brush. Choosing the leaf reuses its matching stored shape. “Add example path” now appends a sample stroke instead of discarding the drawing, so Undo can remove it. Kept brush normalization, artwork, limits and storage format.

## 24. Sound Landscape — ses-manzarasi

- Read: `public/ses-manzarasi/index.html`, lines 1–28, including generated noise/bells, layer levels, preset settings, timed fade/stop, transport and shared-mix restoration.
- First view: layer sliders relate to the sound, but the main Play action followed them. A repeating breath bar animated independently of sound and did not explain any control.
- Unnecessary or unclear copy: “Waveform” was a wave-like sound layer, not a visible waveform. “A fine rain texture through a high-pass filter” exposed an implementation detail. “Start soundscape” plus a separate Stop duplicated transport controls. The medical/sleep disclaimer was unrelated to the creative task.
- Hidden useful behavior: sliders update the playing mix; the selected stop timer fades out over the last five seconds. Share preserves the mix settings, not an exported audio recording.
- Applied: transport precedes the layers; one Play / Stop soundscape button; “Waves” and short sound descriptions; concise fade guidance; “Copy mix link” and on-demand URL. Removed the unrelated animated bar. Sound generation, levels, presets, fade and sharing are preserved.

## 25. Cipher Post — sifre-postasi

- Read: `public/sifre-postasi/index.html`, lines 1–28, including shift and dot encoders, challenge validation, wheel interaction, answer checking, examples, shared-link parsing and puzzle text export.
- First view: the write form is already open, making its second “Write a cipher message” mode button redundant. Example buttons fill the form instead of immediately opening a challenge.
- Unnecessary or unclear copy: three repeated challenge descriptions (“Find the shift from the number of days”, “Decode the words with the dot-and-dash table”, “Write a hidden message for a friend”) restate the examples. “Try this wheel result” does not identify the check action. Shift and code-alphabet controls could appear for the wrong method.
- Hidden useful behavior: starting messages populate the form for editing. Creating a puzzle opens the player and makes its sharing actions available. Returning to writing previously left those actions attached to the old challenge.
- Applied: hid the redundant mode button until a player is open; named examples “Load … message”; disabled Shift for dots and showed the alphabet only for dots. Returning to writing hides the old challenge link/text actions. Kept the useful “For puzzles, not private messages” warning, both encoders, wheel, answer checks and actual attempt/time feedback.

## 26. Daily Planner — gunluk-planlayici

- Read: `public/gunluk-planlayici/index.html`, lines 1–377, including complete CSS/markup, task filters/sort/edit/delete, date validation, note debounce/blur/page lifecycle saves, JSON backup validation and restore confirmation. Changed markup was reread at lines 130–158.
- First view: the task entry and actual task list explain the job, but four large zero counters pushed the form down. The sidebar repeated “Notebook”, “Ideas and reminders” and “Save notes” despite automatic saving.
- Unnecessary or unclear copy: “Plan your day” opens a full advice panel before the user needs advice. “Where is your data?” contains another nested “Help” heading. “Task added. Check the selected filter if it is not visible” appends conditional advice to every successful add. “Clear completed tasks” hides that this permanently removes those records.
- Hidden useful behavior: task Details are optional; editing an existing task restores its details. Notes save after typing, on blur and when the page becomes hidden. Save failure requires a retry or backup. Restoring a backup replaces data only after validation and confirmation.
- Applied: compact summary counters; optional Task notes disclosure that opens when editing a task with notes; one Notes heading and a Retry saving button only after failure. Moved planning tips and storage information into closed disclosures, retained visible backup actions, clarified Delete completed tasks and its existing confirmation, fixed the malformed single-task delete question and shortened add feedback. Kept all task/note fields, saved keys, search, filters, counts, date rules and backup content.

## 27. Recipe Book — tarif-defteri

- Read: `public/tarif-defteri/index.html`, lines 1–623, including all eight original recipes and their full ingredients/method/tips, CSS/print rules, legacy shopping-key migration, filters, portion scaling, combined ingredient calculation, check state, copying and printing.
- First view: recipe choices and the shopping list explain the result. The descriptions help choose food and the ingredients/method are useful content, so they were retained.
- Unnecessary or unclear copy: “Preparation and cooking” is a long time-filter label. “Base recipe … servings” repeats the adjacent editable Servings field. “Copy list” conceals that it copies only missing items.
- Hidden useful behavior: comma-separated search requires every term; changing servings scales ingredients; adding the same recipe accumulates servings and unchecks its ingredients. Copy and Print operate on missing items only, but stayed enabled after the final ingredient was checked. Later dark styling could override the earlier white print background.
- Applied: “Ready in”, “Copy missing items”, shorter search/add feedback and removal of the duplicate base-servings phrase. Copy and Print now follow the live missing-item count. Added scoped white/black print rules and hid the storage disclosure in print. Preserved every recipe, measuring guidance, favorite, menu entry, shopping key migration, quantity calculation and stored checked state.

## Verification boundary

All 27 complete authored source files were read manually. No text-search or keyword-report step was used to discover findings. The later source edits were explicit replacements chosen from that reading. JavaScript from the known line ranges above was compiled with Node's `vm.Script`: 27 of 27 passed. This confirms syntax; it is not a claim that browser actions, audio, downloads or printing were exercised during this review. No graphical control was clicked.
