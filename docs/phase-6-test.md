# Phase 6 Test Notes

## Scope

Phase 6 makes the safe `Bang` action detect another local player target.

Included:

- `BangHitOutcome`
  - `Miss`;
  - `HitTarget`;
  - `Blocked`.
- `BangHitResult`
  - outcome;
  - target;
  - collider;
  - origin;
  - hit/miss/block point;
  - direction;
  - distance;
  - shot sequence id.
- `BangHitTarget`
  - marks an object as a valid safe Bang target;
  - records hit count/source/result;
  - flashes the target briefly as non-violent feedback;
  - ignores duplicate registrations from the same shot sequence.
- `BangActionController`
  - circle-cast based local hit detection;
  - configurable hit radius;
  - range limit;
  - own-collider ignore;
  - optional solid-wall blocking;
  - hit/miss/blocked result storage;
  - hit/miss/blocked impact color feedback;
  - tsinelas effect travels to the actual hit/block/miss point.
- Prefab updates:
  - default player prefab is a `BangHitTarget`;
  - color-variant practice player prefabs now have `CircleCollider2D` hitboxes and `BangHitTarget`.
- EditMode tests for:
  - target hit inside range;
  - miss outside range;
  - wall blocking;
  - no duplicate hit registration from one shot;
  - `TryBang` storing the last hit result;
  - prefab hit-target wiring.

Not included:

- caught state;
- disabling player movement after hit;
- Taya/Hider role restrictions;
- score;
- health/damage;
- projectile GameObject simulation;
- networking;
- Photon;
- WebGL build;
- Docker/Kubernetes/Argo files.

## Design note

Phase 6 uses a local `Physics2D.CircleCastAll` instead of a spawned projectile object. The visible tsinelas still flies forward, but hit detection is resolved once per shot. This keeps the phase small, deterministic, and easy to test. A true networked projectile can still be introduced later if multiplayer feel requires it.

## Safety note

This is still non-violent. A successful Bang is a playful tag hit:

- no realistic weapons;
- no damage;
- no blood/gore;
- no caught state yet;
- target feedback is a short friendly color flash.

## Manual Unity Editor checks

1. Pull latest `main`.
2. Fully close and reopen Unity so project settings and prefab changes reload.
3. Open `unity/` in Unity 2022.3.50f1 or newer.
4. Open `Assets/Scenes/PrototypeMap.unity`.
5. Press Play.
6. Face one of the nearby colored practice players.
7. Press `Space` or the circular Bang button.
8. Confirm the tsinelas travels toward the practice player.
9. Confirm the practice player flashes briefly when hit.
10. Stand too far away or face empty space.
11. Press Bang and confirm it produces miss feedback at the end of range.
12. Put/aim through the center obstacle or wall.
13. Confirm the shot is blocked before reaching a target behind the wall.
14. Confirm Bang still respects cooldown.
15. Confirm no player is removed, frozen, scored, or marked caught yet.
16. Confirm Console has no red errors.

## Exit criteria status

- Target in range is detected: ready for review.
- Target outside range is missed: ready for review.
- Solid wall before target blocks Bang: ready for review.
- One shot registers at most one target hit: ready for review.
- Hit/miss/block feedback is local and safe: ready for review.
- Caught state intentionally deferred to Phase 8.
- Phase 6 approval: pending project owner review.
