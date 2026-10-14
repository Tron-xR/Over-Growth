using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Game.Player;

namespace Game.World
{
    public class RespawnManager : MonoBehaviour
    {
        [SerializeField] Battery battery;
        [SerializeField] PlayerController player;
        [SerializeField] CanvasGroup fade;
        [SerializeField, Min(0f)] float fadeSeconds = 1f;
        [SerializeField] string currentAreaId = "room";
        readonly Dictionary<string, RespawnPoint> points = new Dictionary<string, RespawnPoint>();
        Coroutine respawnRoutine;
        public string CurrentAreaId => currentAreaId;
        public event Action<string> OnAreaChanged;

        void Awake()
        {
            foreach (var p in FindObjectsOfType<RespawnPoint>()) points[p.areaId] = p;
        }

        void OnEnable()
        {
            if (battery == null || player == null) { Debug.LogError("RespawnManager requires Battery and PlayerController references.", this); enabled = false; return; }
            battery.OnDepleted += HandleDepleted;
        }
        void OnDisable()
        {
            if (battery != null) battery.OnDepleted -= HandleDepleted;
            if (respawnRoutine != null) StopCoroutine(respawnRoutine);
        }

        public bool SetCurrentArea(string areaId)
        {
            if (string.IsNullOrEmpty(areaId) || !points.ContainsKey(areaId)) return false;
            if (currentAreaId == areaId) return true;
            currentAreaId = areaId;
            OnAreaChanged?.Invoke(areaId);
            return true;
        }
        void HandleDepleted()
        {
            if (respawnRoutine == null) respawnRoutine = StartCoroutine(Respawn());
        }

        IEnumerator Respawn()
        {
            player.ControlEnabled = false;
            yield return FadeTo(1f);
            if (points.TryGetValue(CurrentAreaId ?? "", out var p))
            {
                var cc = player.GetComponent<CharacterController>();
                cc.enabled = false;
                player.transform.SetPositionAndRotation(p.transform.position, p.transform.rotation);
                cc.enabled = true;
            }
            else Debug.LogError($"No RespawnPoint registered for area '{CurrentAreaId}'.", this);
            battery.Refill();
            yield return FadeTo(0f);
            player.ControlEnabled = true;
            respawnRoutine = null;
        }

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
