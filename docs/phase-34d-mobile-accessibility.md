# Phase 34D: Mobile accessibility

## Goal

Add four small, persistent accessibility options to the existing Android game:

- readable text;
- high contrast;
- reduced motion;
- visual action cues.

This phase does not change networking, game rules, performance budgets,
signing, Play Console state, or any Phase 34E work.

## What changed

The main-menu **SET** panel now contains four independent touch toggles. Their
values are saved locally with Unity `PlayerPrefs` and apply to every runtime
Canvas.

- **Readable Text** increases the preferred font size, raises the minimum
  best-fit size, and uses bold labels while preserving each panel boundary.
- **High Contrast** changes labels to bright white or yellow and adds a dark
  outline.
- **Reduced Motion** stops the settings motion preview, uses a stable walk
  frame, and removes spinning, pulsing, arcing, and expanding action feedback.
- **Visual Action Cues** shows a text banner such as `BANG! CAUGHT`,
  `BANG! MISS`, `SAK! COUNTER`, or `SAK! BLOCKED`. This gives action outcomes a
  visible channel even when sound is unavailable or muted.

Readable text and visual action cues default to on. High contrast and reduced
motion default to off. All four can be changed independently.

## Owner review on Android

1. Build or install Android debug version `0.34.2`.
2. Open Bang-Sak and tap **SET**.
3. Toggle **READABLE TEXT** off and on. Confirm label weight/size visibly
   changes and nothing overlaps.
4. Toggle **HIGH CONTRAST** on. Confirm menu text becomes bright with a dark
   outline; toggle it off again.
5. Watch the blue **MOTION PREVIEW** marker move. Toggle **REDUCED MOTION** on
   and confirm it stops; toggle it off and confirm it moves again.
6. Toggle **VISUAL ACTION CUES** and confirm the preview changes between
   `VISUAL CUE: BANG! CAUGHT` and `VISUAL ACTION CUES OFF`.
7. Leave visual cues on, tap **BACK**, start a local round, and use Bang.
   Confirm a readable outcome banner appears below the round status.

Approve Phase 34D only when every toggle has a visible effect and the settings
screen has no critical overlap on the reference phone.

## Automated checks

The Phase 34D EditMode tests cover default values, independent persistence,
font scaling, high-contrast styling, stable reduced-motion pulses, visual-cue
suppression, and the four settings-panel controls.

Final implementation evidence:

- `225` EditMode tests passed with `0` failures;
- the clean IL2CPP ARM64 Android debug build completed as version `0.34.2`;
- the APK supports API 29+, targets API 35, and is `52,908,778` bytes;
- APK SHA-256:
  `1151a5fffe9041503be9f162f724797323079cf2db2dcf4384dc49c38bd416fc`;
- the APK installed and launched on the Pixel 6 Android 15/API 35 emulator;
- all four toggles changed visibly with no critical overlap;
- two reduced-motion screenshots one second apart were byte-identical;
- a local Bang displayed the visible `BANG! MISS` outcome banner;
- no fatal Android exception was recorded.

## Stop boundary

Stop after the Android owner check. Phase 34E1 room create/join and all later
work remain blocked until explicit approval.
