#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    public static class SaveMenu
    {
        [MenuItem("Game/Dev/Delete Save")]
        public static void DeleteSave()
        {
            string path = Path.Combine(Application.persistentDataPath, "save.json");
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"Deleted save file: {path}");
            }
            else Debug.Log($"No save file found at: {path}");
        }
    }
}
#endif
