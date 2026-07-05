#!/usr/bin/env bash
set -euo pipefail

repo="projectZero2795/palengke-bangsak-game"

gh label create "phase" --repo "$repo" --color "1d76db" --description "Bang-Sak phased development task" 2>/dev/null || true
gh label create "safe-gameplay" --repo "$repo" --color "0e8a16" --description "No realistic weapons, gore, or lethal framing" 2>/dev/null || true
gh label create "needs-review" --repo "$repo" --color "fbca04" --description "Stop and review before continuing" 2>/dev/null || true

create_issue() {
  local title="$1"
  local body="$2"
  gh issue create --repo "$repo" --title "$title" --label "phase,safe-gameplay,needs-review" --body "$body"
}

create_issue "Phase 0: Repository and documentation" "$(cat <<'BODY'
Goal: create the GitHub repo, docs, architecture, roadmap, maintenance plan, object catalog, and issues only.

Deliverables:
- [ ] README
- [ ] Architecture docs
- [ ] Development roadmap
- [ ] Maintenance plan
- [ ] Object catalog from reference image
- [ ] GitHub issues per phase

Tests:
- [ ] Repo exists
- [ ] Docs are clear
- [ ] No gameplay code yet

Exit criteria:
- [ ] Project structure is approved
BODY
)"

create_issue "Phase 1: Unity base project" "$(cat <<'BODY'
Goal: create a clean Unity 2D project foundation.

Deliverables:
- [ ] Unity project inside /unity
- [ ] MainMenu scene
- [ ] PrototypeMap scene
- [ ] Script/art/prefab folder structure

Tests:
- [ ] Project opens in Unity
- [ ] PrototypeMap scene runs
- [ ] Camera works

Exit criteria:
- [ ] Empty game scene loads without errors
BODY
)"

create_issue "Phase 2: Player design" "$(cat <<'BODY'
Goal: create the first readable player character.

Deliverables:
- [ ] Simple 2D top-down player sprite
- [ ] Idle placeholder
- [ ] Walking placeholder
- [ ] Player prefab
- [ ] Player color variants

Tests:
- [ ] Player appears in scene
- [ ] Scale is correct
- [ ] Player is readable on the map

Exit criteria:
- [ ] Player design approved
BODY
)"

create_issue "Phase 3: Player physics and movement" "$(cat <<'BODY'
Goal: make the player move correctly.

Deliverables:
- [ ] Rigidbody2D setup
- [ ] Collider2D setup
- [ ] Keyboard movement
- [ ] Mobile joystick placeholder
- [ ] Movement speed config
- [ ] Wall collision enabled

Tests:
- [ ] Moves up/down/left/right
- [ ] Cannot pass through walls
- [ ] Smooth movement
- [ ] No physics jitter

Exit criteria:
- [ ] Movement and collision stable
BODY
)"

create_issue "Phase 4: Player animation" "$(cat <<'BODY'
Goal: make movement visually clear.

Deliverables:
- [ ] Idle animation
- [ ] Walk animation
- [ ] Direction-facing logic
- [ ] Basic animation controller

Tests:
- [ ] Faces movement direction
- [ ] Animation changes between idle and walking
- [ ] No animation glitches

Exit criteria:
- [ ] Player movement looks acceptable
BODY
)"

create_issue "Phase 5: Bang action design" "$(cat <<'BODY'
Goal: design the safe Bang action.

Deliverables:
- [ ] Cartoon Bang marker concept
- [ ] Safe visual option selected: finger-gun, toy dart, light beam, foam tag, or tsinelas
- [ ] Bang button
- [ ] Cooldown
- [ ] Range indicator

Tests:
- [ ] Pressing Bang triggers safe animation/effect
- [ ] Cooldown prevents spam
- [ ] Effect direction follows player facing

Exit criteria:
- [ ] Bang action is visually safe and clear
BODY
)"

create_issue "Phase 6: Bang projectile / hit physics" "$(cat <<'BODY'
Goal: make Bang detect another player.

Deliverables:
- [ ] Projectile or raycast hit detection
- [ ] Hitbox/collider
- [ ] Range limit
- [ ] Travel speed if projectile-based
- [ ] Hit feedback
- [ ] Miss feedback

Tests:
- [ ] Hits target player in range
- [ ] Misses outside range
- [ ] Blocked by walls if configured
- [ ] No duplicate hits from one shot

Exit criteria:
- [ ] Bang detection works consistently
BODY
)"

create_issue "Phase 7: Close tag action" "$(cat <<'BODY'
Goal: create the close-range alternative to Bang.

Deliverables:
- [ ] Rename knife mechanic to Tag or Close Tap
- [ ] Harmless close-tap visual
- [ ] Short-range cone/circle detection
- [ ] Tag animation
- [ ] Cooldown

Tests:
- [ ] Works only at close range
- [ ] Does not work through walls
- [ ] Cannot be spammed
- [ ] Target receives caught state

Exit criteria:
- [ ] Close-range tag mechanic works safely
BODY
)"

create_issue "Phase 8: Caught state" "$(cat <<'BODY'
Goal: when a hider is caught, the game state changes.

Deliverables:
- [ ] isCaught state
- [ ] Caught animation
- [ ] Disable movement or move to waiting area
- [ ] UI indicator for caught players

Tests:
- [ ] Hit player becomes caught
- [ ] Caught player cannot continue until round resets
- [ ] Hiders-left counter updates

Exit criteria:
- [ ] Caught state reliable
BODY
)"

create_issue "Phase 9: Soil / ground tiles" "$(cat <<'BODY'
Goal: create the map foundation.

Deliverables:
- [ ] Soil tile
- [ ] Road/path tile
- [ ] Grass tile
- [ ] Concrete tile
- [ ] Tilemap setup
- [ ] Sorting layers

Tests:
- [ ] Ground tiles render correctly
- [ ] Player appears above ground
- [ ] No visual gaps

Exit criteria:
- [ ] Basic map floor ready
BODY
)"

create_issue "Phase 10: Trees and natural objects" "$(cat <<'BODY'
Goal: add first obstacles and hiding objects.

Deliverables:
- [ ] Tree sprites
- [ ] Bush sprites
- [ ] Plant pots
- [ ] Collision setup
- [ ] Optional partial hiding zones

Tests:
- [ ] Player collides with tree trunks
- [ ] Player can move around trees
- [ ] Hiding objects do not block camera clarity

Exit criteria:
- [ ] Natural obstacles work
BODY
)"

create_issue "Phase 11: Houses" "$(cat <<'BODY'
Goal: create barangay-style houses.

Deliverables:
- [ ] Small house sprite
- [ ] Medium house sprite
- [ ] Fence pieces
- [ ] Gates
- [ ] Wall collisions
- [ ] Roof sorting if needed

Tests:
- [ ] Player cannot walk through houses
- [ ] Houses are readable
- [ ] Houses create hiding paths

Exit criteria:
- [ ] First residential map area works
BODY
)"

create_issue "Phase 12: Stores and stalls" "$(cat <<'BODY'
Goal: create Filipino-themed marketplace objects.

Deliverables:
- [ ] Sari-sari store
- [ ] Palengke stall
- [ ] Food stall
- [ ] Small signboards
- [ ] Crates/baskets
- [ ] Collision setup

Tests:
- [ ] Stores appear correctly
- [ ] Player can navigate around stalls
- [ ] Map feels Filipino/Palengke-themed

Exit criteria:
- [ ] Marketplace area works
BODY
)"

create_issue "Phase 13: Base point (Sak)" "$(cat <<'BODY'
Goal: create the Sak base.

Deliverables:
- [ ] Base object
- [ ] Base collider/trigger
- [ ] Base visual marker
- [ ] Sak button appears only when hider is near base
- [ ] Base interaction script

Tests:
- [ ] Hider near base can press Sak
- [ ] Hider far from base cannot press Sak
- [ ] Taya cannot use Sak

Exit criteria:
- [ ] Base mechanic works locally
BODY
)"

create_issue "Phase 14: Map layout v1" "$(cat <<'BODY'
Goal: place all objects into a playable map.

Deliverables:
- [ ] Small map with houses, trees, sari-sari store, palengke stalls, roads, base, hiding routes
- [ ] Spawn points
- [ ] Camera boundary

Tests:
- [ ] Player can move around full map
- [ ] No blocked/unreachable areas
- [ ] Map supports chasing and hiding

Exit criteria:
- [ ] First playable map approved
BODY
)"

create_issue "Phase 15: Role system" "$(cat <<'BODY'
Goal: add Taya and Hider roles.

Deliverables:
- [ ] Role enum: Taya, Hider
- [ ] Local role assignment test
- [ ] Different UI per role
- [ ] Different player color/marker per role

Tests:
- [ ] Taya sees Bang/Tag button
- [ ] Hider sees Sak near base
- [ ] Role display is correct

Exit criteria:
- [ ] Roles work locally
BODY
)"

create_issue "Phase 16: Round rules" "$(cat <<'BODY'
Goal: make the local game loop work.

Deliverables:
- [ ] Round timer
- [ ] Hiders-left counter
- [ ] Taya win condition
- [ ] Hider win condition
- [ ] Restart round
- [ ] Result screen

Tests:
- [ ] Taya wins when all hiders caught
- [ ] Hiders win when Sak succeeds
- [ ] Timer ending has configured result
- [ ] Round can restart

Exit criteria:
- [ ] Local single-device prototype playable
BODY
)"

create_issue "Phase 17: UI polish" "$(cat <<'BODY'
Goal: make the MVP understandable.

Deliverables:
- [ ] Main menu
- [ ] How to Play screen
- [ ] Game HUD
- [ ] Result screen
- [ ] Settings placeholder

Tests:
- [ ] New player understands the game
- [ ] Buttons clear on desktop and mobile
- [ ] No broken UI scaling

Exit criteria:
- [ ] Local prototype usable
BODY
)"

create_issue "Phase 18: Photon setup" "$(cat <<'BODY'
Goal: prepare multiplayer without changing game logic too much.

Deliverables:
- [ ] Photon Fusion 2 installed
- [ ] Photon App ID config documented
- [ ] Network bootstrap scene
- [ ] Create room
- [ ] Join room
- [ ] Leave room

Tests:
- [ ] Two clients can join same room
- [ ] Room list/code works
- [ ] No App ID committed as a secret

Exit criteria:
- [ ] Multiplayer room connection works
BODY
)"

create_issue "Phase 19: Multiplayer player spawning" "$(cat <<'BODY'
Goal: spawn players over Photon.

Deliverables:
- [ ] Network player prefab
- [ ] Spawn points
- [ ] Player ownership
- [ ] Networked player names
- [ ] Local camera follows owned player

Tests:
- [ ] Each client controls only own player
- [ ] Other players visible
- [ ] Players spawn correctly

Exit criteria:
- [ ] Multiplayer movement foundation works
BODY
)"

create_issue "Phase 20: Multiplayer movement sync" "$(cat <<'BODY'
Goal: sync player positions.

Deliverables:
- [ ] NetworkTransform or equivalent
- [ ] Smooth remote movement
- [ ] Basic lag handling
- [ ] Movement validation notes

Tests:
- [ ] Two or more players see each other moving
- [ ] Movement smooth enough
- [ ] No duplicate players

Exit criteria:
- [ ] Multiplayer movement stable
BODY
)"

create_issue "Phase 21: Multiplayer Bang/Tag sync" "$(cat <<'BODY'
Goal: make catch mechanics work online.

Deliverables:
- [ ] Networked Bang action
- [ ] Networked Tag action
- [ ] Host-side validation where possible
- [ ] Caught state synced
- [ ] Hit feedback synced

Tests:
- [ ] Taya catches hider online
- [ ] All clients see caught state
- [ ] Invalid hits rejected where possible

Exit criteria:
- [ ] Online catching works
BODY
)"

create_issue "Phase 22: Multiplayer Sak sync" "$(cat <<'BODY'
Goal: make base interaction work online.

Deliverables:
- [ ] Networked base trigger
- [ ] Networked Sak action
- [ ] Hider win event
- [ ] Round result sync

Tests:
- [ ] Hider reaches base online
- [ ] All clients see result
- [ ] Round can restart

Exit criteria:
- [ ] Full online round works
BODY
)"

create_issue "Phase 23: WebGL build" "$(cat <<'BODY'
Goal: build browser version.

Deliverables:
- [ ] Unity WebGL build
- [ ] Compression settings documented
- [ ] Browser compatibility notes
- [ ] Test page

Tests:
- [ ] Game runs in browser
- [ ] Photon connects from browser
- [ ] Keyboard controls work
- [ ] Mobile browser basic test

Exit criteria:
- [ ] WebGL build works locally
BODY
)"

create_issue "Phase 24: Docker static hosting" "$(cat <<'BODY'
Goal: serve WebGL files with Nginx.

Deliverables:
- [ ] Dockerfile
- [ ] Nginx config
- [ ] Local docker-compose test
- [ ] Correct MIME types for Unity WebGL files

Tests:
- [ ] Container serves game
- [ ] Browser loads WebGL files
- [ ] No MIME/compression errors

Exit criteria:
- [ ] Containerized WebGL hosting works
BODY
)"

create_issue "Phase 25: Kubernetes deployment" "$(cat <<'BODY'
Goal: deploy to cluster.

Deliverables:
- [ ] Deployment
- [ ] Service
- [ ] Ingress
- [ ] Namespace decision documented
- [ ] Image from registry
- [ ] TLS if cert-manager exists

Tests:
- [ ] Pod runs
- [ ] Ingress responds
- [ ] games.palengke.es loads game

Exit criteria:
- [ ] Game publicly reachable
BODY
)"

create_issue "Phase 26: Argo CD integration" "$(cat <<'BODY'
Goal: make deployment GitOps-managed.

Deliverables:
- [ ] Argo CD Application
- [ ] Automated sync if consistent with Palengke
- [ ] Self-heal if appropriate
- [ ] README deployment steps

Tests:
- [ ] Argo CD syncs app
- [ ] Rollback possible
- [ ] Changes deploy from Git

Exit criteria:
- [ ] Deployment maintainable
BODY
)"

create_issue "Phase 27: Palengke API placeholder" "$(cat <<'BODY'
Goal: prepare future integration.

Deliverables:
- [ ] PalengkeApiClient
- [ ] Mock user
- [ ] Mock leaderboard
- [ ] Mock coins
- [ ] Configurable API base URL

Tests:
- [ ] Game works without API
- [ ] Mock leaderboard displays
- [ ] No dependency on production Palengke yet

Exit criteria:
- [ ] API layer isolated
BODY
)"

create_issue "Phase 28: Real Palengke integration" "$(cat <<'BODY'
Goal: connect game to Palengke ecosystem.

Deliverables:
- [ ] Login/session validation
- [ ] User identity
- [ ] Score submit
- [ ] Leaderboard
- [ ] Coins/rewards
- [ ] Rate limiting

Tests:
- [ ] Logged-in Palengke user can play
- [ ] Score persists
- [ ] Guest mode behavior defined

Exit criteria:
- [ ] Game connected to Palengke safely
BODY
)"

create_issue "Phase 29: Monitoring and maintenance" "$(cat <<'BODY'
Goal: make the game production-friendly.

Deliverables:
- [ ] Basic logs
- [ ] Error tracking plan
- [ ] Versioning
- [ ] Release notes
- [ ] Backup/config documentation
- [ ] Known issues page

Tests:
- [ ] Errors can be investigated
- [ ] Rollback documented
- [ ] Releases repeatable

Exit criteria:
- [ ] Long-term maintenance documented
BODY
)"

create_issue "Phase 30: Polish and content expansion" "$(cat <<'BODY'
Goal: improve the game after MVP.

Possible additions:
- [ ] More maps
- [ ] Better sprites
- [ ] Skins
- [ ] Badges
- [ ] Seasonal events
- [ ] Tournaments
- [ ] Sound effects
- [ ] Mobile Android build
- [ ] Better anti-cheat

Tests:
- [ ] Additions do not break core gameplay
- [ ] Mobile and desktop still work
- [ ] Safety rules still respected

Exit criteria:
- [ ] Content expansion plan approved
BODY
)"
