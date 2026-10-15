using UnityEngine;

namespace Game.World
{
    [CreateAssetMenu(menuName = "Game/Respawn Config", fileName = "RespawnConfig")]
    public class RespawnConfig : ScriptableObject
    {
        [Tooltip("Player Y below which a free fall respawn starts. The greybox floor is at Y=0.")]
        public float killY = -5f;
    }
}
