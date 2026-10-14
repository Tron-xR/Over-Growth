using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Game.CameraRig
{
    /// Keeps depth-of-field focused on the player (tilt-shift look).
    public class DofFocus : MonoBehaviour
    {
        [SerializeField] Volume volume;
        [SerializeField] Transform player;
        DepthOfField dof;

        void Start() { if (volume != null) volume.profile.TryGet(out dof); }

        void LateUpdate()
        {
            if (dof == null || player == null) return;
            dof.focusDistance.value = Vector3.Distance(transform.position, player.position);
        }
    }
}
