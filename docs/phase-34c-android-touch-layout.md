# Phase 34C: Android touch layout

## Goal

Make the existing menu and local-round controls fit Android touch screens,
display cutouts, and wide landscape aspect ratios. One complete local round
must be playable by touch with no critical overlap. This phase does not add
accessibility settings, change Photon lifecycle behavior, tune performance,
sign a release, or touch Play Console.

## Implemented layout contract

- `SafeAreaCanvasLayout` converts `Screen.safeArea` into normalized canvas
  anchors and reapplies it when resolution, orientation, or cutout changes.
- Menu cards and panels, the mobile joystick, named-Bang buttons, SAK/Bang
  controls, status HUD, caught counter, and round result panel use the shared
  safe-area root.
- All runtime canvases use balanced width/height scaling at the existing
  `800 x 600` reference resolution instead of scaling from width alone.
- Android's supported aspect ratio is raised from `2.1` to `2.4`, covering the
  Pixel 6 `20:9` landscape display without clipping the UI.
- The Android debug build is version `0.34.1`, Phase `34C`.

The full-bleed menu background remains outside the safe-area root so it covers
the screen; interactive content stays inside the safe region.

## Owner review

1. Build from Unity with `Bang-Sak > Build > Android Debug APK`.
2. Start the `BangSak_Pixel6_API35` emulator and install
   `unity/Build/Android/BangSak-debug.apk` using the Phase 34B install commands.
3. Tap `PLAY` on the main menu.
4. Drag the joystick and confirm Taya moves, then tap one named-Hider Bang
   button.
5. Let the round timer reach `00:00` and confirm the result panel is fully
   visible.
6. Tap `Restart` and confirm `ROUND 2` begins.

Confirm throughout that the joystick, named-Hider buttons, status panel, and
result buttons are fully visible and do not critically overlap. Stop there;
accessibility options belong to Phase 34D.

## Optional notch check

The Android 15 emulator can simulate the Pixel-style hole punch:

```bash
ADB=/Applications/Unity/Hub/Editor/2022.3.50f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb
"$ADB" shell cmd overlay enable-exclusive --category com.android.internal.display.cutout.emulation.hole
```

Allow Android several seconds to apply the display overlay, then cold-start the
game. In landscape the verified safe rectangle is inset `128` physical pixels
from the cutout side.

## Automated verification

Run the Unity EditMode suite:

```bash
/Applications/Unity/Hub/Editor/2022.3.50f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics \
  -projectPath unity \
  -runTests -testPlatform EditMode \
  -testResults /tmp/bang-sak-phase34c-editmode.xml \
  -logFile /tmp/bang-sak-phase34c-editmode.log
```

The Phase 34C tests cover normalized safe-area calculation, invalid/cropped
rectangle handling, anchor application, balanced CanvasScaler configuration,
and preserving the joystick layout when it moves into the safe-area root.

## Verified evidence

Verified on 2026-07-12:

- `218` Unity EditMode tests passed with `0` failures;
- final Phase 34C IL2CPP ARM64 debug APK built successfully;
- final APK size is `52,871,806` bytes;
- final APK SHA-256 is
  `e029cb198c85d8042c61eae960578f03479563784324a9c5741618c98d9c73f7`;
- Android package inspection confirmed version `0.34.1`, `minSdk 29`,
  `targetSdk 35`, debug status, and ARM64 native code;
- the main menu fits at `2400 x 1080` without its previous vertical clipping;
- the hole-punch simulation reported a landscape safe area of
  `(128, 0, 2272, 1080)`, and menu/game controls rendered inside it;
- touch input started a local round, moved Taya with the joystick, and triggered
  a named-Bang button;
- the normal timer reached `00:00`, the Hiders-win result panel rendered without
  critical overlap, and tapping `Restart` started Round 2;
- no fatal Android or Unity application crash was present during the round.

No Phase 34D+ feature, Photon lifecycle behavior, signing material, AAB, Play
Console app, or upload was added.
