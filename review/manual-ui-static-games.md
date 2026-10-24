# Manual static UI review

The listed source ranges were read in full, including embedded CSS, authored content, initialization, event handlers and storage/export behavior. Findings were written from that reading, not from a text scan. No automatic word search, regex audit or generated keyword report was used. This record is source review, not a claim of browser interaction testing; earlier feature tests are not presented as manual UI reading.

Shared dependencies read once for all listed pages: `public/assets/appearance.css` 1–9, `public/assets/appearance.js` 1–145, `public/assets/revenue.js` 1–29. The appearance script adds one theme toggle; the revenue loader adds account/advertising/privacy/home links and only a configured ad placement. These shared files were not changed. Regex literals present in the application source were read as source, not executed to search the UI.

## rota-ustasi — Route Master

- Read: `public/rota-ustasi/index.html` 1–61, complete; CSS 3–31, inline JS 38–61. Ranges refer to the pre-edit file read.
- First screen: the map, selectable connected cities, route length and New map make the puzzle apparent. The finish flag requirement was split across the subtitle, mission and first status.
- Unnecessary exact text: “Your finished route is compared with the shortest possible route.” appears both in initial HTML and every pre-finish paint; “Every generated map has a solution.” and “This is a local single-player game.” describe implementation rather than the next action. The four-line Mission repeats the header and map status.
- Useful JS capability: hints choose the shortest viable continuation and refuse to charge at a dead end; next-city buttons also operate the map. Both controls remain. There is no unexposed extra game action needing a new button.
- Applied: one goal sentence includes the flag; removed the repeated Mission/filler comparison; kept actual completed-route comparison, score, daily map, undo/restart and hint charge. Records/date explanation is one short collapsed section.

## kelime-borsasi — Word Market

- Read: `public/kelime-borsasi/index.html` 1–52, complete; the entire dictionary and every price were read on line 38; inline JS 36–52.
- First screen: the eight-word goal is visible, but minimum length and the inventory-click shortcut were hidden behind separate generic Help headings.
- Unnecessary exact text: “Use the word list below to check which words count.” before the word list; “Starting letters use the same rule for everyone.”; the empty-word preview repeats “Daily bonus letter:” while the market already shows the same bonus. “Market closed! Eight words earned” starts a repeated score/best recap already present in the dashboard.
- Useful JS capability: clicking inventory letters builds the input without consuming a submission; live preview calculates both points and returned coins; the hint fills the highest-scoring usable word. Kept all and brought the click/type instruction into view.
- Applied: one explicit eight-word/minimum-length goal; concise inventory/market instructions; named Scoring and Accepted words sections instead of three Help headings; kept formula/currency rules, dictionary, hint charge, daily reset and local record. Removed repeated bonus and scoreboard narration.

## yanki-hafizasi — Echo Memory

- Read: `public/yanki-hafizasi/index.html` 1–51, complete; inline JS 35–51, including pointer capture, key handlers, playback token cancellation and sound envelope.
- First screen: watch/repeat is clear. Keyboard input and the actual long-note threshold were buried under Help; “Try rhythm mode” repeats the existing mode selector.
- Unnecessary exact text: “Each sound is also represented by a color, shape, and number.” repeats visible pad labels; “Mute mode preserves every visual cue.” repeats what the mute toggle leaves on screen. “Start, watch the sequence, then repeat it.” repeats the subtitle.
- Useful JS capability: keys 1–4 support press/release duration as well as mouse/touch. Promoted a short contextual control hint; rhythm mode states the real 0.45-second hold threshold.
- Applied: removed duplicated instruction panels and promotion; retained modes/tempo/mute, three lives, progress, paid replay, per-mode records and twenty-round target. Active reset and post-game replay now use distinct concise labels.

## fabrika-akisi — Factory Flow

- Read: `public/fabrika-akisi/index.html` 1–47, complete; inline JS 30–47, including recipes, reserve-at-start simulation, goal payouts, capped elapsed time and storage validation.
- First screen: production is already advancing. “Connect production lines and build twenty devices.” implies a wiring action that the UI does not provide; lines are fixed recipes.
- Unnecessary exact text: “This is a local simulation with fictional credits.”; three generic “Help” headings around short rules; the full Production chain repeats recipes on every machine card.
- Useful JS capability: the Restart factory control was inside Saving and rules. Moved that existing control into view; confirmation, progression and storage semantics remain intact. Pausing individual lines and auto-selling were already exposed and remain.
- Applied: goal says buy/balance machines; short visible advice explains shared inputs and that sold output counts toward orders. All recipes remain available in a collapsed overview; the save note states device storage/no time-away catch-up without the implementation preamble. Autosave status is concise.

## ikilem-tiyatrosu — Dilemma Theatre

- Read: `public/ikilem-tiyatrosu/index.html` 1–56, complete; all eight authored endings, all scenes/choices/requirements and the complete legacy-decision migration map were read (lines 33–55).
- First screen: real story choices are available immediately. “Choose what happens next. Discover where your decisions lead.” gives no setting or concrete objective.
- Unnecessary exact text: “Last Frequency · Interactive fiction” includes a format label; “Play locally. Your decisions and discovered endings stay in this browser. All events and consequences are fictional.” repeats the saved-device footer and visible fiction; eight copies of “? Undiscovered ending” fill the collection before anything is found. “You chose a path. Its consequences will appear in later scenes.” adds no new information after a neutral choice.
- Useful JS capability: undo restores actual prior state; restarting preserves discovered endings; ending share copies an ending description and the ordinary game link, not a pinned playable ending. All remain; the share button now accurately says Copy ending and game link.
- Applied: one concrete blackout-radio goal; all narrative, gated choices and ending explanations preserved. Only real stat changes show a delta; collection shows a count and found ending names rather than repeated placeholders. Removed the repeated advice after each ending and shortened save/action labels.

## kutle-kacisi — Gravity Escape

- Read: `public/kutle-kacisi/index.html` 1–32, complete; CSS 1–24 and the entire physics/input/drawing/share implementation on line 31.
- First screen: collecting capsules is clear, but the speed-under-90 requirement and hold-to-thrust behavior were buried under Help. The initial Pause and movement buttons looked usable although the game was not playing.
- Unnecessary exact text: “A standalone local physics game. Shared links reproduce the capsule layout; other player results are not collected.”; the duplicate “Your mission” and “Flight controls” Help panels repeat the visible target/buttons.
- Useful JS capability: counter-thrust slows the ship, pointer holds support phones, and copied seed links reproduce the mission. Made the short hold/braking instructions visible and renamed sharing to Copy mission link.
- Applied: the goal includes the real pickup speed threshold; retained collision/fuel/exit rules in one compact section. Pause and flight controls are disabled outside play; Start is disabled while already flying, Reset is disabled when already reset. Ready-state Space now retains normal button activation instead of being consumed as a flight input. Physics, seed and score rules are unchanged.

## sozcuk-adasi — Word Island

- Read: `public/sozcuk-adasi/index.html` 1–34, complete; the entire dictionary, generator, adjacency/territory scoring, input and share logic were read on line 33.
- First screen: “10 words, 30 cells” does not say whether both are required; the code finishes at either target. Minimum length and diagonal adjacency were behind Help.
- Unnecessary exact text: “Every island starts with eight hidden words.”; “A local single-player game. Shared links reproduce the letters; there is no shared leaderboard.”; “Share this island to compare different word strategies.” after the result.
- Useful JS capability: clicking the last selected cell undoes that letter; diagonals are valid, and claimed cells may be reused in another word. Promoted the short adjacency/undo hint and preserved reuse/scoring under a named section.
- Applied: one goal explains 3+ letters and the either/or finish rule; removed duplicate heading/filler and renamed the link action accurately. Claim is disabled below three selected letters; Finish now is disabled with no submitted words and after ending, rather than accepting empty/repeated end actions. Full vocabulary and board generation remain unchanged.

## sokak-orkestrasi — Street Orchestra

- Read: `public/sokak-orkestrasi/index.html` 1–32, complete; full chart generation, audio envelope, timing windows, health, pause/count-in and handlers on line 31.
- First screen: the timing task is clear. Two Help panels and the “Three original arrangements” pitch compete with the actual instrument keys and health rule.
- Unnecessary exact text: “Square March opens with steady drums and answering bass. After the Rain adds short guitar phrases and shifting accents. Night Shift overlaps whistle and bass.”; “Sounds are synthesized in the browser, and mute mode keeps every visual timing cue. Play locally.”; performer-only labels such as “Drums · Ada” add names without any character action.
- Useful JS capability: simultaneous notes require multiple keys together; count-in is four beats, and the selected song/seed can be copied. These are retained with one visible input hint and the existing count-in feedback.
- Applied: removed promotional/implementation paragraphs and redundant Help headings; kept timing/health rules in Scoring and health. Instrument buttons now also respond to standard keyboard activation without double-triggering pointer hits. Pause is disabled before/after play; Start/Restart/Play again reflect actual state. Copy arrangement link describes what is actually shared, not a performance recording.

## topoloji-bahcesi — Topology Garden

- Read: `public/topoloji-bahcesi/index.html` 1–33, complete; full geometry, overlap checks, pointer capture, keyboard moves, hint timer and generation on line 32.
- First screen: dragging to untangle is clear, but edge-through-node conflicts need the short additional rule. The empty Garden rules heading repeats the immediately following Keyboard controls heading.
- Unnecessary exact text: “Every garden is scrambled from a known valid layout. Other valid layouts also count.”; “This game runs locally.”; “Choose a node below” points the wrong way because the numbered controls are above that Help text.
- Useful JS capability: numbered buttons select a node for arrow-key movement. Promoted that instruction immediately beside those buttons; the hint now states its actual five-second duration and 150-point cost.
- Applied: removed duplicated headings/implementation narration and retained score rules in a named section. Reset now selects node 1: previously a selected node from a 12-node board could remain out of range after switching to six nodes, making arrow input fail. Text fields no longer consume puzzle arrow moves. Hint disables while already visible and after completion instead of looking active while doing nothing/charging repeated views.

## paket-rotasi — Parcel Dispatch

- Read: `public/paket-rotasi/index.html` 1–33, complete; generator, route/load/return/refuel controls, all simulation branches and the full rendering loop on line 32.
- First screen: loading two vans and meeting windows is clear, but Dispatch depends on both cargo and a queued stop. These steps were hidden behind Help. Relative windows also contained the untranslated fragment “. dakika” while the dashboard used a wall clock.
- Unnecessary exact text: “Every generated operation has a feasible schedule using both vans and the starting stock. Find a suitable order while dividing the cargo correctly.”; “A local strategy game with fictional orders and credits. Shared links reproduce customers, windows, and stock.”; initial van status “Inventory contains” is incomplete.
- Useful JS capability: routes can change while moving; clicking an already-assigned route button unassigns it. Added explicit Add/Unassign labels rather than hiding that toggle behavior. The existing extra-stock purchases remain under Buy extra stock.
- Applied: visible three-package/load/stops/dispatch/time sequence; matching clock-formatted delivery windows, concise travel rules and At depot status. Disabled zero-cargo minus, full/no-stock plus, empty Dispatch, already-full refuel and ineffective Return states; purchases disable when unaffordable/finished. All order generation, delivery deadlines, rewards and van movement remain unchanged.

## Sanity-check boundary

After the first ten edits, each explicitly recorded inline-script range was passed to the JavaScript parser by its already-known line numbers. All ten parsed. This checks syntax only; it is neither a substitute for the full source reading recorded above nor a claim that the updated browser interactions have been exercised.

## 11. Light Seal — `public/isik-muhru/index.html`
- Read: complete original lines 1–33; embedded CSS 1–24 and the entire optical engine at line 32, including all three layouts, RGB masks, ray loops, mirror controls and scoring.
- First screen: the target color needed an explicit text explanation. Level choices now name both the resulting color and its required red/green/blue components.
- Unnecessary text: removed "Mirror workshop", "Each layout has verified solution orientations, scrambled at the start." and "This puzzle runs locally." The completion advice "Try another seal or a new scramble." repeated existing controls.
- Useful behavior: per-mirror keyboard buttons already operate the same mirrors as canvas clicks; retained. Loop/block behavior and scoring remain under a named detail.
- Applied: a short visible turn/mirror instruction, explicit restart/scramble/link labels, concise result, and disabled mirror buttons after the target is solved.

## 12. Time Deadlock — `public/zaman-cikmazi/index.html`
- Read: complete original lines 1–33; embedded CSS 1–24 and full engine at line 32, including recordings, gate/plate timing, all three maps and the 160-step limit.
- First screen: recording a useful ghost was hidden in Help. The short plate → record → replay explanation is now visible beside the board.
- Unnecessary text: removed "The weight of the past", "Recorded lives", "Leave useful work for your future self." and the local-state implementation paragraph. Repeated "Every ghost advanced one step" was inaccurate once ghosts held their final positions.
- Useful behavior: remove-last-ghost also rewinds the current loop. The button now says "Remove ghost and rewind" and confirms before discarding current moves. Gate and echo markers use G and E instead of unexplained K and Y.
- Applied: meaningful record/restart labels; active controls disable after completion/step limit; text fields and browser shortcuts no longer trigger moves. Restart protects existing loops with confirmation. Recordings' reload boundary remains in the rules.

## 13. Rule Detective — `public/davranis-dedektifi/index.html`
- Read: complete original lines 1–31; embedded CSS 1–24 and full engine at line 30, including all eight predicates, sample generation, six experiments and distinguishing counterexamples.
- First screen: now directly asks for the rule behind ACCEPT and REJECT and places the experiment limit beside the sample form.
- Unnecessary text: full rule names appeared twice, in the reference list and in the final-answer dropdown. The list stays; dropdown choices now use their matching rule numbers. Removed the generic Help/disclaimer and repeated "Testing the same sample again does not spend another experiment."
- Useful behavior: a repeated sample was free but looked like another available experiment. The existing sample button now says "Already in log" and disables for that sample. Genuine wrong-answer counterexamples remain.
- Applied: concise test/final-answer/link labels, no duplicate rule prose, state-correct test button.

## 14. Twin Maze — `public/ikiz-labirent/index.html`
- Read: complete original lines 1–32; embedded CSS 1–24 and full joint-state generator/solver/game at line 31.
- First screen: the two exits must be occupied simultaneously; horizontal mirroring and the wall-stop rule are now visible.
- Unnecessary text: removed "Generated maze pairs are checked for a solution in their combined state space." and "This local game reproduces the same walls from a shared link." Repeating wall behavior after every move obscured the actual move feedback.
- Useful behavior: an exit does not freeze a pawn, and undo can reopen a finished puzzle; both remain. The hint's 150-point cost is now in its button label.
- Applied: direct instruction, short scoring detail, explicit restart/link labels, movement disabled on win, and normal input/browser-key behavior retained outside the maze.

## 15. Ecosystem Deck — `public/deste-ekosistemi/index.html`
- Read: complete original lines 1–34; embedded CSS 1–24 and all twelve cards, six weather definitions and seasonal equations at line 33.
- First screen: the end-of-game resource thresholds were hidden. Each resource now shows its actual finish threshold, and the card → weather → resource-cost order is visible.
- Unnecessary text: removed "Choose one of the three policies for this season:" and the fictional-model/shared-link boilerplate. "Run finished. Resource meters show the final state." repeated the result already on screen.
- Useful behavior: undo can recover a completed/failed season; retained as "Undo season". Pollution/forest/wildlife equations remain under Season rules.
- Applied: concise first/next-card feedback, explicit restart/link labels, hidden empty forecast after the run ends. Authored policies, weather, thresholds and scoring are unchanged.

## 16. Echo Map — `public/yanki-haritasi/index.html`
- Read: complete original lines 1–33; embedded CSS 1–24 and full maze/sonar/oxygen/route logic at line 32.
- First screen: the goal now states three cores and the lower-right exit; movement keys, E sonar and four-move pulse lifetime are visible.
- Unnecessary text: removed "The generated maze is connected.", the computed-route-budget explanation and "This is a local single-player game."
- Useful behavior: wall bumps consume oxygen, visited/bumped cells remain known, batteries recharge sonar. These actual rules are retained concisely. The displayed nearest-core distance is Manhattan distance; its label now says "ignores walls" rather than suggesting a walkable route length.
- Applied: "Charges" meter, explicit restart/link labels, disabled movement after ending, and keyboard guards for text fields/browser shortcuts.

## 17. Video Workshop — `public/video-kurgu/index.html`
- Read: complete original lines 1–28; all embedded CSS 1–22, full interface line 26 and the entire media/timeline/recording engine line 27. Read file limits, metadata loading, seeking, audio routing, MediaRecorder lifecycle, cancellation and visibility handling.
- First screen: Open video and the working example are useful. The output format was unclear in "Export video"; it now says "Record WebM" and the header names the download format.
- Unnecessary text: removed the preset new-clip caption "A small movement, a new beginning." so it does not silently enter users' own clips. Replaced duplicate "Help" headings with Clip limits and Recording. Shortened "An original two-clip edit is ready. WebM recording takes the duration of the video." while keeping recording duration next to its rules.
- Useful behavior: real source audio, trim range, speed, ordering, removal, timeline scrub and cancel are retained. Recording's foreground-tab requirement and reload loss are stated briefly. The eight-video limit now excludes the built-in example, matching the stated limit.
- Applied: playback/export disable on an empty timeline; preview clears stale duration/image; replacement with the example confirms first. File loading and playback acquire the editing lock before asynchronous setup, a cancelled setup cannot proceed to recording, and stale preview completions cannot repaint a running timeline.

## 18. Shader Nodes — `public/dugum-golgelendirici/index.html`
- Read: complete original lines 1–28; embedded CSS 1–22, interface line 26 and full graph/GLSL/WebGL/file engine line 27, including cycle detection, linking, resource replacement, node limits and JSON validation.
- First screen: the formula-to-image task is recognizable, but changes already compile automatically. The header now says that directly.
- Unnecessary control: removed "Update image" because every graph input and output change already calls the same compile function. "Wave-ring example" now says "Reset to wave rings" and confirms replacement.
- Useful behavior: x/y nodes were supported in the engine and operation selector but missing from the new-node menu. They are now exposed there. Graph file, PNG, GLSL, phase and time animation remain; the generated shader detail remains useful for this tool.
- Applied: direct action labels, named Coordinates and limits detail, node operation accessibility labels, disabled Add at the existing cap. PNG/GLSL exports stay disabled without a valid current compiled graph; the time button is disabled when WebGL is unavailable. The last image still remains visible for correcting a cycle.

## 19. Motion Type — `public/hareketli-yazi/index.html`
- Read: complete original lines 1–28; embedded CSS 1–22, interface line 26, all keyframe interpolation, easing, player, generated HTML renderer and import validation line 27. Read all three example text layers and their keys.
- First screen: the temporary-pose requirement was hidden inside Help. It is now visible: "Change a pose, then add a key to keep it. Moving the playhead discards a pose that has no key."
- Unnecessary text: "A three-layer example is ready. Select its keys to inspect the motion." is replaced with the actual edit step. "Example choreography" becomes "Reset to example" with replacement confirmation. "Download animation" now identifies HTML.
- Useful behavior: selecting existing key times, per-key easing, eight layers, scrub, JSON and portable HTML remain. Removing a key is only available at an existing key and when another key remains.
- Applied: a changed temporary pose cannot silently disappear from a project/HTML export or playback; the user is directed to Add key. Key actions disable during playback, selecting a layer stops playback, Add disables at the existing layer cap, and imported/example replacement confirms first. The generated HTML preserves angle-bracket text.

## 20. Music Map — `public/muzik-haritasi/index.html`
- Read: complete original lines 1–28; embedded CSS 1–22, interface line 26 and full pointer/audio/recording/WAV/JSON engine line 27. Read example notes/events and the stereo PCM header/sample generation.
- First screen: the header now explicitly connects placing notes to dragging through them in Walk mode. Duration is labeled in seconds.
- Unnecessary text: the generic Help label becomes Sound and recording limits. "Record up to 24 nodes, 200 notes and a one-minute route" confused map nodes with recorded events; the two limits are now distinct. "Garden-route example" becomes an explicit reset with confirmation.
- Useful behavior: horizontal stereo position, one-second per-node retrigger, timed route replay and stereo WAV remain. Imported frequencies outside the preset note list remain visible as their actual frequency.
- Applied: the selected example/imported node populates its edit fields; deleting the selection chooses a real remaining node. Empty route/selection controls disable. Clear/remove/mode/reset operations stop existing audio. Example/import replacement confirms before replacing the map and recorded route. Audio Stop now follows actual active voices.

## 21. Shadow Stage — `public/golge-sahnesi/index.html`
- Read: complete original lines 1–28; embedded CSS 1–22, interface line 26 and full projection/silhouette/pose/player/PNG/share engine line 27. Read every puppet path and shared-scene validation.
- First screen: the header now states the two-pose workflow. The existing status explains saving A/B; retained.
- Unnecessary text: removed "The light is 500 units from the screen. Rays project each puppet from its depth onto the screen." from Help; the useful effect, source width and ten-puppet limit remain under Light and puppet limits. "Share scene" becomes "Copy scene link".
- Useful behavior: layout/shadow views, depth magnification, light position, soft edges, interpolated poses and real PNG remain. A/B saves disable during playback, so the stale edit pose cannot be saved as the current animated pose.
- Applied: an unsaved displayed pose blocks image/link export or playback with a short instruction to save A or B. Empty scene controls disable, actor selection/add/remove stop playback, and resetting the example confirms replacement.

## 22. World Weaver — `public/dunya-dokumaci/index.html`
- Read: complete original lines 1–28; embedded CSS 1–22, interface line 26 and full noise, terrain brush, settlement, A-star route, PNG/JSON engine line 27. Read all heap operations and import bounds.
- First screen: the generic "Create a world, shape its terrain and connect its settlements." is replaced with the actual drag/tool/click action.
- Unnecessary control: "Recalculate land routes" repeated the recalculation already done after every terrain/sea/settlement change. Removed the redundant button and handler. The generic Help becomes Routes and limits.
- Useful behavior: seed generation, water/steepness routing, editable terrain, settlement tools and map/project exports remain. The map is still explicitly fictional in its artwork.
- Applied: generation/import confirms before replacing edits; placing a town on water or beyond the cap gives a real reason instead of silently doing nothing. Route calculation rejects submerged start/end towns. Saving uses the seed that actually generated the world, rather than an unsubmitted text-field edit; importing restores that applied seed.

## 23. Micro Book — `public/mikro-kitap/index.html`
- Read: complete original lines 1–28; embedded CSS 1–22, interface line 26 and full measurement, wrapping, pagination, page editor, cover, printable HTML and JSON engine line 27. Read the full original sample manuscript.
- First screen: now states pagination → page editing → printable download. The existing measurement/page limits remain under Page limits.
- Unnecessary text: removed "MICRO BOOK" branding and "of small discoveries" from the user's exported cover. "An original small-atlas manuscript is ready. Paginate it to create your first book." was stale because initialization already paginated it. "Download book" now says "Download printable HTML".
- Useful behavior: page edits, cover, navigation, print HTML and editing JSON remain. The cover is now an actual dropdown choice instead of showing the first page option while its content was inactive.
- Applied: page fields/actions disable on cover and at page limits. Navigation and exports commit valid visible page edits; overlong text refuses navigation with a reason. Repagination states it replaces pages and confirms first; page deletion/import also confirms. The editing project preserves the unpaginated manuscript and accepts prior projects without that field. Cover text chooses measured black/white contrast, including in the exported book. Exported page numbers use readable dark text.

## 24. Exhibition Builder — `public/sergi-kurucu/index.html`
- Read: complete original lines 1–28; embedded CSS 1–22, interface line 26 and complete reusable gallery renderer, camera, movement, pointer selection, editor, HTML export and JSON validation line 27. Read all three authored exhibit descriptions.
- First screen: it starts in Edit mode, but "Use arrow keys or W A S D to walk, or click a destination." described Visit mode. Status and pressed states now match the actual mode. The move/select instruction is visible beside editing controls.
- Unnecessary text: removed the generic Help wrapper, shortened the header to the edit → visit/download workflow, and made "Download gallery" explicitly HTML. Authored exhibit descriptions remain untouched.
- Useful behavior: click-to-move editing, keyboard/click visit movement, proximity-based reading and standalone gallery remain. Read disables outside Visit mode or when nothing is nearby; movement keys are captured only in Visit mode and do not consume input/button/browser shortcuts.
- Applied: visible exhibit fields commit before selection changes, visit, add and project/gallery export, preventing silent loss. Empty selection and cap controls disable; removal/import confirms. Generated HTML preserves text with angle brackets.

## 25. Stage Cues — `public/sahne-akisi/index.html`
- Read: complete original lines 1–28; embedded CSS 1–22, interface line 26 and full visual transition evaluator, sound scheduler, rehearsal player, manual cue control, CSV/JSON and visibility handling line 27. Read all eight authored cues and their timing/text.
- First screen: the header now explicitly says this is preview rehearsal. Select → edit → update is visible; event-specific fields show only light color, sound frequency or caption as appropriate.
- Unnecessary text: removed the second generic Help paragraph "This is a browser rehearsal stage rather than physical device control. Sound is synthesized without external files or interfaces." because the header and event names state the actual task. "GO · Next cue" becomes "Jump to next cue" to describe its timeline action. "180-second rehearsal" is corrected to start times up to 180 seconds; a final cue can extend beyond its start.
- Useful behavior: automatic rehearsal, real synthesized sound, transitions, stable same-time order, scrubbing and cue-sheet/project exports remain. Authored cue text remains unchanged.
- Applied: next-cue navigation jumps to the selected cue's start instead of its end, which had prematurely applied later cues. List changes stop old audio scheduling. Stop cancels asynchronous audio startup instead of allowing rehearsal to start afterward. Empty selection/cap/end controls disable; example/import confirms replacement. CSV escapes formula-leading user strings as text.

## 26. Fold Geometry — `public/kat-izleri/index.html`
- Read: complete original lines 1–28; embedded CSS 1–22, interface line 26 and all mesh generation, perspective projection, shading, animation, OBJ/PNG/link export and shared-state validation line 27.
- First screen: the header now explains that three hinges fold four panels. "Camera yaw" and "Camera pitch" become "View left / right" and "View up / down".
- Unnecessary text: the generic Help becomes Model dimensions with a shorter dimensions/intersection description. "10 vertices · 8 triangles" remains only inside the geometric artwork where it describes the mesh; the fold percentage now reads in normal order.
- Useful behavior: independent hinge angles, presets, mesh overlay, progress, view angle, actual OBJ geometry and PNG remain. Download names state OBJ/PNG, and the footer explains which settings affect each export.
- Applied: manual progress and preset changes stop animation so it cannot overwrite the chosen value. Model/image/link export freezes the current shape. Copy link is explicit, and the PNG filename is `fold-geometry.png` instead of `times-izleri.png`.

## Completion and verification boundary
- All 26 assigned HTML files were displayed completely with line numbers in tool output, including every inline stylesheet, full inline script, dictionary, narrative, preset and export template. No output in those reads was truncated. The three shared asset ranges are recorded at the beginning of this document.
- No automatic text search, regular-expression audit scan or generated keyword report was used. Exact known literals were edited after reading their source. Existing application regular expressions were read as code and retained where useful.
- All 26 inline scripts parsed with Node `vm.Script` using explicitly recorded source-line ranges. This is syntax evidence, separate from the manual reading.
- Disposable consumer fixture: `C:\C\ManualUiReview\consumer.mjs`. Thirty-six assertions passed for empty controls, async setup cancellation, retained edit values and actual newline characters, seed persistence, submerged routing rejection, event fields, exact exported text, animation freeze and five actual exported-cover contrast computations. The fixture uses a small DOM/canvas substitute; it is not browser media, layout or visual acceptance. Real codec/WebGL/audio/video output was not exercised by that fixture.
- No staging, commit or push in this review task. Shared appearance/revenue assets and other product sources were not edited.
