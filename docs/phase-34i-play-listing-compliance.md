# Phase 34I: Play listing and compliance

## Result

Phase 34I was completed and sent to Google for review on 2026-07-14 under the
owner's explicit authorization. Google Play Console removed the app-setup gate
and unlocked closed testing after all required listing and app-content tasks
were completed.

This is listing/compliance evidence only. It does not claim a public Play Store
release.

## Store listing

- App name: `Bang-Sak for Palengke`
- Package: `es.palengke.bangsak`
- Category: Game / Casual
- Support email: `soporte@palengke.es`
- Website: `https://bangsak.palengke.es`
- Privacy policy: `https://palengke.es/privacy`
- Short description: `A Filipino-inspired hide-and-seek tag game with local and online multiplayer.`
- Store assets: a 512 x 512 icon, a 1024 x 500 feature graphic, and four
  1920 x 1080 gameplay screenshots for phone, 7-inch tablet, and 10-inch
  tablet placements.

The editable icon and feature-graphic sources are in `store-assets/`. The
screenshots were captured from the live game and intentionally exclude the
leaderboard because it displayed real player names.

## App-content declarations

- App access: all functionality is available without special access; guest
  play is supported.
- Ads: no ads.
- Advertising ID: not used. The signed bundle manifest contains no advertising
  ID permission.
- Government app: no.
- Health features: none.
- Financial features: rewards, points, frequent-flier miles, and other
  incentives, because the game can award Palengke coins.
- Target audience: ages 13–15, 16–17, and 18+.

## Content rating

The questionnaire records mild, pixelated, non-bloody violence against human
characters in a realistic setting, usually shown at a distance with unrealistic
reactions. It records no gore, war, sexual content, gambling, offensive
language, controlled substances, crude humor, voice/text/image sharing,
location sharing, or other restricted themes.

Returned ratings:

- Australia: PG / Mild Violence
- Brazil: 10+
- ESRB: Everyone 10+ / Mild Violence
- Korea: 12+
- Taiwan: 12
- Saudi Arabia: 7
- PEGI: 7 / Mild Violence
- USK: 12
- Rest of world: 7+

## Data safety

The declaration states that data is encrypted in transit and that the Android
game does not create or require an account. The deletion-request URL is
`https://palengke.es/data-deletion`.

The declared data types are:

- approximate location inferred from network/IP information;
- Photon-assigned user IDs;
- app interactions used for online multiplayer.

These types are disclosed as collected and shared with Photon for app
functionality and fraud prevention/security. Approximate location and user IDs
are required and not treated as ephemeral. Multiplayer app interactions are
optional, because the user chooses whether to play online, and are treated as
ephemeral.

## Verification

- Google Play Console showed no unresolved required setup task before closed
  testing became available.
- The listing save succeeded and appeared in Publishing overview.
- The compliance changes were included in the 16-change review submission on
  2026-07-14.
