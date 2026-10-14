using UnityEngine;
using Game.Core;

namespace Game.World
{
    [RequireComponent(typeof(Collider))]
    public class Collectible : MonoBehaviour
    {
        public string id;
        public bool isLog;
        void Start() { if (GameState.Has(Flag)) Destroy(gameObject); }
        string Flag => (isLog ? "log." : "collectible.") + id;
        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            GameState.Set(Flag);
            Destroy(gameObject);
        }
    }
}
