using System;

namespace Game.Senses
{
    public static class SightFadeState
    {
        public static bool CanBegin(bool introDone, bool sightRestored) => introDone && !sightRestored;

        public static float Progress(float elapsed, float duration) =>
            duration <= 0f ? 1f : (float)Math.Min(1d, Math.Max(0d, elapsed / duration));
    }
}
