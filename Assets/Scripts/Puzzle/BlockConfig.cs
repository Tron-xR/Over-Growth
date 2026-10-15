using UnityEngine;

namespace Game.Puzzle
{
    public enum AxisLockMode { Free, GripAxisOnly }

    [CreateAssetMenu(menuName = "Game/Puzzles/Block Config", fileName = "BlockConfig")]
    public class BlockConfig : ScriptableObject
    {
        [Min(0.01f)] public float mass = 0.8f;
        [Min(0f)] public float drag = 5f;
        [Range(0f, 1f)] public float friction = 0.2f;
        [Min(0.1f)] public float grabRange = 1.5f;
        [Min(0f)] public float socketGap = 0.05f;
        [Min(0.01f)] public float snapDuration = 0.2f;
        [Min(0.01f)] public float speedScale = 1f;
        [Min(0.01f)] public float maxMoveSpeed = 2f;
        public AxisLockMode axisLockMode = AxisLockMode.Free;
        public bool snapToGrid;
        [Min(0.01f)] public float gridSize = 1f;
        public float killPlaneY = -5f;

        PhysicsMaterial slideMaterial;

        public PhysicsMaterial SlideMaterial
        {
            get
            {
                if (slideMaterial == null)
                    slideMaterial = new PhysicsMaterial("Block Slide")
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
