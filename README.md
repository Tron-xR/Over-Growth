<div align="center">

# 🧠 GAME TITLE (working title)

**A calm, post-apocalyptic puzzle Metroidvania about a brain in a jar who wakes up in the world he helped destroy.**

![Unity](https://img.shields.io/badge/Unity-URP-black?logo=unity)
![Language](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-PC%20%7C%20Console-blue)
![Status](https://img.shields.io/badge/Status-In%20development-orange)

<!-- Replace with a real screenshot or GIF as soon as you have one -->
<!-- ![Gameplay](docs/media/gameplay.gif) -->

</div>

---

## About

Centuries ago, a visionary scientist set out to free humanity from fragile bodies by moving human brains into mechanical ones. He died before he could decode emotion. His company finished the work without it, and the result was a world of walking brains in metal suits, endless resource wars, and silence.

Now the lab has power again, and the scientist's preserved brain wakes up in a jar.

You explore the overgrown ruins, regain your lost senses (sight, hearing, smell), collect and swap robot parts, solve puzzles that gate every area, and read the logs left behind to learn what your invention did to the world.

## Features

- **Exploration-driven Metroidvania:** areas unlock through puzzles and abilities, with hidden places for the completionist.
- **Part-based customization:** find robot parts and fit them into a limited slot budget. Faster parts drain more battery, so every build is a trade-off.
- **Battery and respawn:** movement drains power. When it runs out, you respawn at the area's respawn point, or the old man in the city finds you "on his evening walk".
- **Four unlockable abilities:** the first is *Core Transfer*, which lets you possess other robots to solve puzzles.
- **Regain your senses:** sight, hearing and smell each unlock a new layer of the game.
- **Non-combat:** no fighting and no health bar, just thinking, exploring and managing power.
- **Lore through logs:** researcher logs and NPCs tell the story of the fall.
- **Single-player, offline, no paywall.**

## Status

Early development.

| Milestone | State |
|---|---|
| M0 Setup | In progress |
| M1 Greybox movement slice (movement, battery, HUD, respawn, save) | Code complete; in-editor runtime check pending |
| M2 Parts and inventory | Next |
| M3 Intro sequence and Preservation Room | Planned |

See [`docs/ROADMAP.md`](docs/ROADMAP.md) for the full plan.

## Tech

- **Engine:** Unity (LTS) with the Universal Render Pipeline
- **Language:** C#
- **Input:** Unity Input System
- **Camera:** third-person, fixed top-angled view
- **Target platforms:** PC and console

## Getting started

### Requirements
- Unity Hub and Unity LTS (see `ProjectSettings/ProjectVersion.txt` for the exact version)
- Git and Git LFS

### Run it
```bash
git lfs install
git clone https://github.com/Tron-xR/REPO_NAME.git
```
1. Open the folder in Unity Hub (Add → select the project folder).
2. Open `Assets/Scenes/SampleScene.unity` and press **Play**.

### Controls (current)
| Action | Key |
|---|---|
| Move | WASD |
| Run | Left Shift |
| Dash | Space |

### Art style setup
Run the menu item **Game → Setup → Create Art Style Volume** to create the lighting and post-processing profile described in [`docs/ART_STYLE.md`](docs/ART_STYLE.md). Add the SSAO renderer feature manually.

### Tests
Window → General → Test Runner → EditMode → Run All.

## Project structure

```
Assets/
  Scripts/      Core, Player, Parts, Abilities, Puzzle, World, Senses, UI, CameraRig, Editor
  Data/         ScriptableObjects (player stats, battery config, parts, input)
  Art/          Concepts, Models, Textures
  Scenes/  Prefabs/  Audio/  Tests/
docs/           Design and engineering documents
AGENTS.md       Instructions and workflow for AI coding agents
```

## Documentation

| Doc | Contents |
|---|---|
| [`docs/GDD.md`](docs/GDD.md) | Game design document |
| [`docs/STORY.md`](docs/STORY.md) | Lore, NPCs, opening sequence |
| [`docs/GAME_FLOW.md`](docs/GAME_FLOW.md) | Player flow and progression graphs |
| [`docs/SYSTEMS.md`](docs/SYSTEMS.md) | System specifications |
| [`docs/CONTENT.md`](docs/CONTENT.md) | Parts, abilities, areas, logs |
| [`docs/ART_STYLE.md`](docs/ART_STYLE.md) | Lighting, camera, post-processing, asset rules |
| [`docs/ROADMAP.md`](docs/ROADMAP.md) | Milestones |
| [`docs/SETUP.md`](docs/SETUP.md) | Environment setup checklist |

## Architecture at a glance

- **Progression gates are flags.** `GameState` stores string flags such as `ability.core_transfer`. Doors, abilities, senses and logs read flags and never reference each other directly.
- **Tuning is data.** Gameplay values live in ScriptableObjects under `Assets/Data`.
- **Systems talk through C# events,** with no singletons beyond `GameState` and `SaveSystem`.
- **Pure logic is testable.** Systems like `PartLoadout` are plain C# classes covered by EditMode tests.

## AI-assisted workflow

This project is developed with AI coding agents under a strict workflow: plan, implement, independent diff review, verification, documentation update, then commit. The rules live in [`AGENTS.md`](AGENTS.md). Every change is reviewed and tested by a human before it is merged.

## Contributing

This is a personal project and is not currently accepting code contributions. Bug reports and feedback are welcome through Issues.

## Credits

- Design, code and concept art: Tron ([@Tron-xR](https://github.com/Tron-xR))
- Third-party assets are listed with their licenses in [`docs/ASSET_LICENSES.md`](docs/ASSET_LICENSES.md).

## License

Code: choose a license (MIT is a common choice) and add a `LICENSE` file.
Game art, story, characters and audio: all rights reserved unless stated otherwise.
