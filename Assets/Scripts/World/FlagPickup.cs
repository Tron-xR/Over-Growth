using System.Collections;
using Game.Core;
using Game.Player;
using UnityEngine;

namespace Game.World
{
    [RequireComponent(typeof(Collider))]
    public class FlagPickup : MonoBehaviour
    {
        [SerializeField] string flag;

        Collider pickupCollider;

        void Awake()
        {
            pickupCollider = GetComponent<Collider>();
            pickupCollider.enabled = false;
        }

        IEnumerator Start()
        {
            yield return null; // Let SaveCoordinator restore flags before enabling collection.
            if (string.IsNullOrWhiteSpace(flag))
            {
                Debug.LogError("FlagPickup requires a GameState flag.", this);
                enabled = false;
                yield break;
            }

            if (GameState.Has(flag)) Destroy(gameObject);
            else pickupCollider.enabled = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!enabled || string.IsNullOrEmpty(flag) || other.GetComponentInParent<PlayerController>() == null) return;
            GameState.Set(flag);
            Destroy(gameObject);
        }
    }
}
