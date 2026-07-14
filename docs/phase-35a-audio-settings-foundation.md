# Phase 35A — Persistent audio settings foundation

## Status

Complete by authorized nonvisual self-review on 2026-07-14. Google Play Phase
34J remains open as an independent external distribution gate; this phase does
not alter or authorize Phase 34K.

## Goal

Add the smallest reusable audio preference layer before any visible controls or
sound assets are introduced.

## Included

- persistent mute state;
- persistent master, music, and SFX levels;
- `0..1` clamping for every stored level;
- deterministic `source × master × channel` volume resolution;
- immediate silence while muted;
- one change notification for each effective stored change;
- a read-only snapshot for future UI and audio-source adapters.

Defaults remain neutral at full volume. Content direction and the final mix are
deferred until actual cues are introduced.

## Explicitly excluded

- Settings-panel controls or any other visible change;
- audio clips, music, ambient loops, or generated sound;
- `AudioSource` or mixer wiring;
- gameplay, Photon, API, reward, or production deployment changes;
- Play Console mutation or public rollout.

## Acceptance criteria and evidence

1. Mute, master, music, and SFX preferences use stable `bangsak.audio.*`
   `PlayerPrefs` keys.
2. The default snapshot is unmuted with all three levels at `1.0`.
3. Inputs below zero and above one are clamped before persistence.
4. Music and SFX resolve independently through the master level and optional
   per-source level.
5. Mute resolves every supported channel to silence.
6. Unknown channels fail explicitly and repeated identical writes do not emit
   duplicate notifications.
7. No scene, prefab, image, control, or sound asset changed.
8. Unity EditMode result:
   `242 passed, 0 failed, 0 skipped` on Unity `2022.3.50f1`, including all six
   focused `Phase35AAudioSettingsTests` cases.

Test report: `/tmp/bang-sak-phase35a-editmode.xml`.

## Next checkpoint

Phase 35B adds only the visible mute and master/music/SFX controls. Because that
changes the Settings panel, it must stop with desktop and Android screenshots
for explicit owner approval.
