using System.Collections.Generic;
using Game.Player;
using Game.Puzzle.Logic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Puzzle
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] InputActionReference interact;
        [SerializeField] InputActionReference throwAction;

        readonly HashSet<Interactable> nearby = new HashSet<Interactable>();
        readonly ObjectInteractionStateMachine state = new ObjectInteractionStateMachine();
        PlayerController player;
        CarryableItem heldItem;
        PushPullBlock grippedBlock;
        GUIStyle promptStyle;

        void Awake()
        {
            player = GetComponent<PlayerController>();
            promptStyle = new GUIStyle { alignment = TextAnchor.MiddleCenter, fontSize = 18, normal = { textColor = Color.white } };
        }

        void OnEnable()
        {
            if (interact == null || interact.action == null)
            {
                Debug.LogError("PlayerInteractor requires the Interact input action.", this);
                enabled = false;
                return;
            }
            interact.action.performed += OnInteract;
            interact.action.Enable();
            if (throwAction != null && throwAction.action != null)
            {
                throwAction.action.performed += OnThrow;
                throwAction.action.Enable();
            }
        }

        void OnDisable()
        {
            if (interact != null && interact.action != null)
            {
                interact.action.performed -= OnInteract;
                interact.action.Disable();
            }
            nearby.Clear();
            if (throwAction != null && throwAction.action != null)
            {
                throwAction.action.performed -= OnThrow;
                throwAction.action.Disable();
            }
        }

        void Update()
        {
            if (state.State == ObjectInteractionState.Gripping && (grippedBlock == null || !grippedBlock.IsGripping))
                state.Release(ObjectInteractionState.Gripping);
        }

        void OnGUI()
        {
            string prompt = state.State switch
            {
                ObjectInteractionState.Carrying => "E: drop    F: throw",
                ObjectInteractionState.Gripping => "E: release",
                _ => FindClosest()?.prompt
            };
            if (!string.IsNullOrEmpty(prompt))
                GUI.Label(new Rect(Screen.width * 0.25f, Screen.height - 76f, Screen.width * 0.5f, 36f), prompt, promptStyle);
        }

        void OnTriggerEnter(Collider other)
        {
            var target = other.GetComponentInParent<Interactable>();
            if (target != null) nearby.Add(target);
        }

        void OnTriggerExit(Collider other)
        {
            var target = other.GetComponentInParent<Interactable>();
            if (target != null) nearby.Remove(target);
        }

        void OnInteract(InputAction.CallbackContext _)
        {
            if (!player.ControlEnabled) return;
            if (state.State == ObjectInteractionState.Carrying) { ReleaseHeldItem(false); return; }
            if (state.State == ObjectInteractionState.Gripping)
            {
                grippedBlock?.ReleaseGrip();
                state.Release(ObjectInteractionState.Gripping);
                grippedBlock = null;
                return;
            }

            Interactable closest = FindClosest();
            if (closest is CarryableItem item)
            {
                if (item.PickUp(player) && state.TryStart(ObjectInteractionState.Carrying))
                {
                    heldItem = item;
                    player.SetCarrySpeedMultiplier(item.CarrySpeedMultiplier);
                    nearby.Remove(item);
                }
                return;
            }
            if (closest is PushPullBlock block)
            {
                if (block.TryBeginGrip(player) && state.TryStart(ObjectInteractionState.Gripping)) grippedBlock = block;
                return;
            }
            closest?.Interact(player);
        }

        void OnThrow(InputAction.CallbackContext _)
        {
            if (player.ControlEnabled && state.State == ObjectInteractionState.Carrying) ReleaseHeldItem(true);
        }

        void ReleaseHeldItem(bool throwIt)
        {
            if (heldItem == null || !heldItem.Release(player, throwIt)) return;
            heldItem = null;
            player.SetCarrySpeedMultiplier(1f);
            state.Release(ObjectInteractionState.Carrying);
        }

        Interactable FindClosest()
        {
            Interactable closest = null;
            float closestDistance = float.PositiveInfinity;
            foreach (var target in nearby)
            {
                if (target == null) continue;
                if (target is CarryableItem carryable && !carryable.CanPickUp(player)) continue;
                if (target is PushPullBlock block && !block.CanGrip(player)) continue;
                float distance = (target.transform.position - transform.position).sqrMagnitude;
                if (distance >= closestDistance) continue;
                closest = target;
                closestDistance = distance;
            }
            return closest;
        }
    }
}
