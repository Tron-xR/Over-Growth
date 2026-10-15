using System.Collections;
using Game.Player;
using Game.Puzzle.Logic;
using UnityEngine;

namespace Game.Puzzle
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class CarryableItem : Interactable
    {
        [SerializeField] CarryConfig config;
        Rigidbody body;
        Collider[] itemColliders;
        Collider[] solidColliders;
        bool[] colliderStates;
        Vector3 startPosition;
        Quaternion startRotation;
        Coroutine collisionIgnoreRoutine;
        Collider[][] ignoredPairs;

        public float Mass => config != null ? config.mass : 0f;
        public float CarrySpeedMultiplier => PushPullLogic.CarrySpeedMultiplier(Mass);

        public bool CanPickUp(PlayerController player) => player != null && config != null && transform.parent == null &&
            (transform.position - player.transform.position).sqrMagnitude <= config.grabRange * config.grabRange;

        void Awake()
        {
            prompt = "E: pick up";
            body = GetComponent<Rigidbody>();
            itemColliders = GetComponentsInChildren<Collider>();
            solidColliders = System.Array.FindAll(itemColliders, collider => !collider.isTrigger);
            colliderStates = new bool[itemColliders.Length];
            for (int i = 0; i < itemColliders.Length; i++) colliderStates[i] = itemColliders[i].enabled;
            startPosition = transform.position;
            startRotation = transform.rotation;
            if (config == null) { Debug.LogError("CarryableItem requires a CarryConfig.", this); enabled = false; return; }

            body.mass = config.mass;
            body.linearDamping = config.drag;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            foreach (var collider in solidColliders) collider.sharedMaterial = config.SlideMaterial;
        }

        void Update()
        {
            if (transform.parent == null && transform.position.y < config.killPlaneY) ResetToStart();
        }

        public bool PickUp(PlayerController player)
        {
            if (!CanPickUp(player)) return false;

            StopIgnoringPlayer();
            SetCollidersEnabled(false);
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
            body.useGravity = false;
            transform.SetParent(player.transform, false);
            transform.localPosition = player.CarryHoldLocalPosition;
            transform.localRotation = Quaternion.identity;
            return true;
        }

        public bool Release(PlayerController player, bool throwIt)
        {
            if (player == null || transform.parent != player.transform || config == null) return false;

            transform.SetParent(null, true);
            body.isKinematic = true;
            SetCollidersEnabled(true);
            Physics.SyncTransforms();
            Bounds bounds = GetSolidBounds();
            Vector3 forward = player.transform.forward;

            if (throwIt)
            {
                float frontOffset = player.CharacterRadius + bounds.extents.magnitude + config.dropClearance;
                Vector3 origin = transform.position + forward * frontOffset;
                var throwCandidates = new Vector3[9];
                throwCandidates[0] = origin;
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * Mathf.PI / 4f;
                    throwCandidates[i + 1] = origin + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * config.dropSearchRadius;
                }
                if (!PushPullLogic.TryChooseFreeSpot(throwCandidates.Length,
                        i => TryPlaceAtFreePosition(throwCandidates[i], out throwCandidates[i]), out int throwIndex))
                    return KeepCarried(player);

                transform.position = throwCandidates[throwIndex];
                body.isKinematic = false;
                body.useGravity = true;
                float throwSpeed = PushPullLogic.ThrowSpeed(player.Loadout?.PushPullForce ?? 1f, Mass,
                    config.throwSpeedScale, config.minThrowSpeed, config.maxThrowSpeed);
                body.linearVelocity = forward.normalized * throwSpeed + Vector3.up * config.throwArc;
                body.angularVelocity = Vector3.zero;
                IgnorePlayerBriefly(player);
                return true;
            }

            Vector3 dropOrigin = player.transform.position + forward * (player.CharacterRadius + bounds.extents.magnitude + config.dropClearance);
            var candidates = new Vector3[9];
            candidates[0] = dropOrigin;
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4f;
                candidates[i + 1] = dropOrigin + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * config.dropSearchRadius;
            }

            if (!PushPullLogic.TryChooseFreeSpot(candidates.Length, candidateIndex =>
            {
                Vector3 candidate = candidates[candidateIndex];
                if (!TryFindGround(player, candidate + Vector3.up * config.groundSearchDistance, config.groundSearchDistance * 2f, out var ground)) return false;
                candidate.y = PushPullLogic.GroundSnapY(candidate.y, bounds.min.y - transform.position.y, ground.point.y, config.dropClearance);
                return TryPlaceAtFreePosition(candidate, out candidates[candidateIndex]);
            }, out int dropIndex)) return KeepCarried(player);

            transform.position = candidates[dropIndex];
            body.isKinematic = false;
            body.useGravity = true;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            return true;
        }

        bool KeepCarried(PlayerController player)
        {
            transform.SetParent(player.transform, false);
            transform.localPosition = player.CarryHoldLocalPosition;
            transform.localRotation = Quaternion.identity;
            SetCollidersEnabled(false);
            body.isKinematic = true;
            body.useGravity = false;
            return false;
        }

        bool TryPlaceAtFreePosition(Vector3 position, out Vector3 placed)
        {
            transform.position = position;
            Physics.SyncTransforms();
            Bounds bounds = GetSolidBounds();
            Vector3 halfExtents = Vector3.Max(bounds.extents - Vector3.one * 0.005f, Vector3.one * 0.001f);
            foreach (var other in Physics.OverlapBox(bounds.center, halfExtents, Quaternion.identity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (other == null) continue;
                bool own = false;
                foreach (var collider in solidColliders) if (other == collider) { own = true; break; }
                if (!own) { placed = default; return false; }
            }
            placed = position;
            return true;
        }

        bool TryFindGround(PlayerController player, Vector3 origin, float distance, out RaycastHit closestHit)
        {
            closestHit = default;
            float closestDistance = float.PositiveInfinity;
            bool found = false;
            foreach (var hit in Physics.RaycastAll(origin, Vector3.down, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == null || hit.transform == player.transform || hit.transform.IsChildOf(player.transform) ||
                    System.Array.IndexOf(itemColliders, hit.collider) >= 0) continue;
                if (hit.distance >= closestDistance) continue;
                closestDistance = hit.distance;
                closestHit = hit;
                found = true;
            }
            return found;
        }

        Bounds GetSolidBounds()
        {
            if (solidColliders.Length == 0) return new Bounds(transform.position, Vector3.zero);
            Bounds bounds = solidColliders[0].bounds;
            for (int i = 1; i < solidColliders.Length; i++) bounds.Encapsulate(solidColliders[i].bounds);
            return bounds;
        }

        void SetCollidersEnabled(bool enabled)
        {
            for (int i = 0; i < itemColliders.Length; i++) itemColliders[i].enabled = enabled && colliderStates[i];
        }

        void IgnorePlayerBriefly(PlayerController player)
        {
            var playerColliders = player.GetComponentsInChildren<Collider>();
            ignoredPairs = new Collider[itemColliders.Length][];
            for (int i = 0; i < itemColliders.Length; i++)
            {
                ignoredPairs[i] = playerColliders;
                foreach (var other in playerColliders) Physics.IgnoreCollision(itemColliders[i], other, true);
            }
            collisionIgnoreRoutine = StartCoroutine(RestorePlayerCollisions());
        }

        IEnumerator RestorePlayerCollisions()
        {
            yield return new WaitForSeconds(config.playerCollisionIgnoreTime);
            SetIgnoredPairs(false);
            collisionIgnoreRoutine = null;
        }

        void StopIgnoringPlayer()
        {
            if (collisionIgnoreRoutine != null) StopCoroutine(collisionIgnoreRoutine);
            collisionIgnoreRoutine = null;
            SetIgnoredPairs(false);
        }

        void SetIgnoredPairs(bool ignored)
        {
            if (ignoredPairs == null) return;
            for (int i = 0; i < ignoredPairs.Length; i++)
            {
                if (itemColliders[i] == null || ignoredPairs[i] == null) continue;
                foreach (var other in ignoredPairs[i])
                    if (other != null) Physics.IgnoreCollision(itemColliders[i], other, ignored);
            }
            ignoredPairs = null;
        }

        void ResetToStart()
        {
            StopIgnoringPlayer();
            transform.SetParent(null, true);
            body.position = startPosition;
            body.rotation = startRotation;
            body.isKinematic = false;
            body.useGravity = true;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            SetCollidersEnabled(true);
        }
    }
}
