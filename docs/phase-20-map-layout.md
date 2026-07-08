# Phase 20: Map layout v1

## Goal

Create the first reviewable playable map contract without adding round rules yet.

This phase keeps gameplay local and simple. It defines where players can spawn,
what the first playable bounds are, and how the camera/play area should be
interpreted by future round-rule, reveal, and multiplayer phases.

## What changed

- Added `PrototypeMapLayoutController`, a versioned map-layout component.
- Added one Taya spawn point.
- Added six Hider spawn points around the nighttime barangay/palengke map.
- Added validation so spawn points must:
  - fit inside map bounds;
  - fit inside camera bounds;
  - keep Hiders separated from Taya;
  - provide at least four Hider spawn slots.
- Expanded the prototype map from `36 x 26` to `52 x 36` so the change is
  actually noticeable during play.
- Added `MapSpawnRole` and `PrototypeMapSpawnPoint` so future systems can ask
  the map for role-aware spawn data.
- Added the `Phase 20 Map Layout` root object to `PrototypeMap`.
- Added visible review spawn markers in Play mode:
  - warm red/orange marker for Taya;
  - cyan/blue markers for Hiders;
  - small `TAYA` / `H1..H6` labels for quick review.
- Added bounded camera follow to the main camera so the bigger map is playable
  instead of feeling like a fixed-screen arena.
- Widened the old Phase 3 wall boundary to match the larger map:
  `50 x 34` playable boundary inside the `52 x 36` map.

## Deliberately not included

- No round timer.
- No win/loss result screen.
- No scoring.
- No reveal/dog/streetlight behavior yet.
- No Photon or multiplayer.
- No final camera-follow polish.

Those belong to later phases after this map layout is approved.

## How to test in Unity

1. Pull the latest `main`.
2. Open the project in Unity Hub using the `/unity` folder.
3. Open `Assets/Scenes/PrototypeMap.unity`.
4. Confirm the scene has a root object named `Phase 20 Map Layout`.
5. Select `Phase 20 Map Layout` and confirm:
   - component id is `prototype_map_layout`;
   - component version is `1`;
   - variant is `night_barangay_palengke_v1`;
   - Taya spawn is around the lower center of the map;
   - Hider spawns are distributed around the map edges/routes;
   - map size is `52 x 36`;
   - camera bounds are `50 x 34`;
   - spawn marker review visuals are enabled.
6. Press Play.
7. Confirm you can see the colored review spawn markers.
8. Move the player around the map and confirm:
   - the camera follows the player;
   - the wall boundary feels much wider than before;
   - the player remains visible while exploring the larger map.
9. Run EditMode tests and confirm:
   - `PrototypeMapLayoutControllerTests`
   - `Phase20PrefabSceneTests`

## Exit criteria

- `PrototypeMap` has a configured versioned map layout component.
- Taya and Hider spawn points are available through code.
- Spawn points are visible during review.
- Spawn validation passes.
- The playable boundary matches the larger map.
- Camera follow keeps the larger map usable.
- The game still runs locally without adding round rules.
