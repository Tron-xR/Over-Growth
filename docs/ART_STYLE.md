# Art style

Reference: scene captured from Unreal (see Assets/Art/Concepts/reference_video.mp4 if added).
Look: warm low-angle sun, long soft shadows with cool blue-grey fill, near-monochrome beige/tan palette, detailed industrial props, strong tilt-shift depth of field focused on the player.

> Decision needed: the GDD says stylized hand-painted low-poly; the reference is realistic PBR. Pick ONE as the shipping style.
> - Direction A (reference look): PBR materials + baked GI + post-processing (below).
> - Direction B (GDD): hand-painted textures, simple/unlit-leaning shaders, same lighting mood and DoF.
> Default: Direction B assets + reference lighting/camera/DoF mood.

## Camera
Perspective, FOV 35–40, pitch ~50–55°, offset ~(0, 10, -7), smooth follow, no rotation.

## Lighting (URP)
- Directional light: warm (≈ #FFE0B3), rotation ≈ (35, -30, 0), soft shadows, shadow distance 60–80.
- Environment: Trilight ambient — sky cool blue-grey, equator neutral, ground warm tan, so shadows read cool.
- GI: bake lightmaps for static geometry; Adaptive Probe Volumes for dynamic objects (player, parts, robots).
- Flickering lab lights: emissive + point light with noise-driven intensity (see lab areas only).

## Post-processing (Global Volume — created by menu Game/Setup/Create Art Style Volume)
| Effect | Setting |
|---|---|
| Depth of Field | Bokeh, aperture 5.6, focal length 70, focus = distance to player (DofFocus.cs) |
| Tonemapping | Neutral |
| Color Adjustments | post-exposure +0.2, saturation -15, warm color filter |
| Bloom | intensity 0.3, threshold 1 |
| Vignette | 0.25 |
| SSAO | MANUAL: add as Renderer Feature on the URP Renderer asset |

## Palette
Base: sand/beige/tan. Shadow: desaturated blue-grey. Accent (jar glow / power / pickups): single teal-green from concept art. Lab area: add flicker + overgrowth greens.

## Asset rules
- Every third-party asset is logged in docs/ASSET_LICENSES.md (name, source URL, license, date).
- Prefer CC0: Kenney, Quaternius, Poly Haven, ambientCG.
- Fab / Asset Store / Sketchfab: confirm license allows Unity + commercial use before import.
- Custom (Blender): jar-brain player, wheel base, G7 legs/arms parts, old man, scavenger, cat.
- Characters: Mixamo or Unity Starter Assets as placeholder only.
- Keep imported assets in Assets/Art/ThirdParty/<source>/.
