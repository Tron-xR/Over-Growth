using System.Collections;
using Game.Dialogue;
using Game.Player;
using Game.Puzzle.Logic;
using UnityEngine;

namespace Game.Puzzle
{
    [RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
    public class PushPullBlock : Interactable
    {
        const string SnapLock = "grab.snap";
        [SerializeField] BlockConfig config;
        [SerializeField] DialogueRunner dialogueRunner;
        Rigidbody body;
        BoxCollider blockCollider;
        RigidbodyConstraints originalConstraints;
        Vector3 startPosition;
        Quaternion startRotation;
        bool originalKinematic;
        bool gripping;
        bool snapInProgress;
        bool heavyThoughtShown;
        PlayerController grippedPlayer;
        Vector3 gripNormal;
        Vector3 gripBlockTargetPosition;
        Quaternion gripBlockTargetRotation;
        GripFace gripFace;
        Coroutine snapRoutine;

        public bool IsGripping => gripping;

        public bool CanGrip(PlayerController player) => player != null && config != null && !gripping &&
            (transform.position - player.transform.position).sqrMagnitude <= config.grabRange * config.grabRange;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            blockCollider = GetComponent<BoxCollider>();
            startPosition = transform.position;
            startRotation = transform.rotation;
            prompt = "E: grip";
            if (config == null) { Debug.LogError("PushPullBlock requires a BlockConfig.", this); enabled = false; return; }

            body.mass = config.mass;
            body.linearDamping = config.drag;
            body.constraints |= RigidbodyConstraints.FreezeRotation;
            originalConstraints = body.constraints;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            blockCollider.sharedMaterial = config.SlideMaterial;
        }

        void Update()
        {
            if (transform.position.y < config.killPlaneY) ResetToStart();
        }

        public override void Interact(PlayerController player) => TryBeginGrip(player);

        public bool TryBeginGrip(PlayerController player)
        {
            if (!CanGrip(player)) return false;

            Bounds bounds = blockCollider.bounds;
            Vector3 localPlayer = transform.InverseTransformPoint(player.transform.position) - blockCollider.center;
            gripFace = PushPullLogic.ChooseGripFace(localPlayer.x, localPlayer.z);
            float socketScale = Mathf.Max(0.0001f, Mathf.Abs(gripFace == GripFace.PositiveX || gripFace == GripFace.NegativeX
                ? transform.lossyScale.x : transform.lossyScale.z));
            PushPullLogic.GripSocketOffset(gripFace, blockCollider.size.x * 0.5f,
                blockCollider.size.z * 0.5f, player.CharacterRadius / socketScale, config.socketGap / socketScale,
                out float socketX, out float socketZ);
            Vector3 localNormal = gripFace switch
            {
                GripFace.PositiveX => Vector3.right,
                GripFace.NegativeX => Vector3.left,
                GripFace.PositiveZ => Vector3.forward,
                _ => Vector3.back
            };
            gripNormal = transform.TransformDirection(localNormal).normalized;
            Vector3 localSocket = blockCollider.center + new Vector3(socketX, 0f, socketZ);
            Vector3 targetPosition = transform.TransformPoint(localSocket);
            targetPosition.y = player.transform.position.y;
            Quaternion targetRotation = Quaternion.LookRotation(-gripNormal, Vector3.up);

            originalKinematic = body.isKinematic;
            body.isKinematic = true;
            body.constraints = (originalConstraints | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ) &
                               ~RigidbodyConstraints.FreezeRotationY;
            gripBlockTargetPosition = body.position;
            gripBlockTargetRotation = body.rotation;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            gripping = true;
            snapInProgress = true;
            grippedPlayer = player;
            player.SetGrippedBlock(this);
            player.AddLock(SnapLock);
            snapRoutine = StartCoroutine(SnapPlayer(player, targetPosition, targetRotation));
            return true;
        }

        IEnumerator SnapPlayer(PlayerController player, Vector3 targetPosition, Quaternion targetRotation)
        {
            Vector3 start = player.transform.position;
            Quaternion startFacing = player.transform.rotation;
            float duration = config.snapDuration;
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = t * t * (3f - 2f * t);
                player.MoveWhileLocked(Vector3.Lerp(start, targetPosition, eased), Quaternion.Slerp(startFacing, targetRotation, eased));
                yield return null;
            }
            player.MoveWhileLocked(targetPosition, targetRotation);
            gripBlockTargetPosition = body.position;
            gripBlockTargetRotation = body.rotation;
            snapInProgress = false;
            snapRoutine = null;
            player.RemoveLock(SnapLock);
        }

        public bool MoveGrip(PlayerController player, Vector2 input, float deltaTime)
        {
            if (!gripping || snapInProgress || player != grippedPlayer) return false;
            bool axisLock = config.axisLockMode == AxisLockMode.GripAxisOnly;
            GripDelta movement = PushPullLogic.ProjectMovement(input.x, input.y, gripNormal.x, gripNormal.z, axisLock);
            Vector2 direction = new Vector2(movement.X, movement.Z);
            if (direction.sqrMagnitude < 0.0001f) return false;

            float speed = PushPullLogic.PushSpeed(player.Loadout?.PushPullForce ?? 1f, config.mass) * config.speedScale;
            if (speed <= 0f)
            {
                ShowHeavyThoughtOnce();
                return false;
            }

            direction = direction.normalized * Mathf.Min(speed, config.maxMoveSpeed) * deltaTime;
            GripDelta resolved = PushPullLogic.ResolveDeltaPerAxis(direction.x, direction.y, (x, z) =>
            {
                Vector3 offset = new Vector3(x, 0f, 0f);
                Vector3 step = new Vector3(0f, 0f, z);
                return IsPathBlocked(offset, step, player);
            });
            Vector3 intended = new Vector3(resolved.X, 0f, resolved.Z);
            if (intended.sqrMagnitude < 0.000001f) return false;
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null) Physics.IgnoreCollision(controller, blockCollider, true);
            try
            {
                Vector3 actual = player.MoveForGrip(intended);
                if (actual.sqrMagnitude < 0.000001f) return false;
                gripBlockTargetPosition += actual;
                gripBlockTargetRotation = Quaternion.Euler(0f,
                    PushPullLogic.MovementYawDegrees(intended.x, intended.z), 0f);
                body.MovePosition(gripBlockTargetPosition);
                body.MoveRotation(gripBlockTargetRotation);
                return true;
            }
            finally
            {
                if (controller != null && blockCollider != null) Physics.IgnoreCollision(controller, blockCollider, false);
            }
        }

        bool IsPathBlocked(Vector3 startOffset, Vector3 delta, PlayerController player)
        {
            float distance = delta.magnitude;
            if (distance <= 0f) return false;
            Bounds bounds = blockCollider.bounds;
            Vector3 targetCenter = gripBlockTargetPosition + (bounds.center - body.position);
            foreach (var hit in Physics.BoxCastAll(targetCenter + startOffset, bounds.extents - Vector3.one * 0.01f,
                         delta / distance, transform.rotation, distance + 0.01f, Physics.DefaultRaycastLayers,
                         QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == null || hit.collider == blockCollider || hit.collider.transform.IsChildOf(transform) ||
                    hit.collider.transform == player.transform || hit.collider.transform.IsChildOf(player.transform)) continue;
                if (hit.distance < distance - 0.01f) return true;
            }
            CharacterController capsule = player.GetComponent<CharacterController>();
            Vector3 center = player.transform.TransformPoint(capsule.center) + startOffset + Vector3.up * 0.02f;
            float radius = capsule.radius * Mathf.Max(player.transform.lossyScale.x, player.transform.lossyScale.z);
            float halfSegment = Mathf.Max(0f, capsule.height * player.transform.lossyScale.y * 0.5f - radius);
            Vector3 top = center + Vector3.up * halfSegment;
            Vector3 bottom = center - Vector3.up * halfSegment;
            foreach (var hit in Physics.CapsuleCastAll(top, bottom, radius - 0.01f, delta / distance,
                         distance + 0.01f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == null || hit.collider.transform == player.transform || hit.collider.transform.IsChildOf(player.transform) ||
                    hit.collider == blockCollider || hit.collider.transform.IsChildOf(transform)) continue;
                if (hit.distance < distance - 0.01f) return true;
            }
            return false;
        }

        void ShowHeavyThoughtOnce()
        {
            if (heavyThoughtShown) return;
            heavyThoughtShown = true;
            if (dialogueRunner == null || !dialogueRunner.HasLineListener)
            {
                Debug.LogWarning("PushPullBlock could not show its too-heavy thought because no DialogueRunner listener is assigned.", this);
                return;
            }
            StartCoroutine(PlayHeavyThought());
        }

        IEnumerator PlayHeavyThought()
        {
            var line = ScriptableObject.CreateInstance<DialogueLine>();
            line.id = "block.too_heavy";
            line.text = "This block is too heavy for my current parts.";
            yield return dialogueRunner.Play(new DialogueSequence(new[] { line }));
            Destroy(line);
        }

        public void ReleaseGrip()
        {
            if (!gripping) return;
            if (snapRoutine != null) StopCoroutine(snapRoutine);
            snapRoutine = null;
            if (snapInProgress && grippedPlayer != null) grippedPlayer.RemoveLock(SnapLock);
            snapInProgress = false;
            body.position = gripBlockTargetPosition;
            body.rotation = gripBlockTargetRotation;
            if (config.snapToGrid)
            {
                Vector3 position = gripBlockTargetPosition;
                position.x = Mathf.Round(position.x / config.gridSize) * config.gridSize;
                position.z = Mathf.Round(position.z / config.gridSize) * config.gridSize;
                body.position = position;
                gripBlockTargetPosition = position;
            }
            body.constraints = originalConstraints | RigidbodyConstraints.FreezeRotation;
            body.isKinematic = originalKinematic;
            if (grippedPlayer != null) grippedPlayer.SetGrippedBlock(null);
            grippedPlayer = null;
            gripping = false;
        }

        void OnDisable() => ReleaseGrip();

        void ResetToStart()
        {
            ReleaseGrip();
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = startPosition;
            body.rotation = startRotation;
        }
    }
}
