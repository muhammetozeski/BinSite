# Games batch 001

Five standalone local games live in `public/rota-ustasi`, `public/kelime-borsasi`, `public/yanki-hafizasi`, `public/fabrika-akisi`, and `public/ikilem-tiyatrosu`. Each has its own `index.html`, styles, logic, and interface. No shared wrapper, downloaded dependencies, account service, or fabricated social activity is required.

## Mechanics and architecture

- **Route Master:** a seeded random generator adds a guaranteed Hamiltonian route and extra weighted edges. A bit-mask dynamic program computes the exact shortest remaining completion for hints and the completed-route comparison. The final city can only be entered last. Map generation supports up to twelve cities.
- **Word Market:** an explicit 1,613-word local English dictionary is the authority. Daily seed selects the opening inventory and bonus letter. Purchases reduce coins; valid submissions consume inventory, refund three coins per character, and award length, rarity, bonus-letter, and growth points. Eight submissions end a run. Duplicate words are rejected.
- **Echo Memory:** user gesture enables Web Audio. Playback and input are separate phases, and a cancellation token prevents an abandoned game's timers from advancing a new game. Visual shapes and numbers preserve play without audio. The rhythm mode distinguishes long presses at 450 milliseconds; the sequence mode ignores press length.
- **Factory Flow:** each machine instance holds its current job and elapsed production time. Inputs are reserved when the job starts, and output is created only upon completion. Machine pauses preserve work. The simulation uses measured tick time, capped after background suspension, with no offline production claim. Resources, machine progress, and completed goals persist locally.
- **Dilemma Theatre:** a finite scene graph changes six bounded state variables and explicit story flags. Choice requirements expose their actual missing condition. Eight endings depend on accumulated state. Undo restores the entire preceding snapshot. Sharing produces a result sentence and the actual game URL; it does not invent friend activity.

## Product discussion

The creative and social agents were consulted. Route Master differs from Gizli Harita because it generates mathematically solvable routes instead of user-authored escape rooms. A future authoring integration could validate creator maps. Dilemma Theatre differs from Hikâye Ağacı because it uses balanced scripted state and endings instead of collaboratively created paragraphs. A result-to-new-play sharing loop is a concrete future experiment; no unimplemented shared outcomes are claimed.

## English product language

All five interfaces, instructions, narrative scenes, endings, and catalog descriptions are English. Word Market uses an A–Z market and a curated 1,613-word English dictionary. Case normalization uses English I/i behavior. Its English records use a new namespace so scores from the former vocabulary do not become the current English daily record; the earlier storage is not deleted.

Dilemma Theatre retains existing story variables and restores earlier decision-log entries through numeric fingerprints of the original choice text. Its thirty-nine earlier choices and six state labels map to the current English text without embedding old-language strings in the public source. Current English decision text is preserved. A local check confirmed that restoring an earlier log leaves the state values unchanged.

Decorative shadows, rounded containers, and glows were removed. The surrounding interfaces use plain high-contrast controls; game-specific colors still communicate state, notes, paths, and resources. Game logic, independent entry points, and existing paths are preserved.

## Local records and revenue

All records are browser-local. Losing browser storage can lose progress. No game claims actual revenue, concurrent players, social voting, or health benefits. Catalog revenue hypotheses are unverified proposals. Display advertising uses an approved publisher network such as AdSense; optional private project storage is a separate paid service. Direct-sponsor offers were removed after the user's correction.

## Validation

All five inline scripts pass the installed Node.js JavaScript parser. Targeted logic checks cover generated routes, inventory economics, memory input phases, production resource conservation, and reachability of the story endings. Browser-only audio and visual presentation still require interactive use; the logic checks do not claim to be a rendered-browser test.
