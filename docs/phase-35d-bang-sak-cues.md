# Phase 35D — Bang and SAK sound cues

## Status

Done; version 2 was owner-approved on 2026-07-15.

On 2026-07-15 the owner confirmed that Phase 35C audio was menu-only. After
being told that Bang/SAK request and confirmed-outcome cues were the separate
Phase 35D scope, the owner said `okay, approved`. Phase 35C is therefore closed
and this phase is authorized. After hearing version 1, the owner reported that
everything sounded the same and clarified that the cues should be more
meaningful. Version 2 therefore replaces four related chirps with action-shaped
pop, success, elastic-counter, and deflection signatures. After the semantic
build was left open for review, the owner said `okay, lets continue next
phases`, closing this listening gate and authorizing Phase 35E.

## Scope and safety

Phase 35D adds four short, nonverbal, procedural cues: Bang request, Bang caught
confirmation, SAK request, and SAK counter confirmation. The sounds are gentle
tonal pops/chirps rather than speech, a gunshot, a blade, an impact, or any
realistic weapon sound. This phase adds no round, reveal, result, ambient, or
music audio and does not change action rules, geometry, cooldowns, UI, or Play
Console distribution.

## Versioned cue contract

The set ID is `bangsak.gameplay_action_cues`, set version `2`, with minimum
compatible version `1`. Unknown future cues must be ignored by older clients.

| Cue | Stable ID | Version | Semantic signature | Duration | Cue level |
| --- | --- | ---: | --- | ---: | ---: |
| Bang request | `gameplay.bang_request` | 2 | Low cartoon pop with a soft percussive transient | 125 ms | 21% |
| Bang caught | `gameplay.bang_caught_confirmed` | 2 | Three-note rising success sparkle, 620 → 1040 Hz | 220 ms | 24% |
| SAK request | `gameplay.sak_request` | 2 | Elastic counter boing, 520 → 240 Hz | 180 ms | 20% |
| SAK countered | `gameplay.sak_countered_confirmed` | 2 | Metallic deflection/contact ring, 760 → 540 Hz | 200 ms | 23% |

The cue level is multiplied by the Phase 35A master and SFX levels. Mute
resolves the live output to zero.

## State binding

- Bang request publishes only after `TryBang` accepts the action. A disabled or
  cooldown-rejected input remains silent.
- SAK request publishes only after `TrySak` accepts the role, enabled-state, and
  cooldown checks.
- Bang caught publishes only when `CaughtStateController.MarkCaught` accepts a
  new Bang-caused state transition. An already-caught target cannot replay it.
- SAK countered publishes only when
  `TayaCounteredStateController.MarkCountered` accepts a new sequence. A
  duplicate sequence cannot replay it.
- Remote confirmed actions pass through the same caught/countered transition
  methods, while the existing network action layer rejects duplicate events.
- Existing thrown-tsinelas marker, range cone, action banner, cooldown rows,
  caught stars/tint, SAK burst/tint, HUD counts, and state flags remain the
  authoritative visual equivalents. Audio never decides or replaces state.

## Playback and compatibility

`BangSakGameplayCueCatalog` deterministically synthesizes four mono 44.1 kHz
clips with separate semantic sound profiles and zero-to-zero envelopes. Bang
request uses a low tonal body plus a deterministic transient, caught uses a
three-note success arpeggio, SAK request uses a springy pitch fall, and a
successful counter uses an inharmonic deflection ring. No binary sound asset
or third-party sample is added. `BangSakGameplayCuePlayer` caches exactly those four clips on one
persistent 2D `AudioSource`, follows live master/SFX/mute changes, and keeps one
bounded pending slot so an immediate confirmed outcome follows its request
without overlapping it. Gameplay audio therefore uses at most one simultaneous
voice and cannot stack unbounded sources or queues. Cue playback failures are
contained and cannot prevent or roll back an accepted gameplay state.

## Objective evidence

| Check | Result |
| --- | --- |
| Focused Phase 35D tests | 7/7 passed |
| Complete EditMode suite | 259/259 passed |
| Procedural sample budget | 127,892 bytes total; test gate remains below 128 KiB |
| Fresh WebGL build | Version 2 passed; 37,609,485 bytes |
| Desktop browser smoke | Fresh build loaded; local Play and accepted Bang request reached visible `BANG! MISS` with no audio/runtime error |
| Fresh Android ARM64 debug APK | Passed; 53,070,694-byte APK |
| Pixel 6 / Android 15 API 35 | Install, cold launch, touch Play, and accepted Bang tap passed |
| Android runtime log | No fatal exception, `NullReferenceException`, `ArgumentException`, `AudioClip`, or `AudioSource` error |

The fresh local WebGL load, scene change, and accepted action produced no new
audio/runtime exception. A stale pre-refresh tab retained earlier local Photon
placeholder-authentication messages; the fresh local-play path did not add
them and multiplayer transport is unchanged by this phase.

## Owner audio review

Use the prepared local WebGL tab or run the fresh build from
`unity/Build/WebGL`:

1. Set Mute to Off and Master/SFX to 100%.
2. Select Play, then use Bang once in open space. Confirm the request cue is a
   low, percussive cartoon pop and the visible `BANG! MISS` feedback still appears.
3. Use the named Bang control on a correct in-range Hider. Confirm the caught
   cue plays once with the caught stars/tint.
4. In a Hider test/client, use SAK in open space, then counter an in-range Taya.
   Confirm the request is an elastic boing, the successful counter is a
   deflection ring, and the burst/tint remains clear.
5. Repeat inputs during cooldown and confirm no rejected-input cue plays.
6. Lower SFX, enable Mute, and confirm all visual gameplay remains usable.
7. Confirm each sound communicates its action without looking at the screen,
   while remaining friendly, short, and non-startling.

The Phase 31 owner listening gate was closed on 2026-07-15 when the owner
approved continuing to the next phases after the semantic version-2 review.

## Rollback

Remove the three Phase 35D audio classes and the four cue publication calls
from Bang, SAK, caught, and countered transitions. Phase 35A settings and Phase
35C menu cues remain independent and require no data migration.
