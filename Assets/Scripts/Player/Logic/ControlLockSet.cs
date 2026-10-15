using System;
using System.Collections.Generic;

namespace Game.Player.Logic
{
    public sealed class ControlLockSet
    {
        readonly HashSet<string> reasons = new HashSet<string>();

        public bool IsLocked => reasons.Count > 0;

        public bool Add(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("A control lock reason is required.", nameof(reason));
            return reasons.Add(reason);
        }

        public bool Remove(string reason) => reasons.Remove(reason);
    }
}
