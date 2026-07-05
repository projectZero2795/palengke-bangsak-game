# Phase 8 test notes

Phase 8 adds the first local caught-state loop. It does not add roles, rounds, win conditions, Photon, or deployment.

## What to test in Unity

Open `Assets/Scenes/PrototypeMap.unity`, press Play, and use the default player.

### Bang catches a target

1. Face a colored practice player.
2. Press Space or the circular tsinelas Bang button.
3. Expected:
   - the target flashes;
   - the target changes into caught feedback;
   - a small `CAUGHT` label appears above the target;
   - the `Hiders Left` counter decreases.

### Tag catches a target

1. Move close to a colored practice player.
2. Press `E` or the `TAG` button.
3. Expected:
   - only close-range targets are caught;
   - walls still block the tag;
   - the same `CAUGHT` label and counter behavior appear.

### Caught state behavior

1. Catch a target.
2. Try catching the same target again.
3. Expected:
   - the target remains caught;
   - caught state is not applied twice;
   - counter does not keep decreasing.

## Scope notes

- This is still a local prototype.
- `Hiders Left` is a practice counter for catchable targets only.
- Real Taya/Hider role assignment waits for Phase 15.
- Round win/loss rules wait for Phase 16.
- No realistic weapons, knives, blood, gore, or lethal combat were added.

## Automated checks

Phase 8 adds edit-mode coverage for:

- caught state activation;
- caught state reset;
- movement/action disabling while caught;
- Bang-to-caught integration;
- Tag-to-caught integration;
- prefab wiring;
- practice counter updates.
