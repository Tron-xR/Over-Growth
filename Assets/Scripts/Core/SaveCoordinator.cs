using System.Collections.Generic;
using Game.Parts;
using Game.Player;
using Game.World;
using UnityEngine;

namespace Game.Core
{
    public class SaveCoordinator : MonoBehaviour
    {
        [SerializeField] Battery battery;
        [SerializeField] PlayerController player;
        [SerializeField] RespawnManager respawnManager;
        [SerializeField] PartData[] partCatalog;
        bool initialized;

        void OnEnable()
        {
            GameState.OnFlagSet += SaveOnFlagSet;
            if (respawnManager != null) respawnManager.OnAreaChanged += SaveOnAreaChanged;
        }

        void OnDisable()
        {
            GameState.OnFlagSet -= SaveOnFlagSet;
            if (respawnManager != null) respawnManager.OnAreaChanged -= SaveOnAreaChanged;
            if (player != null && player.Inventory != null) player.Inventory.OnChanged -= SaveOnLoadoutChanged;
        }

        void Start()
        {
            if (battery == null || player == null || respawnManager == null)
            {
                Debug.LogError("SaveCoordinator requires Battery, PlayerController, and RespawnManager references.", this);
                enabled = false;
                return;
            }

            if (SaveSystem.TryLoad(out var data))
            {
                GameState.Load(data.flags ?? System.Array.Empty<string>());
                var restoredOwned = ResolveParts(data.ownedParts ?? data.equippedParts);
                var restoredEquipped = ResolveParts(data.equippedParts);
                player.RestoreParts(restoredOwned, restoredEquipped);
                bool restorePosition = string.IsNullOrEmpty(data.areaId) || respawnManager.SetCurrentArea(data.areaId);
                if (!restorePosition) Debug.LogWarning($"Saved area '{data.areaId}' is not available; keeping the scene start position.", this);
                if (restorePosition)
                {
                    var characterController = player.GetComponent<CharacterController>();
                    characterController.enabled = false;
                    player.transform.position = data.position;
                    characterController.enabled = true;
                }
                battery.Restore(data.battery);
            }

            player.Inventory.OnChanged += SaveOnLoadoutChanged;
            initialized = true;
        }

        void OnApplicationPause(bool paused) { if (paused) Save(); }
        void OnApplicationQuit() => Save();
        void SaveOnFlagSet(string _) => Save();
        void SaveOnAreaChanged(string _) => Save();
        void SaveOnLoadoutChanged() => Save();

        void Save()
        {
            if (!initialized || !enabled) return;
            var data = new SaveData
            {
                flags = GameState.Snapshot(),
                ownedParts = GetPartIds(player.Inventory.Owned),
                equippedParts = GetPartIds(player.Loadout.Equipped),
                battery = battery.Current,
                areaId = respawnManager.CurrentAreaId,
                position = player.transform.position
            };
            try { SaveSystem.Save(data); }
            catch (System.Exception e) { Debug.LogError($"Could not save game progress: {e.Message}", this); }
        }

        List<PartData> ResolveParts(string[] ids)
        {
            var resolved = new List<PartData>();
            foreach (var id in ids ?? System.Array.Empty<string>())
            {
                bool found = false;
                foreach (var part in partCatalog ?? System.Array.Empty<PartData>())
                    if (part != null && part.id == id) { resolved.Add(part); found = true; break; }
                if (!found) Debug.LogWarning($"Saved part '{id}' is missing from the SaveCoordinator catalog.", this);
            }
            return resolved;
        }

        static string[] GetPartIds(IReadOnlyList<PartData> parts)
        {
            var ids = new string[parts.Count];
            for (int i = 0; i < parts.Count; i++) ids[i] = parts[i].id;
            return ids;
        }
    }
}
