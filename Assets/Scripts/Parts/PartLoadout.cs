using System;
using System.Collections.Generic;

namespace Game.Parts
{
    /// Pure logic: slot budget + aggregated stats. Unit-testable.
    public class PartLoadout
    {
        readonly List<PartData> equipped = new List<PartData>();
        public int TotalSlots { get; set; }
        public IReadOnlyList<PartData> Equipped => equipped;
        public event Action OnChanged;

        public PartLoadout(int totalSlots) { TotalSlots = totalSlots; }

        public int UsedSlots { get { int s = 0; foreach (var p in equipped) s += p.slotCost; return s; } }

        public bool CanEquip(PartData p) => p != null && p.slotCost > 0 && !equipped.Contains(p) && UsedSlots + p.slotCost <= TotalSlots;

        public bool Equip(PartData p)
        {
            if (!CanEquip(p)) return false;
            equipped.Add(p); OnChanged?.Invoke(); return true;
        }

        public bool Unequip(PartData p)
        {
            bool ok = equipped.Remove(p);
            if (ok) OnChanged?.Invoke();
            return ok;
        }

        public void Clear()
        {
            if (equipped.Count == 0) return;
            equipped.Clear();
            OnChanged?.Invoke();
        }

        // Multiplicative stacking; empty loadout = 1.
        public float SpeedMultiplier { get { float v = 1f; foreach (var p in equipped) v *= p.speedMultiplier; return v; } }
        public float PowerDrainMultiplier { get { float v = 1f; foreach (var p in equipped) v *= p.powerDrainMultiplier; return v; } }
        public float PushPullForce { get { float v = 1f; foreach (var p in equipped) v *= p.pushPullForce; return v; } }
    }
}
