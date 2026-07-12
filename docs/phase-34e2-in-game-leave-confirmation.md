# Phase 34E2: In-game leave confirmation

## Goal

Add only an accessible **LEAVE GAME** control during an active multiplayer
round, with a confirmation dialog that supports Cancel and confirmed return to
the main menu.

Remote roster cleanup, freed-slot behavior, Taya reassignment, authority
migration, last-player rules, background/resume, and reconnect are not part of
this phase.

## Visual behavior

- The button appears at the safe-area top-left only during a connected Photon
  game. It is hidden in local play.
- Tapping it opens a centered, high-contrast confirmation over a dim input
  blocker.
- **CANCEL** closes the dialog and the same round continues.
- **LEAVE GAME** disables repeated confirmation, shows `Leaving room...`,
  shuts down the local Photon session, and returns that client to MainMenu.
- Existing readable-text, high-contrast, reduced-motion, and safe-area behavior
  applies to the new UI.

## Owner visual review

Because this phase adds visible controls, Codex must provide screenshots and
stop for explicit owner approval.

1. Review the connected gameplay screenshot and confirm the top-left button
   does not overlap the round HUD or play area controls.
2. Review the confirmation screenshot and confirm the message and both actions
   are readable.
3. Tap **CANCEL** and confirm the same round remains active.
4. Open the dialog again, tap **LEAVE GAME**, and confirm that client returns to
   MainMenu.

## Implementation evidence

- `227` EditMode tests passed with `0` failures;
- a clean IL2CPP ARM64 Android debug build completed as version `0.34.4`;
- the APK supports API 29+, targets API 35, and is `52,934,718` bytes;
- APK SHA-256:
  `6b56322c2735f44b010afef03b6d14e9f8e7fa42ec9148742ae03f15e30c58b2`;
- two Android 15 clients created/joined room `1234` and entered the synchronized
  round;
- the connected client displayed the safe-area top-left control without
  overlapping the centered round HUD, joystick, or named-Bang panel;
- the modal dimmed and blocked the game view while keeping its message,
  **CANCEL**, and **LEAVE GAME** readable;
- Cancel returned to the same running round and its timer continued;
- confirmed leave returned only the leaving client to MainMenu;
- no fatal Android exception or Photon leave warning was recorded;
- remote cleanup/role/authority behavior was not evaluated because it belongs
  to Phase 34E3.

## Stop boundary

Stop after the screenshots and focused acceptance check. Phase 34E3 remote
cleanup/rules remains blocked until explicit visual approval.
