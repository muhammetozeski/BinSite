# Optional advertising without constraining products

`scripts/Prepare-Revenue.mjs` adds a small optional footer to implemented product entry points after the directory build. It does not wrap product interfaces, replace their styles or require them to use a parent layout. The footer points to membership, sponsor campaign preparation, privacy information and discovery.

`functions/revenue-config.js` returns only public publisher configuration. The browser loads AdSense only when both identifiers validate and the server-side `ADSENSE_CONSENT_READY` flag is true. `functions/ads.txt.js` derives the authorized seller entry from the same publisher ID, avoiding an independent publisher-ID copy in source. No ad network credentials are embedded in HTML.

`scripts/Configure-Advertising.ps1` is an owner-run hosting configuration tool. Run it after the approved AdSense publisher and ad unit have been obtained, and again only when those identifiers or the consent setup change. It is not periodic and the deployed website does not execute it. The tool sends only changed advertising keys in the environment PATCH; the Cloudflare token is taken only from the current process environment. Publish through the existing GitHub workflow after saving configuration.

The consent flag does not itself create a consent management platform. Google requires a certified consent platform for personalized advertising in the specified European regions; configure the actual provider's Privacy & messaging or a certified platform before setting this flag. The loader uses Google's own approved tag and does not replace consent handling with a custom checkbox.

The campaign workshop creates a sponsor brief, marked preview and attributed destination URL. Generating a draft does not buy inventory or start a paid campaign. The site makes no audience-size or impression guarantees without observed traffic.

Evidence: the loader and configuration tools were implemented from the official AdSense tag and consent documentation; loading a live ad has not been verified because a publisher account and unit have not been supplied. Primary references: https://support.google.com/adsense/answer/9274019?hl=tr ; https://support.google.com/adsense/answer/13554116?hl=en ; https://developers.google.com/funding-choices/fc-api-docs .
