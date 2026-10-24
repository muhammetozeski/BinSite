# Signal Foundry

Signal Foundry is an experiment-selection and reconstruction game. A player sees a few input/program/output observations, spends a limited number of calibration experiments, then constructs the final output of a previously unseen message. Its social artifact is an immutable, testable challenge and its teaching forks. It does not pass captions and drawings between people, ask a model to judge prose, or require another player to be present for the first session.

The Blazor product is at `/signal-foundry/`. Its owned implementation is `Pages/SignalFoundry.razor`, the product-specific `SignalFoundryBoard` component, `Models/SignalFoundryModels.cs`, `Services/SignalFoundryEngine.cs`, `SignalFoundryClient.cs`, `SignalFoundryExports.cs`, and `wwwroot/signal-foundry.js` under `src/BinSite.Social`. The browser bridge only handles local storage, file reading, downloads and clipboard access.

## First play and author workflow

Choose an editorial mission and start a guest attempt. Read the worked observations, select a cell brush and paint a calibration input. Send one to eight uppercase A–D stamps to spend an experiment. The notebook retains the returned board and shows how many complete languages and distinct final boards remain. Paint the final reconstruction and finish to reveal the hidden mapping and mission explanation. Finishing locks that attempt; a fresh attempt is a separate action.

The 24 original mission definitions in `content/signal-missions.json` cover asymmetric evidence, empty demonstrations, symmetry traps, color ambiguity, operation order, repeated periods, irrelevant clues and teaching forks. They are imported only into the server bundle. Do not copy this file into `wwwroot` or compile its hidden mappings into the client. The repository is public, so withholding a normal player API field is not an anti-cheat guarantee.

The builder works locally without an account or an external service key. Assign four distinct operations, paint up to six teaching inputs, set their programs and choose an unseen final input/program. Outputs are computed rather than typed by the author. The author preview shows the actual result and the ambiguity left by the chosen examples. Local play captures a separate copy of its mapping: later edits to the builder cannot change an attempt already in progress.

Save a draft explicitly in this browser or download author JSON before leaving. An author JSON contains the answer key and is not a player share link. Import validates a replacement before assigning it; one previous draft remains available through Undo last draft replacement. Publishing is explicit, requires a real account and permission for attributed teaching forks, and returns a real server challenge URL. A published version is immutable. A fork retains the exact source identifier and mapping; changing the mapping requires detaching it as an independent language. The server rejects a supplied fork mapping that differs from its source.

## The sf-v1 interpreter

A board is exactly nine integers in row-major order, indexed `row * 3 + column`. Values are 0 empty, 1 red, 2 blue and 3 yellow. Programs are trimmed, then must contain one to eight ASCII uppercase stamps A–D with no internal spaces. A mapping has exactly four different operation indices. Programs execute left to right and never mutate their input arrays.

| Index | Operation | Rule |
| --- | --- | --- |
| 0 | Clockwise quarter turn | input `(r,c)` goes to `(c,2-r)` |
| 1 | Half turn | input `(r,c)` goes to `(2-r,2-c)` |
| 2 | Left/right mirror | input `(r,c)` goes to `(r,2-c)` |
| 3 | Main-diagonal reflection | input `(r,c)` goes to `(c,r)` |
| 4 | Wrapped right shift | input `(r,c)` goes to `(r,(c+1)%3)` |
| 5 | Wrapped downward shift | input `(r,c)` goes to `((r+1)%3,c)` |
| 6 | Color cycle | 1 to 2, 2 to 3, 3 to 1; 0 unchanged |
| 7 | Red/blue swap | 1 and 2 exchange; 0 and 3 unchanged |

The candidate family contains all 8-permute-4 injective assignments: 1,680 mappings in lexicographic A/B/C/D operation order. The client engine and `server/signal-engine.js` use this same order. For input `[1,0,2,0,3,1,2,0,0]` and mapping `[0,4,6,7]`, AB produces `[1,2,0,0,0,3,2,0,1]`; BA produces `[0,1,2,2,0,1,0,3,0]`. These vectors detect accidental reversal of program composition.

Analysis filters mappings against exact observed outputs and groups the surviving mappings by their final target board. It never treats one guessed mapping as established merely because it fits the evidence. A submitted board receives `deduced` when there is one possible final board and it matches; `consistent` when several final boards remain and at least one surviving language produces it; otherwise `inconsistent`. The independent `actualMatch` value says whether it also matches the hidden mapping. A supported ambiguous answer is not failed or presented as unique mastery.

Mission sf-020 deliberately permits no queries after an empty observation, leaving eight distinct final outputs. The other 23 missions have sufficient query allowance to isolate their final output using at most two targeted asymmetric probes. Those are executed fixture results, not a claim that any arbitrary sequence of queries will be informative.

## Network and recovery boundaries

`SignalFoundryClient` uses the real same-origin `commerce/signal` API. Public challenge data includes examples and target input/program, but no hidden mapping, target output or explanation before finish. The server recomputes author-provided example outputs. Creating a guest attempt and its queries requires network access; local author previews, draft manipulation and exports remain separate.

Start and authorized attempt reads return a 64-hex recovery capability. The client stores it with the attempt identifier in this browser and supplies it only through the per-request `X-Signal-Attempt-Key` header on attempt reads, queries and finish. It is never put into a challenge share URL, a global HTTP header or an exported HTML record. This preserves access if two fresh browser tabs set different guest cookies; the server still validates the capability and expiry. Guest attempts expire after seven days and are not an account-recoverable archive.

Each mutation retains its request identifier after an uncertain response failure. Retrying the same action and payload uses that identifier; changing the payload creates a fresh identifier. Completed server responses clear the pending action. No automatic mutation retry is performed. A finished result includes the accepted answer, so restoration prefers that server record over a possibly stale local working board. Resume latest browser attempt reads the saved identifier and capability directly, without first requiring the original public challenge to remain visible; existing attempt snapshots therefore remain recoverable within their own lifetime after a challenge is withdrawn.

Reports use the server's supported moderation reasons and require a real account. The owner-only withdraw action is shown from actual ownership metadata and is still authorized by the server. The game has no public chat or activity feed. Ad eligibility uses the project's existing optional publisher-network loader; no live ad, revenue, private subscription archive or contest integrity is claimed.

## Executed checks and remaining boundaries

A disposable .NET 10 fixture passed 151 assertions for canonical operations, composition, input rejection, API-sized author fields, all 24 mission keys, serialization, query allowance and the deliberately ambiguous alternate answer. A second isolated Razor project compiled the actual product component sources and passed 25 checks for captured preview rules, edit locks, validated import and undo, export escaping, recovery-key exclusion, retry identifiers, request-header scope, accepted-answer restoration and recovery without a public challenge read. A Node fixture compared C# output and mapping enumeration with the real server JavaScript engine for all 1,680 mappings, using varied boards and program lengths. Six browser-bridge checks covered local storage bounds, file bounds/reset and clipboard denial.

These checks execute C# logic, compiled component methods, an HTTP fixture and JavaScript helpers. They do not assert a real browser's visual layout, native file dialog, clipboard permission prompt, printer output or a complete deployed account session. Production API checks and publication status belong to the deployment ledger and dated handover records.
