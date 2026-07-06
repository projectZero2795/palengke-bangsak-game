# Phase 10 test notes: trees and natural objects

Phase 10 adds the first natural objects to the prototype map.

The goal is not final art yet. The goal is to prove that Filipino/barangay
natural objects can be generated, placed, versioned, rendered, and prepared for
future collisions/hiding/reveal systems.

## What changed

- Added deterministic placeholder sprites for:
  - coconut/banana-style tropical tree;
  - bougainvillea-style flowering bush;
  - potted tropical plant;
  - bamboo/banana-style plant cluster.
- Added `PrototypeNaturalObjectSpawner`.
- Added a new root object in `PrototypeMap` named `Phase 10 Natural Objects`.
- The spawner uses the Phase 9 ground tilemap's future object-placement cells.
- Trees and plant pots are solid obstacles.
- Bushes and plant clusters are trigger colliders for later hiding/occlusion
  mechanics.
- Added edit-mode tests for imported sprites, scene wiring, deterministic
  placement, renderers, and colliders.

## Important design note

Bang-Sak is played at night, so these placeholders use darker silhouettes and
warm highlights instead of bright daytime colors. They should be readable
against the Phase 9 nighttime soil/grass/concrete tiles.

These are also versioned placeholders:

- component id: `prototype_natural_object_spawner`
- component version: `1`
- component variant: `night_market_natural_placeholders`

This keeps the object system scalable. Later maps or rooms can choose a
different tree, plant, or reveal-object version without rewriting the gameplay
code.

## How to review in Unity

1. Open the project in Unity.
2. Open `Assets/Scenes/PrototypeMap.unity`.
3. Press Play.
4. Confirm that the map still loads without Console errors.
5. Look around the larger soil-heavy map.
6. Confirm that natural objects appear around the prototype map:
   - tropical trees;
   - flowering bushes;
   - potted plants;
   - plant clusters.
7. Move the player into a tree or plant pot.
   - Expected: the player should not pass through solid natural obstacles.
8. Move near bushes or plant clusters.
   - Expected: they should be visual placeholders now; future hiding/reveal
     behavior is not implemented yet.
9. Confirm the player, Bang cone, Tag range, caught-state stars, and ground
   tiles still look readable with the natural objects present.

## Automated checks

Run Unity edit-mode tests and confirm these pass:

- `PrototypeNaturalObjectSpawnerTests`
- `Phase10PrefabSceneTests`
- previous Phase 9 and movement/action tests

## Not included yet

These are intentionally left for later phases:

- real hiding zones;
- the 5-second hiding reveal rule;
- streetlight/house-light/dog-bark reveal sources;
- houses;
- stores/stalls;
- full map layout;
- role/round rules;
- multiplayer.

Do not start those until their phases are approved.
