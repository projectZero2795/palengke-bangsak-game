# Phase 34E3: Photon leave cleanup and room rules

## Goal

Apply only the remote room-state effects after the Phase 34E2 confirmed leave:
remove the leaver, compact the roster, free the room slot, and keep Taya,
authority, round, and last-player behavior deterministic.

Android pause/resume, involuntary disconnect/reconnect, performance tuning,
release signing, and Play Console work remain outside this phase.

## Deterministic rules

- Photon `ActivePlayers` is the roster source; departed `PlayerRef` values are
  not retained.
- The current Photon Shared master is always compact slot `0` and therefore
  Taya. Remaining real players are sorted by `PlayerRef` and occupy contiguous
  Hider slots.
- A roster change invalidates the previous authority credential and sequence.
  Clients wait up to 60 frames for Photon to expose the new master before
  rebinding gameplay.
- With two or more survivors, the new master restarts the local round, creates
  the new authority round ID, and broadcasts the authoritative snapshot.
- With one survivor, the new scene authority loads MainMenu while remaining in
  the open Photon room. The room can therefore accept a replacement manually.
- A one-player preview creates exactly one Taya descriptor; it does not create
  the former minimum second/ghost Hider.
- Rooms remain open with a maximum of four players, so a replacement can reuse
  any capacity released by a confirmed leave.

## Acceptance review

| Criterion | Evidence | Result |
| --- | --- | --- |
| Remove the voluntary leaver and update counts | All roster slots are rebuilt from current `ActivePlayers`; in the live room Maria left and JuanP immediately became the only `1/4` player. | Pass |
| No ghost player remains | The live survivor roster contained only JuanP; the spawner test also requires exactly one Taya descriptor for a one-player roster. | Pass |
| Taya and authority are deterministic | The current Shared master is moved to slot `0`; tests cover old-authority departure, promoted authority, and sole-survivor cases. | Pass |
| Remaining clients use one round state | After replacement rejoin, both live clients entered Round 1 with `HIDERS 1/1`; gameplay rebinding permits only the agreed master to broadcast the snapshot. | Pass |
| Freed capacity is reusable | Maria manually rejoined the same live room and both clients restored the compact `JuanP · Maria` `2/4` roster. | Pass |
| Last-player behavior is deterministic | JuanP returned to MainMenu still connected to room `1234`; the room status explicitly reported that it remained open for a replacement. | Pass |
| No later-phase work leaked in | No pause/resume, reconnect, performance, signing, bundle, or Play Console behavior changed. | Pass |

## Verification evidence

- complete Unity EditMode suite: `231/231` passed, `0` failed;
- clean IL2CPP ARM64 Android debug build completed as version `0.34.5`;
- APK package: `es.palengke.bangsak`, minimum API 29, target/compile API 35;
- APK size reported by Unity: `812447381` bytes;
- APK SHA-256:
  `968a1ec7dcf7b8665869fd6cad4038e38e4d51b2a2adf271989474d9fc07380d`;
- the final APK installed and launched on two Pixel 6 Android 15/API 35
  emulators, both reporting version `0.34.5`;
- neither emulator recorded a fatal Android exception, null/argument exception,
  or Bang-Sak Photon leave warning during the install/launch smoke check;
- a clean current-source WebGL validation build ran two independent live Photon
  clients in room `1234`;
- both clients began the synchronized round with roster `JuanP · Maria` and
  `HIDERS 1/1`;
- Maria confirmed **LEAVE GAME** and returned to MainMenu; JuanP also returned
  to MainMenu as the sole survivor while remaining connected as `1/4`;
- JuanP's room status contained only `JuanP` and explicitly kept room `1234`
  open for a replacement, proving there was no ghost Maria slot;
- Maria manually rejoined the freed slot; both clients showed
  `JuanP · Maria`, `2/4`, and then began a new synchronized Round 1 with
  `HIDERS 1/1`;
- browser console capture contained no errors or leave warnings on either
  client. Photon emitted only its expected startup warning that `StartGameArgs`
  initially carries no scene before the room authority loads gameplay.

## Next boundary

Phase 34E4 may change only Android pause, background, and resume behavior for a
joined room. Real disconnect/reconnect remains Phase 34E5.
