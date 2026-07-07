# Phase 13 Test Notes

> Superseded by Phase 14 and removed in Phase 15: the corrected Bang-Sak rule
> has no base. This file is kept only as historical context for why the base
> prototype existed.

Phase 13 added the first local SAK base mechanic. That mechanic is now retired.

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

## Historical review steps

These steps were only valid before Phase 15 removed the base:

- move the playable red player toward the green base marker near the bottom
  center of the map;
- confirm the base did not block movement;
- confirm the SAK button appeared only while close to the base;
- confirm clicking SAK, or pressing `R`, triggered a brief green pulse.

## Expected behavior

- SAK is local-only in this phase.
- The base uses a trigger collider, not a wall collider.
- The playable player could use SAK because it had `SakBaseActor.canUseSak = true`.
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
