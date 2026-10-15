using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Game.Player;
using Game.World.Logic;

namespace Game.World
{
    public class RespawnManager : MonoBehaviour
    {
        const string ControlLock = "world.respawn";
        [SerializeField] Battery battery;
        [SerializeField] PlayerController player;
        [SerializeField] RespawnConfig config;
        [SerializeField] CanvasGroup fade;
        [SerializeField, Min(0f)] float fadeSeconds = 1f;
        [SerializeField] string currentAreaId = "room";
        readonly Dictionary<string, RespawnPoint> points = new Dictionary<string, RespawnPoint>();
        Coroutine respawnRoutine;
        bool respawning;
        readonly SafePositionTracker safePosition = new SafePositionTracker();
        public string CurrentAreaId => currentAreaId;
        public float KillY => config != null ? config.killY : float.NegativeInfinity;
        public event Action<string> OnAreaChanged;

        void Awake()
        {
            foreach (var p in FindObjectsOfType<RespawnPoint>()) points[p.areaId] = p;
            ResetSafePosition();
        }

        void OnEnable()
        {
            if (battery == null || player == null || config == null) { Debug.LogError("RespawnManager requires Battery, PlayerController, and RespawnConfig references.", this); enabled = false; return; }
            battery.OnDepleted += HandleDepleted;
        }
        void OnDisable()
        {
            if (battery != null) battery.OnDepleted -= HandleDepleted;
            if (respawnRoutine != null) StopCoroutine(respawnRoutine);
            player?.RemoveLock(ControlLock);
            respawnRoutine = null;
            respawning = false;
        }

        void Update()
        {
            var position = player.transform.position;
            safePosition.TryUpdate(position.x, position.y, position.z, player.IsGrounded, KillY);
            if (RespawnLogic.ShouldRespawn(position.y, KillY, respawning)) BeginRespawn(false);
        }

        public bool SetCurrentArea(string areaId)
        {
            if (string.IsNullOrEmpty(areaId) || !points.ContainsKey(areaId)) return false;
            if (currentAreaId == areaId) return true;
            currentAreaId = areaId;
            ResetSafePosition();
            OnAreaChanged?.Invoke(areaId);
            return true;
        }

        void ResetSafePosition()
        {
            if (points.TryGetValue(CurrentAreaId ?? "", out var point))
                safePosition.Reset(point.transform.position.x, point.transform.position.y, point.transform.position.z);
        }

        void HandleDepleted() => BeginRespawn(true);

        void BeginRespawn(bool refillBattery)
        {
            if (!RespawnLogic.CanBeginRespawn(respawning)) return;
            respawning = true;
            respawnRoutine = StartCoroutine(Respawn(refillBattery));
        }

        IEnumerator Respawn(bool refillBattery)
        {
            player.AddLock(ControlLock);
            yield return FadeTo(1f);
            if (points.TryGetValue(CurrentAreaId ?? "", out var p))
                player.Teleport(p.transform.position, p.transform.rotation);
            else Debug.LogError($"No RespawnPoint registered for area '{CurrentAreaId}'.", this);
            if (refillBattery) battery.Refill();
            yield return FadeTo(0f);
            player.RemoveLock(ControlLock);
            respawnRoutine = null;
            respawning = false;
        }

        public bool TryGetSafePosition(out Vector3 position)
        {
            if (safePosition.TryGet(out float x, out float y, out float z))
            {
                position = new Vector3(x, y, z);
                return true;
            }
            position = GetCurrentRespawnPoint().transform.position;
            return false;
        }

        public RespawnPoint GetCurrentRespawnPoint()
        {
            if (points.TryGetValue(CurrentAreaId ?? "", out var point)) return point;
            throw new InvalidOperationException($"No RespawnPoint registered for area '{CurrentAreaId}'.");
        }

        public void TeleportToCurrentRespawnPoint()
        {
            var point = GetCurrentRespawnPoint();
            player.Teleport(point.transform.position, point.transform.rotation);
            ResetSafePosition(point.transform.position);
        }

        public void RestorePlayerPosition(Vector3 position)
        {
            player.Teleport(position, player.transform.rotation);
            ResetSafePosition(position);
        }

        void ResetSafePosition(Vector3 position) => safePosition.Reset(position.x, position.y, position.z);

        System.Collections.IEnumerator FadeTo(float target)
        {
            if (fade == null) { yield return new WaitForSecondsRealtime(fadeSeconds); yield break; }
            fade.blocksRaycasts = true;
            float start = fade.alpha;
            float startedAt = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startedAt < fadeSeconds)
            {
                fade.alpha = Mathf.Lerp(start, target, (Time.realtimeSinceStartup - startedAt) / fadeSeconds);
                yield return null;
            }
            fade.alpha = target;
            fade.blocksRaycasts = target > 0f;
        }
    }
}
