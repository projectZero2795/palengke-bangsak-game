# Phase 35C — Menu and interface sound cues

## Status

Version 2 implemented and awaiting a renewed owner listening check.

On 2026-07-15 the owner said `let's go` after being told that Phase 35B was the
visual-approval gate and Phase 35C was next. That closes Phase 35B and
authorizes this phase only. After the implementation and its menu-only scope
were restated, the owner said `okay, approved`, closing this listening gate and
authorizing Phase 35D. Later that day the owner reported that the cues sounded
the same and asked for more meaningful sounds. Version 2 keeps the same stable
cue IDs and replaces the shared chirp language with semantic click, confirm,
and back signatures.

## Scope

Phase 35C adds only short menu navigation, confirm, and back cues. It does not
add Bang, SAK, round, reveal, result, ambient, or music audio and does not
change gameplay or Play Console distribution.

## Versioned cue contract

The set ID is `bangsak.menu_interface_cues`, set version `2`, with minimum
compatible version `1`. Unknown future cues must be ignored by older clients.

| Cue | Stable ID | Version | Semantic signature | Duration | Cue level |
| --- | --- | ---: | --- | ---: | ---: |
| Navigate | `menu.navigate` | 2 | Bright tactile tick, 960 → 1160 Hz | 55 ms | 16% |
| Confirm | `menu.confirm` | 2 | Two-note affirmative chime, 520 + 850 Hz | 150 ms | 20% |
| Back | `menu.back` | 2 | Hollow descending back bubble, 620 → 280 Hz | 120 ms | 15% |

The cue level is multiplied by the existing Phase 35A master and SFX levels.
Mute resolves the live output to zero.

## Implementation

- `BangSakMenuCueCatalog` deterministically synthesizes three mono 44.1 kHz
  clips with separate rhythmic and timbral profiles. Navigate is a single
  bright tick, Confirm is an ascending two-note chime with a real gap, and Back
  is a hollow downward bubble. Zero-to-zero envelopes prevent clicks. No
  third-party or generated binary sound asset is stored in the repository.
- `BangSakMenuCuePlayer` owns one persistent 2D `AudioSource`, caches exactly
  three clips, responds immediately to settings changes, and replaces a
  still-fading cue before playing another. At most one menu voice can play, so
  rapid input cannot stack volume or allocate unbounded audio objects.
- `BangSakMenuCueBinding` leaves an inspectable cue classification on every
  menu button. Main-menu panels and links use Navigate; Play, room actions,
  toggles, and level controls use Confirm; Back and Leave use Back.
- Existing button actions execute before their cue. Visual state therefore
  never depends on sound, unmuting can provide immediate confirmation, and the
  shared cue source survives a scene-changing Play confirmation.
- Existing keyboard shortcuts use the same classification. No visible control
  geometry changed; the Audio subtitle now accurately says that menu cues use
  SFX.

## Objective evidence

| Check | Result |
| --- | --- |
| Focused Phase 35C tests | 6/6 passed |
| Complete EditMode suite | 252/252 passed |
| Procedural sample budget | 57,332 bytes total; test gate remains below 64 KiB |
| Fresh WebGL build | Version 2 passed; 37,609,485 bytes |
| Desktop browser smoke | Fresh build loaded and menu navigation played without an audio/runtime exception |
| Fresh Android ARM64 debug APK | Passed; 53,047,114-byte APK |
| Pixel 6 / Android 15 API 35 | Install, cold launch, Settings → Audio, unmute, and SFX 90% touch path passed |
| Android runtime log | No fatal exception, `NullReferenceException`, `ArgumentException`, `AudioClip`, or `AudioSource` error |

The local WebGL console retained only Unity's existing
`persistentDataPath` synchronization deprecation warning. It is unrelated to
the cue implementation.

## Owner audio review

Use the prepared local WebGL tab or run the fresh build from
`unity/Build/WebGL`:

1. Set Mute to Off and Master/SFX to 100%.
2. From the main menu, open Settings or How to hear Navigate.
3. Change SFX down and up to hear Confirm at the new level.
4. Select Back to hear the descending Back cue.
5. Repeat at lower Master/SFX levels, then enable Mute and confirm the same
   visible actions remain usable without sound.
6. Confirm that the tones feel friendly, short, and non-startling, or describe
   the direction to change.

The original listening gate was closed on 2026-07-15, then reopened when the
owner said the cue family sounded the same. Version 2 requires a fresh check of
the semantic sound language before Phase 35E begins.

## Rollback

Remove the three Phase 35C audio classes and their menu bindings, then restore
the prior direct button/keyboard actions. Phase 35A persistence and Phase 35B
controls remain independent and require no data migration.
