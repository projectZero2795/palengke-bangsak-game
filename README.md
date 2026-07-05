# Bang-Sak for Palengke

Bang-Sak is a safe Filipino community hide-and-seek/tag game planned for Unity 2D WebGL at `games.palengke.es`.

The target experience is the reference image provided by the project owner: a polished 2D top-down barangay/palengke game with menu, lobby, round gameplay, result screen, leaderboard, mobile controls, and later Photon multiplayer.

Canonical visual reference: [Bang-Sak roadmap reference](docs/reference/bang-sak-roadmap-reference.jpg).

## Current phase

This repository is currently in **Phase 0: Repository and documentation**.

Phase 0 contains docs, architecture, roadmap, object catalog, maintenance rules, and GitHub issues only.

No Unity project, gameplay code, prefabs, sprites, Photon setup, Docker files, Kubernetes manifests, or deployment files should be added until their specific phase starts.

## Safety and branding rules

Bang-Sak must stay casual, playful, and community-safe.

Do not implement:

- realistic guns;
- knives;
- blood;
- gore;
- lethal combat;
- violent hit reactions.

Use safe visual metaphors instead:

- `Bang` = finger-gun marker, toy dart, cartoon light beam, foam tag, or tsinelas projectile;
- close-range action = `Tag`, `Close Tap`, or harmless foam-touch action;
- caught state = dizzy stars, playful freeze, waiting-zone state, or friendly HUD indicator.

## Target stack

- Game engine: Unity 2D.
- Target build: WebGL first.
- Public host later: `games.palengke.es`.
- Multiplayer later: Photon Fusion 2.
- Palengke integration later: login, points, coins, leaderboard, badges.

## Phase rule

Work on one phase only at a time.

After each phase:

1. Commit changes.
2. Push to GitHub.
3. Update docs if needed.
4. Create or update GitHub issues.
5. Explain how to test.
6. Stop and wait for review before continuing.

Do not skip ahead.

## Documentation

- [Architecture](docs/architecture.md)
- [Development roadmap](docs/roadmap.md)
- [Object catalog from the reference image](docs/object-catalog.md)
- [Maintenance plan](docs/maintenance.md)
- [Phase 0 review checklist](docs/phase-0-review.md)
