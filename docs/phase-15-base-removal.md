# Phase 15 SAK Base Removal

Phase 15 removes only the incorrect Phase 13 SAK base path.

The corrected Bang-Sak rule has no base: Taya catches with `Bang + player name`,
and hiders later get a safe close-range `SAK` counter. This phase does not build
that new SAK counter yet.

## What changed

- Removed the Phase 13 base sprite folder:
  - `Assets/Art/Placeholders/Base`
- Removed the Phase 13 base generator:
  - `tools/generate_phase13_sak_base_assets.py`
- Removed base-only scripts:
  - `SakBaseController`
  - `SakBaseActor`
  - `SakAttemptOutcome`
  - `SakAttemptResult`
  - `SakActionHud`
- Removed the `Phase 13 Sak Base` object from `PrototypeMap`.
- Removed Phase 13 base-only edit-mode tests.
- Added `Phase15BaseRemovalTests` to guard against the old base returning.

## Intentionally not changed

- Bang action.
- Bang hit detection.
- Player movement and animation.
- Caught-state foundation.
- The old separate TAG experiment.

TAG is intentionally left in place so Phase 16 can remove it separately and be
easy to review.

## How to review in Unity

1. Pull the latest repository changes.
2. Open the Unity project from the `unity` folder.
3. Open `Assets/Scenes/PrototypeMap.unity`.
4. Confirm there is no root object named `Phase 13 Sak Base`.
5. Press Play.
6. Confirm:
   - no SAK base marker appears near the bottom-center of the map;
   - no SAK base button appears;
   - the player still moves smoothly;
   - Bang still shows the red cone and tsinelas marker;
   - the old TAG button still exists for now.

## Edit-mode tests added

- `Phase15BaseRemovalTests`

## Not included in Phase 15

- Removing TAG.
- Adding Taya/Hider roles.
- Adding `Bang + player name`.
- Adding the corrected hider SAK counter.
- Round rules.
- Multiplayer.
