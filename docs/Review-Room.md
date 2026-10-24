# Client Review Room

Client Review Room is one substantive product at `/review-room/`, catalog ID 97. It has an English-only C# Blazor interface and flat, strong-color controls. The local workflow requires neither an account nor an external API key. Its catalog entry uses `runtime: blazor`, category `Workspace`, and `adEligible: false`.

## Actual local workflow

1. Select real images or videos. `wwwroot/review-room.js` keeps `File` objects and session-only blob URLs in the browser. It checks supported file signatures, computes SHA-256 over the complete file bytes, and reads real image/video decoder metadata. The C# document receives filename, size, digest, dimensions and duration, not file bytes or blob URLs.
2. Group genuinely different files into versions of the same logical asset. Exact duplicate file revisions are rejected. Image and video kinds cannot be mixed inside one asset.
3. Click the displayed image to capture coordinates relative to its actual rendered rectangle, normalized to `[0,1]`. The center action offers a keyboard-accessible alternative. Existing comments appear as numbered markers. For video, the action pauses the real media element and reads its actual `currentTime`; selecting a saved comment seeks back to that frame.
4. Store comments against the exact asset ID, version ID and point/time. Category, note/blocking priority and open/resolved state are separate fields. Comments on one version do not silently move to another version.
5. Compare two versions' actual comment counts, blockers, notes and decision histories. The product does not claim automatic pixel-difference analysis or authenticated customer approval.
6. Explicitly mark ready for review, request changes, record approval, record delivery or reopen review. Approval requires ready state and no open blocking comments. Delivery requires approval. Reopening a blocking comment on an approved/delivered version requires an explicit review reopen first.
7. Download JSON, a standalone HTML report, or a CSV of actual open editing work. The report includes file identities, anchors, decisions and the selected version comparison. Media is not embedded. CSV fields neutralize spreadsheet formula prefixes.

Supported types are PNG, JPEG, WebP, GIF, AVIF, MP4, WebM and Ogg video where the browser can decode the selected codec. Each file is bounded to 128 MB; selection batches contain at most eight files. The document allows 30 assets, 20 versions per asset, 100 versions overall and bounded comment/decision records. Browser-session handles are not persistent storage, and the UI explicitly asks the user to retain their project JSON and original media.

## Reconnection and portable ownership

Saved JSON has no local token, object URL, media payload or arbitrary remote-media URL field. After restoring on another device, the recipient selects original media through **Reconnect saved media**. A file matches only when its complete SHA-256 digest, byte count and media kind match a saved revision. Renamed identical files can reconnect; changed files cannot impersonate an old revision. The interface displays unmatched files individually and suggests adding genuinely changed bytes as a new version.

Within the same session, already selected files are retained only when the replacement document has an exact matching identity. Other handles and URLs are released. Leaving the product pauses the local player and releases retained blob URLs.

`ReviewRoomRules` applies edits to a detached validated document and commits only on success. Strict JSON restoration rejects missing required fields, unknown remote URL fields, unknown enum values, duplicate IDs, orphan references, mismatched asset/version anchors and invalid decision transitions. Project imports and cloud loads ask before replacing current work. A local file is an editable copy, not a signed approval artifact; the UI and report state this explicitly.

## Editorial content

`ReviewRoomExamples` defines three original fictional delivery/revision packages:

- Workshop Launch Banner: date placement and reading order.
- Desk Organizer Feature Card: headline space and feature-label hierarchy.
- Community Event Schedule: a readable time column and session rows.

For each selected package, browser canvas drawing generates two real PNG files locally. Those bytes receive real file digests. The C# document adds explicitly marked editorial comments and example decisions. The UI and exported report identify the entire package as an editorial demonstration; no real customer or customer action is fabricated. These are examples within one product, not separate counted sites.

## Optional real cloud workspace

`ReviewRoomCloudClient` calls the existing real-account `/api/commerce/workspaces` contract only after explicit cloud actions. It creates kind `review` workspaces or updates them with the expected server revision. The server owns account authentication, quotas, owner/editor/viewer authorization and stale-write rejection. No pretend login is provided; the panel links to the actual Creator account page.

The owner may request an actual viewer/editor invite. A recipient accepts it with their actual account session, loads the shared JSON and reconnects matching original media locally. **Every invited viewer or editor can read all notes in the shared JSON.** There is no per-comment private field or media upload. A viewer can inspect/export the authorized record or make a separate local copy; the app does not represent that as permission to alter the remote workspace.

Cloud payload quotas are separate from the larger local document limit. A server rejection preserves the current local project. The interface does not autosplit or discard records to fit a quota. Cloud history and member-management extensions exist in the root API but are not claimed as an implemented Review Room UI in this delivery.

## Source responsibilities

- `Pages/ReviewRoom.razor` and `.razor.css`: independent local UI, explicit file matching, workflow, comparison, exports and optional cloud panel.
- `Models/ReviewRoomModels.cs`: portable document, actual browser metadata, anchors and decision records.
- `Models/ReviewRoomCloudModels.cs`: actual workspace API response contracts.
- `Services/ReviewRoomRules.cs`: validation and copy-on-success transitions.
- `Services/ReviewRoomExports.cs`: encoded standalone HTML and spreadsheet-safe CSV.
- `Services/ReviewRoomExamples.cs`: original labelled editorial content.
- `Services/ReviewRoomCloudClient.cs`: metadata-only account-authorized synchronization.
- `wwwroot/review-room.js`: browser file reading, byte hashing, decoder metadata, preview markers/player, local generated media and user-initiated downloads.

Deployment must copy `review-room.js` to the public root along with the Blazor runtime and bootstrap document. The product does not change the shared application layer, API authorization logic or other products.

## Verification evidence

The full Blazor Release build completed with zero warnings and zero errors. The route checker passed 22 page directives in 27 Razor source files without normalized duplicates.

An isolated .NET harness in `C:\C\ReviewRoomChecks` compiled the actual production models/rules/exports/examples directly and passed five groups:

1. Normalized image points, unchanged input on rejected edits, blocking approval rules, and explicit delivery/reopen transitions.
2. Distinct revision identity, size-plus-digest reconnection including renamed files, JSON round-trip IDs, exclusion of session tokens/blob URLs, and rejection of missing fields, foreign URL fields and orphan notes.
3. Exact fractional video timestamps, duration bounds and rejection of image anchors on video.
4. Encoded user markup in HTML, actual selected-version comparison, precise worklist anchors and CSV formula neutralization.
5. Three explicitly labelled editorial two-version packages with valid records.

The ES module passed syntax checking. A separate browser-bridge contract test used a known PNG fixture and a decoder adapter to verify the complete-file SHA-256, rename identity, content/signature and size rejection, and reading/pausing the player's timestamp property with networking disabled. This is a contract test, not a rendered-browser codec or file-dialog test.

The root workspace API had independently passed live D1 create/read, invitation/join, role enforcement, stale revision, history and archive checks before integration. That server evidence is separate from end-to-end browser testing of this new optional client panel. Publication and rendered-browser acceptance belong in the root deployment record.
