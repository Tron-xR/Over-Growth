using System;
using System.Collections.Generic;

namespace Game.Puzzle
{
    public static class PuzzleGateLogic
    {
        public static bool RequirementsMet(IEnumerable<string> requiredFlags, Func<string, bool> hasFlag)
        {
            if (requiredFlags == null) return true;
            if (hasFlag == null) throw new ArgumentNullException(nameof(hasFlag));

            foreach (var flag in requiredFlags)
                if (string.IsNullOrWhiteSpace(flag) || !hasFlag(flag)) return false;

            return true;
        }
    }
}
