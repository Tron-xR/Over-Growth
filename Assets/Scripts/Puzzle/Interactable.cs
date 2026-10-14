using UnityEngine;
using UnityEngine.Events;

namespace Game.Puzzle
{
    public class Interactable : MonoBehaviour
    {
        public string prompt = "Interact";
        public UnityEvent onInteract;
        public virtual void Interact() => onInteract?.Invoke();
    }
}
