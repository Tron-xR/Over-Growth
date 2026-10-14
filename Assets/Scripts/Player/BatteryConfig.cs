using UnityEngine;

namespace Game.Player
{
    [CreateAssetMenu(menuName = "Game/Battery Config", fileName = "BatteryConfig")]
    public class BatteryConfig : ScriptableObject
    {
        [Min(0f)] public float max = 100f;
        [Min(0f)] public float baseDrainPerSecond = 0.5f;
        [Min(0f)] public float runDrainMultiplier = 1.8f;
        [Min(0f)] public float idleDrainPerSecond = 0.02f;
    }
}
