# Project: (working title TBD) — brain-in-a-jar puzzle Metroidvania

Unity (LTS, URP) · C# · single-player offline · PC + console · third-person top-angled fixed camera.

## Read first
- docs/GDD.md — design source of truth
- docs/STORY.md — lore, NPCs, logs
- docs/GAME_FLOW.md — player flow + progression graph
- docs/SYSTEMS.md — system specs (build against these)
- docs/ROADMAP.md — milestones; work one task at a time
- docs/CONTENT.md — parts, abilities, areas, collectibles tables
- docs/ART_STYLE.md — lighting, camera, post-processing, asset rules (log assets in docs/ASSET_LICENSES.md)
- docs/SETUP.md — environment/project setup checklist
- docs/STARTER_PROMPT.md — first prompt for a fresh session

## Conventions
- Gameplay tuning lives in ScriptableObjects (Assets/Data), never hardcoded.
- Progression gates are string flags in `GameState` (e.g. `ability.core_transfer`, `door.lab_main`), never direct scene references.
- One MonoBehaviour per file, namespace `Game.<Folder>`.
- No singletons except `GameState` and `SaveSystem`. Communicate via C# events.
- Player is non-combat: no health, only battery. Do not add combat.
- Fixed camera: no player camera rotation.
- Keep scripts small and testable; put pure logic in plain C# classes (e.g. `PartLoadout`) so EditMode tests can cover it.

## Workflow
1. Pick next unchecked task in docs/ROADMAP.md.
2. Plan → implement → add EditMode test where logic is pure → commit.
3. Update the roadmap checkbox and any changed spec in docs/SYSTEMS.md.
4. Unity-editor-only steps (scene wiring, prefabs, art import) are listed as MANUAL in the task; leave a checklist for me.

## Commands
- /next-task, /new-system <name>, /new-part <name>, /new-ability <name>, /review (see .claude/commands)

## Art
- Lighting/post values come from docs/ART_STYLE.md; the menu Game > Setup > Create Art Style Volume (Assets/Scripts/Editor/ArtStyleSetup.cs) is the source of truth for the Volume.
- Never import third-party assets without adding a row to docs/ASSET_LICENSES.md.
- Do not change the art direction (A: PBR reference look / B: hand-painted) without asking.
