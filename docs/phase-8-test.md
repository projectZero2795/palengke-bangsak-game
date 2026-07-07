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
   - three small dizzy stars appear and orbit above the target;
   - the `Hiders Left` counter decreases.

### Historical Tag catch check

> Removed in Phase 16. The caught-state foundation remains, but the old separate
> TAG path no longer exists.

Before Phase 16, this checked that the old TAG experiment could mark a target
caught. The corrected rule replaces this later with a safe hider SAK counter.

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
- prefab wiring;
- practice counter updates.
- text-free animated caught indicator.
