# Phase 28B — Unity WebGL build and local browser test

## Goal

Restore the missing WebGL release prerequisite before Docker and Kubernetes.
This phase builds and tests locally only; it does not publish the game.

## Build pipeline

Required Unity version: `2022.3.50f1` with **WebGL Build Support** installed.

From Unity:

1. Open `Assets/Scenes/MainMenu.unity`.
2. Select `Bang-Sak > Build > Phase 28B WebGL`.

Command-line equivalent:

```bash
/Applications/Unity/Hub/Editor/2022.3.50f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit \
  -projectPath /Users/renz/Documents/palengke-bangsak-game/unity \
  -executeMethod Palengke.BangSak.Editor.Phase28BWebGlBuild.BuildCommandLine
```

Output is written to `unity/Build/WebGL` and remains untracked. The pipeline
creates `build-info.json` with the phase, version, Unity version, target, size,
and UTC build time.

Compression is deliberately disabled for the local review build so a basic
static server can serve it without special `Content-Encoding` headers. Docker
hosting can enable production compression only after its Nginx headers are
tested.

## Local review

```bash
cd /Users/renz/Documents/palengke-bangsak-game/unity/Build/WebGL
python3 -m http.server 8080
```

Then open `http://localhost:8080` and confirm:

- the loading screen completes;
- MainMenu renders;
- guest identity is visible;
- `HOW`, `SET`, `ROOM`, and `SCORES` open and close;
- `PLAY` loads `PrototypeMap`;
- movement and named Bang actions work;
- returning to the menu works;
- no browser Console errors block gameplay.

## Exit criteria

- A clean WebGL build completes with both enabled scenes.
- The build is served locally over HTTP.
- MainMenu and guest gameplay work in a desktop browser.
- No Docker image, Kubernetes resource, DNS, or public release is added yet.

## Verification result

Verified locally on 2026-07-11 with Unity `2022.3.50f1` and Chromium WebGL 2:

- build `0.28.2` completed successfully (24,231,832 bytes);
- MainMenu rendered with the real Palengke leaderboard label;
- `HOW`, `SET`, `ROOM`, and `SCORES` opened and closed;
- `PLAY` loaded `PrototypeMap` with three independent Hider cooldown bars;
- no browser warnings or errors blocked loading or gameplay;
- the live leaderboard correctly fell back to its unavailable state because the
  public game deployment and authenticated end-to-end acceptance are deferred.
