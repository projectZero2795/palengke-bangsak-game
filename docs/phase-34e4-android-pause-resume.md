# Phase 34E4: Android pause and resume

## Goal

Handle a short Android app switch while a player is still connected to a
Photon room. On resume, the client requests and applies one correlated
authoritative round snapshot before reporting synchronization complete.

This phase does not reconnect a client after Photon has actually disconnected
it. Android process suspension, network loss, and room-code reconnect belong
to Phase 34E5. Performance budgets, signing, and Play Console work also remain
outside this phase.

## Resume contract

- Android keeps Unity's background execution flag enabled and records the room
  that was active when the app paused.
- Resume reconciliation runs only when the client is still connected to that
  same room.
- A non-authority client sends a unique 32-character request ID to the current
  room authority.
- The authority validates the request and replies directly to that player with
  the current round snapshot and the same request ID.
- Periodic queued round snapshots may still be applied, but only the matching
  correlated response can complete the resume gate.
- Reliable movement snapshots are paced at 4 Hz and round snapshots at 1 Hz so
  a short pause does not place the resume response behind a growing queue.
- The client reports a clear timeout instead of falsely reporting success when
  the matching response does not arrive within 30 seconds.

## Acceptance review

| Criterion | Evidence | Result |
| --- | --- | --- |
| Background one connected Android client | Emulator B was sent to Android Home for five seconds while joined to room `1234`; its log recorded the connected pause. | Pass |
| Resume without changing rooms | Emulator B resumed still connected to room `1234`; neither client logged disconnect, shutdown, or connection failure. | Pass |
| Restore the authoritative round | Emulator A logged that it sent Round 1 with `99.8s`; Emulator B applied the matching correlated Round 1 response with exactly `99.8s`. | Pass |
| Both clients agree after resume | Final captures show both clients in Round 1 with `HIDERS 1/1`; their rendered timers differ by only the one-second capture interval. | Pass |
| Do not accept stale queued state as resume proof | The matching-request-ID rule has focused protocol coverage, and an earlier live probe correctly timed out instead of accepting an unrelated queued snapshot. | Pass |
| No later-phase work leaked in | No reconnect-after-disconnect, performance-budget tuning, signing, bundle, or Play Console behavior was added. | Pass |

## Verification evidence

- complete Unity EditMode suite: `234/234` passed, `0` failed;
- clean IL2CPP ARM64 Android debug build completed as version `0.34.6`;
- APK package: `es.palengke.bangsak`, minimum API 29, target API 35;
- APK size reported by Unity: `812664911` bytes;
- APK SHA-256:
  `9111f38b67f40ba9d65721b973c7224ff7acc19c9605e5e5073af349b06cb89e`;
- the final APK installed cleanly on two Pixel 6 Android 15/API 35
  emulators;
- both clients created/joined room `1234` and entered Round 1 with
  `HIDERS 1/1` before the pause;
- the resumed client applied the exact authority response in about 1.2 seconds;
- focused logs contained no fatal exception, null/argument exception,
  disconnect, shutdown, synchronization timeout, or resume-send failure.

## Review boundary

This proves the scoped short app-switch case while Photon still considers the
player connected. A longer background period that causes a real disconnect is
intentionally not claimed here and is the first Phase 34E5 case.

## Next boundary

Phase 34E5 may add only involuntary Android disconnect detection and manual
room-code reconnect, followed by one agreed completed round. Performance and
release work remain later phases.
