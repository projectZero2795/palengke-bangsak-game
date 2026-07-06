# Bang-Sak for Palengke

Bang-Sak is a safe Filipino community hide-and-seek/tag game planned for Unity 2D WebGL at `games.palengke.es`.

The target experience is the reference image provided by the project owner: a polished 2D top-down barangay/palengke game with menu, lobby, round gameplay, result screen, leaderboard, mobile controls, and later Photon multiplayer.

Canonical visual reference: [Bang-Sak roadmap reference](docs/reference/bang-sak-roadmap-reference.jpg).

## Current phase

This repository is currently in **Phase 11: Houses**, ready for local residential-object review.

Phase 0 contains docs, architecture, roadmap, object catalog, maintenance rules, and GitHub issues.

Phase 1 added the Unity 2D project foundation.

Phase 2 added placeholder player sprites, a default player prefab, color-variant prefabs, and a non-gameplay preview row in `PrototypeMap`.

Phase 3 added Rigidbody2D/Collider2D movement to the default playable player prefab, keyboard movement, placeholder mobile joystick controls, and simple wall-collision test objects in `PrototypeMap`.

Phase 4 added a script-based player animation controller to the default playable player prefab. It swaps idle/walk frames, tracks 8-direction facing, and selects direction-specific placeholder sprites while moving.

Phase 5 added the approved safe Bang action design: a compact circular tsinelas button, cooldown, smooth red forward-cone range indicator, and non-damaging animated thrown-tsinelas marker effect.

Phase 6 added local Bang hit detection with safe feedback: range-limited circle-cast detection, wall blocking, hit/miss/block result tracking, target hit flash, and hittable practice-player prefabs.

Phase 7 added the harmless close-range Tag / Close Tap alternative: short-range detection, wall blocking, cooldown, TAG button, and friendly tag-hit flash feedback.

Phase 8 added local caught-state behavior for Bang and Tag hits: targets become caught, show a playful animated dizzy-star indicator, caught players lose movement/action controls until reset, and the prototype `Hiders Left` counter updates.

Phase 9 added the first ground foundation: richer 128px nighttime soil, road/path, grass, and concrete placeholder tiles, plus a runtime `Grid`/`Tilemap` builder in `PrototypeMap`. The approved review direction is a Filipino street-market ground style for nighttime Bang-Sak play, with a larger `36 x 26` soil-heavy play area, a configurable map seed, future random object-placement cells, and a wider prototype wall boundary.

Phase 10 added versioned Filipino/barangay natural-object placeholders: a coconut/banana-style tropical tree, bougainvillea-style flowering bush, potted tropical plant, and bamboo/banana-style plant cluster. A runtime `PrototypeNaturalObjectSpawner` places them on valid Phase 9 future object cells. Trees and pots are solid obstacles; bushes and plant clusters are trigger placeholders reserved for later hiding/occlusion/reveal mechanics.

Phase 11 adds versioned barangay residential placeholders: small warm-lit houses, medium concrete houses, wooden/bamboo fences, and closed gates. A runtime `PrototypeResidentialObjectSpawner` places them around the map edges with `BoxCollider2D` wall collisions while keeping center routes playable. Stores, base point, roles, round rules, Photon setup, WebGL builds, Docker files, Kubernetes manifests, and deployment files must wait for their specific phases.

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

- `Bang` = finger-gun marker, toy dart, cartoon light beam, foam tag, or tsinelas projectile;
- close-range action = `Tag`, `Close Tap`, or harmless foam-touch action;
- caught state = dizzy stars, playful freeze, waiting-zone state, or friendly HUD indicator.

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
