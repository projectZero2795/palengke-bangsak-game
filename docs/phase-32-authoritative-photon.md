# Phase 32: Photon Fusion Shared Multiplayer Vertical Slice

## Outcome

Phase 32 replaces the Phase 23-26 compile-safe multiplayer scaffolds with a
real Photon Fusion 2.1.1 transport path for WebGL.

The approved topology is:

- Photon Fusion 2.1.1 Shared Mode;
- fixed `eu` region for this first vertical slice;
- four players maximum;
- room creator owns scene changes and round state;
- each player owns their movement and Bang/SAK action input;
- manual room-code rejoin after a disconnect.

Shared Mode is suitable for the WebGL target, but it is distributed authority,
not a dedicated authoritative server. Phase 33 must validate and harden client
messages, rate limits, action outcomes, and reward/result submission before this
path is treated as integrity-safe.

## Included

- Photon Fusion SDK `2.1.1 stable build 2177` under `Assets/Photon`;
- Fusion weaving for `BangSak.Runtime`;
- real room create, join, leave, four-player limit, and EU region selection;
- Fusion scene-authority loading so all clients enter `PrototypeMap` together;
- deterministic dense roster slots from Photon `PlayerRef` values;
- Taya/Hider ownership and spawn binding for the active room roster;
- movement snapshots and Bang/SAK events over Photon reliable streams;
- room-creator timer, hider-count, result, and restart authority;
- safe validation of protocol version, message type, sender slot, enum values,
  player IDs, sequence values, and a 16 KiB payload limit;
- manual rejoin into a room whose gameplay scene is already active;
- local guest play fallback when no Photon room is connected.

## Local secret/configuration rule

`Assets/Photon/Fusion/Resources/PhotonAppSettings.asset` and its `.meta` file
remain ignored by Git. The Photon Fusion App ID is configured only in the local
Unity project and is never committed.

On a new workstation, open the project and use the Fusion Hub to set a Fusion
App ID before testing multiplayer. Keep the fixed region set to `eu` for Phase
32 review so both clients use the same room region.

## Automated verification

Run the Unity EditMode suite:

```bash
/Applications/Unity/Hub/Editor/2022.3.50f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics \
  -projectPath unity \
  -runTests -testPlatform EditMode \
  -testResults /tmp/bang-sak-phase32-editmode.xml \
  -logFile /tmp/bang-sak-phase32-editmode.log
```

Expected result: `200` tests passed, `0` failed.

Build WebGL with:

```bash
/Applications/Unity/Hub/Editor/2022.3.50f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics \
  -projectPath unity \
  -executeMethod Palengke.BangSak.Editor.Phase28BWebGlBuild.BuildCommandLine \
  -logFile /tmp/bang-sak-phase32-webgl.log \
  -quit
```

The verified artifact was `37,362,436` bytes and completed successfully with
Unity `2022.3.50f1`.

## Owner review: two WebGL clients

1. Build WebGL using the command above or `Bang-Sak > Build > Phase 28B WebGL`.
2. From the repository root, run:

   ```bash
   python3 -m http.server 8080 --bind 127.0.0.1 --directory unity/Build/WebGL
   ```

3. Open `http://127.0.0.1:8080/` in two separate browser tabs.
4. In tab 1, choose `ROOM`, then `CREATE`. Confirm `Connected`, room `1234`,
   `1/4 players`, and region `EU`.
5. In tab 2, choose `ROOM`, then `JOIN 1234`. Confirm both clients show
   `2/4 players`.
6. In tab 1, choose `BACK`, then `PLAY`. The room creator starts the round.
7. Confirm both clients enter the map, tab 1 is Taya, tab 2 has Hider/SAK
   controls, and both show one remaining Hider and Round 1.
8. Move each local player and confirm its remote replica follows in the other
   tab. Use Bang/SAK when the players are in range and confirm the outcome is
   visible in both tabs.
9. Reload tab 2, open `ROOM`, and `JOIN 1234` again. Confirm it returns directly
   to the active gameplay scene with the current round state.
10. Let the timer reach zero. Confirm both clients show `Hiders win!` and
    `Time is up.`
11. In tab 2, choose `Restart`. Confirm both clients enter Round 2.

## Verified live evidence

The implementation was exercised with two separate WebGL clients against the
configured Photon EU application:

- room `1234` reported `1/4`, then `2/4` players;
- Fusion scene authority loaded `PrototypeMap` on both clients;
- creator received Taya controls and joiner received Hider/SAK controls;
- both clients reported `HIDERS 1/1`;
- reliable movement and round-state streams were delivered in both directions;
- a reloaded joiner manually rejoined the active gameplay scene;
- both clients reached the same `Hiders win! / Time is up.` result;
- the joiner requested restart and both clients entered Round 2.

## Accepted Phase 32 limitations

- The room creator is the round authority; this is not a dedicated server.
- Shared Mode server-proxied reliable callbacks identify the local target, so
  the Phase 32 envelope carries the logical sender slot. Phase 33 must add
  stronger anti-spoof, rate-limit, and outcome validation.
- A joiner cannot start the first round. It waits for the room creator.
- Rejoin is manual; there is no automatic retry/backoff UI yet.
- Rejoining recreates the local prototype actor. A caught/disconnected actor's
  exact gameplay state is not restored yet; Phase 33 owns rejoin integrity.
- Fixed names and roster order remain prototype data.
- Photon logs a harmless warning when the runner connects on the main menu
  before the room creator selects the network gameplay scene.

## Stop rule

After the commit and GitHub push, stop for project-owner review. Do not begin
Phase 33 until Phase 32 is explicitly approved.
