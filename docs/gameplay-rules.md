# Bang-Sak Gameplay Rules

This file captures gameplay decisions that must be considered when their
implementation phase arrives. Do not implement a later rule early just because
it is documented here.

## Core Bang-Sak rule

Bang-Sak has two roles:

- `Taya`
- `Hider`

The corrected game loop is:

1. Taya searches for all hiders.
2. When Taya sees a hider, Taya calls `Bang` plus that hider/player name.
3. A valid `Bang + name` catches or neutralizes that hider for the round.
4. Hiders do not run to a base.
5. Hiders can use `SAK` as a close-range counter against Taya.

`SAK` is the hider counter. In the Palengke game it must be represented safely:

- harmless surprise tap;
- foam mark;
- cartoon `SAK!` burst;
- friendly non-lethal impact feedback.

Do not implement realistic knives, killing animations, blood, gore, or lethal
combat.

Phase plan for this correction:

- Phase 14: document the corrected rule only.
- Phase 15: remove the incorrect SAK base only.
- Phase 16: remove the separate TAG mechanic only.
- Phase 17: add Taya/Hider roles.
- Phase 18: add the local `Bang + player name` catch rule.
- Phase 19: add the safe hider SAK counter.

## Stealth and reveal rule

Bang-Sak is a nighttime barangay/palengke game. Hiding should be useful, but
not permanent.

### Hide duration

- A hider can stay hidden for up to `5` seconds.
- After `5` seconds, the game should reveal that hider through a friendly
  nighttime clue.
- The reveal should not be violent or scary.

Recommended reveal clues:

- a nearby streetlight turns on or flickers toward the hiding player;
- a house/window light turns on near the hiding player;
- a neighborhood dog barks toward the hiding player.

These are examples of the scalable `RevealSource` category. Future reveal
objects must follow the shared category contract in
[Object Design, Implementation, and Versioning](object-design-versioning.md),
so new reveal providers can be added without rewriting the stealth rules.

Implementation phase:

- Phase 10/11 can add visual/audio reveal-source objects such as streetlights,
  house lights, or dog markers.
- Phase 16 should implement the actual round-rule timer and reveal state.
- Phase 21 should synchronize this reveal state online.

### Walk vs run reveal

Movement should support stealth:

- Walking should not reveal the hider direction.
- Running should reveal the hider direction, similar to directional noise clues
  in games like PUBG.
- The reveal should show direction/intensity, not an exact permanent wallhack.
- The clue can be a sound ripple, footprint/noise arrow, dog bark direction, or
  light sweep.

Recommended initial tuning:

- walk speed: safer and quieter;
- run speed: faster but produces directional reveal pings;
- reveal ping cadence while running: short repeated pings, not continuous exact
  tracking;
- hiding timeout: `5` seconds before a reveal clue is triggered.

Implementation note:

- The run/walk system should emit a generic reveal event such as
  `RevealEvent(direction, intensity, duration)` instead of coupling directly to
  a specific dog/light/street object.
- RevealSource objects can then decide how to display that event based on their
  type, version, and variant.

### Gameplay intent

This mechanic should create a fun tradeoff:

- hiders can hide briefly to break line-of-sight;
- taya gets clues when hiders camp too long;
- running helps escape but gives away direction;
- walking is slower but stealthier.

Keep the mechanic readable on mobile and safe for kids/community branding.

## Component selection intent

Gameplay rules should eventually be selectable through safe component versions
and variants.

Examples:

- admins can enable the default `five_second_hide` reveal rule;
- rooms can use a compatible Taya/player variant;
- players can vote for maps with different styles or RevealSource categories;
- seasonal modes can use different object variants without changing the core
  game code.

Do not implement gameplay-affecting selections as uncontrolled client-only
choices. They must be selected by admin/room/map/vote config and validated
against the component compatibility rules.
