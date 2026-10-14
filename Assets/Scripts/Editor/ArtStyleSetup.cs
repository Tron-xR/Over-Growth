#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Game.EditorTools
{
    /// Menu: Game > Setup > Create Art Style Volume
    /// Creates the post-processing profile + Global Volume, warm sun, cool ambient, and camera angle.
    public static class ArtStyleSetup
    {
        const string ProfilePath = "Assets/Data/ArtStyleProfile.asset";

        [MenuItem("Game/Setup/Create Art Style Volume")]
        public static void Create()
        {
            Directory.CreateDirectory("Assets/Data");

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);

            var dof = profile.Add<DepthOfField>(true);
            dof.mode.Override(DepthOfFieldMode.Bokeh);
            dof.focusDistance.Override(10f);
            dof.aperture.Override(5.6f);
            dof.focalLength.Override(70f);

            var tone = profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.Neutral);

            var color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(0.2f);
            color.saturation.Override(-15f);
            color.colorFilter.Override(new Color(1f, 0.95f, 0.88f));

            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(0.3f);
            bloom.threshold.Override(1f);

            var vig = profile.Add<Vignette>(true);
            vig.intensity.Override(0.25f);

            // Components must be saved as sub-assets or they are lost on reload.
            foreach (var c in profile.components) AssetDatabase.AddObjectToAsset(c, profile);
            EditorUtility.SetDirty(profile);

            var go = new GameObject("Global Volume");
            var vol = go.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.sharedProfile = profile;

            SetupLight();
            SetupAmbient();
            SetupCamera();

            AssetDatabase.SaveAssets();
            Debug.Log("Art style set up. Add SSAO manually: URP Renderer asset > Add Renderer Feature > Screen Space Ambient Occlusion.");
        }

        static void SetupLight()
        {
            Light sun = Object.FindObjectOfType<Light>();
            if (sun == null || sun.type != LightType.Directional)
            {
                var g = new GameObject("Sun");
                sun = g.AddComponent<Light>();
                sun.type = LightType.Directional;
            }
            sun.color = new Color(1f, 0.88f, 0.70f);
            sun.intensity = 1.4f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(35f, -30f, 0f);
            RenderSettings.sun = sun;
        }

        static void SetupAmbient()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.62f, 0.75f);
            RenderSettings.ambientEquatorColor = new Color(0.60f, 0.58f, 0.55f);
            RenderSettings.ambientGroundColor = new Color(0.55f, 0.45f, 0.35f);
        }

        static void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;
            cam.fieldOfView = 38f;
            cam.transform.SetPositionAndRotation(new Vector3(0f, 10f, -7f), Quaternion.Euler(55f, 0f, 0f));
            var data = cam.GetComponent<UniversalAdditionalCameraData>();
            if (data != null) data.renderPostProcessing = true;
        }
    }
}
#endif
