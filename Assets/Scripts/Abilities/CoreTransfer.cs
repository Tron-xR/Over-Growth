using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Player;

namespace Game.Abilities
{
    /// Ability 1: move the core into a nearby robot for a short time.
    public class CoreTransfer : Ability
    {
        [SerializeField] float range = 6f;
        [SerializeField] float duration = 15f;
        [SerializeField] InputActionReference move;
        [SerializeField] PlayerController player;
        PossessableRobot target;
        float endAt;

        void Reset() { id = "core_transfer"; }

        public override void Activate()
        {
            target = FindObjectsOfType<PossessableRobot>()
                .Where(r => Vector3.Distance(r.transform.position, transform.position) <= range)
                .OrderBy(r => Vector3.Distance(r.transform.position, transform.position))
                .FirstOrDefault();
            if (target == null) return;
            IsActive = true;
            player.ControlEnabled = false;
            endAt = Time.time + duration;
            // TODO: camera follow target, VFX
        }

        public override void Deactivate()
        {
            IsActive = false;
            target = null;
            player.ControlEnabled = true;
        }

        void Update()
        {
            if (!IsActive) return;
            if (Time.time >= endAt || target == null) { Deactivate(); return; }
            // PlayerController owns the enabled lifetime of this shared Move action.
            var v = move.action.ReadValue<Vector2>();
            target.Move(new Vector3(v.x, 0, v.y), Time.deltaTime);
        }
    }
}
