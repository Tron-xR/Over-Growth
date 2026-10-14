using UnityEngine;

namespace Game.CameraRig
{
    /// Fixed-angle top-down follow. No player rotation of camera.
    public class FixedFollowCamera : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(0f, 10f, -7f);
        [SerializeField] float smoothTime = 0.15f;
        Vector3 vel;

        public void SetTarget(Transform t) => target = t;

        void Start() { if (target) transform.rotation = Quaternion.LookRotation(-offset.normalized); }

        void LateUpdate()
        {
            if (!target) return;
            transform.position = Vector3.SmoothDamp(transform.position, target.position + offset, ref vel, smoothTime);
        }
    }
}
