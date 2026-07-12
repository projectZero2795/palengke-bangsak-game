# Bang-Sak for Palengke

Bang-Sak is a safe Filipino community hide-and-seek game built with Unity 2D WebGL at `bangsak.palengke.es`.

The target experience is the reference image provided by the project owner: a polished 2D top-down barangay/palengke game with menu, lobby, round gameplay, result screen, leaderboard, mobile controls, and later Photon multiplayer.

Canonical visual reference: [Bang-Sak roadmap reference](docs/reference/bang-sak-roadmap-reference.jpg).

## Current phase

This repository is currently at **Phase 34E4: Android pause/resume**.
Phase 34D was owner-approved after reviewing Android accessibility build
`0.34.2`. Phase 34E1 passed the authorized self-review after two Android 15
clients joined room `1234` with the matching `JuanP · Maria` roster. Phase
34E2 added a compact safe-area exit icon and Cancel/confirm dialog. The owner
approved the revised icon on 2026-07-12 after Android build `0.34.4` passed the
focused behavior check and 227 tests. Urgent Phase 29A then audited the exact
DNS, edge, certificate, GitOps, verification, redirect, and rollback boundary
without changing production. Phase 29B added `bangsak.palengke.es` alongside
the unchanged `games.palengke.es` endpoint and passed the complete guest,
authenticated reward, leaderboard, and two-client Photon acceptance matrix.
Phase 29C made the new hostname canonical while preserving old paths and
queries through a temporary redirect. At the owner's explicit direction,
Phase 29D used live traffic evidence to complete the application cleanup on
2026-07-12. The legacy redirect and renewable certificate remain retained
through at least 2026-10-10. Phase 34E3 then passed the authorized nonvisual
self-review: deterministic roster compaction, Taya/authority transfer,
last-player lobby return, freed capacity, and no-ghost behavior are covered by
231 passing tests and Android build `0.34.5`.

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

Phase 20 adds the first reviewable playable map layout contract. `PrototypeMap` now has a versioned `Phase 20 Map Layout` component with Taya and Hider spawn points, visible review spawn markers, bounded camera follow, map/camera bounds, validation helpers, and a widened `50 x 34` wall boundary inside a noticeably larger `52 x 36` nighttime barangay/palengke map. This phase intentionally does not add round rules, win/loss screens, scoring, reveal behavior, or multiplayer.

Phase 21 adds the first local round loop: `02:30` timer, Hiders-left status, Taya win when all Hiders are caught, Hiders win when Taya is countered by SAK, default Hiders win on timer expiry, restart by `R` or button, and actor placement on Phase 20 spawn points. The old default-player hiders counter HUD is removed because the Phase 21 round HUD owns that display. Multiplayer, scoring, Palengke API, and stealth/reveal dog/light rules remain out of scope.

Phases 22–26 add UI polish and compile-safe multiplayer scaffolds for Photon room lifecycle, player spawning, movement synchronization, and Bang/SAK action events. Real Photon transport remains a later integration step.

Phase 27 adds an isolated Palengke API placeholder with a configurable base URL, offline mock user and coins, and a visible mock leaderboard. It makes no production HTTP requests and contains no API credentials or tokens.

Phase 27A makes cooldowns visible inside each Hider button with independent progress, seconds labels, and disabled state during that Hider's recharge. There is no shared Bang cooldown bar or global `READY` label. SAK retains its radial cooldown feedback.

Phase 28 connects the Unity WebGL client to authenticated Palengke identity, persistent scores, server-calculated coin rewards, and the live leaderboard. Session failure falls back safely to guest play; no token or server credential is embedded in the Unity project.

Phase 28B restores the missing release prerequisite with a reproducible Unity WebGL build pipeline and local browser smoke test. It does not add Docker, Kubernetes, DNS, or a public release.

Phase 28C packages the approved WebGL artifact in a non-root Nginx container with health, cache, MIME, and browser security configuration. It remains a local container test only; Kubernetes and public release belong to the next approved phase.

Phase 29 publishes the immutable game image through the existing Palengke GitOps platform at `https://bangsak.palengke.es`. Two hardened Kubernetes replicas, health probes, disruption protection, TLS routing, rollback notes, and the live Bang-Sak leaderboard API are in place.

Phase 30 adds structured production logs, request correlation, scripted public verification, release/version records, operations and rollback guidance, configuration/backup ownership, and a maintained known-issues register without recreating the removed monitoring stack.

Phase 31 turns the requested post-release work into a dependency-ordered plan:
authoritative multiplayer and integrity first, followed by a small Phase
34A–34K Android/Google Play release track, sound, art, maps/component selection,
cosmetics/badges, role power-ups, seasonal events, and tournaments. It defines
common regression, safety, fairness, accessibility, security, performance, and
production gates; it intentionally implements none of those later features
yet.

Phase 32 imports Photon Fusion 2.1.1 and replaces the compile-safe room,
movement, action, and round scaffolds with a real two-client WebGL vertical
slice. Photon Shared Mode now provides EU room create/join/leave, synchronized
scene loading, dense roster roles, movement and Bang/SAK streams, room-creator
round state, results, restart, and manual room-code rejoin. Shared Mode remains
distributed authority; Phase 33 owns integrity and anti-cheat hardening.

Phase 33 replaces direct peer-authored state with credential-bound requests to
the room creator. The authority validates sender, sequence, role, round state,
movement rate/speed/bounds, named-Bang eligibility, SAK role, cooldown, and
restart state; it recalculates action geometry and broadcasts only confirmed
movement, action, caught-state, timer, result, and restart state. Multiplayer
score submission is restricted to the room authority and a stable per-round
idempotency ID. Shared Mode still trusts the player-hosted room creator, so this
is a practical baseline rather than dedicated-server competitive security.

Phase 34A records the approved Android release decisions without creating a
build or changing Play Console. Phase 34B installs the Unity Android toolchain
and adds a reproducible IL2CPP ARM64 debug APK for `es.palengke.bangsak`. The
APK targets API 35 and supports API 29 and newer. Phase 34C adds safe-area and
balanced wide-screen layout across the menu and round controls. Phase 34D adds
readable-text, high-contrast, reduced-motion, and visual-action-cue settings.
The former broad Android Photon lifecycle phase is split into Phase 34E1–34E5
so create/join, voluntary leave confirmation, remaining-room leave rules,
pause/resume, and reconnect can each be reviewed separately. Release signing,
Play Console creation, and store upload remain later owner-gated phases.
The requested closer perspective, dark ambient visibility, cone-only local
vision, and self-only minimap are recorded separately as future Phases
42A–42E; none is implemented in the current phase.

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
- Target builds: WebGL first, then Android through the Phase 34A–34K Google Play
  release track.
- Web release: `bangsak.palengke.es`.
- Android release target: publicly downloadable from Google Play after Phase
  34K approval.
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
6. Self-review nonvisual acceptance criteria and continue only when all pass.
7. For visible additions or changes, provide screenshots and stop for explicit
   owner approval.

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
- [Phase 21 round rules](docs/phase-21-round-rules.md)
- [Phase 22 UI polish](docs/phase-22-ui-polish.md)
- [Phase 23 Photon setup scaffold](docs/phase-23-photon-setup.md)
- [Phase 24 multiplayer player spawning](docs/phase-24-multiplayer-player-spawning.md)
- [Phase 25 multiplayer movement sync](docs/phase-25-multiplayer-movement-sync.md)
- [Phase 26 multiplayer Bang/SAK sync](docs/phase-26-multiplayer-bang-sak-sync.md)
- [Phase 27 Palengke API placeholder](docs/phase-27-palengke-api-placeholder.md)
- [Phase 27A visible action cooldowns](docs/phase-27a-visible-action-cooldowns.md)
- [Phase 28 real Palengke integration](docs/phase-28-real-palengke-integration.md)
- [Phase 28B WebGL build and local browser test](docs/phase-28b-webgl-build.md)
- [Phase 28C Docker static hosting](docs/phase-28c-docker-static-hosting.md)
- [Phase 29 Kubernetes deployment and public release](docs/phase-29-kubernetes-public-release.md)
- [Phase 30 monitoring and maintenance](docs/phase-30-monitoring-maintenance.md)
- [Phase 31 polish and content expansion plan](docs/phase-31-polish-content-expansion.md)
- [Phase 32 Photon Fusion Shared multiplayer](docs/phase-32-authoritative-photon.md)
- [Phase 33 multiplayer integrity baseline](docs/phase-33-multiplayer-integrity.md)
- [Phase 34 Android and Google Play roadmap](docs/phase-34-android-play-roadmap.md)
- [Phase 34A Android release decisions](docs/phase-34a-android-release-decisions.md)
- [Phase 34B Android debug build](docs/phase-34b-android-debug-build.md)
- [Phase 34C Android touch layout](docs/phase-34c-android-touch-layout.md)
- [Phase 34D mobile accessibility](docs/phase-34d-mobile-accessibility.md)
- [Phase 34E1 Android Photon create/join](docs/phase-34e1-android-photon-create-join.md)
- [Phase 34E2 in-game leave confirmation](docs/phase-34e2-in-game-leave-confirmation.md)
- [Phase 34E3 Photon leave cleanup](docs/phase-34e3-photon-leave-cleanup.md)
- [Phase 29A domain migration audit](docs/phase-29a-domain-migration-audit.md)
- [Phase 29B dual-host routing](docs/phase-29b-dual-host-routing.md)
- [Phase 29B1 signed-in auth bridge hotfix](docs/phase-29b1-auth-bridge-hotfix.md)
- [Phase 29C canonical hostname cutover](docs/phase-29c-canonical-cutover.md)
- [Phase 29D old-host application cleanup](docs/phase-29d-old-host-cleanup.md)
- [Urgent domain migration roadmap](docs/urgent-domain-migration-roadmap.md)
- [Production operations](docs/operations.md)
- [Known issues](docs/known-issues.md)
- [Release 0.28.2](docs/releases/0.28.2.md)
- [Release 0.33.0](docs/releases/0.33.0.md)
- [Release 0.33.1](docs/releases/0.33.1.md)
- [Release 0.33.2](docs/releases/0.33.2.md)
- [Release 0.33.3](docs/releases/0.33.3.md)
