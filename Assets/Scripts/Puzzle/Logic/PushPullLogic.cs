using System;

namespace Game.Puzzle.Logic
{
    public enum ObjectInteractionState { Idle, Carrying, Gripping }
    public enum GripFace { PositiveX, NegativeX, PositiveZ, NegativeZ }

    public struct GripDelta
    {
        public readonly float X, Z;
        public GripDelta(float x, float z) { X = x; Z = z; }
    }

    public sealed class ObjectInteractionStateMachine
    {
        public ObjectInteractionState State { get; private set; }

        public bool TryStart(ObjectInteractionState next)
        {
            if (State != ObjectInteractionState.Idle || next == ObjectInteractionState.Idle) return false;
            State = next;
            return true;
        }

        public bool Release(ObjectInteractionState expected)
        {
            if (State != expected) return false;
            State = ObjectInteractionState.Idle;
            return true;
        }
    }

    public static class PushPullLogic
    {
        public static float PushSpeed(float partForce, float mass)
        {
            if (!IsFinite(partForce) || !IsFinite(mass) || partForce < 0f || mass <= 0f || partForce < mass) return 0f;
            return partForce / mass;
        }

        public static float GroundSnapY(float rootY, float colliderBottomOffset, float groundY, float clearance) =>
            IsFinite(rootY) && IsFinite(colliderBottomOffset) && IsFinite(groundY) && IsFinite(clearance)
                ? groundY + Math.Max(0f, clearance) - colliderBottomOffset
                : rootY;

        public static float CarrySpeedMultiplier(float mass) =>
            IsFinite(mass) && mass > 0f ? (float)(1d / Math.Sqrt(1d + mass)) : 1f;

        public static GripFace ChooseGripFace(float playerLocalX, float playerLocalZ)
        {
            if (Math.Abs(playerLocalX) >= Math.Abs(playerLocalZ))
                return playerLocalX < 0f ? GripFace.NegativeX : GripFace.PositiveX;
            return playerLocalZ < 0f ? GripFace.NegativeZ : GripFace.PositiveZ;
        }

        public static float ProjectGripInput(float inputX, float inputY, float normalX, float normalZ) =>
            Math.Max(-1f, Math.Min(1f, inputX * normalX + inputY * normalZ));

        public static GripDelta ProjectMovement(float inputX, float inputZ, float normalX, float normalZ, bool axisLock)
        {
            if (!axisLock) return new GripDelta(inputX, inputZ);
            float projected = ProjectGripInput(inputX, inputZ, normalX, normalZ);
            return new GripDelta(normalX * projected, normalZ * projected);
        }

        public static float MovementYawDegrees(float x, float z)
        {
            if (Math.Abs(x) >= Math.Abs(z)) return x < 0f ? -90f : x > 0f ? 90f : 0f;
            return z < 0f ? 180f : 0f;
        }

        public static GripDelta ResolveDeltaPerAxis(float x, float z, Func<float, float, bool> blocked)
        {
            float resolvedX = blocked != null && blocked(x, 0f) ? 0f : x;
            float resolvedZ = blocked != null && blocked(resolvedX, z) ? 0f : z;
            return new GripDelta(resolvedX, resolvedZ);
        }

        public static void GripSocketOffset(GripFace face, float halfX, float halfZ, float radius, float gap,
            out float x, out float z)
        {
            float reach = (face == GripFace.PositiveX || face == GripFace.NegativeX ? halfX : halfZ) + radius + gap;
            x = face == GripFace.PositiveX ? reach : face == GripFace.NegativeX ? -reach : 0f;
            z = face == GripFace.PositiveZ ? reach : face == GripFace.NegativeZ ? -reach : 0f;
        }

        public static float ThrowSpeed(float force, float mass, float scale, float minSpeed, float maxSpeed)
        {
            if (!IsFinite(force) || !IsFinite(mass) || !IsFinite(scale) || force <= 0f || mass <= 0f) return 0f;
            float min = Math.Max(0f, minSpeed);
            float max = Math.Max(min, maxSpeed);
            return Math.Max(min, Math.Min(max, force / mass * Math.Max(0f, scale)));
        }

        public static bool TryChooseFreeSpot(int candidateCount, Func<int, bool> isFree, out int index)
        {
            if (candidateCount > 0 && isFree != null)
            {
                for (int i = 0; i < candidateCount; i++)
                    if (isFree(i)) { index = i; return true; }
            }

            index = -1;
            return false;
        }

        static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
