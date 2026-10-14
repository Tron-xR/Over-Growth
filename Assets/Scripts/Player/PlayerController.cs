using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Parts;
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
        float dashReadyAt;
        bool dashing;

        public PartLoadout Loadout { get; private set; }
        public bool ControlEnabled { get; set; } = true;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            battery = GetComponent<Battery>();
            if (stats == null) { Debug.LogError("PlayerStats is required.", this); enabled = false; return; }
            Loadout = new PartLoadout(stats.totalSlots);
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

        public void RestoreParts(IEnumerable<PartData> parts)
        {
            Loadout.Clear();
            foreach (var part in parts) Loadout.Equip(part);
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
            cc.Move(Physics.gravity * Time.deltaTime);
            battery.Tick(moving, running, Time.deltaTime);
        }

        void OnDash(InputAction.CallbackContext _)
        {
            if (!ControlEnabled || dashing || Time.time < dashReadyAt || battery.Current < stats.dashBatteryCost * Loadout.PowerDrainMultiplier) return;
            StartCoroutine(DashRoutine());
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
        }
    }
}
