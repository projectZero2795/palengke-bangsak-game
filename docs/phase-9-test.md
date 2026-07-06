# Phase 9 test notes

Phase 9 adds the first ground foundation for `PrototypeMap`.

## What changed

- Added deterministic placeholder sprites for:
  - soil;
  - road/path;
  - grass;
  - concrete.
- Added `PrototypeGroundTilemap`, a small runtime builder that creates:
  - `Grid`;
  - `Tilemap`;
  - `TilemapRenderer`.
- Added a `Phase 9 Ground Tilemap` root object in `PrototypeMap`.
- Ground renders below players by using a negative sorting order.

## What to test in Unity

1. Open `Assets/Scenes/PrototypeMap.unity`.
2. Press Play.
3. Expected:
   - a tiled ground foundation appears;
   - it contains soil, paths, grass patches, and concrete corners;
   - the player and practice targets render above the ground;
   - movement, Bang, Tag, and caught-state behavior still work.

## Scope notes

- This is still placeholder art.
- Object collisions, houses, trees, and hiding objects are intentionally not part of Phase 9.
- Phase 10 starts natural objects.
