# Phase 27A — Visible action cooldowns

## Goal

Make Bang and SAK recharge state obvious without changing action timing or game
rules.

## What changed

- The separate circular tsinelas button stays visually simple; cooldown
  feedback belongs to the named Hider buttons.
- Each named Bang button has an independent cooldown keyed by Hider name.
- Every Hider-name button has its own visible progress strip and seconds badge
  so each Hider's recharge is readable at the point of interaction.
- The named Bang panel now shows a progress bar, countdown, and `READY` state.
- The panel reserves a separate footer row and progress track so cooldown
  feedback does not overlap the Hider instruction.
- The SAK button now has the same radial countdown treatment.
- The existing `1.25` second duration is unchanged, but using Bang for one
  Hider no longer blocks Bang actions for other Hiders.

## How to review

1. Open `Assets/Scenes/MainMenu.unity` and press Play.
2. Select `PLAY` and enter the local round as Taya.
3. Click a named Hider button to throw the tsinelas.
4. Confirm only that Hider's button disables and counts down from about `1.2s`.
5. Immediately click a different Hider and confirm that action is available and
   starts its own independent countdown.
6. Confirm the circular tsinelas button has no cooldown overlay or countdown.
7. In a Hider/SAK test setup, use SAK and confirm its button shows the radial
   radial countdown and becomes available again.
8. Confirm the cooldown duration and Bang/SAK outcomes have not changed.

## Tests

Run all Unity EditMode tests, especially:

- `ActionCooldownDisplayTests`
- `BangActionControllerTests`
- `SakCounterControllerTests`

## Exit criteria

- Bang cooldown progress and seconds are visible on each Hider button.
- Each named Bang button blocks only its own Hider during cooldown.
- SAK cooldown progress and seconds are visible.
- Players receive a clear `READY` state.
- Gameplay timing remains unchanged.
