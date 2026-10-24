# Static UI follow-up

All 36 assigned `public/<slug>/index.html` files were displayed and read completely, including embedded CSS, inline JavaScript, dictionaries, narratives, examples and export renderers. This was manual source reading, not an automated word search. The earlier [review](manual-ui-static-games.md) supplied context; its prior checks were not counted as this reading. The combined Factory/Dilemma output was truncated; the omitted ranges were subsequently displayed separately.

## Games: 16 read, 12 changed

Ranges below describe the files before these edits. Separate reset/new-map, undo, keyboard alternatives, scoring rules and authored game content remain intact. Header navigation remains available; the shared footer duplication is root-owned.

| Slug | Read lines | Applied change |
| --- | --- | --- |
| rota-ustasi | 1–61 | Shortened repeated start/result narration; readable dark-mode counters. |
| kelime-borsasi | 1–52 | Removed result advice repeating Restart; letter-count text inherits the tile's readable ink. |
| yanki-hafizasi | 1–51 | Removed repeated key instructions and scoreboard narration; readable note labels on illuminated pads. |
| fabrika-akisi | 1–47 | Shortened restored-state text; readable toolbar, machine units, recipes and completed goals. |
| ikilem-tiyatrosu | 1–56 | Readable save status and discovered-ending names. Narrative and endings retained. |
| kutle-kacisi | 1–32 | Removed a repeated pickup-speed sentence; kept the threshold in the visible goal and retained the exit position. |
| sozcuk-adasi | 1–34 | Selection-order numbers inherit readable tile ink. |
| sokak-orkestrasi | 1–32 | Removed duplicated final score/hit recap; retained perfect-hit and longest-combo results. Failure uses the visible Band health term. |
| topoloji-bahcesi | 1–33 | No additional change. |
| paket-rotasi | 1–33 | Removed final budget/time recap already in counters; kept score. Readable delivered-order cards. |
| isik-muhru | 1–33 | No additional change. |
| zaman-cikmazi | 1–33 | No additional change. |
| davranis-dedektifi | 1–31 | Readable observation cards and striped sample marker. |
| ikiz-labirent | 1–32 | Readable rules surface in dark mode. |
| deste-ekosistemi | 1–34 | Removed repetitive post-failure coaching; retained the failure and completed-season count. |
| yanki-haritasi | 1–33 | No additional change. |

## Creative tools: 20 read, 17 changed

Every file below was read over its complete original lines 1–28, including its full single-line engine. Existing implementation regular expressions were read as code; no keyword/regex audit scan was performed.

| Slug | Applied change |
| --- | --- |
| ses-gorseli | No additional change. |
| mantik-tahtasi | Copy circuit link and Reset to half-adder identify the existing actions; readable status in either theme. |
| perspektif-oda | Copy layout link and Reset to reading room identify existing actions; removed repeated slider instructions. |
| kagit-katla | Removed repeated print-scale advice and export branding; generated label ink follows the selected cover color's measured luminance. |
| dugum-hikaye | Readable story-reader surface. |
| sozcuk-bestesi | Readable poem surface and progress text. |
| ruzgar-sanati | Reset to vortex makes replacement explicit. |
| jest-fircasi | Removed two repeated brush explanations; retained that existing strokes preserve their original brushes. |
| ses-manzarasi | Removed four descriptions repeating the layer names. All four level controls remain. |
| sifre-postasi | Shortened the generic heading; dot-and-dash mode points to its alphabet instead of an unavailable wheel; readable cipher/work/key surfaces and status. |
| video-kurgu | Open videos matches multiple selection; shortened obvious recording narration. |
| dugum-golgelendirici | Canvas accessibility label describes the image rather than Actual WebGL implementation. |
| hareketli-yazi | No additional change. |
| muzik-haritasi | Shortened playback narration. |
| golge-sahnesi | Empty link field stays hidden until Copy scene link; manual copying remains available. |
| dunya-dokumaci | No additional change. |
| mikro-kitap | Removed Show cover, which duplicated the existing Cover menu entry; retained navigation/edit validation. Removed font/pixel implementation prose while keeping page limits. |
| sergi-kurucu | Readable nearby-exhibit information surface. |
| sahne-akisi | Removed Stop duplicating Play/Pause. The remaining control cancels pending audio startup as well as pausing active rehearsal. |
| kat-izleri | Shortened the canvas accessibility label; empty link field appears only after Copy fold link. |

Total: **29 changed, 7 unchanged**. No new feature, shared stylesheet, catalog builder, revenue source or ReleaseDeck module was changed by this pass. No staging or push was performed.

## Verification boundary

All 36 inline scripts parsed with Node `vm.Script`. For generated-HTML tools, known complete source-line ranges were used; splitting on every script opening would incorrectly truncate a script containing an HTML export string. Syntax parsing is separate from manual reading and does not establish browser behavior.

Fifteen changed game foreground/background pairs were calculated from their authored opaque colors; the lowest contrast was 4.94:1. Five actual Paper Fold `generate()` SVG outputs followed their selected black, white, gray, ochre and dark-purple backgrounds with the selected readable label ink. A consumer with deferred audio readiness checked Stage Cues pending-start cancellation, a fresh start and pause against its production inline engine. Consumer DOM/canvas substitutes did not exercise native audio, codecs, WebGL, printing, browser layout or visual acceptance. Existing unchanged feature tests were not repeated or presented as new UI-reading evidence.
