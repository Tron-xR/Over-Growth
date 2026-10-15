using UnityEngine;

namespace Game.Puzzle
{
    [CreateAssetMenu(menuName = "Game/Puzzles/Carry Config", fileName = "CarryConfig")]
    public class CarryConfig : ScriptableObject
    {
        [Min(0.01f)] public float mass = 0.8f;
        [Min(0f)] public float drag = 0.2f;
        [Range(0f, 1f)] public float friction = 0.2f;
        [Min(0.1f)] public float grabRange = 1.5f;
        [Min(0f)] public float dropClearance = 0.03f;
        [Min(0.1f)] public float groundSearchDistance = 5f;
        [Min(0.1f)] public float dropSearchRadius = 0.8f;
        [Min(0.01f)] public float throwSpeedScale = 5f;
        [Min(0f)] public float minThrowSpeed = 2f;
        [Min(0f)] public float maxThrowSpeed = 12f;
        [Min(0f)] public float throwArc = 2f;
        [Min(0f)] public float playerCollisionIgnoreTime = 0.25f;
        public float killPlaneY = -5f;

        PhysicsMaterial slideMaterial;

        public PhysicsMaterial SlideMaterial
        {
            get
            {
                if (slideMaterial == null)
                    slideMaterial = new PhysicsMaterial("Carryable Slide")
                    {
                        staticFriction = friction,
                        dynamicFriction = friction,
                        frictionCombine = PhysicsMaterialCombine.Minimum
                    };
                return slideMaterial;
            }
        }
    }
}
