# Roadmap (tick as done; [M] = manual Unity editor step)

## M0 — Setup
- [ ] [M] Install Unity LTS, create 3D URP project in this folder (Assets/ already scaffolded)
- [ ] [M] Install packages: Input System, Cinemachine (optional), TextMeshPro, Test Framework
- [ ] git init, enable Git LFS, first commit
- [ ] Copy concept art + reference video into Assets/Art/Concepts
- [ ] [M] Follow docs/SETUP.md steps 1-17
- [ ] [M] Run Game > Setup > Create Art Style Volume; add SSAO renderer feature
- [ ] Decide art direction A vs B (docs/ART_STYLE.md) and record it

## M1 — Greybox movement (vertical slice A)
- [x] GameState + SaveSystem skeleton (SaveCoordinator persists flags, loadout, battery, position, and area)
- [x] PlayerController (walk/run/dash) + FixedFollowCamera
- [x] Battery + drain + HUD bar
- [x] RespawnManager + RespawnPoint (fade-out, teleport, refill, fade-in)
- [x] Greybox scene with capsule player

## M1.5 — Look dev
- [ ] DofFocus on camera, tune DoF/lighting in greybox
- [ ] Pick 1 free environment kit, log in ASSET_LICENSES.md, dress one test room
- [ ] Model jar-brain + wheel base in Blender, import, replace capsule

## M2 — Parts & inventory
- [ ] PartData + PartLoadout (+EditMode tests)
- [ ] Pickup + inventory UI (Parts tab, slot bar)
- [ ] Wheel base vs legs stats changing movement/drain

## M3 — Intro sequence + Preservation Room
- [ ] Black screen, "power restored" VO, thought dialogue system
- [ ] Sight fade-in (SenseManager)
- [ ] Room puzzle: mobility device + access card → door
- [ ] Lab reveal (music drop, flicker lights)

## M4 — Gates, puzzles, logs
- [ ] Interactable + PuzzleGate (flags)
- [ ] Push/pull block puzzle using part force
- [ ] Logs + Logs tab, collectibles + Collectibles tab

## M5 — Abilities
- [ ] Ability framework + AbilityController
- [ ] Core Transfer + PossessableRobot
- [ ] Abilities 2–4 (design first in CONTENT.md)

## M6 — City + NPCs
- [ ] Old man (recharge + respawn dialogue)
- [ ] Scavenger (shop: parts for currency/collectibles)
- [ ] Map tab + area unlocks

## M7 — Content, polish, build
- [ ] Hidden areas + shiny quest + post-game
- [ ] Audio pass, VFX, hand-painted assets
- [ ] Save/load hardening, settings, controller support
- [ ] PC build, then console dev-kit pass
