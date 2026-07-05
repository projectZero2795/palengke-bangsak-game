# Maintenance Plan

## Working rule

Bang-Sak must be built in small, reviewable phases. Each phase has a GitHub issue and must be completed independently.

## Branching and commits

- Use small commits with clear messages.
- Each phase should be easy to review from a diff.
- Do not mix future-phase implementation into the current phase.

## Per-phase checklist

Every phase must end with:

- changes committed;
- changes pushed;
- README/docs updated if behavior or setup changed;
- GitHub issue updated;
- test steps documented;
- known risks documented;
- review requested.

## Safety review

Before merging or deploying any gameplay phase, verify:

- no realistic guns;
- no knives;
- no blood/gore;
- no lethal framing;
- action labels remain playful: Bang, Tag, Close Tap, Sak;
- visuals stay suitable for a Filipino community game.

## Deployment rule

Do not deploy until the correct phase:

- No Kubernetes before WebGL build works.
- No Argo CD before Kubernetes deployment works.
- No Palengke API before multiplayer works.

## Configuration rule

- Photon App ID must be documented and configured when Photon starts.
- Do not commit production secrets.
- Runtime config should be preferred for public/client-visible configuration.

## Release notes

Each release should include:

- version or commit;
- phase completed;
- test summary;
- known issues;
- rollback notes once deployment phases begin.

