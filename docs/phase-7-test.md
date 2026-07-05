# Phase 7 Test Notes

## Scope

Phase 7 adds the harmless close-range alternative to Bang: `Tag`.

Included:

- `TagHitOutcome`
  - `Miss`;
  - `HitTarget`;
  - `Blocked`.
- `TagHitResult`
  - outcome;
  - target;
  - collider;
  - origin;
  - hit/miss/block point;
  - direction;
  - distance;
  - shot sequence id.
- `TagHitTarget`
  - marks an object as a valid close-tag target;
  - records tag-hit count/source/result;
  - flashes the target briefly as friendly non-violent feedback;
  - ignores duplicate registrations from the same tag sequence.
- `TagActionController`
  - close-range circle-cast detection;
  - short range under 1 Unity unit;
  - solid-wall blocking;
  - cooldown;
  - keyboard trigger with `E`;
  - safe tap/pulse visual using the existing non-violent burst sprite.
- `TagActionHud`
  - runtime-created compact `TAG` button;
  - positioned left of the Bang button;
  - cooldown text while unavailable.
- Prefab updates:
  - default player prefab now has `TagActionController`;
  - default player prefab now has `TagActionHud`;
  - default player prefab is a `TagHitTarget`;
  - color-variant practice player prefabs are `TagHitTarget` objects.
- EditMode tests for:
  - close-range tag hit;
  - miss outside close range;
  - wall blocking;
  - cooldown spam prevention;
  - no duplicate hit registration from one tag;
  - prefab Tag controller/HUD/target wiring.

Not included:

- real knife mechanic;
- realistic weapon visuals;
- caught state;
- movement disabling;
- waiting area;
- role restrictions;
- scoring;
- networking;
- Photon;
- WebGL build;
- Docker/Kubernetes/Argo files.

## Important caught-state note

The original Phase 7 checklist says the target should receive a caught state, but Phase 8 is the dedicated caught-state phase. To avoid skipping ahead, Phase 7 records a local `TagHitTarget` event and friendly target flash only. Full `isCaught`, movement disable, waiting area, and HUD caught indicators must be implemented in Phase 8.

## Safety note

This is a harmless close tap:

- no knife;
- no stab animation;
- no damage;
- no blood/gore;
- no lethal combat;
- no caught-state gameplay yet.

## Manual Unity Editor checks

1. Pull latest `main`.
2. Fully close and reopen Unity so project settings and prefab changes reload.
3. Open `unity/` in Unity 2022.3.50f1 or newer.
4. Open `Assets/Scenes/PrototypeMap.unity`.
5. Press Play.
6. Confirm there is a compact `TAG` button near the Bang button.
7. Walk close to a colored practice player.
8. Face the practice player and press `E`, or click the `TAG` button.
9. Confirm the target flashes briefly.
10. Confirm Tag does not work when the target is farther away.
11. Confirm Tag is blocked by a wall/obstacle between the player and target.
12. Confirm Tag cooldown prevents spam.
13. Confirm the target is not frozen, removed, scored, or moved yet.
14. Confirm Console has no red errors.

## Exit criteria status

- Tag works only at close range: ready for review.
- Tag does not work through walls: ready for review.
- Tag cannot be spammed: ready for review.
- Target receives local tag-hit event/feedback: ready for review.
- Full caught state intentionally deferred to Phase 8.
- Phase 7 approval: pending project owner review.
