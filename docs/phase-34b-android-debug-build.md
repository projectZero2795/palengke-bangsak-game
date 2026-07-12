# Phase 34B: Android debug build

## Goal

Install the Unity Android toolchain and add one reproducible ARM64 debug-APK
build. The APK must install and open the main menu on the Phase 34A reference
profile. This phase does not change touch gameplay, sign a release, create a
Play Console app, or upload anything.

## Implemented build contract

`Phase34BAndroidBuild` is available from both:

- Unity menu: `Bang-Sak > Build > Phase 34B Android Debug APK`;
- command line method:
  `Palengke.BangSak.Editor.Phase34BAndroidBuild.BuildCommandLine`.

Every run configures the approved Android identity and compatibility values
before building:

| Setting | Value |
| --- | --- |
| Output | `unity/Build/Android/BangSak-debug.apk` |
| Package | `es.palengke.bangsak` |
| Version | `0.34.0` (`versionCode 1`) |
| Minimum Android | Android 10 / API 29 |
| Target Android | Android 15 / API 35 |
| CPU | ARM64 (`arm64-v8a`) |
| Scripting backend | IL2CPP |
| Orientation | Landscape left and right only |
| Build type | Development/debug APK, no production signing |

The build also writes ignored local metadata to
`unity/Build/Android/build-info.json`, including the APK SHA-256. `Build/`
remains excluded from Git so compiled binaries are not stored in the source
repository.

## Build it

The simplest path while the project is open is the Unity menu command above.
Wait for the Console to report `Phase 34B Android debug APK completed`.

For a closed Unity Editor, run from the repository root:

```bash
/Applications/Unity/Hub/Editor/2022.3.50f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit \
  -projectPath "$(pwd)/unity" \
  -executeMethod Palengke.BangSak.Editor.Phase34BAndroidBuild.BuildCommandLine \
  -logFile /tmp/bang-sak-phase34b-android.log
```

An IL2CPP Android build can take several minutes because Unity compiles the
managed project to native ARM64 code.

## Owner review on this Mac

The `BangSak_Pixel6_API35` emulator is already configured as Android 15 / API
35, `1080 x 2400`, density `420`, ARM64.

1. Build the APK from the Unity menu.
2. Open Terminal and start the reference phone:

   ```bash
   SDK=/Applications/Unity/Hub/Editor/2022.3.50f1/PlaybackEngines/AndroidPlayer/SDK
   "$SDK/emulator/emulator" -avd BangSak_Pixel6_API35
   ```

3. Leave that Terminal open. In a second Terminal, from the repository root,
   install and open the game:

   ```bash
   ADB=/Applications/Unity/Hub/Editor/2022.3.50f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb
   "$ADB" install -r -t unity/Build/Android/BangSak-debug.apk
   "$ADB" shell am start -W -n es.palengke.bangsak/com.unity3d.player.UnityPlayerActivity
   ```

4. On the emulator's first fullscreen launch, press Android's `Got it` button.
5. Confirm the Bang-Sak main menu is visible in landscape with `PLAY`, `ROOM`,
   `HOW`, `SCORES`, and `SET` cards.

Stop there. Playing a whole round by touch is the Phase 34C gate, not this
phase.

## Verified evidence

Verified on 2026-07-12:

- Unity Android Build Support, SDK/NDK, OpenJDK, platform tools, API 35, Android
  Emulator, and the Android 15 ARM64 system image are installed;
- `213` Unity EditMode tests passed with `0` failures;
- clean IL2CPP ARM64 APK build succeeded;
- APK size is `52,870,822` bytes;
- APK SHA-256 is
  `eee180fb5d215794c4ab1cdebf8e3273aa1ca904804256b8a4dc58b7a63c9ba7`;
- Android package inspection confirmed package/version, `minSdk 29`,
  `targetSdk 35`, debug status, internet permission, and ARM64 native code;
- the APK installed on the Pixel 6 Android 15 reference emulator;
- a cold launch completed and focused
  `es.palengke.bangsak/com.unity3d.player.UnityPlayerActivity`;
- the landscape main menu rendered at `2400 x 1080` with no fatal Android or
  Unity crash in the launch log.

No Play Console application was created, no signing material was configured,
and no APK or bundle was uploaded.
