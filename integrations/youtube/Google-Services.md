# Google service jobs and required access

Research from primary Google documentation on 24 October 2026. Only the public YouTube oEmbed/thumbnail path in README.md was executed. The Drive workflows below were not connected, and no playlist, video or Drive file was changed.

## YouTube

| User job | Official route | Required access |
|---|---|---|
| Inspect pasted video links, get titles/channel names/returned thumbnails | YouTube's own `/oembed` endpoint and returned `i.ytimg.com` image | Implemented and observed without a key or sign-in; public embed-metadata availability only |
| Add real duration, public statistics and video-resource status to that inspection | `videos.list`, requesting only needed `part`/`fields` | An enabled YouTube Data API project with API key for public data; authorized OAuth where private data is requested |
| Collect public playlist entries into an ordered link/reference list | `playlistItems.list`, paginating the response | Enabled Data API/key for public lists; user authorization such as `youtube.readonly` for permitted private account data |
| Apply an explicitly reviewed playlist edit | `playlistItems.insert` or `update`; `playlists.insert` for a new list | OAuth authorization for the actual user's channel; insert accepts `youtube` or `youtube.force-ssl` (partner scope is not the normal route). An API key alone does not grant write authority |

The inspected [videos.list documentation](https://developers.google.com/youtube/v3/docs/videos/list) reports one quota unit per call. [Playlist insertion](https://developers.google.com/youtube/v3/docs/playlistItems/insert) reports fifty units and the accepted write scopes. These are provider quotas, not currency charges or a promise of unlimited traffic. [YouTube authorization](https://developers.google.com/youtube/v3/guides/authentication) requires real Google Cloud authorization credentials for account operations. No such write operation was tested here. The first local tool has no account-connection button or fabricated connected state.

## Drive: first practical job

A small useful first workflow is **selected document export**: the user chooses Docs, Sheets or Slides in Google Picker; the app reads each chosen file's name/type/download capability, offers an appropriate export format, then downloads the actual exports with per-file failures. A combined package can preserve names and a result manifest. This avoids repeatedly opening each document and exporting it separately. It is a proposed first job, not a delivered Drive feature.

For a web integration, the official [Picker setup](https://developers.google.com/workspace/drive/picker/guides/web-picker) needs the enabled Picker/Drive APIs, a browser API key, OAuth client ID, application project number and actual authorized site origin. The browser receives an access token from Google Identity Services after the user grants access. No client secret belongs in public browser source. Keep short-lived access tokens in memory for this first on-demand flow rather than inventing offline access.

Prefer the documented **`https://www.googleapis.com/auth/drive.file`** scope with Picker: it grants access to files created/opened/shared with the app, rather than exposing the whole Drive. This scope is per-file but is not a read-only scope; the first implementation would use only metadata GET and export/download operations. Google lists it as non-sensitive and recommends it for most apps. Broad `drive.readonly` and `drive.metadata.readonly` scopes are listed as restricted and should not be requested merely to implement a selected-file exporter. See [Drive scope classification](https://developers.google.com/workspace/drive/api/guides/api-specific-auth).

The actual data calls would be `files.get` with a narrow fields mask (name, MIME type and `capabilities.canDownload`), `files.export` for Workspace documents, or `files.get?alt=media` for ordinary files the user selected and is allowed to download. The documented `files.export` response is limited to 10 MB; blocked downloads and unsupported types must stay explicit failures. See [Drive downloads/exports](https://developers.google.com/workspace/drive/api/guides/manage-downloads). No private file was fetched to validate this proposal.

Other supported jobs can be added around the same explicit selection: download-readiness/link collection, or a previewed batch rename/copy using the relevant Drive update/copy methods and file permissions. Those writes would require a later explicit user action. This research does not claim account approval, a working OAuth connection, or access to arbitrary Drive contents.
