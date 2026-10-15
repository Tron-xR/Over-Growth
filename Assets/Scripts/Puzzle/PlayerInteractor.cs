using System.Collections.Generic;
using Game.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Puzzle
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] InputActionReference interact;

        readonly HashSet<Interactable> nearby = new HashSet<Interactable>();
        PlayerController player;

        void Awake() => player = GetComponent<PlayerController>();

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
        }

        void OnDisable()
        {
            if (interact != null && interact.action != null)
            {
                interact.action.performed -= OnInteract;
                interact.action.Disable();
            }
            nearby.Clear();
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

            Interactable closest = null;
            float closestDistance = float.PositiveInfinity;
            foreach (var target in nearby)
            {
                if (target == null) continue;
                float distance = (target.transform.position - transform.position).sqrMagnitude;
                if (distance >= closestDistance) continue;
                closest = target;
                closestDistance = distance;
            }

            closest?.Interact();
        }
    }
}
