# Next API products

Research date: 2026-10-25. These are three product hypotheses, not implemented services or demonstrated demand. Evidence below is limited to six official API documents; no connected-account operation or payment was attempted. None is claimed to be a new invention.

## 1. Series Release Pipeline — preferred next investigation

**Buyer hypothesis:** a small education or media team publishing a multi-part, multilingual YouTube series every week.

**Job:** owned local video files + approved titles/translations + episode order and release times → upload explicitly as private, retain returned video identities, check each episode against the release plan, stop ambiguous failures for reconciliation, then apply only approved release actions → a completed series package with actual channel URLs and a clear unresolved-action list. Media should travel directly from the creator's computer to Google, rather than through our free web host. A partial release must never be described as an atomic transaction.

**Why choose it:** Excel can list episodes; this service must execute and reconcile the related remote jobs. A generic upload form or approval screen is insufficient. The first keyless session could validate a three-video local package and preview its language/time plan. The first completed connected job must prepare those three actual videos, not merely save their planned settings. Return triggers are the next series and a changed episode.

**Verified API boundary:** `videos.insert` accepts video uploads, metadata translations and `status.publishAt`; it returns a video resource. It requires user authorization, including the available `youtube.upload` scope. The current reference states a project upload allowance of 100 calls/day at one unit per call, with a 256 GB file limit; this replaces the old 1,600-unit assumption. It currently says unverified projects are not restricted to private viewing, but that statement is documentation evidence, not a tested public upload. [Upload contract](https://developers.google.com/youtube/data/reference/rest/v3/videos/insert?apix=true).

An API project and OAuth client credentials plus each channel owner's consent are required; an API key alone cannot authorize uploads. YouTube policies require explicit control over writes, prohibit downloading/caching its audiovisual content without prior approval, and require ordinary stored metadata to be refreshed or deleted after 30 days. Selling an API client is permitted subject to the policies; selling API access itself is restricted. Quota extension is an audit process, not an assumed paid unlimited tier. [Developer policies](https://developers.google.com/youtube/terms/developer-policies).

**Paid hypothesis and decision test:** charge for independently useful release orchestration, recovery and team-owned release plans, not access to YouTube or playback. Measure completion of a real three-episode job, manual returns to Studio, recovery after one failed action and a second release. Existing-private-video selection, update/read recovery semantics and Studio's existing batch capabilities still need specific verification before implementation. Playlist, caption and end-screen automation are outside this verified scope. No viewer-growth or payment evidence exists.

## 2. Course Kit Publisher — conditional alternative

**Buyer hypothesis:** a course producer repeatedly delivering changing lessons from Google Docs/Slides to separate cohorts.

**Job:** explicitly selected, authorized source documents + lesson order + required-item rules → export the documents, build a versioned PDF delivery set and navigable lesson package, compare later source changes, rebuild affected lessons after approval → a stable learner release whose earlier version is retained. The recurring operation is source-to-delivery production, not media review or a folder list.

**Keyless first value:** local PDFs can become a working ordered lesson package and missing-item report. Connected source refresh needs a Google Cloud API project/OAuth client and the document owner's grant. `files.export` supports `drive.file` among its scopes and limits each exported document to 10 MB. Broader scopes may require a security assessment; whole-account access must not be inferred from a selected-file grant. [Export contract](https://developers.google.com/workspace/drive/api/reference/rest/v3/files/export).

Current Drive documentation describes standard usage without an additional API charge, but introduces May 2026 quotas and announces future charging changes later in 2026. New-project limits include one million quota units/minute/project and a 1 TB/day egress threshold; these are not a free hosting/storage allowance for our service. [Usage and pricing limits](https://developers.google.com/workspace/drive/api/guides/limits).

**Paid hypothesis and rejection rule:** scheduled rebuilds, private cohort releases and retained versions could earn a subscription. Sharing must be explicitly authorized, private by default and independent of the source document's sharing settings; downloaded copies cannot be recalled. Reject this direction if producers only need a ZIP, or our workflow cannot materially improve repeated exports over Drive itself. Demand and willingness to pay remain untested.

## 3. Cohort Session Operator — conditional alternative

**Buyer hypothesis:** an independent workshop host who repeatedly needs a minimum group size, limited seats and accepted replacements after cancellations.

**Job:** organizer time windows/capacity + participants' explicit commitments → form a group only when its minimum is reached, request acceptance from waitlisted replacements, obtain organizer approval → create the actual Calendar event/invitations and reconcile later changes with the same event identity. No participant is silently marked accepted. This differs from an entered staffing table by operating the participant commitment and cancellation cycle.

**Keyless first value:** manual commitments can produce a feasible group plan and an importable calendar file; that is not a Google reservation. Real writes require an OAuth client, an authorized organizer and calendar write access. Google's guide documents event insertion, later field updates, attendee notifications and client-chosen event IDs to avoid duplicate creation after uncertain responses. It uses the `calendar` scope; use a narrower supported scope only after verifying its method coverage. Private-calendar conflict checking is not established by this six-source review. [Event creation contract](https://developers.google.com/workspace/calendar/api/guides/create-events).

Current Calendar documentation lists standard use without an additional API charge, 10,000 requests/minute/project, 600/minute/user/project and a one-million-request daily billing threshold for the new quota model. It announces charging changes later in 2026; operational and invitation limits still apply. [Usage and pricing limits](https://developers.google.com/workspace/calendar/api/guides/quota).

**Paid hypothesis and rejection rule:** recurring cohorts, waitlist recovery and correctly reconciled calendar changes could be paid organizer features. Reject an implementation that only saves availability or exports a timetable. The uncertainty is whether existing booking products already solve the buyer's full cancellation/quorum job cheaply enough.

## Selection

The creative-agent challenge strengthened the YouTube direction: remote completion and safe continuation must be the product, while approvals support that job. Drive delivery is closer to the existing Review Room/Catalog family; calendar planning is closer to Shift & Cover unless genuine external commitments drive it. Investigate the narrow three-episode YouTube release first. These six documents establish API capabilities and constraints, not an advantage over incumbent products or evidence that anyone will pay.
