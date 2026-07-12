# Bang-Sak Architecture

## Goal

Build Bang-Sak as a Unity 2D game with a production WebGL release and Palengke
integration, then add authoritative Photon multiplayer and carefully gated
mobile/content expansion without weakening the core rules.

## High-level architecture

```text
Player Browser -> bangsak.palengke.es -> Unity WebGL build (canonical)
Android device -> Unity Android client (planned)

Both clients
  -> Photon Cloud for authoritative rooms and multiplayer (planned)
  -> Palengke API for login, coins, scores, and leaderboard (live)
  -> Palengke API for cosmetics, badges, events, and tournaments (planned)
```

## Phase boundaries

The project must stay incremental:

- Local gameplay first.
- The local prototype, WebGL, Docker/Kubernetes, and initial Palengke API are
  already released.
- Authoritative Photon multiplayer is the next gameplay dependency.
- Gameplay-affecting content, anti-cheat, and tournaments build on authoritative
  multiplayer rather than trusting the client.
- Each expansion remains a separately reviewed phase.

## Planned subsystems

### Unity client

- Scenes: menu, lobby, prototype map, result, leaderboard.
- Player controller: top-down movement, physics, collision, animation.
- Local game state: roles, timer, caught state, hider count, result flow.
- Safe actions: Taya `Bang + player name` marker and hider close-range SAK counter.
- UI: desktop and mobile HUD, joystick, buttons, menus.

### Authoritative multiplayer planned

- Photon Fusion 2.
- Room create/join/leave.
- Network player spawning.
- Network movement sync.
- Network Bang-name and SAK-counter validation.
- Networked round state and results.

### Palengke API

- User identity.
- Score submit.
- Leaderboard.
- Coins/rewards.
- Rate limiting and abuse prevention.
- Planned cosmetic ownership, badges, event configuration, and tournaments.

### Deployment live

- Unity WebGL output.
- Static Docker image with Nginx.
- Kubernetes deployment/service/ingress.
- Argo CD GitOps app.
- Production monitoring and rollback notes.

### Content expansion planned

- Desktop/mobile accessibility and Android release path.
- Versioned audio, art, maps, cosmetics, badges, and seasonal variants.
- Selectable compatible components and map voting.
- Authoritative role power-ups with no paid gameplay advantage.
- Result integrity, anti-cheat hardening, and tournaments.

See [Phase 31 — Polish and content expansion plan](phase-31-polish-content-expansion.md).

## Security and config

- Do not commit production secrets.
- Photon App ID is client-visible config, but should still be loaded through runtime config when practical.
- Palengke API credentials and tokens must never be embedded in the WebGL client.
