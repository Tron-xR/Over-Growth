# Setup checklist (do in order)

## Accounts / tools
1. Unity Hub + Unity LTS (Unity 6 LTS recommended) with modules: Windows Build Support (or Mac), plus console modules later.
2. Code editor with Unity support (VS Code + C# Dev Kit, Rider, or Visual Studio) — set as External Script Editor in Unity.
3. Git + Git LFS (`git lfs install`). Create a GitHub repo (private).
4. Claude Code (or your CLI agent), installed and logged in; launch it from the project root.
5. Blender (models), Krita or Substance Painter (hand-painted textures), Audacity (audio).
6. Optional: Fab / Unity Asset Store accounts for free assets.

## Unity project
7. Create a new project from the "Universal 3D" (URP) template in a temp folder.
8. Copy the scaffold's `Assets/`, `docs/`, `.claude/`, `AGENTS.md`, `CLAUDE.md`, `.gitignore`, `.gitattributes` into the new project root (merge `Assets/`; do not overwrite ProjectSettings/Packages).
9. Package Manager: Input System (accept the backend restart), TextMeshPro, Test Framework. Cinemachine optional.
10. Project Settings → Player → Active Input Handling = Input System Package.
11. Create Input Actions asset (Assets/Data/Controls.inputactions): Move (Vector2), Run, Dash, Interact, Ability, Inventory.
12. Menu: Game → Setup → Create Art Style Volume (sets lighting + post-processing). Then add SSAO Renderer Feature manually.
13. `git init`, commit, push.

## First scene (greybox)
14. Plane + capsule "Player" (CharacterController, Battery, PlayerController, tag "Player"), assign Input Action references.
15. Main Camera + FixedFollowCamera + DofFocus (assign Volume + player).
16. Empty "RespawnManager" (assign battery/player) and one RespawnPoint with areaId "room".
17. Press Play: walk/run/dash, battery drains.
