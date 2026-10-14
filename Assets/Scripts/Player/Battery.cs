using System;
using UnityEngine;
using Game.Parts;

namespace Game.Player
{
    public class Battery : MonoBehaviour
    {
        [SerializeField] BatteryConfig config;

        public float Current { get; private set; }
        public float Max => config != null ? config.max : 0f;
        public event Action<float, float> OnChanged;
        public event Action OnDepleted;

        public PartLoadout Loadout { get; set; }

        void Awake()
        {
            if (config == null) { Debug.LogError("BatteryConfig is required.", this); enabled = false; return; }
            Current = config.max;
        }

        public void Tick(bool moving, bool running, float dt)
        {
            if (Current <= 0f) return;
            float mult = Loadout != null ? Loadout.PowerDrainMultiplier : 1f;
            float rate = moving ? config.baseDrainPerSecond * mult * (running ? config.runDrainMultiplier : 1f)
                                : config.idleDrainPerSecond;
            Spend(rate * dt);
        }

        public void Spend(float amount)
        {
            if (Current <= 0f || amount <= 0f) return;
            Current = Mathf.Max(0f, Current - amount);
            OnChanged?.Invoke(Current, Max);
            if (Current <= 0f) OnDepleted?.Invoke();
        }

        public void Refill(float amount = float.MaxValue)
        {
            if (amount <= 0f) return;
            Current = Mathf.Min(Max, Current + amount);
            OnChanged?.Invoke(Current, Max);
        }

        public void Restore(float current)
        {
            Current = Mathf.Clamp(current, 0f, Max);
            OnChanged?.Invoke(Current, Max);
            if (Current <= 0f) OnDepleted?.Invoke();
        }
    }
}
