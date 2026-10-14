using System;
using System.IO;
using UnityEngine;

namespace Game.Core
{
    [System.Serializable]
    public class SaveData
    {
        public string[] flags;
        public string[] equippedParts;
        public float battery;
        public string areaId;
        public Vector3 position;
    }

    public static class SaveSystem
    {
        static string Path => System.IO.Path.Combine(Application.persistentDataPath, "save.json");

        public static void Save(SaveData data) =>
            File.WriteAllText(Path, JsonUtility.ToJson(data, true));

        public static bool TryLoad(out SaveData data)
        {
            data = null;
            if (!File.Exists(Path)) return false;
            try
            {
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(Path));
                return data != null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not load save data: {e.Message}");
                return false;
            }
        }
    }
}
