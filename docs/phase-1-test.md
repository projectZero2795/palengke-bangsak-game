# Phase 1 Test Notes

## Scope

Phase 1 creates the clean Unity 2D project foundation only.

Included:

- `unity/Packages/manifest.json`
- `unity/ProjectSettings/ProjectVersion.txt`
- `unity/ProjectSettings/ProjectSettings.asset`
- `unity/ProjectSettings/EditorBuildSettings.asset`
- `unity/Assets/Scenes/MainMenu.unity`
- `unity/Assets/Scenes/PrototypeMap.unity`
- required folder structure under `unity/Assets`

Not included:

- player art;
- player prefab;
- movement scripts;
- input system package;
- gameplay code;
- Photon;
- WebGL build;
- Docker/Kubernetes/Argo files.

## Static checks completed

- Unity project folders exist.
- Required scenes exist.
- Required script/art/prefab folders exist.
- Scenes contain a `Main Camera`.
- `EditorBuildSettings.asset` references both Phase 1 scenes.

## Console error fix

Unity 2022.3.50f1 on macOS produced repeated editor-console errors from `com.unity.inputsystem@1.7.0` during Phase 1 review:

- `NullReferenceException` in `UnityEngine.InputSystem.InputSystem.InitializeInEditor`
- `TypeInitializationException during event processing of Editor update`

Phase 1 does not require input handling yet, so the Input System package was removed from `unity/Packages/manifest.json`. Input work belongs to Phase 3, where the package/version can be selected and tested intentionally with movement controls.

## Unity Editor availability

Unity Hub 3.19.3 and `xvfb` were installed on the codex server during Phase 1, but the Hub process still requires Electron/display behavior and did not expose a reliable non-interactive editor install path in this headless environment.

Because Phase 1 is only the project foundation, the repository is ready for manual Unity Editor review instead of pretending the editor was opened successfully on the server.

## Manual Unity Editor checks required

Phase 1 final approval requires opening the project in Unity 2022.3 LTS or newer:

1. Open `unity/` from Unity Hub.
2. Let Unity import packages.
3. Open `Assets/Scenes/MainMenu.unity`.
4. Press Play and confirm the empty scene loads without errors.
5. Open `Assets/Scenes/PrototypeMap.unity`.
6. Press Play and confirm the empty map scene loads without errors.
7. Confirm the camera renders a blank 2D scene.

## Exit criteria status

- Project scaffold: ready.
- Empty scene load test: pending Unity Editor review.
