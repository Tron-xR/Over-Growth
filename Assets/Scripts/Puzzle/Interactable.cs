using UnityEngine;
using UnityEngine.Events;
using Game.Player;

namespace Game.Puzzle
{
    public class Interactable : MonoBehaviour
    {
        public string prompt = "Interact";
        public UnityEvent onInteract;
        public virtual void Interact() => onInteract?.Invoke();
        public virtual void Interact(PlayerController player) => Interact();
    }
}
