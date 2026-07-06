# Phase 9 test notes

Phase 9 adds the first ground foundation for `PrototypeMap`.

The first review pass replaced the very basic tiles with the approved
bottom-center direction: warmer street-market ground, painted accents, and a
larger soil-heavy play area so future hiding objects have room to breathe.

## What changed

- Added deterministic 128px placeholder sprites for:
  - soil;
  - road/path;
  - grass;
  - concrete.
- Enlarged the default ground from `16 x 12` tiles to `36 x 26` tiles.
- Added a deterministic map seed (`2795`) so the ground layout can be changed
  or regenerated without hand-painting the whole map.
- Added future object-placement cells. They do not spawn trees/stalls/houses
  yet, but Phase 10+ can use them to place objects randomly while avoiding
  roads and map edges.
- Enlarged the prototype wall boundary so the player can move around a bigger
  play space instead of being trapped in the old tiny test box.
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
   - a larger tiled ground foundation appears;
   - soil is the dominant playable surface;
   - it contains colorful market-style paths, grass patches, and concrete pads;
   - the player and practice targets render above the ground;
   - the prototype wall boundary gives the player much more room to move;
   - in the Inspector, `Phase 9 Ground Tilemap` exposes `Map Size` and
     `Map Seed`, which can later be used to generate bigger/different maps;
   - movement, Bang, Tag, and caught-state behavior still work.

## Scope notes

- This is still placeholder art, but it now follows the chosen warmer
  street-market visual direction.
- Random object placement is prepared, but actual object spawning, object
  collisions, houses, trees, and hiding objects are intentionally not part of
  Phase 9.
- Phase 10 starts natural objects.
