using UnityEngine;

namespace Game.Player
{
    [CreateAssetMenu(menuName = "Game/Player Stats", fileName = "PlayerStats")]
    public class PlayerStats : ScriptableObject
    {
        [Min(0f)] public float walkSpeed = 3f;
        [Min(0f)] public float runSpeed = 5.5f;
        [Min(0f)] public float dashSpeed = 14f;
        [Min(0f)] public float dashTime = 0.18f;
        [Min(0f)] public float dashCooldown = 0.8f;
        [Min(0f)] public float dashBatteryCost = 2f;
        [Min(0f)] public float turnSpeed = 720f;
        public Vector3 carryHoldLocalPosition = new Vector3(0f, 1f, 0.8f);
        [Min(0f)] public float throwImpulse = 6f;
        [Min(0f)] public float throwLift = 1.5f;
        [Min(0)] public int totalSlots = 4;
    }
}
