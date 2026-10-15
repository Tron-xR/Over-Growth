using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Parts;
using Game.Player.Logic;
using System.Collections.Generic;

namespace Game.Player
{
    [RequireComponent(typeof(CharacterController), typeof(Battery))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] PlayerStats stats;
        [SerializeField] InputActionReference move, run, dash;

        CharacterController cc;
        Battery battery;
        readonly ControlLockSet controlLocks = new ControlLockSet();
        float dashReadyAt;
        bool dashing;
        float verticalVelocity;
        Coroutine dashRoutine;
        float carrySpeedMultiplier = 1f;
        Game.Puzzle.PushPullBlock grippedBlock;

        public PartInventory Inventory { get; private set; }
        public PartLoadout Loadout => Inventory?.Loadout;
        public Vector3 CarryHoldLocalPosition => stats.carryHoldLocalPosition;
        public float ThrowImpulse => stats.throwImpulse;
        public float ThrowLift => stats.throwLift;
        public float CharacterRadius => cc != null ? cc.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z) : 0.35f;
        public Vector2 MoveInput => move != null && move.action != null ? move.action.ReadValue<Vector2>() : Vector2.zero;
        public bool ControlEnabled => !controlLocks.IsLocked;
        public bool IsGrounded => cc != null && cc.isGrounded;

        public void AddLock(string reason) => controlLocks.Add(reason);
        public void RemoveLock(string reason) => controlLocks.Remove(reason);
        public void SetCarrySpeedMultiplier(float multiplier) => carrySpeedMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f);

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            battery = GetComponent<Battery>();
            if (stats == null) { Debug.LogError("PlayerStats is required.", this); enabled = false; return; }
            Inventory = new PartInventory(stats.totalSlots);
            battery.Loadout = Loadout;
        }

        void OnEnable()
        {
            if (move == null || run == null || dash == null || move.action == null || run.action == null || dash.action == null) { Debug.LogError("Move, Run, and Dash actions are required.", this); enabled = false; return; }
            move.action.Enable(); run.action.Enable(); dash.action.Enable();
            dash.action.performed += OnDash;
        }

        void OnDisable()
        {
            if (dash != null) dash.action.performed -= OnDash;
            if (move != null && move.action != null) move.action.Disable();
            if (run != null && run.action != null) run.action.Disable();
            if (dash != null && dash.action != null) dash.action.Disable();
        }

        public void RestoreParts(IEnumerable<PartData> ownedParts, IEnumerable<PartData> equippedParts)
        {
            Inventory.Restore(ownedParts, equippedParts);
        }

        void Update()
        {
            if (!ControlEnabled || dashing) return;

            Vector2 in2 = MoveInput;
            if (grippedBlock != null)
            {
                bool gripMoving = grippedBlock.MoveGrip(this, in2, Time.deltaTime);
                battery.Tick(gripMoving, false, Time.deltaTime);
                return;
            }

            Vector3 dir = new Vector3(in2.x, 0f, in2.y);
            bool moving = dir.sqrMagnitude > 0.01f;
            bool running = run.action.IsPressed();

            float speed = (running ? stats.runSpeed : stats.walkSpeed) * Loadout.SpeedMultiplier * carrySpeedMultiplier;
            if (moving)
            {
                dir.Normalize();
                cc.Move(dir * speed * Time.deltaTime);
                Quaternion target = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, stats.turnSpeed * Time.deltaTime);
            }
            ApplyGravity();
            battery.Tick(moving, running, Time.deltaTime);
        }

        void ApplyGravity()
        {
            if (cc.isGrounded && verticalVelocity < 0f) verticalVelocity = -1f;
            else verticalVelocity += Physics.gravity.y * Time.deltaTime;
            cc.Move(Vector3.up * verticalVelocity * Time.deltaTime);
        }

        void OnDash(InputAction.CallbackContext _)
        {
            if (!ControlEnabled || dashing || grippedBlock != null || Time.time < dashReadyAt || battery.Current < stats.dashBatteryCost * Loadout.PowerDrainMultiplier) return;
            dashRoutine = StartCoroutine(DashRoutine());
        }

        public void SetGrippedBlock(Game.Puzzle.PushPullBlock block)
        {
            grippedBlock = block;
        }

        public void MoveWhileLocked(Vector3 position, Quaternion rotation)
        {
            if (cc == null || !cc.enabled) return;
            cc.Move(position - transform.position);
            transform.rotation = rotation;
        }

        public Vector3 MoveForGrip(Vector3 delta)
        {
            if (cc == null || !cc.enabled) return Vector3.zero;
            Vector3 start = transform.position;
            cc.Move(delta);
            return transform.position - start;
        }

        IEnumerator DashRoutine()
        {
            dashing = true;
            dashReadyAt = Time.time + stats.dashCooldown;
            battery.Spend(stats.dashBatteryCost * Loadout.PowerDrainMultiplier);
            Vector3 dir = transform.forward;
            for (float t = 0; t < stats.dashTime; t += Time.deltaTime)
            {
                cc.Move(dir * stats.dashSpeed * Loadout.SpeedMultiplier * carrySpeedMultiplier * Time.deltaTime);
                yield return null;
            }
            dashing = false;
            dashRoutine = null;
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (dashRoutine != null) StopCoroutine(dashRoutine);
            dashRoutine = null;
            dashing = false;
            verticalVelocity = 0f;
            cc.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            cc.enabled = true;
            cc.Move(Vector3.zero);
        }
    }
}
