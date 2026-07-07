# Phase 14 Rules Documentation Correction

Phase 14 is documentation only.

No Unity code, scenes, prefabs, sprites, tests, or gameplay components are
changed in this phase. The goal is to make the real Bang-Sak rule explicit so
the next cleanup phases can be reviewed one at a time.

## Corrected rule

- There are hiders and Taya.
- Taya must find all hiders.
- When Taya sees a hider, Taya says `Bang` and the player name.
- A valid `Bang + player name` catches or neutralizes that hider.
- There is no base.
- Hiders can use `SAK` as a close-range counter against Taya.

## Safety adaptation

The Palengke version must keep the same gameplay idea without realistic weapon
or lethal visuals.

Use safe SAK visuals later:

- cartoon `SAK!` burst;
- harmless surprise tap;
- foam mark;
- playful impact sparkle.

Do not use:

- realistic knives;
- realistic guns;
- killing animations;
- blood or gore;
- lethal combat framing.

## New phased cleanup plan

1. Phase 14: document the corrected rule only.
2. Phase 15: remove the incorrect SAK base only.
3. Phase 16: remove the separate TAG mechanic only.
4. Phase 17: add Taya/Hider roles.
5. Phase 18: add the local `Bang + player name` catch rule.
6. Phase 19: add the safe hider SAK counter.

## Review checklist

- Confirm the README says Phase 14 is docs-only.
- Confirm `docs/gameplay-rules.md` has the corrected rule.
- Confirm `docs/roadmap.md` splits the cleanup into small reviewable phases.
- Confirm this phase does not modify Unity assets or code.
