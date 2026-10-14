using UnityEngine;

namespace Game.Core
{
    [System.Serializable]
    public class SaveData
    {
        public string[] flags;
        public string[] ownedParts;
        public string[] equippedParts;
        public float battery;
        public string areaId;
        public Vector3 position;
    }
}
