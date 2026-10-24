# Shift & Cover Desk

Shift & Cover Desk is one local roster-planning product at `/shift-cover-desk/`, catalog ID 100. It uses the existing Blazor client and the English-only interface. An organizer enters capabilities, actual availability, coverage requirements and limits, then inspects a checked schedule or unresolved coverage. Its eight fictional editable rosters are examples inside this product, not eight separate sites or real workers.

## Time and constraint model

`ShiftCoverProject` stores minute-resolution wall-clock strings in `yyyy-MM-ddTHH:mm` format and one explicit fixed UTC offset. The same clock applies to every shift and availability interval. Overnight work uses the next day's end date, rather than an ambiguous time-only value. Supported offsets range from UTC−12:00 to UTC+14:00 in 15-minute steps. The period spanning all shifts and availability is at most 62 days; a shift is longer than zero and at most 24 hours.

There is no automatic named-zone or daylight-saving conversion. A period needing different offsets must be split into separate projects. Changing the offset reinterprets every wall-clock value and changes its exported UTC instant. The component explains this before its explicit settings-save action. This avoids deriving a zone from browser controls: [`datetime-local` does not include a time zone](https://developer.mozilla.org/en-US/docs/Web/HTML/Reference/Elements/input/datetime-local).

`ShiftCoverRules.CandidateIssues` is the shared constraint engine. A person must have the required role; the union of their entered availability must cover the complete interval. Touching windows combine, but a one-minute gap remains a gap. The shift must fit the person's single-shift limit and their total hours across the entire project. Overlapping assignments are rejected, including two seats in the same shift. Every non-overlapping assigned pair must satisfy that person's entered rest minimum. Time intervals are half-open: one ending exactly when the next begins does not overlap, though a positive rest minimum can still block the pair.

Hour and rest values are organizer inputs, not professional or legal defaults. Empty availability means unavailable. Capability labels do not verify a certification; the planner records no employee acceptance. Labels are chosen by the user; email, phone and other personal details are not required.

## Suggestions and manual changes

`ShiftCoverPlanner.Suggest` begins with only locked assignments. Those assignments must pass the current rules, or the action is refused without altering the original project. Up to eight deterministic greedy passes try different seat priorities and workload preferences. At most 80,000 dynamic candidate checks are performed; bounded baseline capability/availability checks are separate. The result with the highest observed seat count is retained, using squared workload as a tie-break. Generated assignment identities and seat/person mappings are deterministic for the same input; the result's save timestamp is not a reproducibility claim.

This is a heuristic with no backtracking proof. A complete valid result is checked with `AssignmentIssues` before installation. An open seat can still have eligible people: another assignment arrangement may cover more seats. The interface and exports distinguish this from actual current blockers and never report that a partial search proves infeasibility or optimality. Each open row exposes its present blocker summary, and the manual panel lists the exact candidate reasons.

`SetSeat` removes only the selected occupant inside a detached copy, validates the proposed replacement with the same rules, and commits only on success. Clearing a seat is explicit. `Swap` changes both people in a detached copy and validates both resulting assignments before committing. A failed replacement or swap preserves the original document. Locks preserve seats during suggestions; explicit organizer changes can replace, clear or swap them.

Saving changed availability, limits, roles or shift times can expose conflicts in existing assignments. Those assignments remain visible as a draft, accompanied by actual reasons; they are not silently deleted. The organizer can clear or repair them. Deleting an assigned person or requirement asks before removing its occupied seats. Reducing headcount refuses to silently remove occupied seat numbers.

## Local persistence and portable output

`ShiftCoverRules.Restore` validates the full bounded format, required properties, unknown properties, unique identities, references, dates and seat numbers before an import can replace the current roster. The local JSON limit is 2 MiB. A nonempty existing project requires replacement confirmation. Editing uses detached copies so rejected changes cannot partly mutate it.

`shift-cover.js` first writes IndexedDB, waits for transaction completion, and reads back the exact saved JSON. If this fails, it attempts and verifies a localStorage write. If both fail, the action reports failure and the in-memory roster remains available for JSON export. On load, the newest project timestamp selects between existing primary/fallback copies. `Persist` stamps the actual active save, including when an older imported or vault snapshot is opened; otherwise an old fallback copy could wrongly outrank the newly opened document. Availability and assignments never enter diagnostic logs.

The complete local planner, rules, examples and file output require neither an account nor an API key. They can operate after the page has loaded without server requests. This product does not install a service worker or promise a new offline navigation/reload session.

`ShiftCoverExports` produces:

- Full project JSON with definitions, actual assignments and current draft conflicts.
- All-seat CSV, including open requirements, current conflict labels and unresolved reasons; per-person CSV contains only that person's actual assignments. Formula-leading user text is prefixed, cells are quoted, and a UTF-8 byte-order mark is included.
- Standalone escaped HTML with per-person timetables, entered limits, current validation status and every open seat. The report contains supplied values and can be printed after opening; it fetches no external assets.
- All-person or per-person iCalendar files containing only actual assigned events. Any current assignment conflict blocks calendar output, while draft JSON and labelled CSV remain available. Open seats are excluded, and each event describes the roster's outstanding coverage. Events are tentative local plans, not meeting requests or worker acceptance.

Calendar output uses UTC `DTSTART`/`DTEND`, stable project/shift/seat/person UIDs, revision `SEQUENCE`, CRLF delimiters, escaped TEXT values, and folding at 75 UTF-8 octets without splitting Unicode scalars. These decisions follow the primary [iCalendar specification, RFC 5545](https://www.rfc-editor.org/rfc/rfc5545), especially sections 3.1, 3.3.5 and 3.3.11. Files are snapshots; they are not calendar subscriptions and do not automatically remove or update previously imported events.

## Optional owner-only vault

`ShiftCoverVault` uses the existing authenticated `/api/commerce/projects` routes with `kind: "roster"`. It never uses collaborative-workspace routes, invitations or version-history APIs. A real account is opened or recovered at `/uyelik/`; local startup does not contact it.

The UI saves a new independent private snapshot with an explicit outer label of 1–100 characters, separate from the roster's 140-character title. The JSON body is limited to 65,536 UTF-8 bytes even for paid members. Server authorization and the actual combined project allowance control creation/read/delete; the client does not grant capacity or premium rights. Membership may increase saved-project capacity. A server error does not become a successful-save acknowledgement, and the UI advises checking the list before resubmitting a save after a connection failure.

All stored labels, dates, availability, limits and assignments form the whole private document. This is owner-only storage, not a shared staff portal or field-level privacy system. The component provides create/list/open/delete; it does not silently overwrite a previously saved snapshot. Its catalog entry is `adEligible: false`.

## Source and execution evidence

Source responsibilities are separated into `Models/ShiftCoverModels.cs`; `Services/ShiftCoverRules.cs`, `ShiftCoverPlanner.cs`, `ShiftCoverExamples.cs`, `ShiftCoverExports.cs`, and `ShiftCoverVault.cs`; `Pages/ShiftCoverDesk.razor` and its isolated stylesheet; and `wwwroot/shift-cover.js`. They use the existing `SocialApi`, logging and Blazor bootstrap without modifying shared services. The catalog source is `catalog/business-roster.json`.

The coordinated shared Release build completed with zero warnings and zero errors. After the local timestamp correction, the updated verbatim Razor component also compiled separately against that build's unchanged production models and services. No package or tool was downloaded.

Seventy-three executed C# assertions cover all eight examples; deterministic valid output; locked seats; explicit search-budget exhaustion on the maximum 240-seat/60-person input; skills; empty, adjacent and gapped availability; inclusive hour and rest boundaries; actual overlap and overnight intervals; positive fractional and negative UTC offsets; accepted/rejected atomic replacements and swaps; malformed/unknown/duplicate imports; HTML/CSV/calendar encoding; UTC calendar values and 75-octet UTF-8 folding; calendar refusal on conflicts; owner-only vault request/read/delete contracts; label/body preflight limits; component busy-selection/import guards; and the actual-save timestamp of an older imported document. Vault checks used a controlled HTTP handler, not a claimed live authenticated browser session.

Node.js parsed the module and executed ten controlled storage/download assertions: verified fallback, readback, size rejection, total storage failure retaining old data, primary transaction completion/readback, newer-copy selection in both directions, aborted transaction fallback, and local blob-link creation/cleanup with filename rejection. These are browser-interface fixtures, not a real IndexedDB browser transaction, file-dialog interaction or calendar-client import. Independent source inspection by the social-product agent found no additional substantive issue in the declared rule, solver, export, vault and component flows. No live end-user or revenue observation is claimed.
