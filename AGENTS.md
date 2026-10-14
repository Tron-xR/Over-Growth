# AGENTS.md — Project: (working title TBD) — brain-in-a-jar puzzle Metroidvania

Unity (LTS, URP) · C# · single-player offline · PC + console · third-person top-angled fixed camera.
This file is the single source of agent instructions for every tool (Codex, Claude Code, others). CLAUDE.md only imports it.

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

## Art
- Lighting/post values come from docs/ART_STYLE.md; the menu Game > Setup > Create Art Style Volume (Assets/Scripts/Editor/ArtStyleSetup.cs) is the source of truth for the Volume.
- Never import third-party assets without adding a row to docs/ASSET_LICENSES.md.
- Do not change the art direction (A: PBR reference look / B: hand-painted) without asking.

## Minimalism tooling (optional)
If the Ponytail skill is installed, use `lite` or `full` intensity. Its "write less" rule never overrides the conventions above (ScriptableObject tuning, flag-based gates, event communication, testable pure logic) or the safeguards in the workflow below.

## Commands
- /next-task, /new-system <name>, /new-part <name>, /new-ability <name>, /review — see "Command Workflows" below. Claude Code reads them from .claude/commands; other agents follow the same descriptions here.

---

## Extended Agent Workflow

### 1. Session Initialization
- Read `docs/GDD.md`, `docs/STORY.md`, `docs/GAME_FLOW.md`, `docs/SYSTEMS.md`, `docs/ROADMAP.md`, `docs/CONTENT.md`, `docs/ART_STYLE.md`, and `docs/SETUP.md` before starting a new feature or system.
- Read relevant files in `.claude/commands/` before implementing an existing workflow command.
- Inspect Git status and existing changes before editing anything. Never overwrite unrelated user changes.
- Identify the current milestone and select the next unchecked task in `docs/ROADMAP.md`.
- Work on one roadmap task at a time. Do not silently expand scope.

### 2. Planning
Before implementation:
1. State the task's objective and acceptance criteria.
2. Identify relevant scripts, ScriptableObjects, scenes, prefabs, documentation, and tests.
3. Identify dependencies and possible regressions.
4. Separate automated work from Unity Editor-only work.
5. List the files expected to change.
6. If the task conflicts with the design documents or project conventions, stop and explain the conflict before proceeding.

For substantial tasks, present the plan and wait for approval. For small, clearly scoped roadmap tasks, proceed directly unless a decision or destructive change requires confirmation.

### 3. Implementation
- Follow the architecture, naming, namespace, and file-organization rules defined above.
- Keep gameplay tuning in ScriptableObjects under `Assets/Data`.
- Keep progression gates in `GameState` string flags.
- Use C# events for communication between systems; introduce no new singletons beyond the approved exceptions.
- Keep gameplay non-combat and preserve the battery-only player resource model.
- Preserve the fixed camera and established art direction.
- Write pure game logic in plain C# classes wherever practical.
- Prefer small, focused changes over broad refactoring.
- Do not introduce new packages, dependencies, or third-party assets without checking compatibility and obtaining approval where required.
- Do not fabricate Unity APIs, successful Editor operations, or test results.

### 4. Independent Code Review
After implementation, perform a separate review pass before committing.

Review for:
- Functional correctness and acceptance criteria.
- Violations of `AGENTS.md` and the design documents.
- Null references, lifecycle errors, event subscription leaks, and incorrect event unsubscription.
- Save/load compatibility and progression flag consistency.
- Serialization compatibility and missing ScriptableObject references.
- Unnecessary allocations and performance issues in frequently executed code.
- Missing EditMode tests and untested edge cases.
- Accidental scene, prefab, asset, or unrelated documentation changes.
- License documentation for any imported third-party assets.

Classify findings as Critical, High, Medium, or Low. Fix Critical and High findings before proceeding. Fix lower-severity findings when they are within scope; otherwise document them.

The review must be based on the actual diff and repository state. Do not claim an independent review occurred unless it was performed.

### 5. Automated Verification
After review:
1. Run relevant EditMode tests for pure logic.
2. Run relevant PlayMode tests when gameplay integration is affected and the Unity test environment is available.
3. Compile or build using the project's supported Unity tooling when available.
4. Run applicable static checks and inspect the final diff.
5. Investigate failures introduced by the changes, fix them, and rerun the relevant checks.

If Unity Editor tooling is unavailable, do not treat a standalone C# compilation as proof that the entire Unity project compiles. Report exactly which checks ran, which passed, and which could not be performed.

Never report a test, compilation, build, or runtime check as passed unless its result was observed.

### 6. Unity Editor and Manual Work
Identify tasks that require Unity Editor interaction, including:
- Scene creation and wiring.
- Prefab configuration and component assignment.
- Inspector references and ScriptableObject asset creation.
- Input Action asset configuration.
- Art imports, materials, lighting, and Volume setup.
- Animator, Timeline, and other Editor-managed asset configuration.

Use available, verified Unity Editor automation tools where appropriate. Otherwise, label these tasks `MANUAL` and provide a concise checklist containing:
- Asset or scene to open.
- Objects or components to create or modify.
- Exact settings or references to assign.
- Expected result.
- Verification steps.

Do not claim these steps are complete when they have not been performed.

### 7. Documentation and Roadmap
Once implementation and verification are complete:
- Mark the relevant roadmap checkbox only when its acceptance criteria are satisfied.
- Update `docs/SYSTEMS.md` when system behavior, interfaces, or specifications change.
- Update other relevant documentation when implementation changes its documented behavior.
- Record required Editor work as `MANUAL` with actionable steps.
- Update `docs/ASSET_LICENSES.md` for third-party assets before treating their integration as complete.
- Keep documentation consistent with actual implementation; do not mark incomplete work as finished.

### 8. Commit
Before committing:
1. Inspect `git diff` and `git diff --check`.
2. Confirm unrelated user changes are preserved.
3. Confirm the review and required automated checks are complete, or clearly record any limitations.
4. Stage only files relevant to the current task.
5. Use a concise commit message describing the change.

Do not rewrite history, force-push, delete user data, or perform destructive Git operations without explicit approval.

If a required check fails, do not make a normal completion commit unless the user explicitly requests a checkpoint commit. Clearly report the blocker.

### 9. Completion Report
At the end of every task, provide:
- **Implemented:** What changed and why.
- **Files changed:** The relevant scripts, assets, tests, and documentation.
- **Review:** Findings and their resolution.
- **Verification:** Exact tests, compilation checks, and build checks performed, with results.
- **Manual steps:** Outstanding Unity Editor actions, if any.
- **Roadmap:** Task status and any remaining acceptance criteria.
- **Commit:** Commit hash and message, if a commit was actually created.
- **Blockers:** Any unresolved failures, limitations, or decisions requiring the user's input.

Never describe partially completed work as finished.

### 10. Command Workflows
- `/next-task`: Select the next unchecked roadmap task, inspect its dependencies, and follow this workflow.
- `/new-system`: Check the existing specifications, propose the architecture, define acceptance criteria, and implement only after resolving design conflicts.
- `/new-part`: Follow the content tables and system specifications; add required data, tests, documentation, and manual Editor steps.
- `/new-ability`: Follow existing progression, battery, part-transfer, and event conventions; add pure-logic tests where appropriate.
- `/review`: Review the actual working-tree diff, report prioritized findings, and do not modify files unless explicitly asked.

Treat these commands as workflow entry points, not permission to bypass planning, testing, review, documentation, or safety requirements.
