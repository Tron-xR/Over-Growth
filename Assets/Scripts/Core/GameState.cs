using System;
using System.Collections.Generic;

namespace Game.Core
{
    /// Global progression flags. Gates/abilities/senses/logs all go through this.
    public static class GameState
    {
        static readonly HashSet<string> flags = new HashSet<string>();
        public static event Action<string> OnFlagSet;

        public static bool Has(string flag) => flags.Contains(flag);

        public static bool HasAll(IEnumerable<string> required)
        {
            foreach (var f in required) if (!flags.Contains(f)) return false;
            return true;
        }

        public static void Set(string flag)
        {
            if (flags.Add(flag)) OnFlagSet?.Invoke(flag);
        }

        public static string[] Snapshot()
        {
            var a = new string[flags.Count];
            flags.CopyTo(a);
            return a;
        }

        public static void Load(IEnumerable<string> saved)
        {
            flags.Clear();
            foreach (var f in saved) flags.Add(f);
        }

        public static void Clear() => flags.Clear();
    }
}
