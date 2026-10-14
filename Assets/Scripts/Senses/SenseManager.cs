using UnityEngine;
using Game.Core;

namespace Game.Senses
{
    /// Toggles presentation layers as senses are regained.
    public class SenseManager : MonoBehaviour
    {
        public System.Action<string> OnSenseGained;
        void OnEnable() { GameState.OnFlagSet += OnFlag; }
        void OnDisable() { GameState.OnFlagSet -= OnFlag; }
        void OnFlag(string f) { if (f.StartsWith("sense.")) OnSenseGained?.Invoke(f); }
        // TODO: sight = fade/blur volume, hearing = audio layer, smell = hint VFX
    }
}
