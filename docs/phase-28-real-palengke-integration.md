# Phase 28 — Real Palengke integration

## Goal

Connect Bang-Sak to authenticated Palengke identity, persistent scores,
leaderboards, and coins while preserving a safe guest mode.

## Palengke backend

- Added authenticated session endpoint: `GET /games/bang-sak/session`.
- Added idempotent score submission: `POST /games/bang-sak/scores`.
- Added public leaderboard: `GET /games/bang-sak/leaderboard`.
- Added persistent per-game wallets and score submissions.
- Coin rewards are calculated server-side from the accepted score and capped at
  50 coins per round.
- Score range is validated and submissions are limited to 10 new rounds per
  user per minute.
- Reusing a round ID returns the original submission without awarding coins
  twice.

## Unity client

- `PalengkeApiClient` now uses the real Palengke endpoints.
- WebGL reads the existing shared Palengke access token through a small
  JavaScript bridge; tokens and credentials are never serialized in Unity.
- The main-menu leaderboard loads live scores and displays the authenticated
  user and coin balance.
- Completed rounds submit a bounded score using a unique idempotency key.
- Missing, expired, or unavailable sessions fall back to guest mode. Guests can
  play and view the leaderboard but cannot persist scores or earn coins.

## Score contract

The prototype score is deterministic and bounded:

- 500 points for a completed round result;
- 200 points per caught Hider;
- 5 points per whole second remaining;
- maximum accepted server score: 100,000.

This is a Phase 28 integration contract, not a final competitive balance.

## Deployment requirement

- Apply Palengke migration `074_bang_sak_game_scores.sql`.
- Deploy the Palengke backend before the Phase 28 Unity WebGL build.
- Add the production game origin to `PALENGKE_CORS_ORIGINS`.
- Host on a Palengke subdomain where the existing shared authentication cookie
  is available.

## How to review

### Guest mode

1. Open `Assets/Scenes/MainMenu.unity` and press Play in the Editor.
2. Confirm the footer says `Guest Player · 0 coins`.
3. Open `SCORES` and confirm local gameplay remains available if the backend is
   unavailable.
4. Complete a round and confirm there are no gameplay errors; guest scores are
   not submitted.

### Authenticated WebGL

1. Deploy migration 074 and the updated Palengke backend in a test environment.
2. Build and host Bang-Sak WebGL on an allowed Palengke origin.
3. Sign in to Palengke, open the game, and confirm the real display name and
   coin balance appear.
4. Complete a round and confirm the score persists once.
5. Re-submit the same round ID through an API test and confirm no coins are
   awarded twice.
6. Open `SCORES` and confirm the persisted score appears.

## Tests

- Palengke backend: `go test ./...`
- Unity EditMode:
  - `PalengkeApiClientTests`
  - `Phase28PalengkeIntegrationTests`
  - `PrototypeRoundRulesControllerTests`

## Exit criteria

- Authenticated Palengke identity and coins load in WebGL.
- Scores persist idempotently.
- Leaderboard reflects persisted best scores.
- Rewards are server-calculated and rate limited.
- Guest mode remains playable without persistence.
