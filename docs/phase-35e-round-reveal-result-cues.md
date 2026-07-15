# Phase 35E — Round, reveal, pickup-ready, and result cues

## Status

Implemented and awaiting the project owner's listening review.

On 2026-07-15 the owner approved continuing after reviewing the semantic
version-2 menu and Bang/SAK direction. Phase 35E therefore adds only the next
audio layer. Phase 35F ambient audio has not started.

## Dependency correction

Round start and result states already exist and are authoritative, so their
cues are live in this phase. The planned reveal mechanic and match pickups do
not exist yet. Their versioned cue definitions and explicit confirmed-state
hooks are included, but no current controller calls them. Reveal remains silent
until Phase 42C owns a confirmed reveal state; pickup-ready remains silent until
Phase 39 owns authoritative pickup state. Phase 35E does not fabricate either
mechanic merely to make a sound play.

## Versioned cue contract

The set ID is `bangsak.round_reveal_result_cues`, set version `1`, with minimum
compatible version `1`. Unknown future cues must be ignored by older clients.

| Cue | Stable ID | Version | Semantic signature | Duration | Cue level | Live binding |
| --- | --- | ---: | --- | ---: | ---: | --- |
| Round started | `round.started` | 1 | Two count-in taps followed by a brighter go tone | 320 ms | 20% | Local confirmed start/restart and new remote running snapshot |
| Reveal confirmed | `reveal.confirmed` | 1 | Two separated radar-style pings | 240 ms | 18% | Reserved for Phase 42C confirmed reveal state |
| Pickup ready confirmed | `pickup.ready_confirmed` | 1 | Three-note rising ready sparkle | 180 ms | 19% | Reserved for Phase 39 authoritative pickup state |
| Taya wins | `result.taya_wins` | 1 | Firm two-note resolved cadence | 290 ms | 22% | Local confirmed result and new remote result snapshot |
| Hiders win | `result.hiders_win` | 1 | Playful four-note escape cadence | 310 ms | 22% | Local confirmed result and new remote result snapshot |

All cue levels are multiplied by the Phase 35A master and SFX levels. Mute
resolves the live output to zero.

## State binding and visual equivalents

- `PrototypeRoundRulesController.StartRound` publishes only after the state,
  timer, actor reset, counts, and round number are confirmed. Restart therefore
  gets one new start cue.
- `FinishRound` publishes the winner-specific cue only after the result title,
  message, state, and remaining time are final. Repeated ticks cannot replay it.
- A remote client publishes only when an authoritative network snapshot enters
  a new running round or a new finished result. An identical repeated snapshot
  stays silent.
- The existing timer, `ROUND` label, hider count, full result panel, winner
  title/message, restart control, and frozen actor state remain the visual and
  gameplay authority. Audio never decides a result.
- Reveal and pickup-ready service methods are intentionally explicit
  `Confirmed` hooks. They have no runtime caller until their future visual and
  authoritative states exist.

## Playback and compatibility

`BangSakRoundCueCatalog` deterministically synthesizes five mono 44.1 kHz
clips with distinct rhythmic and timbral profiles. No binary sound asset or
third-party sample is added. `BangSakRoundCuePlayer` caches exactly five clips
on one persistent 2D `AudioSource`, follows live master/SFX/mute changes, and
uses at most one voice plus one bounded pending cue. Playback or listener
failure is contained and cannot prevent or roll back round state.

## Objective evidence

| Check | Result |
| --- | --- |
| Focused Phase 35E tests | 8/8 passed |
| Complete EditMode suite | 267/267 passed |
| Procedural sample budget | 236,376 bytes total; test gate remains below 256 KiB |
| Fresh WebGL build | Passed; 37,626,615 bytes |
| Desktop browser smoke | Fresh uncached build loaded and entered `ROUND 1`; no audio/runtime warning or error |

## Owner audio review

1. Set Mute to Off and Master/SFX to 100%.
2. Select Play and confirm the round starts with two count-in taps and a clear
   brighter go tone while the timer and `ROUND 1` remain visible.
3. Let Hiders win by time or SAK and confirm the playful escape cadence plays
   once with the Hiders result panel.
4. Restart, catch all Hiders, and confirm the firmer Taya cadence plays once
   with the Taya result panel.
5. Repeat a received result snapshot or continue waiting on the result screen;
   confirm the result cue does not replay.
6. Lower SFX, enable Mute, and confirm the timer/result UI remains complete.
7. Confirm that start, Taya win, and Hiders win communicate different meanings
   without looking at the screen.

Reveal and pickup-ready cannot be owner-listened in live gameplay yet because
their mechanics deliberately do not exist. Their hooks are covered by focused
tests and will be reviewed in their owning phases.

## Rollback

Remove the three Phase 35E audio classes and the publication calls in
`PrototypeRoundRulesController`. Phase 35A settings and Phase 35C/35D cues
remain independent and require no data migration.
