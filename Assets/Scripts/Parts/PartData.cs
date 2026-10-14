using UnityEngine;

namespace Game.Parts
{
    public enum PartCategory { Locomotion, Arm, Leg, Core, Misc }

    [CreateAssetMenu(menuName = "Game/Part", fileName = "Part_")]
    public class PartData : ScriptableObject
    {
        public string id;
        public string displayName;
        public PartCategory category;
        [Min(1)] public int slotCost = 1;
        public float speedMultiplier = 1f;
        public float pushPullForce = 1f;
        public float powerDrainMultiplier = 1f;
        public GameObject visualPrefab;
        [TextArea] public string description;
    }
}
