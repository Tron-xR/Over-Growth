using System;
using System.Collections.Generic;

namespace Game.Parts
{
    public class PartInventory
    {
        readonly List<PartData> owned = new List<PartData>();
        public IReadOnlyList<PartData> Owned => owned;
        public PartLoadout Loadout { get; }
        public event Action OnChanged;

        public PartInventory(int totalSlots)
        {
            Loadout = new PartLoadout(totalSlots);
            Loadout.OnChanged += () => OnChanged?.Invoke();
        }

        public bool Owns(PartData part) => part != null && owned.Contains(part);

        public bool Add(PartData part)
        {
            if (part == null || string.IsNullOrWhiteSpace(part.id) || owned.Exists(p => p.id == part.id)) return false;
            owned.Add(part);
            OnChanged?.Invoke();
            return true;
        }

        public bool Equip(PartData part) => Owns(part) && Loadout.Equip(part);
        public bool Unequip(PartData part) => Loadout.Unequip(part);

        public void Restore(IEnumerable<PartData> ownedParts, IEnumerable<PartData> equippedParts)
        {
            owned.Clear();
            foreach (var part in ownedParts ?? Array.Empty<PartData>()) Add(part);
            var restoredEquipped = new List<PartData>();
            foreach (var part in equippedParts ?? Array.Empty<PartData>())
                if (Owns(part)) restoredEquipped.Add(part);
            Loadout.Restore(restoredEquipped);
            OnChanged?.Invoke();
        }
    }
}
