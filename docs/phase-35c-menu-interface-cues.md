# Phase 35C — Menu and interface sound cues

## Status

Done and owner-approved on 2026-07-15.

On 2026-07-15 the owner said `let's go` after being told that Phase 35B was the
visual-approval gate and Phase 35C was next. That closes Phase 35B and
authorizes this phase only. After the implementation and its menu-only scope
were restated, the owner said `okay, approved`, closing this listening gate and
authorizing Phase 35D.

## Scope

Phase 35C adds only short menu navigation, confirm, and back cues. It does not
add Bang, SAK, round, reveal, result, ambient, or music audio and does not
change gameplay or Play Console distribution.

## Versioned cue contract

The set ID is `bangsak.menu_interface_cues`, set version `1`, with minimum
compatible version `1`. Unknown future cues must be ignored by older clients.

| Cue | Stable ID | Version | Sweep | Duration | Cue level |
| --- | --- | ---: | ---: | ---: | ---: |
| Navigate | `menu.navigate` | 1 | 460 → 560 Hz | 65 ms | 16% |
| Confirm | `menu.confirm` | 1 | 540 → 720 Hz | 105 ms | 20% |
| Back | `menu.back` | 1 | 520 → 400 Hz | 85 ms | 15% |

The cue level is multiplied by the existing Phase 35A master and SFX levels.
Mute resolves the live output to zero.

## Implementation

- `BangSakMenuCueCatalog` deterministically synthesizes the three mono 44.1 kHz
  clips with a smooth zero-to-zero envelope. No third-party or generated binary
  sound asset is stored in the repository.
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
| Procedural sample budget | 44,988 bytes total; test gate remains below 64 KiB |
| Fresh WebGL build | Passed; 37,589,561 bytes |
| Desktop browser smoke | Settings → Audio → SFX 90% → mute on/off → Back passed; no runtime exception |
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

The Phase 31 owner listening gate was closed on 2026-07-15 when the owner said
`okay, approved` after confirming these sounds were only for the menu and being
told that Bang/SAK cues were the next separate phase.

## Rollback

Remove the three Phase 35C audio classes and their menu bindings, then restore
the prior direct button/keyboard actions. Phase 35A persistence and Phase 35B
controls remain independent and require no data migration.
