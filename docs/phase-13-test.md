# Phase 13 Test Notes

Phase 13 adds the first local SAK base mechanic.

## What changed

- Added a deterministic placeholder sprite generator:
  - `tools/generate_phase13_sak_base_assets.py`
- Added one SAK base sprite:
  - `Assets/Art/Placeholders/Base/sak_base_placeholder.png`
- Added versioned local base scripts:
  - `SakBaseController`
  - `SakBaseActor`
  - `SakAttemptOutcome`
  - `SakAttemptResult`
- Added `SakActionHud`, a small SAK button that appears only when the playable
  player is near an active base.
- Added `Phase 13 Sak Base` to `PrototypeMap`, placed near the bottom-center of
  the larger nighttime test map.
- Added edit-mode tests for the base mechanic, sprite import settings, player
  prefab wiring, and scene wiring.

## How to review in Unity

1. Pull the latest repository changes.
2. Open the Unity project from the `unity` folder.
3. Open `Assets/Scenes/PrototypeMap.unity`.
4. Press Play.
5. Move the playable red player toward the green base marker near the bottom
   center of the map.
6. Confirm:
   - the player can walk over/through the base trigger area;
   - the base does not block movement;
   - the SAK button is hidden while far away;
   - the SAK button appears only when the player is close enough;
   - clicking SAK, or pressing `R`, triggers a brief green pulse on the base;
   - leaving the base area hides the SAK button again.

## Expected behavior

- SAK is local-only in this phase.
- The base uses a trigger collider, not a wall collider.
- The playable player can use SAK because it has `SakBaseActor.canUseSak = true`.
- This is a mechanic foundation only: no Taya/Hider role enforcement, round win
  condition, scoring, multiplayer sync, or Palengke API call is added yet.

## Edit-mode tests added

- `SakBaseControllerTests`
- `Phase13PrefabSceneTests`

## Not included in Phase 13

- Role system.
- Round rules.
- Base win result.
- Spawn points.
- Multiplayer SAK sync.
- Palengke points/rewards.
