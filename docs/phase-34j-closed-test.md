# Phase 34J: Closed testing

## Current status

Phase 34J is in progress. Google rejected the first 2026-07-14 submission for
two reasons: the financial declaration described non-cash game coins as a
financial incentive, and version code `1` contained the Unity runtime affected
by CVE-2025-59489. Both issues were remediated. Google Play Console confirmed
that the replacement 16-change submission, including version code `2`, was sent
for review on 2026-07-14. The review can take up to seven days or longer, and
production access still depends on the required testing period and evidence.

No production rollout was created or authorized.

## Closed-track configuration

- Track: Closed testing / Alpha
- Release: `2 (0.34.9)`
- Version code: `2`
- Artifact: `unity/Build/Android/BangSak-0.34.9-cve-patched-vc2.aab`
- Artifact SHA-256:
  `6becadcab64fed42de481eb178656e2f511b1a296b31a0e8c0e0fffe5a07dcc0`
- Availability: 176 named countries/regions plus the rest of world
- Tester management: email list `Bang-Sak internal testers`
- Valid tester accounts in the list: 45
- Feedback address: `soporte@palengke.es`
- Opt-in URL: `https://play.google.com/apps/testing/es.palengke.bangsak`

Release notes:

> Security update for the first Bang-Sak closed test. Applies Unity's official
> CVE-2025-59489 runtime remediation. Includes local practice, private Photon
> multiplayer rooms, Android touch controls, accessibility settings, reconnect
> handling, and the Palengke leaderboard.

## Policy remediation

- Unity's official Application Patcher `1.3.3` patched the signed Phase 34G AAB
  and incremented its version code from `1` to `2`.
- The patcher reported `Patch successful`.
- Bundletool validation and `jarsigner` verification passed after patching.
- The upload certificate still matches the external upload keystore.
- The manifest still reports package `es.palengke.bangsak`, version name
  `0.34.9`, minimum API `29`, target API `35`, and only the approved `INTERNET`
  and `ACCESS_NETWORK_STATE` permissions.
- The vulnerable `xrsdk-pre-init-library` marker is absent from `libunity.so`;
  the remediated `8rsdk-pre-init-library` marker is present.
- Version code `2` is available to internal testers and replaced code `1` in
  the closed-track submission. Code `1` was not included in the replacement
  release.
- The financial features declaration now correctly states that the game has no
  financial features.

## Submission evidence

- Play Console accepted release `2 (0.34.9)` for a full closed-track rollout.
- The attached tester list contains 45 accounts; Play showed zero opted-in
  testers at submission time.
- The replacement release, worldwide availability, track resume, tester list,
  feedback address, store listing, and corrected compliance answers were sent
  for Google review. Play showed `16 changes sent for review` and then `Changes
  in review` while automated quick checks were running.
- Play reported only two non-blocking bundle recommendations: upload native
  debug symbols and a deobfuscation file for improved crash analysis.
- The missing Advertising ID declaration found by the quick check was completed
  as `No` before the submission was accepted.

## Remaining gate

1. Wait for Google to approve the closed-track changes and make the opt-in page
   active.
2. Invite the validated testers and recruit additional real volunteers through
   `https://palengke.es/bangsak-android-test`, without fabricating accounts or
   sending unsolicited mail. The page recruits up to 100 volunteers and is
   live; invitations must wait until the Play opt-in page becomes active.
3. Confirm at least the Play-required number of testers remain opted in for the
   full required continuous period and collect meaningful use/feedback evidence.
4. Apply for production access only when Play Console enables that action.
5. Keep Phase 34K blocked until production access is granted and the owner
   explicitly authorizes the public rollout.
