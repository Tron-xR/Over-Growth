namespace Game.World.Logic
{
    public static class RespawnLogic
    {
        public static bool ShouldRespawn(float playerY, float killY, bool respawnInProgress) =>
            CanBeginRespawn(respawnInProgress) && IsFinite(playerY) && IsFinite(killY) && playerY < killY;

        public static bool CanBeginRespawn(bool respawnInProgress) => !respawnInProgress;

        public static bool IsSavedPositionValid(float x, float y, float z, float killY, bool areaExists) =>
            areaExists && IsFinite(x) && IsFinite(y) && IsFinite(z) && IsFinite(killY) && y >= killY;

        static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }

    public sealed class SafePositionTracker
    {
        float x, y, z;
        bool hasPosition;

        public void Reset(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            hasPosition = IsFinite(x) && IsFinite(y) && IsFinite(z);
        }

        public bool TryUpdate(float x, float y, float z, bool grounded, float killY)
        {
            if (!grounded || !RespawnLogic.IsSavedPositionValid(x, y, z, killY, true)) return false;
            Reset(x, y, z);
            return true;
        }

        public bool TryGet(out float x, out float y, out float z)
        {
            x = this.x;
            y = this.y;
            z = this.z;
            return hasPosition;
        }

        static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
