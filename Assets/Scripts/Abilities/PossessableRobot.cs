using UnityEngine;

namespace Game.Abilities
{
    public class PossessableRobot : MonoBehaviour
    {
        public float moveSpeed = 3f;
        public void Move(Vector3 dir, float dt) => transform.position += dir * moveSpeed * dt;
    }
}
