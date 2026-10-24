# Creative products, batch 001

## Implemented product contracts

Each application is independently usable at `public/<slug>/index.html`. No shared script, stylesheet, backend, downloaded asset, or parent page is required.

| Product | Main artifact | State and sharing |
| --- | --- | --- |
| Çizgi Döngüsü | A 32 by 32 pixel animation with up to 48 frames | Local browser save; validated JSON import/export; individual PNG export |
| Ritim Masası | Six synthesized instrument tracks across sixteen steps | Validated full pattern in URL fragment; explicit local save/load; audio starts from a user action |
| Sonsuz Çizgi Roman | A three to six panel vector comic | Local browser save; validated project file and URL fragment; SVG and PNG export |
| Kural Atölyesi | A binary 96 by 64 cellular automaton | Birth/survival rules, wrapped or bounded edges, bit-packed cells in URL fragment; PNG export |
| Gizli Harita | An authored one to six room escape adventure | Local draft, validated project file and URL fragment; separate author/player modes |

No fake user activity or network collaboration is shown. URL fragments contain the creative content itself. Gizli Harita also includes answers in that content; it is a casual game-sharing format, not a secure assessment format. No analytics, live audience numbers, or revenue outcomes are implemented or asserted.

## Scope decisions

- Animation uses editable pixels, not an imported drawing or video. Previous-frame ghosts are excluded from exported PNGs. Undo preserves up to twenty project changes.
- Rhythm uses browser audio synthesis with an audio-clock scheduler, per-instrument mute, timing swing, a bass root, and melodic interval choices. Backgrounding the tab stops playback.
- Comics render native SVG with escaped user text. Scene backgrounds, faces, and speech balloons are generated locally. Raster export renders that same SVG.
- The automaton calculates all next cells from the complete previous generation. A stable generation pauses automatically. Drawing makes the current pattern the new reset point.
- Escape rooms support normalized code answers, four-way choices, and reordered object sequences. Structural completeness is checked before playing, sharing, or exporting. A completed player can copy the adventure into the authoring flow.

## Growth discussion

The creative, social, and games agents compared visitor-to-creator loops. Gizli Harita is this batch's leading hypothesis because one player's completion naturally offers a new authored invitation. The distinction from the games agent's generated route puzzles is author-controlled narrative and clues. The distinction from Hikâye Ağacı is solving a bounded authored challenge rather than contributing a paragraph to a shared tree.

Useful future measurements are invite landing, first puzzle attempt, completion, entering author mode, creating a changed adventure, and sharing that changed adventure. These measurements are proposed; this batch does not silently introduce tracking. A later shared adventure repository, playtest feedback, branching routes, and creator collections could be developed after actual demand is observed.

Possible paid extensions are event adventure packs, workshop workspaces, comic assets, and audio/animation export tools. These remain unverified hypotheses and are not current revenue.

## Validation responsibility

Twenty checks passed in Node.js using a mocked DOM: animation project validation, malformed-pixel rejection, flood-fill barriers and line endpoints; rhythm URL round trip, tempo validation and initial-step scheduling; comic validation, escaped SVG input, full-caption preservation and Unicode sharing; glider motion after four generations, stable-block behavior and cell bit packing; escape map Unicode sharing, duplicate-choice rejection, incomplete-draft recovery, solved-state preservation after an editor visit and completion through all three puzzle types; unique catalog entries.

The root owner handles assembled-site syntax checks and deployment verification. A mocked browser environment does not establish real browser layout, audio output quality, clipboard permission behavior, or raster download behavior.
