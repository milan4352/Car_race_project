# Draw & Race — Game Plan & Requirements

*Working title. Fully 3D Unity mobile racing game built around user-drawn tracks.*

## 1. Concept Overview

Players draw their own closed-loop race track using simple lines, then race a car around it. Every track is unique, so content is effectively infinite and driven by the community rather than the dev team alone. Players start with one car and unlock more through race wins, earning coins along the way. The drawn path is stored as simple 2D points and extruded into the 3D world at render time — which also makes those same points reusable for an in-race minimap later.

## 2. Feature Set

### 2.1 Track Creation
- Canvas-based drawing (line input) to create a closed-loop track
- Adjustable track width (sets difficulty)
- Auto-placed checkpoints at turns (heading change > 30°), used to validate real lap completion
- Self-intersection check at draw time (blocks invalid loops in V1)
- Save tracks locally and to the cloud
- Auto-generated thumbnail per track

### 2.2 Racing Mechanics
- Full 3D car physics — grip, drift, acceleration, suspension
- Soft off-track penalty (speed/grip reduction) instead of hard walls
- AI bots / ghost cars for single-player variety
- Multiple control schemes: tilt, on-screen wheel, buttons

### 2.3 Car Progression
- 1 starter car; additional cars unlock after race-win milestones
- Each car has distinct stats (top speed, handling, acceleration) — not just cosmetic
- Garage page for viewing/selecting cars

### 2.4 Currency & Rewards
- Coins awarded per race win, scaled by track difficulty/length
- Bonus coins for: first clear of a track, personal best, perfect lap (no penalties)
- All reward logic validated and issued server-side

### 2.5 Core Navigation
- Home, Track Editor, Track Library, Garage, Profile/Stats, Settings

### 2.6 Social & Community
- Friends list with time comparisons
- Public track gallery with ratings/likes
- Share race result or track drawing as an image
- Follow favorite track creators
- Report button for inappropriate tracks/usernames

### 2.7 Customization
- Player-drawn car livery, reusing the track-editor drawing tool
- Cosmetic track themes (desert/snow/night) as reskins — no new geometry needed
- Unlockable trail effects, horns, victory animations

### 2.8 Competitive
- Per-track leaderboards (global + friends)
- Weekly featured track / tournament with bonus rewards
- Ghost car of the current world-record holder

### 2.9 Retention
- Daily login rewards and daily challenges
- Achievement badges (first win, 10 tracks drawn, drift master, etc.)
- Daily streak bonuses

### 2.10 Monetization (fair, skill-first)
- Opt-in rewarded video ads via Unity LevelPlay ("watch to double coins")
- Cosmetic-only in-app purchases
- No loot boxes, no pay-to-win

### 2.11 Accessibility
- Multiple control schemes
- Assist mode (auto-brake in corners)
- Colorblind-friendly palette

### 2.12 Live-ops & Technical
- Analytics on track popularity / editor drop-off points
- Crash reporting
- Profanity filter for names and track titles
- Cloud save across devices

### 2.13 Onboarding
- Guided tutorial track teaching drawing + controls together
- Pre-made sample tracks so new users can race before drawing anything

### 2.14 Visual Style & Graphics
- Drawn path extruded into a real 3D road mesh with a PBR surface material
- 3D props (trees, rocks, buildings) placed procedurally along the track
- Real-time lighting and shadows, particle effects (dust, tire smoke, splashes) tied to surface/action
- Elevated/angled 3D camera by default, so the player's drawn track stays visible
- Biome-based environments (city, desert, snow, coast) — one at V1 launch, expanding after
- Tiered graphics quality setting (Low/Medium/High) for device performance

### 2.15 Future / Later Versions
- Track elevation — ramps, jumps, banked turns — now that the engine is true 3D
- Reverse mode and mirror mode (free "new" content from existing tracks)
- Power-ups: boost pads, nitro pickups placed during track creation
- New modes: Checkpoint Rush, Elimination
- Real-time multiplayer racing
- Figure-eight / self-intersecting track support
- Weather effects (rain/snow/fog)

## 3. Technical Architecture (Summary)
- **Engine:** Unity 6 LTS (currently Unity 6.3 LTS)
- **Language:** C#
- **Physics:** Unity's built-in 3D physics
- **Path/road tooling:** Splines package + SplineExtrude to generate the 3D road mesh from the drawn path
- **Render pipeline:** URP (Universal Render Pipeline) — built for mobile performance, unlike HDRP
- **Camera:** Cinemachine, elevated 3D follow
- **Input:** Unity Input System package, for cross-platform tilt/touch controls
- **Backend:** Unity Gaming Services — Economy, Leaderboards, Cloud Code, Cloud Save, Remote Config (Firebase via its Unity SDK is a valid alternative)
- **Ads:** Unity LevelPlay (ad mediation)
- **UI:** UGUI, or UI Toolkit for more complex menus

## 4. Graphics & Rendering Approach

The user still draws a simple 2D centerline — that part of the experience doesn't get harder just because the output is 3D.

- **Road generation:** the drawn spline is extruded into a 3D road mesh via the Splines package's SplineExtrude, with a PBR (physically based) road material.
- **Environment dressing:** 3D props (trees, rocks, buildings, barriers) placed procedurally along the spline, spaced to avoid the drivable surface.
- **Lighting:** real-time lighting for dynamic elements, baked lightmaps for static geometry to keep performance in check.
- **Cars:** 3D models with proper materials and shadows.
- **Particles:** dust, tire smoke, splashes, and snow spray, tied to surface type and car action.
- **Camera:** elevated/angled by default rather than a low chase view, so the track the player drew stays visible — Cinemachine makes alternate presets easy to add later.
- **Post-processing:** bloom, ambient occlusion, and color grading via URP volumes.

**Mobile performance considerations**
- LOD (Level of Detail) groups on 3D models so distant props render cheaper
- GPU instancing for repeated props (trees, barriers) to control draw call count
- Mobile GPUs throttle under sustained load — test on target devices across full play sessions, not just short editor bursts
- Graphics quality tiers (Low/Medium/High) scale texture resolution, shadow detail, particle density, and LOD distances

## 5. Functional Requirements

### 5.1 Track Editor
- **FR-1.1** The system shall let a user draw a track as a continuous line on a canvas.
- **FR-1.2** The system shall detect whether the drawn path forms a valid closed loop before allowing save.
- **FR-1.3** The system shall reject paths that self-intersect (V1).
- **FR-1.4** The user shall be able to set track width via a slider before saving.
- **FR-1.5** The system shall auto-place checkpoints at points where heading changes by more than ~30°.
- **FR-1.6** The user shall be able to name a track and save it locally and/or to the cloud.
- **FR-1.7** The system shall generate a thumbnail image for each saved track.

### 5.2 Racing Engine
- **FR-2.1** The system shall simulate car acceleration, braking, steering, and drift using 3D physics.
- **FR-2.2** The system shall apply a speed/grip penalty (not a hard block) when a car's position exceeds the track's width boundary.
- **FR-2.3** The system shall only count a lap as complete if all checkpoints were crossed in the correct order.
- **FR-2.4** The system shall record total race time and per-checkpoint timestamps for each attempt.
- **FR-2.5** The system shall support at least three control schemes (tilt, virtual wheel, buttons).

### 5.3 Progression & Economy
- **FR-3.1** A new user account shall start with exactly one car.
- **FR-3.2** The system shall unlock a new car once a user reaches that car's configured race-win threshold.
- **FR-3.3** Each car shall have a distinct stat profile (speed, handling, acceleration).
- **FR-3.4** The system shall award coins upon a validated race win, scaled to track difficulty and length.
- **FR-3.5** The system shall award bonus coins for first clear, personal best, and perfect-lap outcomes.
- **FR-3.6** Coin totals and unlock state shall only be modified by server-side logic (Cloud Code), never directly by the client.

### 5.4 Social & Community
- **FR-4.1** A user shall be able to publish a track to a public gallery.
- **FR-4.2** A user shall be able to rate or like a published track.
- **FR-4.3** A user shall be able to add friends and view a comparison of best times.
- **FR-4.4** A user shall be able to export a race result or track drawing as a shareable image.
- **FR-4.5** A user shall be able to report a track or username for inappropriate content.

### 5.5 Accounts & Profile
- **FR-5.1** The system shall support account creation/sign-in via Unity Authentication (or Firebase Auth if that backend is chosen instead).
- **FR-5.2** A user's profile shall display races won/played, best times, and unlocked cars.

### 5.6 Moderation
- **FR-6.1** The system shall filter profanity in usernames and track titles at submission time.
- **FR-6.2** Reported tracks/users shall be queued for review before removal or action (manual for V1, tooling for V2).

### 5.7 Graphics & Environment Rendering
- **FR-7.1** The system shall render the user-drawn path as a 3D road mesh matching the selected biome.
- **FR-7.2** The system shall procedurally place environment props along the track edges without overlapping the drivable surface.
- **FR-7.3** The system shall render real-time or baked shadows for the car and major props.
- **FR-7.4** The system shall render particle effects (dust, tire smoke, water splash, snow spray) based on surface type and car action.
- **FR-7.5** The system shall support at least one biome/theme at V1 launch, expanding to 3+ afterward.
- **FR-7.6** The system shall provide a graphics quality setting (Low/Medium/High) that adjusts shadow, particle, and texture-resolution levels.

## 6. Non-Functional Requirements

### 6.1 Security
- **NFR-1.1** The client shall never have write access to its own `coins`, `unlockedCars`, or `stats` fields; only server-side logic (Cloud Code) may modify them.
- **NFR-1.2** Every race result shall be validated server-side (checkpoint order, plausible timing) before rewards are issued.
- **NFR-1.3** Race-submission and track-publish calls shall be rate-limited to reduce spam and replay abuse.
- **NFR-1.4** All user-generated content (track geometry, names) shall be validated and bounded (e.g., max point count) before storage.
- **NFR-1.5** No API keys or secrets shall be embedded in client code.
- **NFR-1.6** Note: "zero vulnerabilities" isn't an achievable absolute — treat security as an ongoing practice (dependency updates, server-code review, testing before major launches), not a one-time checkbox.

### 6.2 Performance
- **NFR-2.1** Gameplay shall maintain 60fps on mid-range target devices at the Medium graphics tier.
- **NFR-2.2** Track editor input latency shall stay under ~16ms for responsive drawing.
- **NFR-2.3** Cold app start shall be under ~3 seconds on target devices.
- **NFR-2.4** The Low graphics tier shall maintain the frame-rate target on lower-end devices by reducing particle count, shadow detail, and texture resolution.
- **NFR-2.5** 3D rendering shall use LOD groups and GPU instancing for repeated props to control draw call count on mobile GPUs.

### 6.3 Scalability
- **NFR-3.1** Leaderboard data shall use the Leaderboards service's per-track/per-board structure to keep queries cheap as usage grows.
- **NFR-3.2** Backend logic shall run through Cloud Code so it scales automatically with load.

### 6.4 Usability & Accessibility
- **NFR-4.1** The app shall offer a colorblind-friendly palette option.
- **NFR-4.2** The app shall offer an assist mode (e.g., auto-brake) for new or younger players.
- **NFR-4.3** Core navigation shall be usable one-handed on standard phone form factors.

### 6.5 Compatibility
- **NFR-5.1** The app shall support current Android and iOS minimum versions per Unity's supported range at build time.
- **NFR-5.2** UI shall adapt to both phone and tablet aspect ratios.

### 6.6 Reliability
- **NFR-6.1** Crash-free session rate shall be tracked, with a target above 99%.
- **NFR-6.2** A track drawn offline shall not be lost if the device loses connectivity before sync.

### 6.7 Maintainability
- **NFR-7.1** Track rendering, physics, and game/economy logic shall be kept in separate modules so gameplay code doesn't depend on rendering implementation details.
- **NFR-7.2** Checkpoint and lap-validation logic shall have automated test coverage, since it directly gates rewards.

### 6.8 Compliance & Privacy
- **NFR-8.1** If the app is available to under-13 users, data collection and any social/chat features shall comply with COPPA (or regional equivalent).
- **NFR-8.2** A privacy policy and terms of service shall be published and linked, as required for app store submission.
- **NFR-8.3** If in-app purchases are enabled and the app is rated for children, purchases shall be parental-gated.

### 6.9 Asset Delivery
- **NFR-9.1** Biome/texture/3D-model asset packs beyond the V1 default theme shall be downloadable on-demand rather than bundled in the base install, to control app size.

## 7. Technical & Platform Requirements
- Unity 6 LTS (6.3 LTS or later at time of development)
- C#
- Packages: Splines, Cinemachine, Input System, URP
- Backend: Unity Gaming Services (Economy, Leaderboards, Cloud Code, Cloud Save, Remote Config) — or Firebase via Unity SDK as an alternative
- Ads: Unity LevelPlay
- Target platforms: Android and iOS (phone + tablet), built via Unity's mobile build pipeline

## 8. Data Requirements
- Track path data shall be stored as an ordered list of 2D points, extruded into the 3D road at render time; the same data doubles as the source for a future in-race minimap.
- Track documents shall cap stored path points (e.g., 500) to control payload size and load time.
- Race documents shall retain checkpoint timestamps for validation and anti-cheat auditing.
- User records shall separate publicly-readable fields (name, stats) from protected fields (coins, unlocks) enforced at the backend/Cloud Code level.

## 9. Roadmap

### V1 — 3D MVP
Track editor with 3D road generation (Splines/SplineExtrude), full 3D physics, one polished biome/theme, 1 starter car + unlock chain, coin economy via UGS Economy with Cloud Code validation, leaderboards, local + cloud save, tutorial + sample tracks.

### V2 — Content Expansion
Public track gallery with ratings, friends/social comparison, car livery customization, expanded biomes (desert/snow/city) with props and particle effects, daily challenges/streaks, moderation tooling.

### V3 / Later
Track elevation (ramps, jumps, banked turns), reverse & mirror modes, power-ups, new race modes (Checkpoint Rush, Elimination), real-time multiplayer, figure-eight track support, weather effects (rain/snow/fog).

## 10. Out of Scope for V1
- Track elevation / verticality (ramps, jumps, banked turns) — flat tracks only at V1
- Real-time multiplayer racing
- Self-intersecting / figure-eight tracks
- Power-ups
- Multi-biome art beyond the single V1 default theme
- Full moderation dashboard (manual review only at this stage)
