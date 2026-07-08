# Phase 25 — Multiplayer movement sync scaffold

## Goal

Prepare player movement for Photon multiplayer without importing or hard-wiring
Photon Fusion classes yet.

This phase keeps local Play Mode behavior the same as Phase 24:

- JuanP is the local Taya.
- Maria, Pedro, and Ana are remote hider preview players.
- The local camera and joystick still target only the local Taya.

The new work is underneath: every spawned preview player now receives a movement
sync controller that separates local authority from remote replicas.

## What changed

- Added `PrototypeNetworkMovementAuthority`.
- Added `PrototypeNetworkMovementSnapshot`.
- Added `PrototypeNetworkMovementSyncController`.
- Updated `PrototypeNetworkPlayerSpawner` to attach/configure movement sync for
  every spawned player.
- Added EditMode tests for the movement sync contract, authority setup, snapshot
  sequence handling, and remote smoothing.
- Extended the player spawner tests so they verify:
  - local Taya has local movement authority;
  - remote hiders are remote replicas;
  - remote hider movement input is disabled.

## Why Photon is not directly referenced yet

Fusion SDK is not committed in the repo yet. Adding unconditional `Fusion.*`
references now would make the project fail to compile for anyone who has not
imported the SDK in Unity.

So this phase creates the adapter seam first:

1. Local player captures movement snapshots.
2. Remote players accept newer snapshots only.
3. Remote players smooth toward snapshot positions.
4. Phase 26+ can plug Photon transport into this layer.

## How to review

1. Pull the latest repo.
2. Open `/unity` in Unity.
3. Open `Assets/Scenes/PrototypeMap.unity`.
4. Press Play.
5. Confirm:
   - you are still local Taya;
   - the HUD shows `Hiders 3/3`;
   - only the Bang/name UI is shown for you;
   - the old green SAK base button does not appear;
   - movement still feels the same as Phase 24.
6. Optional inspector check:
   - select `JuanP Network Preview` and confirm
     `Prototype Network Movement Sync Controller` authority is
     `Local Authority`;
   - select `Maria Network Preview`, `Pedro Network Preview`, or
     `Ana Network Preview` and confirm authority is `Remote Replica`.

## Tests

Run Unity EditMode tests after the editor imports the project:

- `PrototypeNetworkMovementSyncControllerTests`
- `PrototypeNetworkPlayerSpawnerTests`

Expected result: all tests pass.

## Exit criteria

- The project still compiles without Photon Fusion installed.
- Local preview still works exactly like Phase 24.
- Local player movement can be captured as snapshots.
- Remote players can accept and smooth newer snapshots.
- Older remote snapshots are ignored.
