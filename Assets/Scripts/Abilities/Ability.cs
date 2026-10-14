using UnityEngine;

namespace Game.Abilities
{
    public abstract class Ability : MonoBehaviour
    {
        public string id;                 // unlock flag = "ability." + id
        public bool IsActive { get; protected set; }
        public string UnlockFlag => "ability." + id;
        public abstract void Activate();
        public abstract void Deactivate();
    }
}
