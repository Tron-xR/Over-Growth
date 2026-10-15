# Systems (build against these)

## GameState
String-flag store. API: `Set(flag)`, `Has(flag)`, `OnFlagSet` event. `SaveCoordinator` persists flags as JSON through `SaveSystem`. Used for doors, abilities, senses, collectibles, logs, and progression.

## Player movement
CharacterController on the ground plane. Walk/run/dash. `PlayerStats` in Assets/Data tunes walk/run/dash speeds, dash time/cooldown/cost, turn speed, and total slots. Final speed = base x `PartLoadout.SpeedMultiplier`. Input uses Move, Run, Dash, Interact, Ability, and Inventory actions. PlayerController owns Move/Run/Dash action enable/disable; CoreTransfer reads the same shared Move action. InventoryUI owns the Inventory action enable/disable. Control is enabled only while PlayerController's named lock set is empty; callers use `AddLock(reason)` and `RemoveLock(reason)` so independent systems do not release each other's locks.

## Battery
`Battery.Current/Max`. `BatteryConfig` in Assets/Data tunes capacity and idle/walk/run drain. Moving drain = base x `PartLoadout.PowerDrainMultiplier`; running applies the configured run multiplier. Dash spends its configured cost scaled by the part drain multiplier. Idle drain is minimal. `OnChanged` drives the battery HUD; `OnDepleted` fires once per depletion and starts RespawnManager. Chargers/cat/old man refill is future work.

## Parts & slots (customization)
`PartData` ScriptableObject: id, displayName, category (Locomotion/Arm/Leg/Core/Misc), slotCost, speedMultiplier, pushPullForce, powerDrainMultiplier, prefab. `PartLoadout`: totalSlots, Equip/Unequip (rejects invalid, duplicate, or over-budget parts), and multiplicative stat aggregation. Empty loadout multipliers are 1. Base model = wheel; final upgrade = legs/arms rig (see concept art).

`PartInventory` tracks owned parts separately from equipped parts and only equips owned parts. `PartPickup` adds the part and sets `part.owned.<id>` in GameState; the flag prevents the pickup from returning. `SaveCoordinator` saves owned and equipped part IDs and resolves them through its PartData catalog. Old saves without `ownedParts` treat their equipped parts as owned.

## Respawn
`RespawnPoint` per area. `RespawnConfig` in Assets/Data sets `killY` (default -5, five units below the greybox floor at Y=0). RespawnManager tracks the active area ID (set through `SetCurrentArea`), triggers the existing fade, `world.respawn` control lock, and teleport sequence when the player falls below `killY`; this fall respawn does not change battery. Battery depletion uses the same sequence and refills battery. The player controller clears its vertical motion and any dash when teleported. A plain `SafePositionTracker` keeps the last grounded position above `killY` for saves. Load restores a saved position only when its coordinates are finite, its Y is at or above `killY`, and its area has a RespawnPoint; otherwise it logs a warning and uses the current area's RespawnPoint. An unavailable saved area also logs a warning and falls back to the scene's configured current area. Old man in city is a special respawn with dialogue (not yet implemented).

## Abilities (4)
`Ability` base (id, unlock flag, Activate/Deactivate). `AbilityController` holds unlocked abilities, one active bind.
1. Core Transfer: possess a `PossessableRobot` for a limited time; control it; return to jar on timeout/cancel.
2-4. TBD (placeholders in CONTENT.md).

## Puzzles & gates
`PlayerInteractor` listens to the existing `Player/Interact` action and invokes the closest in-range `Interactable`. `FlagPickup` sets its configured `GameState` flag and hides itself when that flag is restored. `PuzzleGate` checks requirements only when interacted with; the Preservation Room exit requires `item.mobility_device` and `item.access_card`, shows a thought through `DialogueRunner` when locked, and sets `door.room_exit` when opened. It restores the open visual and blocker from that saved flag after load. SampleScene contains the greybox room, both trigger pickups, the Interact action reference, DialogueRunner/DialogueBox, and wired exit gate. MANUAL verification: enter with a save where both item flags are unset; confirm each pickup sets its flag and disappears; interact with the closed exit before collecting both and confirm the thought appears; collect both, interact again, and confirm the blocker/closed visual disable; restart and confirm collected pickups stay gone and the door stays open. Puzzle mechanics: push/pull blocks (force from parts), pressure plates, possessed-robot access, power routing. Gates never reference each other directly, only flags.

Object interaction has three mutually exclusive states in `ObjectInteractionStateMachine`: Idle, Carrying, and Gripping. `PlayerInteractor` chooses the nearest in-range `Interactable`; while carrying or gripping, it ignores other targets. Its screen prompt shows the action currently available.

`CarryableItem` toggles between pickup and drop on E. Drop and throw search multiple ground/free positions with overlap checks; the ground cast excludes the item and player colliders, and uses collider bounds to place the collider bottom above the hit surface. If no free spot exists, the object stays carried. F throws in the facing direction with an upward arc, using clamped speed derived from `PartLoadout.PushPullForce / mass`. Player/item collisions are ignored briefly after a throw. Carry speed is multiplied by `1 / sqrt(1 + mass)`. `CarryConfig` tunes mass, drag, friction, pickup range, drop search/clearance, throw speed/arc, collision-ignore duration, and kill-plane Y. Dropped or thrown props below that Y return to their original position and rotation.

`PushPullBlock` is a separate grip interaction. E snaps the player to the nearest block face with the `grab.snap` control lock. `BlockConfig.axisLockMode` defaults to `Free`, which lets the player move in any direction while the kinematic block follows the CharacterController's achieved displacement, preserving their grip offset; `GripAxisOnly` retains the legacy axis projection. Each horizontal axis is swept for both bodies so movement slides along an unblocked axis. The block's yaw snaps to the resolved cardinal movement direction while gripping; pitch and roll remain frozen, and all rotation stays frozen after release. Player/block collision is ignored only during the synchronized CharacterController/Rigidbody movement step, then immediately restored. Movement speed is based on push/pull force versus mass; insufficient force shows one thought line and leaves the block still. Blocks use interpolation while gripped, Continuous Dynamic collision detection, and the configured slide material. Optional grid snapping is disabled by default. `BlockConfig` tunes mass, friction, drag, pickup distance, socket gap, snap duration, axis lock, speed, grid snapping, and kill-plane Y. Falling blocks reset to their scene-start transform.

`Controls/Player/Throw` is bound to F and gamepad West; Interact remains E and uses gamepad Right Shoulder to avoid a binding conflict. MANUAL setup: on each carryable prefab, keep a Rigidbody, solid collider, `CarryableItem`, and child trigger radius, then assign a `CarryConfig`; on each block prefab, use a Rigidbody, root BoxCollider, `PushPullBlock`, child trigger radius, and `BlockConfig`, with the scene DialogueRunner assigned. Confirm PlayerInteractor's Throw reference points to the Throw action and `BlockConfig.axisLockMode` is `Free` unless the prefab intentionally wants grip-axis-only movement. Verify E pickup/drop, safe ground snap and blocked-drop retention, F/gamepad-West throw arc and collision grace, carry slowdown, four-face and diagonal grips, free diagonal movement/socket retention and cardinal yaw following, both wall-block cases, too-heavy thought once, optional grid snap, and each object's kill-plane reset. EditMode pure logic tests cover projection, axis resolution, socket offsets, movement yaw, force/mass speed, and snap math; PlayMode physics tests cover three drop layouts, diagonal block grip, and thrown-object landing. Current SampleScene contains one block and two carryable scene objects; there are no gameplay prefab assets yet.

## Senses
Flags `sense.sight`, `sense.hearing`, `sense.smell`. Sight: after `intro.done`, SenseManager holds the `sense.sight` control lock while fading the Global Volume from blurred, dark, and vignetted to its configured clear values, then sets `sense.sight` and removes its lock. SaveCoordinator persists the flag, so a restored sight state skips the fade. The Volume needs Depth of Field, Vignette, and Color Adjustments. Hearing: ambient/positional audio layer and audio-cue puzzles. Smell: reveals hidden/collectible hints (VFX trail). Presentation toggled by SenseManager.

## Inventory UI
Tabs: Parts (equip, slot bar), Collectibles, Quests, Map (unlocked areas), Logs.

## Dialogue
`DialogueLine` ScriptableObjects hold a stable ID, text, and optional audio. `DialogueRunner` steps through lines and raises `OnLineRequested`; `DialogueBox` listens and presents each line with its typewriter effect. Intro lines play over a separate black-screen overlay; `IntroSequence` holds the `ui.intro_sequence` player control lock while playing, then sets `intro.done` in `GameState` and releases its lock. The existing `SaveCoordinator` persists the flag, so later loads skip the intro. The opening sequence uses one line for the optional “power restored” VO followed by three thought lines. MANUAL: author the four line assets and wire the separate overlay, runner, DialogueBox, and IntroSequence in SampleScene; assign its PlayerController reference.

## Collectibles & logs
Shiny items (optional, flagged); logs (text entries unlocking lore). Both stored as flags + data assets.

## Save
Single-slot JSON at persistentDataPath: flags, owned and equipped part IDs, battery, position, and area. `SaveCoordinator` restores these values and saves on flag/inventory/area changes, app pause, and application quit. Position saves use the last grounded location above `killY`, including while falling or respawning, so other progress still saves without preserving a bad location. Each saveable PartData must be listed in its catalog for ID resolution on load.

## M1 greybox
`Assets/Scenes/SampleScene.unity` contains the capsule player, floor, fixed-follow camera, room respawn point, battery HUD, and respawn fade overlay. Input actions live in `Assets/Data/Controls.inputactions`.

## Rendering / look
URP. Warm directional sun, cool trilight ambient, baked GI + probe volumes, global Volume (DoF bokeh focused on player via DofFocus, Neutral tonemapping, mild bloom/vignette, SSAO feature). See docs/ART_STYLE.md.
