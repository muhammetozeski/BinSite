# YouTube link inspection

The `/youtube-tools/` component checks up to twenty pasted video links, obtains real public titles/channel names/returned thumbnails, shows each lookup outcome, previews/downloads a returned image, copies/exports the result table and builds a thumbnail sheet. No account, provider key, server proxy or database is used for this implemented path. The UI does not claim duration, views or region-specific playability.

## Runtime

- `youtube-core.mjs`: exact-host URL normalization, bounded four-worker oEmbed checks, duplicate reuse within a batch and formula-safe table output.
- `Prepare.mjs`: copies the module into `src/BinSite.Social/wwwroot/integrations/youtube/` and compares SHA-256 bytes.
- `src/BinSite.Social/wwwroot/youtube-inspector.js`: isolated per-component sessions, cancellation, real image downloads, clipboard/table output and Canvas contact sheet.
- `src/BinSite.Social/Pages/YouTubeInspector.razor` and `.razor.css`, plus `Models/YouTubeInspectorModels.cs`: compact route and controls.

Run `node integrations/youtube/Prepare.mjs` before publication. The prepared browser copy is included, so the normal Blazor static-asset publication can carry it. The main site's owner supplies the route bootstrap/navigation and combined build; this change does not modify shared router/theme/catalog files.

Input is not put into browser storage or sent to BinSite. The normalized video ID is sent directly to the official YouTube endpoint with credentials omitted. Image preview/download contacts the returned YouTube thumbnail host. Duplicate input rows remain visible; one batch request is reused for the same ID. No returned embed HTML is inserted. Unsupported hosts, credential-bearing URLs and playlist-only links are rejected locally.

## Observed public API evidence, 24 October 2026

A real credential-free GET to `https://www.youtube.com/oembed?url=https%3A%2F%2Fwww.youtube.com%2Fwatch%3Fv%3DVIDEO_ID&format=json` returned:

| Video ID | HTTP | Actual returned title | Actual returned channel | Returned thumbnail |
|---|---:|---|---|---|
| `jNQXAC9IVRw` | 200 | Me at the zoo | jawed | `https://i.ytimg.com/vi/jNQXAC9IVRw/hqdefault.jpg`, 480×360 |
| `7lCDEYXw3mM` | 200 | Google I/O 101: Q&A On Using Google APIs | Google for Developers | `https://i.ytimg.com/vi/7lCDEYXw3mM/hqdefault.jpg`, 480×360 |
| `00000000000` | 404 | No public details returned | No public details returned | None claimed |

With the actual BinSite Origin header, YouTube returned matching Access-Control-Allow-Origin. Both returned image URLs answered HTTP200, image/jpeg and CORS `*`; downloaded bytes had actual JPEG start/end markers. The implemented production module consumed those real responses. This is evidence of the observed official service, not a promised permanent unauthenticated contract or unlimited quota. A failed lookup does not distinguish private, removed, restricted or embedding-disabled videos.

The richer Data API request for contentDetails/statistics without a key or authorized identity returned HTTP403. Such values are absent from this tool rather than guessed. [YouTube's richer video method](https://developers.google.com/youtube/v3/docs/videos/list) documents those separate fields and API-project access.

## Checks and boundary

Twenty-three assertions covered exact-host parsing, input cap, real available/unavailable responses, duplicate reuse, absent invented fields, table escaping, CORS/JPEG bytes and the no-key Data API refusal. Fourteen browser-module consumer assertions exercised real metadata/image bytes/table output, controlled Canvas drawing/cleanup and cancellation. They did not exercise native Canvas pixels, the graphical download prompt or a browser click path. An isolated compilation of the actual Razor/model sources completed with zero warnings/errors using installed .NET10 and offline package sources. No shared project build, provider-account action or private Drive read was performed by this integration task.

The remaining deployment acceptance belongs to the combined main-site release. These sources do not assert that `/youtube-tools/` has already been published.

See [Google services research](Google-Services.md) for the scope/key boundaries and the proposed first Drive job.
