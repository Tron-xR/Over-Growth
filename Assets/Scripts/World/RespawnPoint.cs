using UnityEngine;

namespace Game.World
{
    public class RespawnPoint : MonoBehaviour
    {
        public string areaId;
        [Tooltip("Optional dialogue id played on respawn (e.g. old man).")]
        public string respawnDialogueId;
    }
}
