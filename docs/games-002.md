# Advanced independent games

This article records the distinct mechanics and verification boundaries of the implemented stage-five games in `catalog/games-002.json`. Only completed entries appear in the catalog. Each game has its own independent HTML, styles, JavaScript, instructions, and seed-sharing controls; no parent shell supplies its game rules.

## English product language

All implemented products in this article use English interfaces, instructions, observations, policy text, and catalog metadata. Word Island uses the same curated 1,613-word English list as Word Market, with English-only A–Z board letters. One hundred generated English boards retained eight distinct embedded words and a reachable thirty-cell territory target, and a complete submission run reached that target.

All sixteen existing game entry points across both batches passed JavaScript parsing and initialization with a minimal DOM/canvas interface stub. Visible text, accessibility labels, and string-literal inspection found no remaining Turkish-character text. This supports the language conversion and startup paths, not a rendered-browser acceptance test. Decorative rounded containers, shadows, and glows were removed while preserving meaningful game-state colors.

## Mechanics

- **Gravity Escape (41):** inverse-square planetary gravity and directional thrust update momentum in two bounded time substeps. Capsules require both proximity and limited speed; collecting all unlocks a separate exit. Collision, leaving communication range, fuel, and elapsed time affect the outcome. This is continuous flight control rather than a route graph.
- **Word Island (42):** eight-neighbor letter paths claim cells. Word length, newly claimed area, and contact with existing territory affect score; repeated words are disallowed. Eight unique words are embedded in separate rows so at least thirty-two claimable cells exist within the ten-word allowance. This uses spatial path and territory decisions, unlike Word Market's inventory/coin economy.
- **Street Orchestra (43):** four simultaneous timing lanes contain authored rhythmic patterns for three tempos and deterministic seeded fills. The player must perform; an editor/sequencer does not automatically supply success. Timing error, missed notes, combo, and collective energy determine whether the sixteen-bar concert completes. Web Audio is synthesized locally, and visual timing remains usable in mute mode.
- **Topology Garden (44):** a planar grid graph with at most one diagonal per cell is scrambled. Drag or keyboard movement attempts to remove edge crossings. Node overlap and edges passing through unrelated nodes are separate invalid-placement conditions, so collapsing the graph cannot satisfy the puzzle. A temporary ghost of a valid layout costs points.
- **Parcel Dispatch (45):** the player allocates two cargo types from one depot to two capacity-limited vehicles, sequences deliveries with time windows, and pays for stock/fuel changes. Vehicles advance in the same simulation minute. A known feasible two-vehicle assignment determines generated time windows; a late order ends the operation. This is scheduling and inventory allocation rather than another single-vehicle shortest-path game.

## Evidence and conditions

Installed Node.js parsed the five scripts. Targeted execution used the actual game functions with a minimal DOM/canvas interface stub, not a rendered browser.

- Flight checks exercised capsule collection, the locked/unlocked exit condition, and collision failure.
- One hundred generated word boards contained eight distinct valid embedded words; an actual submission sequence claimed the target area and completed the game.
- All three musical charts were ordered and inside their concert duration; an exactly timed input produced the expected perfect-hit and combo score.
- Three hundred generated graph solutions across all three sizes had no crossing or invalid node placement.
- One hundred delivery seeds completed under the known feasible two-vehicle plan with nonnegative fuel. A separate late-deadline check failed before an overdue delivery could be accepted.

These checks support mechanics and solvability conditions; they do not claim a completed browser/audio acceptance test, production traffic, advertising approval, payment, or commercial performance.

## Optical, temporal, inference, coupled-maze and ecological systems

- **Light Seal (46):** cardinal rays reflect at `/` or `\` mirrors, stop at blocks or repeated ray states, and independently contribute red/green/blue bit masks at the target. Three authored optical layouts require purple, yellow, or white. Seeded orientation scrambling changes the initial work, not the rules. Correct authored mirror orientations were executed and reached all three target masks.
- **Time Deadlock (47):** each rewind retains an entire path. All prior paths restart simultaneously, advance with player turns, and hold their final positions. Pressure plates inspect those time-indexed ghost positions before admitting movement into a door. A time-aware search solved all three actual layouts through the real movement, record, rewind, plate, and exit functions.
- **Rule Detective (48):** the player chooses experiments from 108 color/shape/number/pattern combinations to distinguish eight explicit logic rules. The final hypothesis is checked against the hidden rule, and failure supplies a real counterexample from the feature universe. Pairwise rule distinguishability and one hundred cases solvable within six adaptively selected experiments were checked.
- **Twin Maze (49):** horizontal commands are mirrored while vertical commands are shared. A wall can stop one character while the other moves. Generation searches the 625 joint position states and accepts layouts with a solution. One hundred generated pairs were solved by executing the actual movement transitions, ending with both characters at their own exit.
- **Ecosystem Deck (50):** a seeded twelve-season deck offers three policy cards per turn. Card effects, a revealed weather event, and a coupled ecological update execute in that order. The model is explicitly fictional. Bounded finite state transitions were checked, a beam search found a sustainable plan for each of thirty searched decks, and one such plan completed all twelve turns through the actual choice handler. This sample does not assert every possible seed has a sustainable policy.

## Sonar exploration

**Echo Map (51)** generates a connected depth-first maze with three signal cores, battery pickups, and an exit. Sonar temporarily reveals a radius for four moves; visited paths and bumped walls remain remembered. The oxygen budget is the shortest evaluated core-order route plus sixty moves. Thirty seeded mazes were completed through the actual movement handler, collecting all cores and reaching the exit with at least the promised oxygen margin. The interface and rules are English. These checks use the game functions and a canvas stub rather than rendered browser output.
