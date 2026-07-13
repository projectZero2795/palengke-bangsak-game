# Phase 34F: Android performance

## Scope

Measure and tune only Android frame pacing, memory, and APK size on the
approved Pixel 6 / Android 15 reference profile. Signing, Play Console work,
listing content, and publication remain Phase 34G or later.

The owner removed thermal behavior from this phase on 2026-07-13. No thermal
measurement or thermal acceptance criterion is required. With that criterion
removed, the configured `BangSak_Pixel6_API35` emulator is the accepted Phase
34F reference environment.

## Approved budgets

| Metric | Budget |
| --- | ---: |
| Sustained frame rate | At least `30 FPS` |
| P95 presented-frame time | At most `33.3 ms` |
| Sustained frame-time regression | At most `10%` from opening to late gameplay |
| Peak application PSS | At most `1 GB` |
| Debug APK size | At most `60 MB` |

## Tuning

The first Android baseline was capped near 30 FPS. Its opening compositor
window measured `30.00 FPS` and `35.046 ms` P95, so it failed the approved
frame-time ceiling.

Android builds now request `60 FPS` through
`Application.targetFrameRate`. The target is an explicit, tested runtime
constant. Phase 34F build metadata advances to version `0.34.8`.

## Reference run

- AVD: `BangSak_Pixel6_API35`
- model: `sdk_gphone64_arm64` (Pixel 6 profile)
- Android: `15` / API 35
- display: `1080 x 2400`, landscape
- package: `es.palengke.bangsak`
- build: Phase `34F`, version `0.34.8`, ARM64 IL2CPP development APK
- workload: local `02:30` round with Taya plus three Hiders
- frame source: Android SurfaceFlinger presentation timestamps from the Unity
  `SurfaceView`; each result contains `126` consecutive frame intervals
- memory source: `adb shell dumpsys meminfo es.palengke.bangsak`

One full measured round reached `00:00` and the expected `Hiders win! Time is
up.` result. A separately scheduled late-gameplay sample was captured at
`00:45` to prevent ADB collection latency from mixing result-screen frames
into the late gameplay window.

| Window | Round timer | FPS | Average frame | P95 frame | App PSS |
| --- | ---: | ---: | ---: | ---: | ---: |
| Opening gameplay | `02:12` | `59.53` | `16.799 ms` | `17.711 ms` | `269,615 KB` |
| Midpoint gameplay | `01:17` | `60.00` | `16.666 ms` | `17.391 ms` | `274,730 KB` |
| Late gameplay | `00:45` | `60.00` | `16.668 ms` | `17.615 ms` | `275,975 KB` |

The late P95 is `0.54%` lower than the opening P95, so there is no sustained
frame-time regression. The largest PSS observed across gameplay and result
sampling was `276,387 KB`, well below `1 GB`.

The final APK is `52,962,378` bytes, below both decimal `60 MB` and binary
`60 MiB`. Its SHA-256 is
`6835183346380b390b2a2215c70dea8da3fce351b024fae500c6dc773d96b881`.

## Acceptance review

| Acceptance criterion | Evidence | Result |
| --- | --- | --- |
| At least 30 FPS | All three gameplay windows measured `59.53–60.00 FPS`. | Pass |
| P95 at most 33.3 ms | Worst gameplay P95 was `17.711 ms`. | Pass |
| No sustained regression above 10% | Late P95 changed by `-0.54%` from opening. | Pass |
| Peak PSS at most 1 GB | Peak observed PSS was `276,387 KB`. | Pass |
| APK at most 60 MB | APK is `52,962,378` bytes. | Pass |
| Thermal criterion | Explicitly removed by the owner on 2026-07-13. | Not required |
| Automated regression | Unity EditMode suite passed `236/236`. | Pass |
| Scope stayed inside Phase 34F | No signing key, AAB, Play Console, listing, or publication change was made. | Pass |

Phase 34F passes the authorized nonvisual self-review. Phase 34G may produce
the first versioned signed Android App Bundle, but must keep all keys and
passwords outside Git.
