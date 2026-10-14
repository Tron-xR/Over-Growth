using UnityEngine;
using UnityEngine.InputSystem;
using Game.Core;

namespace Game.Abilities
{
    public class AbilityController : MonoBehaviour
    {
        [SerializeField] Ability[] abilities;
        [SerializeField] InputActionReference useAbility;
        int selected;

        void OnEnable() { useAbility.action.Enable(); useAbility.action.performed += OnUse; }
        void OnDisable() { useAbility.action.performed -= OnUse; }

        void OnUse(InputAction.CallbackContext _)
        {
            if (abilities.Length == 0) return;
            var a = abilities[selected];
            if (!GameState.Has(a.UnlockFlag)) return;
            if (a.IsActive) a.Deactivate(); else a.Activate();
        }

        public void Select(int i) { if (i >= 0 && i < abilities.Length) selected = i; }
    }
}
