# Phase 27A — Visible action cooldowns

## Goal

Make Bang and SAK recharge state obvious without changing action timing or game
rules.

## What changed

- The main tsinelas Bang button now has a radial remaining-time overlay and a
  seconds label.
- Named Bang buttons are disabled together while Bang is cooling down.
- Every Hider-name button has its own visible progress strip and seconds badge
  so the shared Bang recharge is readable at the point of interaction.
- The named Bang panel now shows a progress bar, countdown, and `READY` state.
- The panel reserves a separate footer row and progress track so cooldown
  feedback does not overlap the Hider instruction.
- The SAK button now has the same radial countdown treatment.
- Existing `1.25` second Bang and SAK cooldown values are unchanged.

## How to review

1. Open `Assets/Scenes/MainMenu.unity` and press Play.
2. Select `PLAY` and enter the local round as Taya.
3. Click a named Hider button to throw the tsinelas.
4. Confirm all named buttons temporarily disable, the shared bar refills, and
   the countdown changes from about `1.2s` to `READY`.
5. Confirm the circular tsinelas button shows a radial cooldown overlay.
6. In a Hider/SAK test setup, use SAK and confirm its button shows the same
   radial countdown and becomes available again.
7. Confirm the cooldown duration and Bang/SAK outcomes have not changed.

## Tests

Run all Unity EditMode tests, especially:

- `ActionCooldownDisplayTests`
- `BangActionControllerTests`
- `SakCounterControllerTests`

## Exit criteria

- Bang cooldown progress and seconds are visible.
- Named Bang buttons cannot be clicked during cooldown.
- SAK cooldown progress and seconds are visible.
- Players receive a clear `READY` state.
- Gameplay timing remains unchanged.
