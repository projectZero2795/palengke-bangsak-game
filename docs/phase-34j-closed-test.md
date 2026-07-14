# Phase 34J: Closed testing

## Current status

Phase 34J is in progress. On 2026-07-14 Google Play Console accepted the closed
track configuration into review as part of a 16-change submission. The review
can take up to seven days or longer, and production access still depends on the
required testing period and evidence.

No production rollout was created or authorized.

## Closed-track configuration

- Track: Closed testing / Alpha
- Release: `1 (0.34.9)`
- Version code: `1`
- Artifact: the signed AAB already validated in Phase 34G and uploaded in Phase
  34H
- Availability: 176 named countries/regions plus the rest of world
- Tester management: email list `Bang-Sak internal testers`
- Valid tester accounts in the list: 45
- Feedback address: `soporte@palengke.es`
- Opt-in URL: `https://play.google.com/apps/testing/es.palengke.bangsak`

Release notes:

> First Bang-Sak closed test. Includes local practice, private Photon
> multiplayer rooms, Android touch controls, accessibility settings, reconnect
> handling, and the Palengke leaderboard.

## Submission evidence

- Play Console accepted release `0.34.9` for a full closed-track rollout.
- The attached tester list contains 45 accounts; Play showed zero opted-in
  testers at submission time.
- The release, worldwide availability, track resume, tester list, feedback
  address, store listing, and compliance answers were sent for Google review.
- Play reported only two non-blocking bundle recommendations: upload native
  debug symbols and a deobfuscation file for improved crash analysis.
- The missing Advertising ID declaration found by the quick check was completed
  as `No` before the submission was accepted.

## Remaining gate

1. Wait for Google to approve the closed-track changes and make the opt-in page
   active.
2. Invite the validated testers and recruit additional real volunteers through
   the public testing page, without fabricating accounts or sending unsolicited
   mail.
3. Confirm at least the Play-required number of testers remain opted in for the
   full required continuous period and collect meaningful use/feedback evidence.
4. Apply for production access only when Play Console enables that action.
5. Keep Phase 34K blocked until production access is granted and the owner
   explicitly authorizes the public rollout.
