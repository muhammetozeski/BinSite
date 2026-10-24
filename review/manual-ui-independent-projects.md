# Manual interface review — independent projects

Status: complete source review and local corrections for all four products. Review coverage, source findings and focused execution checks are separated below. Deployment was authorized afterward; publication evidence is recorded separately. No keyword scan, regular-expression scan or generated word report was used. Private credentials, private directories and compiled framework output were excluded.

Every listed full range was read as authored source, including linked application behavior, and subsequent amendments were reviewed. Line counts describe the current working copy; they are metadata, not a text assessment. Identical Freight browser copies were hash-compared against fully read canonical modules. These source reads do not claim native browser use.

## InterlockExpeditions

Project root: `C:\E\Claude\WebSiteleri\InterlockExpeditions`.

Reviewed baseline: `1224ab7`. Documentation read before implementation: README, AGENTS, documentation index, Room-Protocol, Appearance, Payment-Boundary and the previously read current Guide.

### Complete source inventory

| Source path relative to this project | Full current range | Actual read coverage |
| --- | --- | --- |
| `content/episodes.mjs` | 1–64 | Full source and reviewed amendments |
| `content/further-episodes.mjs` | 1–77 | Full source and reviewed amendments |
| `functions/api/[[path]].js` | 1–27 | Full source and reviewed amendments |
| `public/_headers` | 1–9 | Full source and reviewed amendments |
| `schema/001-core.sql` | 1–19 | Full source and reviewed amendments |
| `schema/002-billing.sql` | 1–3 | Full source and reviewed amendments |
| `server/accounts.mjs` | 1–26 | Full source and reviewed amendments |
| `server/billing.mjs` | 1–106 | Full source and reviewed amendments |
| `server/engine.mjs` | 1–135 | Full source and reviewed amendments |
| `server/rooms.mjs` | 1–145 | Full source and reviewed amendments |
| `server/security.mjs` | 1–37 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/App.razor` | 1–1 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/GlobalUsings.cs` | 1–1 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/InterlockExpeditions.Portal.csproj` | 1–4 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/Models/ExpeditionModels.cs` | 1–27 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/Pages/Account.razor` | 1–29 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/Pages/Control.razor` | 1–36 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/Pages/Help.razor` | 1–7 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/Pages/Home.razor` | 1–10 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/Pages/Host.razor` | 1–19 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/Pages/Membership.razor` | 1–32 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/Pages/Privacy.razor` | 1–3 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/Pages/RoomPage.razor` | 1–60 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/Program.cs` | 1–11 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/Services/ExpeditionApi.cs` | 1–70 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/Shared/ExpeditionPage.cs` | 1–26 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/Shared/LiveExpeditionPage.cs` | 1–45 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/Shared/Shell.razor` | 1–2 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/Shared/StationConsole.razor` | 1–43 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/_Imports.razor` | 1–8 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/wwwroot/assets/contrast.js` | 1–35 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/wwwroot/assets/device.js` | 1–18 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/wwwroot/assets/mark.svg` | 1–1 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/wwwroot/assets/site.css` | 1–1 | Full source and reviewed amendments |
| `src/InterlockExpeditions.Portal/wwwroot/index.html` | 1–1 | Full source and reviewed amendments |

### Changed-file manifest

- `content/episodes.mjs`
- `content/further-episodes.mjs`
- `src/InterlockExpeditions.Portal/Pages/Account.razor`
- `src/InterlockExpeditions.Portal/Pages/Control.razor`
- `src/InterlockExpeditions.Portal/Pages/Help.razor`
- `src/InterlockExpeditions.Portal/Pages/Home.razor`
- `src/InterlockExpeditions.Portal/Pages/Host.razor`
- `src/InterlockExpeditions.Portal/Pages/Membership.razor`
- `src/InterlockExpeditions.Portal/Pages/Privacy.razor`
- `src/InterlockExpeditions.Portal/Pages/RoomPage.razor`
- `src/InterlockExpeditions.Portal/Shared/LiveExpeditionPage.cs`
- `src/InterlockExpeditions.Portal/Shared/Shell.razor`
- `src/InterlockExpeditions.Portal/Shared/StationConsole.razor`
- `src/InterlockExpeditions.Portal/wwwroot/assets/device.js`
- `src/InterlockExpeditions.Portal/wwwroot/assets/mark.svg`
- `src/InterlockExpeditions.Portal/wwwroot/assets/site.css`
- `src/InterlockExpeditions.Portal/wwwroot/index.html`

### Reading status

The complete authored portal, stylesheet, screen scripts, episode content, API routes, engine and persistence contract were read. The first combined backend output truncated part of hostAction; server/rooms.mjs lines 121 through EOF were then read separately. Initial source and all subsequent local patches were reviewed. This is a source review; changed screens have not been deployed or exercised in an authenticated native browser.

### Route / — choose an adventure

- User job: pick a story and start a cooperative room. The original headline, "A different card. The same machine.", names neither the game nor the first action.
- Unnecessary copy: "No adventure editor or event spreadsheet is needed." This answers an implementation discussion instead of helping a player choose.
- Repetition: the opening paragraph, three OPEN/INVITE/COOPERATE blocks and final "Bring two devices, not a download" panel repeat setup information. Episode versions do not help this choice.
- Useful behavior behind the screen: every story link already supplies an episode query parameter, and each episode supplies its own default duration and briefing.
- Local correction: one direct purpose sentence, two-device/time/free requirements, story cards and one action per story. Story briefings remain expandable; puzzle content and endings remain intact.

### Route /host — create or resume a room

- User job: choose a story and clock, then open its host controls.
- Unnecessary button: "Create this real room". "Real" adds no action information.
- Hidden behavior: AccountAsync and RoomsAsync expose the paid allowance, but the original page always said "Free: two active rooms, up to six devices per room." A paid host could therefore read the wrong limit.
- Broken continuation: the selected story was passed into /host, but its sign-in link went to /account and Account's Continue returned to bare /host. The chosen story was lost.
- Local correction: concise Story/Room name/Time limit fields, actual account-dependent limits, Create room, and a fixed internal episode query carried through sign-in. No external return URL is accepted.

### Route /account — keep host access

- User job: create or recover host access, then return to the chosen story.
- Unnecessary copy: "You are signed in with a real host session. Your crew identity is separate from these room-management permissions."
- Repetition: a newly created account rendered Continue in its key panel and another room action in the signed-in panel.
- Useful behavior: the private recovery key can be downloaded, is shown once, and is the account recovery mechanism. Deletion is refused during an active paid period or open checkout.
- Local correction: keep the private-key warning/download, show one continuation after creation, carry the selected episode, shorten sign-in/sign-out copy and describe deletion's actual scope. No key or secret was read.

### Route /control/{RoomId} — host the current room

- User job: invite players, start/pause/resume and handle problems.
- Unnecessary labels: "Refresh real room state", "Actual crew roster" and "Host control applied to the real room."
- Hidden useful behavior: replacing an invite rejects new use of the old code while preserving existing players. Pause preserves both the clock and already armed windows. Restart costs five seconds. These were scattered among long paragraphs.
- Misleading initial state: a pending load could render the owner-access instruction before access had been checked.
- Local correction: explicit loading state, invite/start controls first, concise roster, consequences beside optional Replace/Restart/End actions, activity behind a disclosure, and no success announcement after every ordinary host click. Remove and delete controls remain protected.

### Route /room/{RoomId} — cooperate on the puzzle

- User job: join one crew and use its clue alongside the other crew's controls.
- Unnecessary copy: "A private room-player capability is stored only on this device; it is never part of the invitation." The join form instead needs the short fact that the player is saved on this device.
- Unnecessary repeated status: "Your control was applied to the shared machine." The changed controls and puzzle feedback already show the result.
- Incorrect contextual instruction: "The server advances only when both crews have checked a valid objective." It also appeared in the final Arm puzzle, which has no Check action.
- Unnecessary action for guests: "Host controls · owner access required". The global host navigation remains available to the owner.
- Hidden useful behavior: existing players automatically resume, late players join the current puzzle, four-second refresh updates controls, and both device storage and a cookie can recover access.
- Local correction: loading state before Join, short Join/Resume labels, roster open while waiting and collapsed during play, Check guidance only on Check puzzles, no generic click-success chatter, Refresh plus "Updates automatically", and retained hints/reports/read-only teammate controls.
- Actual background failure found by full reading: device.js accessed sessionStorage before attempting localStorage, so blocked session storage prevented its supposed fallback. Saving now tries both independently; reading tries the second store after a blocked/corrupt first store.
- Another behavior issue: automatic refresh errors stayed in Status after refresh succeeded. A separate connection warning now clears after success without erasing a form validation error.

### Route /help — learn controls and reconnect

- User job: understand setup or a specific machine.
- Unnecessary copy: "The shared machine is real server state: changing a control on one console changes what the other crew reads after the next refresh."
- Useful mechanics retained: separate crew controls, joint Check, five-second penalties, overlapping fifteen-second Arm windows, pause, reconnect and late joining.
- Local correction: short setup steps, action-based puzzle instructions and optional machine/key details. The source contains puzzle answers; this is kept as a fair-play explanation rather than a cryptographic secrecy claim.

### Route /membership — understand actual allowances

- User job: see limits, subscribe if available or manage an existing subscription.
- Unnecessary copy: "A verified payment notification must confirm a live paid period." The user needs to know to refresh after checkout, not the notification protocol.
- Hidden behavior: larger limits apply to newly created rooms; older rooms retain their original capacity. An open checkout blocks account deletion until expiry.
- Problem found: the unconfigured-provider branch hid an already recorded subscription/active allowance. A failed initial read also had no visible retry button.
- Local correction: actual recorded status remains visible, unavailable checkout stays unavailable, Refresh is always reachable, future allowances are not marketed while unconfigured, and cancellation/new-room/checkout-expiry conditions are kept in an optional billing section. No payment state or provider guard changed.

### Route /privacy — know access and deletion

- User job: understand what is stored, who can see it and how to delete it.
- Unnecessary copy: "No payment or advertising is fabricated." Also: "Provider configuration and actual availability are shown separately from any future business hypothesis."
- Useful behavior retained: invitation access, private account keys, device-access loss, operator access, host deletion/cascades, fifty-room retention limit, provider backups, reports, network-derived abuse controls and payment-provider handling.
- Local correction: direct access/storage/deletion statements without hosting-stack names, protocol names or business-process narration. No advertising loader is present in this release.

### Shared interface and styling

- Removed footer copy: "Original fictional adventures · English · No camera, microphone or GPS required." The language label is irrelevant; device requirements and privacy facts remain where they affect a choice.
- Retained machine semantics: six mechanisms, color plus text on lamps, map legend, readonly other-crew values, hints, valid alternative balance arrangements and the final two-terminal timing rule.
- The bright chartreuse/orange shell was changed to subdued blue/tan accents on readable dark surfaces. The two crews and active map/lamp states remain visually distinct. No new shared layout or external UI package was introduced.
- The loading document now says only "Loading." rather than teaching the game before the application has opened.
- No account, room, report, payment or live fixture was created during this manual review.

### Local verification status

The changed C# portal compiled in Release with zero warnings/errors after the edited Razor sources were saved. Six consumer checks passed for blocked session storage, blocked device storage, corrupt primary data, both stores unavailable and no saved player. No live account or native browser session was created. The complete earlier live game acceptance remains historical evidence, not a claim that these local interface changes are published.

## RetryAssurance

Project root: `C:\E\Claude\WebSiteleri\RetryAssurance`.

Reviewed baseline: `fb9f28e`. Documentation read before implementation: README, AGENTS, documentation index, Guide, Architecture, Portal, Runner, Capability-Authority, Operations and Release-1.0.0.

### Complete source inventory

| Source path relative to this project | Full current range | Actual read coverage |
| --- | --- | --- |
| `Directory.Build.props` | 1–1 | Full source and reviewed amendments |
| `examples/BrokenClient/BrokenClient.csproj` | 1–1 | Full source and reviewed amendments |
| `examples/BrokenClient/Program.cs` | 1–3 | Full source and reviewed amendments |
| `examples/CorrectClient/CorrectClient.csproj` | 1–1 | Full source and reviewed amendments |
| `examples/CorrectClient/Program.cs` | 1–3 | Full source and reviewed amendments |
| `examples/ExampleClients/ExampleClients.csproj` | 1–1 | Full source and reviewed amendments |
| `examples/ExampleClients/SyntheticJobClient.cs` | 1–24 | Full source and reviewed amendments |
| `functions/api/[[path]].js` | 1–21 | Full source and reviewed amendments |
| `public/_headers` | 1–6 | Full source and reviewed amendments |
| `public/_redirects` | 1–1 | Full source and reviewed amendments |
| `public/_routes.json` | 1–1 | Full source and reviewed amendments |
| `schema/001-service.sql` | 1–23 | Full source and reviewed amendments |
| `schema/002-checkout-intents.sql` | 1–5 | Full source and reviewed amendments |
| `scripts/Package-Runner.ps1` | 1–48 | Full source and reviewed amendments |
| `server/accounts.mjs` | 1–26 | Full source and reviewed amendments |
| `server/billing.mjs` | 1–28 | Full source and reviewed amendments |
| `server/executions.mjs` | 1–62 | Full source and reviewed amendments |
| `server/projects.mjs` | 1–34 | Full source and reviewed amendments |
| `server/protocol.mjs` | 1–21 | Full source and reviewed amendments |
| `server/security.mjs` | 1–23 | Full source and reviewed amendments |
| `src/RetryAssurance.Contracts/RetryAssurance.Contracts.csproj` | 1–1 | Full source and reviewed amendments |
| `src/RetryAssurance.Contracts/ServiceModels.cs` | 1–55 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/App.razor` | 1–1 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/GlobalUsings.cs` | 1–2 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/Models/PortalModels.cs` | 1–48 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/Pages/Account.razor` | 1–44 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/Pages/Billing.razor` | 1–29 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/Pages/Compare.razor` | 1–10 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/Pages/Home.razor` | 1–5 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/Pages/NewProject.razor` | 1–18 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/Pages/Packs.razor` | 1–7 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/Pages/Privacy.razor` | 1–7 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/Pages/Project.razor` | 1–50 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/Pages/Projects.razor` | 1–19 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/Pages/Run.razor` | 1–29 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/Pages/RunnerGuide.razor` | 1–15 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/Program.cs` | 1–12 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/RetryAssurance.Portal.csproj` | 1–1 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/Services/AccountSession.cs` | 1–15 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/Services/PortalApi.cs` | 1–22 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/Shared/ActionPage.cs` | 1–27 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/Shared/ServiceLayout.razor` | 1–10 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/Shared/StatusMessage.razor` | 1–2 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/_Imports.razor` | 1–11 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/wwwroot/appearance.css` | 1–8 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/wwwroot/appearance.js` | 1–146 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/wwwroot/icon.svg` | 1–1 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/wwwroot/index.html` | 1–2 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/wwwroot/portal.css` | 1–2 | Full source and reviewed amendments |
| `src/RetryAssurance.Portal/wwwroot/portal.js` | 1–4 | Full source and reviewed amendments |
| `src/RetryAssurance.Runner/Infrastructure/AppConstants.cs` | 1–17 | Full source and reviewed amendments |
| `src/RetryAssurance.Runner/Infrastructure/AppPaths.cs` | 1–21 | Full source and reviewed amendments |
| `src/RetryAssurance.Runner/Infrastructure/ExceptionBoundary.cs` | 1–187 | Full source and reviewed amendments |
| `src/RetryAssurance.Runner/Infrastructure/GlobalUsings.cs` | 1–7 | Full source and reviewed amendments |
| `src/RetryAssurance.Runner/Infrastructure/HelperFunctions.cs` | 1–61 | Full source and reviewed amendments |
| `src/RetryAssurance.Runner/Infrastructure/Logger.cs` | 1–188 | Full source and reviewed amendments |
| `src/RetryAssurance.Runner/Infrastructure/Settings.cs` | 1–341 | Full source and reviewed amendments |
| `src/RetryAssurance.Runner/OperationContext.cs` | 1–14 | Full source and reviewed amendments |
| `src/RetryAssurance.Runner/RetryAssurance.Runner.csproj` | 1–1 | Full source and reviewed amendments |
| `src/RetryAssurance.Runner/RetryAssuranceClient.cs` | 1–59 | Full source and reviewed amendments |
| `src/RetryAssurance.Runner/RunnerCommand.cs` | 1–31 | Full source and reviewed amendments |
| `src/RetryAssurance.Runner/RunnerOptions.cs` | 1–12 | Full source and reviewed amendments |
| `src/RetryAssurance.Runner/ScenarioTransport.cs` | 1–35 | Full source and reviewed amendments |

### Changed-file manifest

- `examples/ExampleClients/SyntheticJobClient.cs`
- `server/projects.mjs`
- `src/RetryAssurance.Contracts/ServiceModels.cs`
- `src/RetryAssurance.Portal/App.razor`
- `src/RetryAssurance.Portal/Models/PortalModels.cs`
- `src/RetryAssurance.Portal/Pages/Account.razor`
- `src/RetryAssurance.Portal/Pages/Billing.razor`
- `src/RetryAssurance.Portal/Pages/Compare.razor`
- `src/RetryAssurance.Portal/Pages/Home.razor`
- `src/RetryAssurance.Portal/Pages/NewProject.razor`
- `src/RetryAssurance.Portal/Pages/Packs.razor`
- `src/RetryAssurance.Portal/Pages/Privacy.razor`
- `src/RetryAssurance.Portal/Pages/Project.razor`
- `src/RetryAssurance.Portal/Pages/Projects.razor`
- `src/RetryAssurance.Portal/Pages/Run.razor`
- `src/RetryAssurance.Portal/Pages/RunnerGuide.razor`
- `src/RetryAssurance.Portal/Services/PortalApi.cs`
- `src/RetryAssurance.Portal/Shared/ActionPage.cs`
- `src/RetryAssurance.Portal/Shared/ServiceLayout.razor`
- `src/RetryAssurance.Portal/wwwroot/appearance.js`
- `src/RetryAssurance.Portal/wwwroot/icon.svg`
- `src/RetryAssurance.Portal/wwwroot/index.html`
- `src/RetryAssurance.Portal/wwwroot/portal.css`
- `src/RetryAssurance.Runner/Infrastructure/ExceptionBoundary.cs`
- `src/RetryAssurance.Runner/Infrastructure/Logger.cs`
- `src/RetryAssurance.Runner/Infrastructure/Settings.cs`
- `src/RetryAssurance.Runner/RetryAssuranceClient.cs`
- `src/RetryAssurance.Runner/RunnerCommand.cs`
- `src/RetryAssurance.Runner/ScenarioTransport.cs`

### Reading status

All eleven routes, layout/status/action components, portal transport/models, CSS/appearance/screen scripts, contracts, runner/examples and linked service/schema files were read completely. The portable packaging script and build properties were also read because the download instructions depend on their real folder layout. Changes remain local and uncommitted.

### Route / — understand and start a check

- User job: test whether a retry creates duplicate jobs. The original button "Start a real execution" opened a project setup form, not an execution.
- Unnecessary copy: "No enterprise SDK platform required." Also: "Pricing and market demand remain unvalidated." Neither helps complete a check.
- Useful behavior: one client invocation sends repeated requests to a three-second delayed-reply scenario; the client runs locally or in the user's build, never in the portal.
- Local correction: direct purpose, Set up a check / Try the examples, three short steps and optional scenario limits. .NET compatibility and test-data requirements remain because they affect whether the user can run the product.

### Route /projects — find work

- Unnecessary copy: "No sample organization or run is inserted for you." The empty state needs a creation action.
- Original refresh label: "Refresh actual projects".
- Useful behavior: the response lists this account's actual project memberships and roles, while limits count owned projects.
- Local correction: sign-in state before the private request, concise project names/roles, New project and Refresh, a retry after failed loading, and limits/access in a disclosure. Opaque IDs no longer dominate the list.

### Route /projects/new — choose a check policy

- Unnecessary button: "Create account-owned project and immutable revision 1".
- Hidden important behavior: a permitted final failure or unresolved timeout can still pass the duplicate-prevention policy when only one job exists. At least two requests are needed; one successful request is incomplete.
- Original friction: two naming fields and a single-choice pack dropdown precede the action; an unsigned visitor could fill them before receiving an authentication refusal.
- Local correction: actual sign-in gating with a safe internal continuation; project name and acceptable outcomes first; explicit timeout/failure meaning; rule/version/naming details remain available. No scenario semantics were changed.

### Route /account — sign in and retain recovery

- Unnecessary copy: "The service does not invent an email verification or password-reset flow."
- Hidden behavior: the recovery key is shown once and cannot be recovered later. The old signed-in project action appeared before the key panel and could take a new account away before saving its only credential.
- Local correction: key-save step first, reveal/copy/download retained, safe internal next-route continuation and explicit refresh/recovery. Invitation acceptance remains an actual account action and does not expose its token as a query parameter.
- Deletion still names its full actual scope and blocks an active paid period. No private credential file or environment value was read.

### Route /projects/{Id} — authorize, run and inspect

- Unnecessary labels: "ACTUAL PROJECT", "Create actual runner capability" and "Inspect actual evidence".
- Actual permission mismatch found: GET returns every project key's metadata; the old UI showed Revoke for any key to an editor, but the POST allows only that editor's own keys. A boolean canRevoke projection now reflects the current role and creator relation; the UI retains only the available revoke controls. No authorization was relaxed.
- Hidden useful behavior retained: keys expire after ninety days, only ten can be active, revocation blocks later child requests without deleting historical evidence, settings refresh preserves an unsaved draft, and older runs retain their settings.
- Important replay mistake: the old generated replay command always launched CorrectClient, even for a user's own recorded client. It could replace the tested client rather than retest a fix.
- Local correction: key creation and running first, check settings/history and team management behind disclosures, clearly labelled portable example command, and comparison options that must be added to the user's own runner command. Readonly viewers do not get misleading execution actions.
- Existing invitations do not overwrite a current member's role; the owner is directed to change that member below. Run list is labelled recent rather than unlimited.
- Remaining product gap: there is no individual-project deletion route. No fictitious Delete project action was added.

### Route /runs/{Id} — inspect actual request effects

- Unnecessary label: "SERVER-OWNED JOURNAL · ONE LOGICAL OPERATION".
- Misleading original display: each request showed "HTTP" followed by its recorded response status. The first request can time out on the client before receiving those prepared headers. It is now explicitly a prepared reply in request details, not proof of delivery.
- Hidden useful data recovered: the API already sends runnerObservation.detail, but ExecutionRecord omitted it. The typed note now survives portal display and evidence export, labelled as client-reported.
- Local correction: jobs created / requests / keys first; request rows say created, reused or rejected, with stable Key 1 / Data 1 grouping from actual hashes. Full IDs, hashes, body sizes, times and pinned settings remain expandable. No code-change cause is guessed from a counter.
- Incomplete runs now tell the user to refresh if the client is still running, otherwise start a new run. The thirty-minute access deadline is visible; the interface does not imply cross-process resume.
- Repetition uses options for the user's own runner, not a substituted CorrectClient. Export disclosure retains the fact that client notes/labels are included.

### Route /compare/{CurrentId}/{BaselineId} — compare two records

- Unnecessary label: "TWO SEPARATE EXECUTIONS · RETAINED EVIDENCE".
- Original "Compatible scenario and contract pins" is replaced with plain same/different check and scenario language.
- Useful behavior preserved: comparison is refused as like-for-like when contract/scenario content differs; both actual resource/request/key counts and client outcomes remain.
- Local correction: baseline and selected run columns, concise metrics, full-result links and optional interpretation limits. A difference is not presented as proof that a particular source change fixed the problem.

### Route /packs — understand the supported fault

- Unnecessary copy: "A bounded executable server behavior, not an AI-generated description or an arbitrary customer script."
- Useful limits: JSON POST jobs, first reply held three seconds, at most six requests, a stable key/data pair reuses the job and a different key creates another.
- Local correction: four action-based steps, supported request and request budget, advanced version/hash details and a load retry.
- Preservation issue caught by complete backend reading: pack hashes include its authored metadata. Editing the existing pack description/title in protocol.mjs would break stored contract pins. That immutable pack data was not edited; only its presentation changed.

### Route /runner — finish the first actual job

- Original said "Build or download" but all generated commands used examples/CorrectClient/bin/Release/net10.0/CorrectClient.dll. The actual ZIP contains RetryAssurance.Examples.NET10/CorrectClient/CorrectClient.dll, so the download path did not match the commands.
- Local correction: the real release link, extraction-folder instructions and working portable paths are primary; source builds remain optional. Broken/stable-key examples are identified as examples, not customer incidents.
- Useful controls retained: private RA_PROJECT_TOKEN input, actual --baseline / --revision / --json options, evidence links and exit codes. Own-client requirements remain explicit: supplied HttpClient, JSON POST jobs, stable retry key, synthetic data and a sub-three-second timeout.
- The guide no longer describes running CorrectClient as proof that the user's own client was corrected.

### Route /account/billing — see actual access

- Unnecessary copy: "A proposed subscription's attractiveness is a demand hypothesis, not a claim of customers or revenue."
- Useful state retained: actual current limits, unconfigured checkout, active Team access, live/test distinction, linked records and provider management.
- Local correction: short free/current allowance view, actual unavailability rather than setup narration, test checkout clearly separate from live access, history behind a disclosure and Refresh reachable after an initial failure.
- No provider value was configured, no checkout started and no paid allowance invented.

### Route /privacy — understand what is sent and saved

- Unnecessary implementation sentence: "Effect commit and journal insertion complete in an atomic database batch before delayed response headers are released."
- Actual user-relevant distinction retained: code runs in the user's environment; supplied transport does not sanitize arbitrary body data or sandbox separate connections. Only test input belongs here.
- Saved data and client notes, operator access, raw-body/header exclusion, key hashing, member roles/revocation, thirty-minute run access, deletion, exported copies and card handling remain described plainly.
- The prepared-response versus client-receipt distinction is now clear without database terminology.

### Shared interface, appearance and console output

- Removed the universal banner "REAL CLIENT · SYNTHETIC REMOTE EFFECTS" and unsigned status "Local source, real service". Navigation, actual account alias/sign-in and the theme control remain.
- Generic clipboard success no longer warns about secrets when copying a non-secret command. Private-key instructions remain at the key.
- Huge hero type and neon accents were reduced; pass/fail/incomplete remain distinct readable states. The contrast observer now watches disabled changes too.
- CLI metadata calls previously printed every observer/control request. They now retain disk diagnostics without console flooding; ordinary results and evidence links remain. Invalid arguments produce a useful message and exit 2.
- Logger treated a string as IEnumerable and wrote its characters on separate log lines; the string case now stays intact. The generated unused Language setting was removed. No new settings feature was advertised.
- The supplied client, scenario behavior, pack hashes and earlier evidence records were preserved. There is no claim of broader fault coverage, source attestation or a retest of arbitrary client code.

### Verification and remaining boundary

Portal and runner/example Release builds passed with zero warnings/errors. Twelve production-handler/SQLite projection assertions passed for owner, two editors and viewer permissions. Four actual DTO checks covered the restored note, export round-trip and permission defaults. A copied temporary CLI payload produced exit 2 and only two actionable error lines for invalid options, before any credential read or request. Its fresh settings contained EnableLogging only.

These are local source/build/consumer results. No real account, provider action, HTTP fixture or native authenticated browser session was created. Current monthly usage and individual-project cleanup are not exposed by existing read/delete routes; no data or controls were fabricated to cover those gaps.

## ThreeBeatFreight

Project root: `C:\E\Claude\WebSiteleri\ThreeBeatFreight`.

Reviewed baseline: `ac1ce5c`. Documentation read before implementation: README, AGENTS, documentation index, Guide, Rules-and-Service, Content-and-Replay and Revenue-and-Locker, including its complete evidence/body-boundary sections.

### Complete source inventory

| Source path relative to this project | Full current range | Actual read coverage |
| --- | --- | --- |
| `Directory.Build.props` | 1–8 | Full source and reviewed amendments |
| `functions/ads.txt.js` | 1–1 | Full source and reviewed amendments |
| `functions/api/[[path]].js` | 1–2 | Full source and reviewed amendments |
| `game/content.mjs` | 1–46 | Full source and reviewed amendments |
| `game/engine.mjs` | 1–242 | Full source and reviewed amendments |
| `game/rival.mjs` | 1–11 | Full source and reviewed amendments |
| `schema/001.sql` | 1–9 | Full source and reviewed amendments |
| `schema/002.sql` | 1–2 | Full source and reviewed amendments |
| `scripts/Build.ps1` | 1–14 | Full source and reviewed amendments |
| `server/handler.mjs` | 1–85 | Full source and reviewed amendments |
| `server/http.mjs` | 1–6 | Full source and reviewed amendments |
| `server/revenue.mjs` | 1–39 | Full source and reviewed amendments |
| `src/ThreeBeatFreight.Client/App.razor` | 1–1 | Full source and reviewed amendments |
| `src/ThreeBeatFreight.Client/Models/FreightModels.cs` | 1–30 | Full source and reviewed amendments |
| `src/ThreeBeatFreight.Client/Pages/Freight.razor` | 1–96 | Full source and reviewed amendments |
| `src/ThreeBeatFreight.Client/Program.cs` | 1–7 | Full source and reviewed amendments |
| `src/ThreeBeatFreight.Client/ThreeBeatFreight.Client.csproj` | 1–6 | Full source and reviewed amendments |
| `src/ThreeBeatFreight.Client/_Imports.razor` | 1–5 | Full source and reviewed amendments |
| `src/ThreeBeatFreight.Client/wwwroot/_headers` | 1–9 | Full source and reviewed amendments |
| `src/ThreeBeatFreight.Client/wwwroot/_routes.json` | 1–1 | Full source and reviewed amendments |
| `src/ThreeBeatFreight.Client/wwwroot/assets/ad-consent.mjs` | 1–8 | Full source and reviewed amendments |
| `src/ThreeBeatFreight.Client/wwwroot/assets/appearance.css` | 1–7 | Full source and reviewed amendments |
| `src/ThreeBeatFreight.Client/wwwroot/assets/appearance.js` | 1–145 | Full source and reviewed amendments |
| `src/ThreeBeatFreight.Client/wwwroot/assets/content.mjs` | 1–46 | Full source and reviewed amendments |
| `src/ThreeBeatFreight.Client/wwwroot/assets/engine.mjs` | 1–242 | Full source and reviewed amendments |
| `src/ThreeBeatFreight.Client/wwwroot/assets/freight.css` | 1–4 | Full source and reviewed amendments |
| `src/ThreeBeatFreight.Client/wwwroot/assets/freight.mjs` | 1–68 | Full source and reviewed amendments |
| `src/ThreeBeatFreight.Client/wwwroot/assets/rival.mjs` | 1–11 | Full source and reviewed amendments |
| `src/ThreeBeatFreight.Client/wwwroot/icon.svg` | 1–1 | Full source and reviewed amendments |
| `src/ThreeBeatFreight.Client/wwwroot/index.html` | 1–3 | Full source and reviewed amendments |
| `src/ThreeBeatFreight.Client/wwwroot/privacy.html` | 1–1 | Full source and reviewed amendments |

### Changed-file manifest

- `server/handler.mjs`
- `server/revenue.mjs`
- `src/ThreeBeatFreight.Client/App.razor`
- `src/ThreeBeatFreight.Client/Models/FreightModels.cs`
- `src/ThreeBeatFreight.Client/Pages/Freight.razor`
- `src/ThreeBeatFreight.Client/wwwroot/assets/appearance.js`
- `src/ThreeBeatFreight.Client/wwwroot/assets/freight.css`
- `src/ThreeBeatFreight.Client/wwwroot/assets/freight.mjs`
- `src/ThreeBeatFreight.Client/wwwroot/icon.svg`
- `src/ThreeBeatFreight.Client/wwwroot/privacy.html`


### Route / — Play, Friends, Arena lab, Rules

- **Purpose:** plan three commands, deliver cargo and review the resolved beats. The board and command controls communicate the game, but the first visit originally buried the playable task under a slogan, three large headline fragments and repeated explanations. Removed “Three commands. Four energy. Shared trouble.”, “SOLO · SAME RULES AS LIVE ROOMS”, “No account or key needed for solo. Records stay in this browser.” and the footer “Original fictional freight yards · English game content”. The introduction now says “Bring the cargo home.” and offers the first delivery.
- **Play:** retained all 24 authored contracts, daily choice, contract hints, energy limit, cargo/recovery targets, score, six-round match result and rule mechanics. Removed the result paragraph “The result belongs to these actual room participants. No global ranking is implied.” and the contract-list justification “Original solo contracts. Your best local attempts are marked below; they are not online rankings.” Daily/local-result storage meaning is stated once.
- **Hidden useful behavior:** the engine's preview assumes every other courier waits. Preserved that condition beside the preview. Direction changes previously left the old preview displayed; Direction now invokes the same invalidation as changing command kind. Direction also locks after completion. A caption above the board distinguishes current state from a previous round/beat. Tile contents now have individual accessible labels inside a group instead of being swallowed by the parent image role.
- **Replay:** import already supports read-only theatre and export already supports completed rounds before match end. Exposed Download replay whenever a recorded round is available. Theatre replaces the disabled command editor with viewing instructions. Removed “These are missing player submissions, not computer players.” while retaining the actual deadline-wait aliases.
- **Friends:** removed “Bring a real rival” and “2–4 actual couriers.” Replaced the primary explanation with the invitation task. The 90-second deadline, automatic waits, closing-versus-leaving distinction and reconnection are explained together. The unsigned invitation landing now opens Friends with the room code populated. Register/recover/sign-out no longer jump back to an old solo match; the newly created recovery key stays visible. Empty nickname/key/invitation submissions are disabled. Private key warning and the manual invitation fallback remain.
- **Arena lab:** replaced “Build a yard worth fighting over” and the format/validation introduction with the painting task. Terrain, dock/source placement and balance limits remain in a specific disclosure. Deleted “A dispatch bot is an automated rival, never a claimed online player.” Bot actions stay labelled as bot matches. The existing chosen-yard control is exposed when starting bot practice from Play. File import/export and validated arena painting remain intact.
- **Substantive save correction:** entering a room or importing a replay used a different current state, but later saving an arena serialized that current view over the previous solo save. The module now keeps the last personal match separately; only personal match starts/commits update it. Arena/cloud-design saves preserve it, and Resume saved match exposes it. A blocked/full storage write no longer throws after mutating the JavaScript state and leaves C# stale: gameplay continues with a visible “Changes are not saved on this device” warning and a download action. No durable save is claimed when storage fails.
- **Locker and fees:** signed-out users no longer see a fabricated current 0/3 usage display. Current usage is shown only for an account. Removed “The owner has not configured a payment account.” Subscription unavailable status remains, alongside real free capacity, test checkout distinction and paid-period/retained-yard consequences. No checkout or ad configuration was activated.
- **Rules:** preserved all simultaneous movement, shove/brace, automatic collection, hazard, tie, deadline and file-content rules. Deleted the “Support the yard” paragraph's owner/provider-readiness narrative. Generic failures no longer promise that a write definitely succeeded; they ask the player to refresh and check the plan.
- **Appearance:** softened interface lime/pink to blue/tan, reduced headline size and thick accents, retained four distinguishable numbered couriers and terrain symbols. The appearance observer now reacts to disabled controls. Game engine rules and content were not recolored or versioned.

### Route /privacy.html

- Replaced database vendor names, cookie flags, storage key names, hashes, fabricated-player disclaimers and the “Infrastructure” making-of explanation with actual stored information and user choices.
- Preserved operator access, room participants' access, protected unrevealed plans, recovery-key risk, session duration, no automatic retention/account deletion, downloaded-file sharing, subscriptions not canceled by sign-out and optional ad privacy choices.
- The privacy page does not assert that owner-configurable advertising/payment is currently enabled.

### Reading and focused verification

The canonical game/content/rival modules were read completely. Their browser copies were SHA-256 identical at inspection, so the identical copies are covered by the canonical full read. All request handlers, quota/payment checks, CMP loader, renderer-facing bridge and schema were read; no secrets or provider account were opened.

Release build completed with zero warnings/errors. Ten production-module consumer assertions covered personal-save persistence across rooms and imported theatre, restoration, resume and blocked storage. These are Node/browser-primitive fixtures, not native browser sessions. No room/player/payment was created by this review.


## StageFold

Project root: `C:\E\Claude\WebSiteleri\StageFold`.

Reviewed baseline: `5405cf6`. Documentation read before implementation: README, AGENTS, documentation index, Architecture, Product, Rendering and Service-Contract.

### Complete source inventory

| Source path relative to this project | Full current range | Actual read coverage |
| --- | --- | --- |
| `Directory.Build.props` | 1–1 | Full source and reviewed amendments |
| `functions/api/[[path]].js` | 1–26 | Full source and reviewed amendments |
| `public/_headers` | 1–5 | Full source and reviewed amendments |
| `public/_redirects` | 1–1 | Full source and reviewed amendments |
| `public/_routes.json` | 1–1 | Full source and reviewed amendments |
| `schema/001-stagefold.sql` | 1–17 | Full source and reviewed amendments |
| `server/accounts.mjs` | 1–11 | Full source and reviewed amendments |
| `server/billing.mjs` | 1–28 | Full source and reviewed amendments |
| `server/cuts.mjs` | 1–32 | Full source and reviewed amendments |
| `server/film.mjs` | 1–25 | Full source and reviewed amendments |
| `server/moderation.mjs` | 1–5 | Full source and reviewed amendments |
| `server/projects.mjs` | 1–26 | Full source and reviewed amendments |
| `server/security.mjs` | 1–21 | Full source and reviewed amendments |
| `src/StageFold.App/App.razor` | 1–4 | Full source and reviewed amendments |
| `src/StageFold.App/Pages/Account.razor` | 1–26 | Full source and reviewed amendments |
| `src/StageFold.App/Pages/Cut.razor` | 1–34 | Full source and reviewed amendments |
| `src/StageFold.App/Pages/Cuts.razor` | 1–12 | Full source and reviewed amendments |
| `src/StageFold.App/Pages/Guide.razor` | 1–3 | Full source and reviewed amendments |
| `src/StageFold.App/Pages/Membership.razor` | 1–14 | Full source and reviewed amendments |
| `src/StageFold.App/Pages/Privacy.razor` | 1–3 | Full source and reviewed amendments |
| `src/StageFold.App/Pages/Project.razor` | 1–24 | Full source and reviewed amendments |
| `src/StageFold.App/Pages/Projects.razor` | 1–10 | Full source and reviewed amendments |
| `src/StageFold.App/Pages/Studio.razor` | 1–98 | Full source and reviewed amendments |
| `src/StageFold.App/Program.cs` | 1–11 | Full source and reviewed amendments |
| `src/StageFold.App/Services/ServiceModels.cs` | 1–25 | Full source and reviewed amendments |
| `src/StageFold.App/Services/StageApi.cs` | 1–32 | Full source and reviewed amendments |
| `src/StageFold.App/Shared/ActionPage.cs` | 1–31 | Full source and reviewed amendments |
| `src/StageFold.App/Shared/StageLayout.razor` | 1–4 | Full source and reviewed amendments |
| `src/StageFold.App/Shared/Status.razor` | 1–2 | Full source and reviewed amendments |
| `src/StageFold.App/StageFold.App.csproj` | 1–1 | Full source and reviewed amendments |
| `src/StageFold.App/_Imports.razor` | 1–10 | Full source and reviewed amendments |
| `src/StageFold.App/wwwroot/app.css` | 1–4 | Full source and reviewed amendments |
| `src/StageFold.App/wwwroot/appearance.css` | 1–8 | Full source and reviewed amendments |
| `src/StageFold.App/wwwroot/appearance.js` | 1–146 | Full source and reviewed amendments |
| `src/StageFold.App/wwwroot/files.js` | 1–1 | Full source and reviewed amendments |
| `src/StageFold.App/wwwroot/icon.svg` | 1–1 | Full source and reviewed amendments |
| `src/StageFold.App/wwwroot/index.html` | 1–1 | Full source and reviewed amendments |
| `src/StageFold.App/wwwroot/stage.js` | 1–110 | Full source and reviewed amendments |
| `src/StageFold.Core/Film.cs` | 1–62 | Full source and reviewed amendments |
| `src/StageFold.Core/FilmRules.cs` | 1–101 | Full source and reviewed amendments |
| `src/StageFold.Core/StageFold.Core.csproj` | 1–1 | Full source and reviewed amendments |
| `src/StageFold.Core/Starters.cs` | 1–86 | Full source and reviewed amendments |

### Changed-file manifest

- `src/StageFold.App/Pages/Account.razor`
- `src/StageFold.App/Pages/Cut.razor`
- `src/StageFold.App/Pages/Cuts.razor`
- `src/StageFold.App/Pages/Guide.razor`
- `src/StageFold.App/Pages/Membership.razor`
- `src/StageFold.App/Pages/Privacy.razor`
- `src/StageFold.App/Pages/Project.razor`
- `src/StageFold.App/Pages/Projects.razor`
- `src/StageFold.App/Pages/Studio.razor`
- `src/StageFold.App/Services/StageApi.cs`
- `src/StageFold.App/Shared/StageLayout.razor`
- `src/StageFold.App/wwwroot/app.css`
- `src/StageFold.App/wwwroot/appearance.js`
- `src/StageFold.App/wwwroot/icon.svg`
- `src/StageFold.App/wwwroot/index.html`
- `src/StageFold.App/wwwroot/stage.js`
- `src/StageFold.Core/Starters.cs`


### Routes / and /studio — Studio

- **Purpose:** make a timed film from moving performers and captions. “Direct a small film.” / “Move the performers. Find the pause. Change the ending.” did not name the concrete controls well. The opening now says “Make a short film” with one task sentence and “Start with a story”.
- Removed “Original first-party starter”, “Actual scene differences”, “A shot is continuous movement, not a still panel.”, “The stage uses original vector shapes.” and the paragraph beginning “Local means this browser.” None was moved into Help.
- Kept Apply as the main editing action, Undo/Redo nearby; grouped new/open actions and downloads into two small menus. Editable file, playable film, preview image and supported video remain separate accurate outputs. Kept the no-audio/visible-tab recording consequence. Exact positions, gestures, expressions, shot limits and caption timing remain.
- **Hidden useful behavior:** Apply validates the current film, keeps up to 30 applied history states and saves a valid draft; invalid forms remain editable. Replacing the film with a blank/starter/import or reloading shared work previously replaced the editor immediately. These actions now ask Replace open film / Keep editing first. The import is parsed before proposing replacement. Shared revisions still require a separate explicit save.
- Preserved the distinction between a local personal draft and shared access. Shared viewers may experiment/export a personal copy but cannot save back. The shared revision, role and loaded-history warning remain visible.
- Shortened success messages to the completed action: “Draft saved.”, “Starter opened.”, “Editable file downloaded.” and “Playable film downloaded.” Deleted the creation/renderer/network self-description from exports' success banners; required remix credits remain in the artifact.
- Twelve original 28-second starter stories and their dialogue were read in full and retained. Their repeated making-of summary was shortened to “A short story to direct, rewrite or remix.”

### Route /account

- Replaced “YOUR CAST NAME, YOUR KEY”, “Keep your work together.”, “Create a real account” and secure-cookie narration with the account's actual purpose and fields.
- Kept the one-time private key, no-email-reset warning, download, delete scope and licensed-copy consequence. Invitation acceptance is disabled until the new key is acknowledged.
- **Hidden useful behavior:** remix/shared-film links lost the selected film through sign-in. A bounded cut/project return is now kept as an internal link, revealed after sign-in and key acknowledgment. No external return address or key is accepted in a URL.
- Refresh account allows recovery from an initial request failure. The key remains private and is not included in films.

### Route /projects

- Removed “A PRODUCTION, NOT A FEED”, the second purpose paragraph and the repeated explanation of the creation process.
- Film titles, role, revision, saved time, owned quota and Open studio remain. The empty state names the existing Save to shared work action. Invitations do not consume owned quota in the underlying service; no quota behavior changed.

### Route /project/{Id}

- Removed “THE CUTTING ROOM”, “People with actual access”, “Open the actual public cut”, “Project access updated on the server.” and “An actual immutable public cut is available at the link below.”
- Retained owner/editor/viewer controls, one-use invitation expiry/revocation, saved revisions, restoring as a new revision, public license/rights confirmation and private-project deletion consequences.
- Publishing remains owner-only and uses the saved revision, not uncommitted studio edits. Public film and private membership/history remain separate. These user decisions are still visible; no privacy promise was weakened.

### Route /cuts

- Removed “WATCH A CUT. TAKE ANOTHER DIRECTION.”, “Real published films appear here.”, “There are no invented accounts, views or reactions.” and “Written and directed for this product. These are editorial examples, not community posts or customers.”
- The page now presents Watch and remix, search, actual matching publications and a clearly named Starter films section. Removed the decorative timestamp fragment labelled CUT; actual author/license/withdrawn state remain.
- Empty results offer a starter directly. No invented public post, view or reaction was added.

### Route /cut/{Id}

- Removed “AN IMMUTABLE PUBLIC CUT”, “This cut changed a real source” and “Reports are not public comments and do not fabricate an automatic moderation decision.”
- Play/pause/seek, editable/playable downloads, remix and real changed-shot comparison remain. Full fingerprint is secondary under Downloads and credit; required author/source/license attribution stays adjacent to the actual reuse choices.
- Reports say they go privately to the operator. Withdrawal still states that existing licensed copies/remixes remain. Withdrawn/hidden availability continues to be enforced by the service.

### Route /membership

- Removed “KEEP THE LOCAL STUDIO OPEN”, “Proposed price”, “Price and willingness to pay are unvalidated.”, provider-mode/configuration booleans and the explanation of merchant keys/signed notification secrets.
- Shows free allowance, collaboration limits/price, current active/free access, actual unavailable/test checkout state and a short billing/saved-work disclosure. Refresh is available even after an initial failure. Real server membership checks and provider grants were not changed or activated.

### Route /guide

- Replaced “A SMALL STAGE, A COMPLETE WORKFLOW” / “Make the pause do some work.” with Make your first film.
- Deleted “It is an original first-party film, not a user's post.”, canvas/MediaRecorder implementation terms, the finite-vector/script-proof paragraph and “These are editorial direction exercises. They do not imply an audience, a class or customer results.”
- Retained actual starter edit steps, fit-caption behavior, export choices, recording interruption/no audio, backup/undo limits, invitation steps, revision conflict recovery and public reuse requirements. The 12 direction prompts remain useful creative exercises.

### Route /privacy

- Kept actual saved draft/device preference, uploads to shared work, role/history access, exported copies, key recovery, public license/withdrawal, account deletion and paid-period consequences.
- Removed the original-vector release description, hash implementation narration, fake-person/moderation disclaimers and key/price/webhook setup instructions. Explicitly states operator access to stored films.

### Shared appearance, renderer and linked service review

- The theme script wrote data-ra-theme while its CSS/observer used data-sf-theme; changing the theme did not change the intended surface styles. Both initial selection and setTheme now write the correct marker.
- Softened interface orange/blue/yellow and thick accents, raised tiny action text, removed opacity-based disabled fading and kept readable disabled colors. Distinct film artwork/cast vocabulary and responsive studio structure remain.
- Removed footer vector/media making-of copy. Boot failure no longer claims that an unsaved scene is safe. Generic billing/service errors translate setup internals into the failed user action, while specific form and permission errors remain.
- Completely read renderer animation, drag handles, playback, JSON/PNG/HTML output, recording cancellation, typed validation, 12 starter films, service roles/revisions/fork/public projections, account deletion, moderation and billing/schema. No backend authority or paid grant was changed.
- Release build: zero warnings/errors. 22 actual compiled component/film checks covered replacement confirmation/cancellation, queued valid import and all 12 starter durations/validation. Four production appearance-script checks covered dark default, toggle marker, stored choice and restored light choice. These do not claim native rendering, file dialogs, recording codecs or provider/live identity execution.


## Verification boundary and publication

The corrected C# applications compiled in Release mode. New targeted checks stayed in `C:\C\ManualIndependentUi`; they did not create live accounts, player rooms, charges or publications. Historical live checks are not evidence of this interface review. Publication uses each existing repository workflow after explicit authorization; commit/run/delivery evidence is recorded separately below. BinSite's index remains under the root agent's control.

### Authorized publication status

The following reviewed commits were pushed to the existing main branches after root authorization. All four repository indexes and working trees were empty immediately afterward. No BinSite index mutation was made.

| Repository | Reviewed commit | Existing workflow run | Current observation |
| --- | --- | --- | --- |
| InterlockExpeditions | `fa58cde28a3b0cb6a0b089371b9042c37f5855c7` | [40133875142](https://github.com/muhammetozeski/InterlockExpeditions/actions/runs/40133875142) | Completed successfully; see delivery evidence below |
| RetryAssurance | `8faff1023907e4e5856ed1b19f23faf1660874f5` | [40133874692](https://github.com/muhammetozeski/RetryAssurance/actions/runs/40133874692) | Completed successfully; see delivery evidence below |
| ThreeBeatFreight | `bafa224e015aee2fd4e75915af61c1c9c241e4b3` | [40133874878](https://github.com/muhammetozeski/ThreeBeatFreight/actions/runs/40133874878) | Completed successfully; see delivery evidence below |
| StageFold | `064e2fd6f612d6ca434704f88528808a00fba7bb` | [40133874865](https://github.com/muhammetozeski/StageFold/actions/runs/40133874865) | Completed successfully; see delivery evidence below |

Source/build findings above remain distinct from live browser observations. No authenticated UI, payment, video recording or room interaction was performed during this review.

### Delivery observed after publication

All four existing workflows completed successfully. Public unauthenticated GET requests returned HTTP 200 for each root document and the two changed assets listed below. Each asset matched the reviewed source after normalizing line endings and trailing newlines. This proves source delivery, not rendered UI, authenticated actions, payment, recording or native file handling.

| Product | Workflow result | Public asset delivery |
| --- | --- | --- |
| [InterlockExpeditions](https://muhammetozeski-interlock-expeditions.pages.dev) | [40133875142](https://github.com/muhammetozeski/InterlockExpeditions/actions/runs/40133875142) — success | `/assets/site.css`, `/assets/device.js` — HTTP 200, source match |
| [RetryAssurance](https://muhammetozeski-retry-assurance.pages.dev) | [40133874692](https://github.com/muhammetozeski/RetryAssurance/actions/runs/40133874692) — success | `/portal.css`, `/appearance.js` — HTTP 200, source match |
| [ThreeBeatFreight](https://three-beat-freight.pages.dev) | [40133874878](https://github.com/muhammetozeski/ThreeBeatFreight/actions/runs/40133874878) — success | `/assets/freight.css`, `/assets/freight.mjs` — HTTP 200, source match |
| [StageFold](https://muhammetozeski-stagefold.pages.dev) | [40133874865](https://github.com/muhammetozeski/StageFold/actions/runs/40133874865) — success | `/app.css`, `/appearance.js` — HTTP 200, source match |
