# Bang-Sak Architecture

## Goal

Build Bang-Sak as a Unity 2D WebGL game that can start as a stable local prototype, then later add Photon multiplayer, WebGL hosting, Kubernetes deployment, Argo CD, and Palengke API integration.

## High-level architecture

```text
Player Browser
  -> games.palengke.es
  -> Unity WebGL build
  -> Photon Cloud for rooms and multiplayer
  -> Palengke API later for login, coins, scores, leaderboard, badges
```

## Phase boundaries

The project must stay incremental:

- Local gameplay first.
- Photon only after local prototype is stable.
- WebGL build only after the local Unity game works.
- Docker/Kubernetes only after WebGL works.
- Palengke API only after multiplayer works.

## Planned subsystems

### Unity client

- Scenes: menu, lobby, prototype map, result, leaderboard.
- Player controller: top-down movement, physics, collision, animation.
- Local game state: roles, timer, caught state, hider count, result flow.
- Safe actions: Bang marker/projectile, Tag/Close Tap, Sak base interaction.
- UI: desktop and mobile HUD, joystick, buttons, menus.

### Multiplayer later

- Photon Fusion 2.
- Room create/join/leave.
- Network player spawning.
- Network movement sync.
- Network Bang/Tag/Sak validation.
- Networked round state and results.

### Palengke API later

- User identity.
- Score submit.
- Leaderboard.
- Coins/rewards.
- Rate limiting and abuse prevention.

### Deployment later

- Unity WebGL output.
- Static Docker image with Nginx.
- Kubernetes deployment/service/ingress.
- Argo CD GitOps app.
- Production monitoring and rollback notes.

## Security and config

- Do not commit production secrets.
- Photon App ID is client-visible config, but should still be loaded through runtime config when practical.
- Palengke API credentials and tokens must never be embedded in the WebGL client.

