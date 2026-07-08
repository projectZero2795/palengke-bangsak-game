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
- Added `MapSpawnRole` and `PrototypeMapSpawnPoint` so future systems can ask
  the map for role-aware spawn data.
- Added the `Phase 20 Map Layout` root object to `PrototypeMap`.
- Widened the old Phase 3 wall boundary to match the current larger map:
  `34 x 24` playable boundary inside the `36 x 26` map.

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
   - map size is `36 x 26`;
   - camera bounds are `34 x 24`.
6. Press Play.
7. Move the player around the map and confirm the playable wall boundary feels
   wider than before.
8. Run EditMode tests and confirm:
   - `PrototypeMapLayoutControllerTests`
   - `Phase20PrefabSceneTests`

## Exit criteria

- `PrototypeMap` has a configured versioned map layout component.
- Taya and Hider spawn points are available through code.
- Spawn validation passes.
- The playable boundary matches the larger map.
- The game still runs locally without adding round rules.
