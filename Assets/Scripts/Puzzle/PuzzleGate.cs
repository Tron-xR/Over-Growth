using UnityEngine;
using Game.Core;

namespace Game.Puzzle
{
    /// Opens when all required flags are set. Optionally sets a flag when opened.
    public class PuzzleGate : MonoBehaviour
    {
        [SerializeField] string[] requiredFlags;
        [SerializeField] string flagOnOpen;
        [SerializeField] GameObject closedVisual;
        [SerializeField] Collider blocker;
        public bool IsOpen { get; private set; }

        void OnEnable() { GameState.OnFlagSet += OnFlagSet; Evaluate(); }
        void OnDisable() { GameState.OnFlagSet -= OnFlagSet; }

        void OnFlagSet(string _) => Evaluate();

        void Evaluate()
        {
            if (IsOpen || !GameState.HasAll(requiredFlags)) return;
            IsOpen = true;
            if (blocker) blocker.enabled = false;
            if (closedVisual) closedVisual.SetActive(false);
            if (!string.IsNullOrEmpty(flagOnOpen)) GameState.Set(flagOnOpen);
        }
    }
}
