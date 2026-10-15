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

        public PartInventory Inventory { get; private set; }
        public PartLoadout Loadout => Inventory?.Loadout;
        public bool ControlEnabled => !controlLocks.IsLocked;
        public bool IsGrounded => cc != null && cc.isGrounded;

        public void AddLock(string reason) => controlLocks.Add(reason);
        public void RemoveLock(string reason) => controlLocks.Remove(reason);

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

            Vector2 in2 = move.action.ReadValue<Vector2>();
            Vector3 dir = new Vector3(in2.x, 0f, in2.y);
            bool moving = dir.sqrMagnitude > 0.01f;
            bool running = run.action.IsPressed();

            float speed = (running ? stats.runSpeed : stats.walkSpeed) * Loadout.SpeedMultiplier;
            if (moving)
            {
                dir.Normalize();
                cc.Move(dir * speed * Time.deltaTime);
                Quaternion target = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, stats.turnSpeed * Time.deltaTime);
            }
            if (cc.isGrounded && verticalVelocity < 0f) verticalVelocity = -1f;
            else verticalVelocity += Physics.gravity.y * Time.deltaTime;
            cc.Move(Vector3.up * verticalVelocity * Time.deltaTime);
            battery.Tick(moving, running, Time.deltaTime);
        }

        void OnDash(InputAction.CallbackContext _)
        {
            if (!ControlEnabled || dashing || Time.time < dashReadyAt || battery.Current < stats.dashBatteryCost * Loadout.PowerDrainMultiplier) return;
            dashRoutine = StartCoroutine(DashRoutine());
        }

        IEnumerator DashRoutine()
        {
            dashing = true;
            dashReadyAt = Time.time + stats.dashCooldown;
            battery.Spend(stats.dashBatteryCost * Loadout.PowerDrainMultiplier);
            Vector3 dir = transform.forward;
            for (float t = 0; t < stats.dashTime; t += Time.deltaTime)
            {
                cc.Move(dir * stats.dashSpeed * Loadout.SpeedMultiplier * Time.deltaTime);
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
