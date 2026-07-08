# Phase 26 — Multiplayer Bang/SAK sync scaffold

## Goal

Prepare Bang-name catches and SAK counters for multiplayer without requiring the
Photon Fusion SDK to be imported yet.

This phase does **not** change the local gameplay review flow. When you press
Play in `PrototypeMap`, JuanP should still be the local Taya and the controls
should feel the same as Phase 25.

The new work is a network action-event layer that Photon can carry later.

## What changed

- Added `PrototypeNetworkActionKind`.
- Added `PrototypeNetworkActionOutcome`.
- Added `PrototypeNetworkActionEvent`.
- Added `PrototypeNetworkActionSyncController`.
- Updated `PrototypeNetworkPlayerSpawner` so every spawned preview player gets
  action sync configured with its network identity.
- Added EditMode tests for:
  - component contract;
  - local Bang-name event capture;
  - local SAK event capture;
  - remote Bang-hit application;
  - remote SAK-counter application;
  - duplicate remote action rejection.

## Event model

Each action event carries:

- action kind: Bang-name call or SAK counter;
- outcome: hit, name mismatch, miss, blocked, countered, wrong role;
- actor network id;
- target network id when applicable;
- called name and target display name;
- origin, point, direction, facing direction;
- sequence number;
- sent timestamp.

This gives Photon a compact message shape later while keeping local validation
inside the existing Bang/SAK controllers.

## Why Fusion is not referenced directly yet

The repo still compiles without Photon Fusion installed. Phase 26 creates the
adapter seam only. Later, when Fusion is imported, the adapter can send these
events over RPC/input/state authority without rewriting gameplay rules.

## How to review

1. Pull the latest repo.
2. Open `/unity` in Unity.
3. Open `Assets/Scenes/PrototypeMap.unity`.
4. Press Play.
5. Confirm the local preview still works:
   - you are local Taya;
   - HUD shows `Hiders 3/3`;
   - Bang/name buttons still catch hiders;
   - Hider SAK behavior remains unchanged when testing hider mode later;
   - no console compile errors.
6. Optional inspector check:
   - select any generated network player;
   - confirm it has `Prototype Network Action Sync Controller`.

## Tests

Run Unity EditMode tests after import:

- `PrototypeNetworkActionSyncControllerTests`
- `PrototypeNetworkPlayerSpawnerTests`

Expected result: all tests pass.

## Exit criteria

- Project compiles without Photon Fusion installed.
- Spawned players receive an action sync controller.
- Local Bang/SAK results can be represented as compact network events.
- Remote Bang/SAK events can apply the same caught/countered state.
- Duplicate remote events are ignored.
