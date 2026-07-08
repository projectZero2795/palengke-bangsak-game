# Bang-Sak for Palengke

Bang-Sak is a safe Filipino community hide-and-seek game planned for Unity 2D WebGL at `games.palengke.es`.

The target experience is the reference image provided by the project owner: a polished 2D top-down barangay/palengke game with menu, lobby, round gameplay, result screen, leaderboard, mobile controls, and later Photon multiplayer.

Canonical visual reference: [Bang-Sak roadmap reference](docs/reference/bang-sak-roadmap-reference.jpg).

## Current phase

This repository is currently in **Phase 20: Map layout v1**, ready for review before round win/loss rules are added.

Phase 0 contains docs, architecture, roadmap, object catalog, maintenance rules, and GitHub issues.

Phase 1 added the Unity 2D project foundation.

Phase 2 added placeholder player sprites, a default player prefab, color-variant prefabs, and a non-gameplay preview row in `PrototypeMap`.

Phase 3 added Rigidbody2D/Collider2D movement to the default playable player prefab, keyboard movement, placeholder mobile joystick controls, and simple wall-collision test objects in `PrototypeMap`.

Phase 4 added a script-based player animation controller to the default playable player prefab. It swaps idle/walk frames, tracks 8-direction facing, and selects direction-specific placeholder sprites while moving.

Phase 5 added the approved safe Bang action design: a compact circular tsinelas button, cooldown, smooth red forward-cone range indicator, and non-damaging animated thrown-tsinelas marker effect.

Phase 6 added local Bang hit detection with safe feedback: range-limited circle-cast detection, wall blocking, hit/miss/block result tracking, target hit flash, and hittable practice-player prefabs.

Phase 7 added an experimental separate TAG action. The corrected rule says SAK is the hider counter, not a separate TAG mechanic, so this experiment was removed in Phase 16.

Phase 8 added local caught-state behavior. Bang targets become caught, show a playful animated dizzy-star indicator, caught players lose movement/action controls until reset, and the prototype `Hiders Left` counter updates. The caught-state foundation stays for the corrected Bang/SAK rule.

Phase 9 added the first ground foundation: richer 128px nighttime soil, road/path, grass, and concrete placeholder tiles, plus a runtime `Grid`/`Tilemap` builder in `PrototypeMap`. The approved review direction is a Filipino street-market ground style for nighttime Bang-Sak play, with a larger `36 x 26` soil-heavy play area, a configurable map seed, future random object-placement cells, and a wider prototype wall boundary.

Phase 10 added versioned Filipino/barangay natural-object placeholders: a coconut/banana-style tropical tree, bougainvillea-style flowering bush, potted tropical plant, and bamboo/banana-style plant cluster. A runtime `PrototypeNaturalObjectSpawner` places them on valid Phase 9 future object cells. Trees and pots are solid obstacles; bushes and plant clusters are trigger placeholders reserved for later hiding/occlusion/reveal mechanics.

Phase 11 added versioned barangay residential placeholders: small warm-lit houses, medium concrete houses, wooden/bamboo fences, and closed gates. A runtime `PrototypeResidentialObjectSpawner` places them around the map edges with `BoxCollider2D` wall collisions while keeping center routes playable.

Phase 12 added versioned Filipino marketplace placeholders: sari-sari store, palengke stall, food stall, SARI signboard, and crates/baskets. A runtime `PrototypeStoreObjectSpawner` places them around the existing market areas with tight `BoxCollider2D` wall collisions while avoiding road cells.

Phase 13 added a first local SAK base based on an incorrect rule assumption. The corrected Bang-Sak rule has no base, so that work was marked for removal.

Phase 14 documents the corrected rule: Taya finds hiders and calls `Bang + player name`; hiders do not run to a base; hiders can use `SAK` as a close-range counter against Taya. Unity code, scenes, prefabs, and assets are intentionally unchanged in this phase.

Phase 15 removes only the incorrect base path from Phase 13: the base sprite, scene base object, base scripts, base HUD, base generator, and base-specific tests. Bang, movement, caught state, and the old separate TAG experiment are intentionally preserved so Phase 16 can remove TAG separately.

Phase 16 removes only the old separate TAG path: TAG scripts, TAG HUD, TAG prefab components, and TAG-specific tests. Bang, movement, caught state, map objects, and hider target prefabs are preserved. The corrected safe hider SAK counter is still not implemented yet; that comes in a later phase after roles and named Bang behavior.

Phase 17 adds the first local role system: `Taya` and `Hider`, role-aware hider counting, and Taya-only Bang availability. Roles are gameplay-only for now, without floating text labels above players: Taya can throw tsinelas, and Hiders will get SAK later. The default playable player is configured as Taya, and the color-variant practice players are configured as Hiders. This phase intentionally does not add `Bang + player name`, the corrected SAK counter, round rules, or multiplayer.

Phase 18 adds the corrected local `Bang + player name` rule. The Taya player now has compact per-person Bang buttons with a tsinelas icon and prototype hider names, such as `Maria`. Clicking a person button calls that name and throws immediately. A Bang only catches when the clicked/called name matches the hider that was hit; hitting the wrong named hider gives wrong-name feedback and does not mark the hider caught. This phase intentionally does not add the hider SAK counter, round win/loss rules, or multiplayer.

Phase 19 adds the first local safe hider `SAK` counter. Hiders get a close-range, cooldown-limited SAK action that can counter Taya only when Taya is nearby and not behind a wall. The feedback is a playful cartoon burst/stun tint, not a weapon, blood, gore, or lethal combat. Round win/loss rules, scoring, multiplayer, and final UI polish remain out of scope.

Phase 20 adds the first reviewable playable map layout contract. `PrototypeMap` now has a versioned `Phase 20 Map Layout` component with Taya and Hider spawn points, map/camera bounds, validation helpers, and a widened `34 x 24` wall boundary inside the `36 x 26` nighttime barangay/palengke map. This phase intentionally does not add round rules, win/loss screens, scoring, reveal behavior, or multiplayer.

## Safety and branding rules

Bang-Sak must stay casual, playful, and community-safe.

The default mood is nighttime barangay/palengke play: cool shadows, readable
characters, and warm community light sources instead of a bright daytime arena.

Do not implement:

- realistic guns;
- knives;
- blood;
- gore;
- lethal combat;
- violent hit reactions.

Use safe visual metaphors instead:

- `Bang` = Taya's spoken `Bang + player name` call plus a safe marker effect;
- `SAK` = hider close-range counter against Taya, represented with a harmless cartoon `SAK!` / surprise-tap / foam-mark effect;
- caught state = dizzy stars, playful freeze, waiting-zone state, or friendly HUD indicator.

The real-world wording of SAK must not become a realistic weapon mechanic in
the Palengke game. Do not show realistic knives, killing, blood, gore, or
lethal combat.

## Target stack

- Game engine: Unity 2D.
- Target build: WebGL first.
- Public host later: `games.palengke.es`.
- Multiplayer later: Photon Fusion 2.
- Palengke integration later: login, points, coins, leaderboard, badges.

## Phase rule

Work on one phase only at a time.

After each phase:

1. Commit changes.
2. Push to GitHub.
3. Update docs if needed.
4. Create or update GitHub issues.
5. Explain how to test.
6. Stop and wait for review before continuing.

Do not skip ahead.

## Documentation

- [Architecture](docs/architecture.md)
- [Development roadmap](docs/roadmap.md)
- [Gameplay rules](docs/gameplay-rules.md)
- [Object catalog from the reference image](docs/object-catalog.md)
- [Object/component design, implementation, and versioning](docs/object-design-versioning.md)
- [Maintenance plan](docs/maintenance.md)
- [Phase 0 review checklist](docs/phase-0-review.md)
- [Phase 1 test notes](docs/phase-1-test.md)
- [Phase 2 test notes](docs/phase-2-test.md)
- [Phase 3 test notes](docs/phase-3-test.md)
- [Phase 4 test notes](docs/phase-4-test.md)
- [Phase 5 test notes](docs/phase-5-test.md)
- [Phase 6 test notes](docs/phase-6-test.md)
- [Phase 7 test notes](docs/phase-7-test.md)
- [Phase 8 test notes](docs/phase-8-test.md)
- [Phase 9 test notes](docs/phase-9-test.md)
- [Phase 10 test notes](docs/phase-10-test.md)
- [Phase 11 test notes](docs/phase-11-test.md)
- [Phase 12 test notes](docs/phase-12-test.md)
- [Phase 13 test notes](docs/phase-13-test.md)
- [Phase 14 rules documentation correction](docs/phase-14-rules-correction.md)
- [Phase 15 SAK base removal](docs/phase-15-base-removal.md)
- [Phase 16 TAG removal](docs/phase-16-tag-removal.md)
- [Phase 17 role system](docs/phase-17-role-system.md)
- [Phase 18 corrected Bang-name rule](docs/phase-18-named-bang.md)
- [Phase 19 safe SAK counter](docs/phase-19-sak-counter.md)
- [Phase 20 map layout v1](docs/phase-20-map-layout.md)
