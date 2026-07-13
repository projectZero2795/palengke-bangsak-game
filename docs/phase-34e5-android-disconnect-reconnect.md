# Phase 34E5: Android disconnect and reconnect

## Goal

Handle a real involuntary Photon disconnect on Android, return the affected
client to MainMenu without losing the room code, and allow an explicit manual
join to the same room. After reconnect, both clients must complete one agreed
round.

This phase does not add silent automatic reconnect, performance tuning,
signing, bundle generation, or Play Console work.

## Disconnect contract

- Intentional leave and connection-attempt failures keep their existing paths.
- Only an unexpected loss from an active room starts disconnect recovery.
- Duplicate Fusion shutdown/disconnect callbacks are collapsed into one
  recovery operation.
- The dead runner, gameplay bindings, authority credentials, and sequence
  state are cleaned up once.
- The disconnected room code is preserved while the client enters the failed
  state and returns to MainMenu.
- Rejoin remains an explicit player action through the existing room JOIN
  flow; there is no hidden retry or automatic room creation.
- The surviving room uses the Phase 34E3 last-player behavior and keeps its
  freed slot available for the reconnecting player.

## Acceptance review

| Criterion | Evidence | Result |
| --- | --- | --- |
| Detect a real involuntary Android disconnect | Emulator B's `eth0` interface was disabled during Round 1; Fusion emitted `ShutdownReason.Error` and Bang-Sak logged the unexpected loss in room `1234`. | Pass |
| Return safely to the room menu | The recovery path cleaned the dead runner and loaded MainMenu; Emulator A also returned to MainMenu as the sole connected survivor while keeping the room open. | Pass |
| Reconnect explicitly with the same room code | After the Android network stack was restored, Emulator B used JOIN `1234`; the room panel reported `Connected`, roster `JuanP · Maria`, and local player `Maria`. | Pass |
| Restore one coherent round | The room authority started a new Round 1 and both clients showed `HIDERS 1/1`. | Pass |
| Finish one agreed round | The real 2:30 timer expired naturally; both clients showed `00:00`, `DONE`, and `Hiders win! Time is up.` | Pass |
| Avoid duplicate or unsafe recovery | Focused tests cover intentional, duplicate, connecting, and empty-room exclusions; the live run performed one recovery and one manual rejoin. | Pass |
| No later-phase work leaked in | No performance-budget tuning, signing, bundle, store listing, or Play upload was added. | Pass |

## Verification evidence

- complete Unity EditMode suite: `235/235` passed, `0` failed;
- clean IL2CPP ARM64 Android debug build completed as version `0.34.7`;
- APK package: `es.palengke.bangsak`, minimum API 29, target API 35;
- APK size reported by Unity: `812718843` bytes;
- APK SHA-256:
  `1ad56ddbac2956de8dedbcde9b8d2a9fb71d5043d0e3b6272977eda796846338`;
- the APK installed on two Pixel 6 Android 15/API 35 emulators;
- both clients first joined room `1234` and entered Round 1;
- disabling Emulator B's network interface caused an actual Fusion shutdown,
  not a voluntary leave or short app pause;
- the emulator network stack required a reboot to restore Android's managed
  route, making the reconnect probe stricter than a brief link toggle;
- Emulator B manually joined room `1234` after recovery and restored the
  `JuanP · Maria` roster;
- both clients completed the full restarted timer with the same hider-win
  result;
- the final focused logs contained no fatal exception, null/argument
  exception, connection failure, synchronization timeout, resume-send
  failure, or integrity rejection.

## Next boundary

Phase 34F may measure and tune only the agreed Android performance budgets.
Signing, bundles, and Play Console work remain Phase 34G and later.
