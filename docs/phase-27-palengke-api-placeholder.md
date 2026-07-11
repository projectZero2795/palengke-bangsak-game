# Phase 27 — Palengke API placeholder

## Goal

Prepare an isolated integration boundary for future Palengke accounts, coins,
and leaderboards without contacting or depending on the production service.

## What changed

- Added `PalengkeApiClient` with a configurable, normalized API base URL.
- Added an offline mock user (`JuanP`) with a mock coin balance.
- Added a ranked five-player mock leaderboard.
- Added a `SCORES` dashboard tile and offline leaderboard panel to `MainMenu`.
- Updated the menu footer to show the mock player and coins.
- Added EditMode coverage for the client contract, mock data, URL configuration,
  and leaderboard UI.

## Isolation and safety

- `useMockData` is enabled by default.
- `IsProductionApiEnabled` is always false in this phase.
- No HTTP request code, credentials, tokens, or production dependency is added.
- Disabling mock mode fails explicitly instead of silently contacting a server.

## How to review

1. Open `/unity` with Unity `2022.3.50f1`.
2. Open `Assets/Scenes/MainMenu.unity` and press Play.
3. Confirm the footer shows `JuanP · 125 coins`.
4. Click `SCORES` or press `L`.
5. Confirm the offline leaderboard shows five ranked mock players and highlights
   `JuanP`.
6. Confirm the panel says that it is an offline mock and no API request is made.
7. Return with `BACK` or Escape, then start a local game and confirm it still
   works without any API service.

## Tests

Run Unity EditMode tests, especially:

- `PalengkeApiClientTests`
- `Phase27PalengkeApiTests`

Expected result: all tests pass.

## Exit criteria

- The API boundary is isolated from gameplay.
- API base URL is configurable.
- Mock user, coins, and leaderboard are available offline.
- Mock leaderboard is visible from the main menu.
- The game has no production Palengke dependency.
