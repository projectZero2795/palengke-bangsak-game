# Phase 24: Multiplayer player spawning scaffold

## Goal

Prepare Bang-Sak player spawning for future Photon ownership while keeping the current local prototype playable.

This phase does not connect to Photon yet. It creates a local network-style spawn preview that uses the same concepts we will need later:

- player ID
- display name
- role
- spawn slot
- local ownership
- remote non-owned players

## What changed

- Added `PrototypeNetworkPlayerDescriptor`.
- Added `PrototypeNetworkPlayerIdentity`.
- Added `PrototypeNetworkPlayerSpawner`.
- Added a `Phase 24 Network Player Spawner` object to `PrototypeMap`.
- The spawner builds a 4-player preview roster:
  - `JuanP` as local `Taya`
  - `Maria` as `Hider`
  - `Pedro` as `Hider`
  - `Ana` as `Hider`
- The local spawned player keeps keyboard/joystick control.
- Remote preview players do not read keyboard input.
- The camera follows the local spawned player.
- The mobile joystick targets the local spawned player.
- The old single playable Phase 3 player is disabled at runtime so round rules use the spawned roster.

## Why this is still safe before Photon

Photon Fusion is still not imported, so this phase avoids direct Fusion dependencies.

The future Fusion adapter should instantiate or bind network-owned actors through this same identity/spawn contract instead of replacing gameplay components.

## How to review in Unity

1. Pull the latest repo.
2. Open `unity/Assets/Scenes/PrototypeMap.unity`.
3. Press Play.
4. Confirm you see multiple players spawned around the map.
5. Confirm keyboard/joystick controls only the local player.
6. Confirm the camera follows the local player.
7. Confirm Taya/Hider roles still behave:
   - local Taya has Bang target buttons.
   - hiders can be caught.
   - round timer/counter still works.

## Expected result

- One local owned actor is controlled by the user.
- Other spawned preview actors are visible but not keyboard controlled.
- No duplicate old Phase 3 playable actor should affect the round.
- The round still starts and tracks spawned hiders.

## Test coverage

- `PrototypeNetworkPlayerSpawnerTests`
- `Phase24PrefabSceneTests`
- `PlayerMovementControllerTests`

## Exit status

Ready for review as a local network-spawn scaffold.

Real Photon player spawning remains for the next adapter step after Fusion SDK import.
