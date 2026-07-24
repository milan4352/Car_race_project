# Draw & Race — 3D Unity Mobile Racing Game

A fully 3D Unity mobile racing game built around user-drawn tracks extruded into 3D URP road meshes.

## Technical Architecture & Technology Stack
- **Engine:** Unity 6 LTS (`6000.5.5f1`)
- **Render Pipeline:** Universal Render Pipeline (URP) with high-graphics settings (SSAO, Bloom, PBR materials, Cascade shadows)
- **Path Tooling:** Unity Splines (`com.unity.splines`) & `SplineExtrude`
- **Camera System:** Cinemachine (`com.unity.cinemachine`)
- **Input:** Unity Input System (`com.unity.inputsystem`)
- **Backend:** Unity Gaming Services (Auth, Economy, Cloud Code, Cloud Save, Leaderboards)

## Project Structure
- `Assets/`
  - `Art/` (Materials, Shaders, Textures, Models, Particles, PostProcessing)
  - `Scripts/`
    - `Core/` (Pipeline configurator, GameManager)
    - `TrackEditor/` (2D Canvas drawing, Spline converter, Checkpoint placement)
    - `Vehicle/` (3D Physics Car controller, Skidmarks, Particle FX)
    - `UI/` (HUD, Track Editor UI, 3D Garage Showroom)
    - `Backend/` (UGS authentication, Cloud Code anti-cheat race validation)
  - `Scenes/`
- `Packages/` (Unity Package Manager manifest)
- `ProjectSettings/`

## Graphics Tiers
- **High Tier:** SSAO enabled, 4-cascade shadows, 150m shadow distance, motion blur, ACES tonemapping.
- **Medium Tier:** 60 FPS target for mid-range mobile devices, 2-cascade shadows, SSAO.
- **Low Tier:** Mobile GPU optimized, single shadow cascade, reduced particle density.
