# Bang-Sak Object and Component Design, Implementation, and Versioning

Bang-Sak maps and gameplay should eventually be created from JSON/YAML
configuration. To make that safe, every gameplay object and every reusable
component needs a stable contract, not only art or code.

This applies to normal objects such as trees, houses, stalls, and crates, and to
stealth/reveal objects such as streetlights, house lights, and dog-bark clues.
It also applies to systems/components such as player movement, Bang/Tag actions,
hide/reveal logic, HUD widgets, map generation, spawn rules, networking adapters,
and Palengke API adapters.

## Component-first rule

Every reusable piece should be treated as a component.

Examples:

- `player_movement`
- `player_animation`
- `bang_action`
- `tag_action`
- `caught_state`
- `ground_generator`
- `reveal_source`
- `round_timer`
- `sak_base`
- `mobile_joystick`
- `role_badge_hud`
- `photon_room_adapter`
- `palengke_leaderboard_adapter`

Each component should be:

- scalable;
- configurable;
- versioned;
- replaceable;
- selectable by admins or, where appropriate, players/rooms;
- testable;
- safe to reference from future JSON/YAML maps or registries.

Avoid hardcoding behavior directly into scene objects when the behavior should
be reusable by later maps, variants, multiplayer, or live tuning.

## Object/component lifecycle

Each object or component should move through these stages.

### 1. Design

Define what the object/component is supposed to do before creating the prefab,
script, registry entry, or config.

Design fields:

- `componentId` or `objectId`: stable machine name, for example
  `streetlight_reveal`, `player_movement`, or `bang_action`.
- `displayName`: human-readable name, for example `Streetlight reveal`.
- `phase`: first implementation phase.
- `category`: `ground`, `obstacle`, `hiding`, `reveal_source`, `base`, `ui`,
  `movement`, `action`, `round_rule`, `network`, `api`, etc.
- `visualRole`: what the player should understand at a glance.
- `gameplayRole`: blocker, occluder, reveal clue, decoration, trigger, spawn, etc.
- `nightReadability`: how it stays visible in nighttime play.
- `mobileReadability`: how it stays readable on small screens.
- `safeBranding`: why it is friendly/community-safe.
- `scalingRule`: how more variants/instances can be added later.

### 2. Implementation

Define how the object/component behaves in Unity.

Implementation fields:

- `prefabPath`: expected Unity prefab path, if it has a prefab.
- `scriptPath`: expected script path, if it is code-only.
- `spritePath` or `spriteFolder`: source art location, if visual.
- `sortingLayer` / `sortingOrder`: visual ordering.
- `collider`: none, solid, trigger, or occlusion-only.
- `physics`: static, dynamic, trigger-only, or visual-only.
- `scripts`: required MonoBehaviours.
- `config`: tunable values such as radius, duration, cooldown, or reveal type.
- `events`: emitted/listened events, such as `RevealEvent` or `CaughtEvent`.
- `dependencies`: other components required.
- `testRequirements`: edit-mode or play-mode checks needed before approval.

### 3. Version and variants

Maps and registries should reference stable component/object IDs plus
versions/variants. This lets us add new art or behavior later without breaking
old maps.

Version fields:

- `version`: behavior contract version, for example `1`.
- `variant`: visual/content/behavior flavor, for example `warm_pole`,
  `blue_window`, `brown_dog`, `slow_walk`, or `compact_hud`.
- `assetVersion`: art revision, for example `placeholder_1`, `painted_2`.
- `compatibleWith`: older behavior versions this version can replace safely.
- `migrationNotes`: what changed if a map needs migration.

Rules:

- Use `version` when behavior changes.
- Use `variant` when only the visual/content flavor changes, or when a
  behavior variant intentionally shares the same contract.
- Do not rename `objectId` or `componentId` after maps/configs start using it.
- Do not remove old versions until no map references them.
- Placeholder art can be `assetVersion: placeholder_1`; polished art can replace
  it later without changing gameplay behavior.
- Every new feature should answer: what is the stable ID, what is the version,
  what can be configured, and how can it be replaced later?

## Selection model

The long-term goal is that admins, rooms, or players can choose which component
versions/variants are used without rewriting code.

Examples:

- admins choose the default game mode, map style, reveal-source set, or player
  skill rules;
- players vote for a map that uses different ground/object/reveal components;
- a room chooses `classic_taya` or `speed_taya`;
- a map chooses `night_market` ground plus `light` and `animal_sound`
  RevealSource categories;
- future events can temporarily use seasonal object variants.

Selection should happen through registries and config, not hardcoded scene
changes.

### Selection layers

When multiple places define a component, resolve it in this order:

1. hard safety rules that cannot be overridden;
2. production compatibility rules;
3. room-specific selected config;
4. player vote result;
5. admin default config;
6. map default config;
7. component registry default.

### Compatibility rules

Selectable components must declare compatibility:

- `requires`: other component IDs/versions needed;
- `conflictsWith`: components that cannot be active together;
- `allowedModes`: game modes where it can be used;
- `allowedRoles`: roles that can use it, for example `taya` or `hider`;
- `minGameVersion`: minimum game client version;
- `networkVersion`: network contract version, if synchronized online.

If a selected component is incompatible, the game should fall back to the
nearest safe default and log/report the mismatch.

## Admin/player selectable examples

```yaml
gameModes:
  classic:
    version: 1
    components:
      player_movement:
        version: 1
        variant: quiet_walk_fast_run
      taya_role:
        version: 1
        variant: classic_taya
      reveal_rules:
        version: 1
        variant: five_second_hide

maps:
  night_market_v1:
    version: 1
    components:
      ground_generator:
        version: 1
        variant: night_market
      reveal_source_set:
        version: 1
        variants:
          - light
          - animal_sound
          - movement_noise

roomSelection:
  mapVote:
    enabled: true
    candidates:
      - night_market_v1
      - barangay_houses_v1
  componentOverrides:
    taya_role:
      version: 1
      variant: classic_taya
```

Rules:

- Admins can set safe defaults.
- Players can vote only from allowed/compatible choices.
- Player-selected cosmetics can be broader, but gameplay-affecting components
  must be controlled by the room/map/admin rules.
- Do not let a client silently choose a gameplay-affecting networked component
  that other clients do not know about.

## Global component categories

Use these categories as the first registry vocabulary. Add more only when the
existing categories do not fit.

| Category | Examples | Must be scalable by |
| --- | --- | --- |
| `movement` | `player_movement`, walk/run tuning, joystick input | speed profiles, input providers, stamina/noise variants |
| `animation` | `player_animation`, caught stars, action effects | direction sets, frame sets, sprite variants |
| `action` | `bang_action`, `tag_action`, `sak_action` | cooldown, range, visual marker, role restrictions |
| `round_rule` | hide timeout, hiders-left, win rules, timer | tuning values, enabled/disabled rule modules |
| `reveal_source` | streetlight, dog bark, house light, running noise | source type, reveal mode, trigger, variant |
| `map_object` | tree, bush, house, stall, crate | version, variant, collider profile, placement rules |
| `map_generator` | ground generator, object placer, spawn placer | map seed, size, biome/theme, placement weights |
| `ui` | role badge, timer, joystick, buttons, result panel | layout profile, mobile/desktop variant, theme |
| `audio` | bark, footsteps, lamp hum, UI clicks | cue ID, volume group, variant |
| `network` | Photon room adapter, sync components | protocol version, prediction/sync mode |
| `api` | Palengke user, leaderboard, rewards | API version, mock/real adapter, retry policy |

## Example component registry

```yaml
components:
  player_movement:
    category: movement
    version: 1
    variants:
      - default
      - quiet_walk_fast_run
    defaults:
      walkSpeed: 2.4
      runSpeed: 4.2
      runningRevealEnabled: true
      runningRevealCadenceSeconds: 0.8
    selectableBy:
      - admin
      - room
    compatibility:
      allowedModes:
        - classic
      networkVersion: 1

  bang_action:
    category: action
    version: 1
    variants:
      - tsinelas_marker
    defaults:
      range: 4.5
      cooldownSeconds: 1.2
      safeVisualOnly: true
    selectableBy:
      - admin
      - room

  ground_generator:
    category: map_generator
    version: 1
    variants:
      - night_market
    defaults:
      mapSize:
        x: 36
        y: 26
      seed: 2795
      futureObjectPlacementEnabled: true
    selectableBy:
      - admin
      - map_vote

  taya_role:
    category: role
    version: 1
    variants:
      - classic_taya
      - fast_taya_no_extra_power
    defaults:
      canBang: true
      canTag: true
      safeVisualOnly: true
    selectableBy:
      - admin
      - room
    compatibility:
      allowedRoles:
        - taya
```

## Example YAML map object entry

```yaml
objects:
  - objectId: streetlight_reveal
    version: 1
    variant: warm_pole
    position:
      x: 12
      y: 8
    rotation: 0
    config:
      revealRadius: 5
      revealDurationSeconds: 1.5
      clueType: light_sweep

  - objectId: dog_bark_reveal
    version: 1
    variant: brown_aspin
    position:
      x: 4
      y: 16
    config:
      barkRadius: 8
      directionOnly: true
      cooldownSeconds: 6
```

## Required reveal objects

These objects support the stealth/reveal rule documented in
[Gameplay Rules](gameplay-rules.md).

Reveal objects must be scalable. They should all belong to a shared
`RevealSource` category with subtype-specific presentation.

### RevealSource category

All reveal-source objects should share the same core implementation contract:

- `category`: `reveal_source`
- `revealSourceType`: stable subtype such as `light`, `sound`, `animal`,
  `environment`, or `movement_noise`
- `revealMode`: `direction_only`, `area_hint`, `brief_position`,
  or `line_sweep`
- `trigger`: `hide_timeout`, `running`, `manual_round_event`, or
  `proximity`
- `durationSeconds`
- `cooldownSeconds`
- `radius`
- `exactPositionAllowed`: usually `false`; prefer direction/area clues
- `mobileCue`: how it appears on small screens
- `audioCue`: optional sound ID
- `visualCue`: optional light/sprite/effect ID

The round-rule code in Phase 16 should depend on the shared category/contract,
not on hardcoded object IDs. That way we can add more reveal objects later by
registering new `RevealSource` variants.

### RevealSource subtypes

| RevealSource type | Example object IDs | Phase | Design intent | Implementation contract | Versions/variants |
| --- | --- | ---: | --- | --- | --- |
| `light` | `streetlight_reveal`, `market_lamp_reveal` | 10, 16 | A lamp turns on/flickers/sweeps toward a hider who hides too long. | Triggered reveal source, light sweep/spot marker, optional audio hum, no collision unless the pole/object itself is solid. | `v1` placeholder cone/pulse; variants `warm_pole`, `blue_pole`, `market_lamp`, `lamp_string`. |
| `house_light` | `house_light_reveal`, `balcony_light_reveal` | 11, 16 | A house/window/door light turns on near the hiding player. | Attached to house prefab/window, short warm glow and direction/area clue. | `v1` window glow; variants `small_window`, `balcony_light`, `door_light`, `curtain_glow`. |
| `animal_sound` | `dog_bark_reveal`, `rooster_noise_reveal` | 10, 16 | Friendly neighborhood animal sound points toward a hiding player. | Directional audio/visual ping source, optional small animal sprite, no attack/chase behavior. | `v1` bark/chirp icon ping; variants `brown_aspin`, `black_aspin`, `offscreen_bark`, `rooster`. |
| `movement_noise` | `run_noise_reveal`, `footstep_reveal` | 16, 21 | Running gives away hider direction; walking stays quiet. | Directional ping emitted by running hider at cadence; not exact permanent tracking. | `v1` ripple/arrow ping; variants `footstep`, `dust`, `sound_wave`. |
| `environment` | `loose_can_reveal`, `hanging_bell_reveal` | 12, 16+ | Market/residential objects can make a clue when disturbed. | Optional future trigger for noisy map props; should be disabled until the core hide/run reveal works. | `v1` prop noise ping; variants `can`, `bell`, `plastic_chair`, `bucket`. |

## Implementation timing

- Phase 10/11 may add visual placeholder versions of `RevealSource` objects.
- Phase 14 may place reveal-source objects in the first playable map.
- Phase 16 implements the actual 5-second hide timeout and walk/run reveal
  rules.
- Phase 21 synchronizes reveal events online.

Until Phase 16, reveal objects can exist visually but must not affect gameplay.

## Example scalable RevealSource registry

The eventual object registry can look like this:

```yaml
revealSources:
  streetlight_reveal:
    category: reveal_source
    revealSourceType: light
    version: 1
    variants:
      - warm_pole
      - blue_pole
      - market_lamp
    defaults:
      revealMode: line_sweep
      trigger: hide_timeout
      durationSeconds: 1.5
      cooldownSeconds: 5
      radius: 7
      exactPositionAllowed: false

  dog_bark_reveal:
    category: reveal_source
    revealSourceType: animal_sound
    version: 1
    variants:
      - brown_aspin
      - black_aspin
      - offscreen_bark
    defaults:
      revealMode: direction_only
      trigger: hide_timeout
      durationSeconds: 1
      cooldownSeconds: 6
      radius: 9
      exactPositionAllowed: false
```
