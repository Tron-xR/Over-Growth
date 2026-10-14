using Game.Core;
using Game.Parts;
using Game.Player;
using UnityEngine;

namespace Game.World
{
    [RequireComponent(typeof(Collider))]
    public class PartPickup : MonoBehaviour
    {
        [SerializeField] PartData part;

        string Flag => part == null ? string.Empty : "part.owned." + part.id;

        void Start()
        {
            if (!string.IsNullOrEmpty(Flag) && GameState.Has(Flag)) Destroy(gameObject);
        }

        void OnTriggerEnter(Collider other)
        {
            if (part == null) return;
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null || !player.Inventory.Add(part)) return;
            GameState.Set(Flag);
            Destroy(gameObject);
        }
    }
}
