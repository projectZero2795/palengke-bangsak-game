# Phase 34E1: Android Photon create and join

## Goal

Verify only that two Android clients can create and join the same real Photon
Fusion Shared room and see the same stable roster before gameplay.

This phase does not add voluntary leave, remaining-room cleanup, Android
background/resume, reconnect, performance tuning, signing, or Play Console
work.

## What changed

The room panel now has a larger mobile-safe status card that shows:

- Photon SDK/provider and connection state;
- the active room code;
- the stable visible player roster;
- the local player's assigned roster name;
- the existing connection status message and player count.

The first client uses **CREATE**, which creates room `1234` in the EU region.
The second client uses **JOIN 1234**. For two players, both panels must show
`Players: JuanP · Maria`; client A shows `You: JuanP`, while client B shows
`You: Maria`.

## Owner review

1. Open Android build `0.34.3` on two Android devices or emulators.
2. On client A, tap **ROOM**, then **CREATE**.
3. Wait until it shows `Connected`, room `1234`, and `Players: JuanP`.
4. On client B, tap **ROOM**, then **JOIN 1234**.
5. Wait until both clients show `Players: JuanP · Maria` and `2/4 players`.
6. Confirm client A says `You: JuanP` and client B says `You: Maria`.
7. Stop there. Starting gameplay, leaving, backgrounding, or reconnecting is
   outside this phase.

## Automated checks

EditMode tests cover room-code normalization/validation and stable roster
formatting for zero through four players. The Android acceptance check records
the final two-client room panels and scans both clients for fatal errors.

Final implementation evidence:

- `226` EditMode tests passed with `0` failures;
- the final clean IL2CPP ARM64 Android debug build completed as version
  `0.34.3`;
- the APK supports API 29+, targets API 35, and is `52,917,986` bytes;
- APK SHA-256:
  `47e728ef84ba0cd7b59f0351d5814389ef8732867087a329da729e7d3b32aa6a`;
- two isolated Pixel 6 Android 15/API 35 emulator clients installed and
  launched the same APK;
- client A created EU room `1234` and client B joined it;
- both clients displayed `Players: JuanP · Maria` and `2/4 players`;
- client A displayed `You: JuanP`; client B displayed `You: Maria`;
- neither client recorded a fatal Bang-Sak Android exception;
- no Phase 34E2+ behavior was implemented or exercised.

## Stop boundary

Stop after the matching two-client roster is reviewed. Phase 34E2 Leave Game
confirmation remains blocked until explicit owner approval.
