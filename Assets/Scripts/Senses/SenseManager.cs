using System.Collections;
using Game.Core;
using Game.Player;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Game.Senses
{
    public class SenseManager : MonoBehaviour
    {
        const string IntroDoneFlag = "intro.done";
        const string SightFlag = "sense.sight";
        const string ControlLock = "sense.sight";

        [SerializeField] PlayerController player;
        [SerializeField] Volume volume;
        [SerializeField, Min(0f)] float fadeSeconds = 3f;

        DepthOfField depthOfField;
        Vignette vignette;
        ColorAdjustments colorAdjustments;
        float clearAperture, clearVignette, clearExposure;
        Coroutine fade;
        bool initialized;

        void Awake()
        {
            if (player != null) player.AddLock(ControlLock);
        }

        void OnEnable() => GameState.OnFlagSet += OnFlagSet;

        void OnDisable()
        {
            GameState.OnFlagSet -= OnFlagSet;
            if (fade != null) StopCoroutine(fade);
        }

        IEnumerator Start()
        {
            if (player == null || volume == null || volume.profile == null)
            {
                Debug.LogError("SenseManager requires PlayerController and a Global Volume with a profile.", this);
                enabled = false;
                yield break;
            }

            yield return null; // SaveCoordinator restores flags in Start.
            initialized = true;
            player.AddLock(ControlLock);
            if (!CacheProfile()) yield break;

            if (GameState.Has(SightFlag))
            {
                SetSight(1f);
                player.RemoveLock(ControlLock);
            }
            else if (SightFadeState.CanBegin(GameState.Has(IntroDoneFlag), GameState.Has(SightFlag))) BeginFade();
        }

        void OnFlagSet(string flag)
        {
            if (!initialized) return;
            if (flag == IntroDoneFlag && SightFadeState.CanBegin(true, GameState.Has(SightFlag))) BeginFade();
            else if (flag == SightFlag)
            {
                if (fade != null) StopCoroutine(fade);
                fade = null;
                SetSight(1f);
                player.RemoveLock(ControlLock);
            }
        }

        void BeginFade()
        {
            if (fade != null) return;
            if (!CacheProfile())
            {
                return;
            }

            depthOfField.aperture.Override(1.4f);
            vignette.intensity.Override(0.85f);
            colorAdjustments.postExposure.Override(-3f);
            fade = StartCoroutine(FadeIn());
        }

        bool CacheProfile()
        {
            if (depthOfField != null) return true;
            if (!volume.profile.TryGet(out depthOfField) || !volume.profile.TryGet(out vignette) || !volume.profile.TryGet(out colorAdjustments))
            {
                Debug.LogError("Sight fade requires Depth of Field, Vignette, and Color Adjustments on the Volume.", this);
                return false;
            }

            clearAperture = depthOfField.aperture.value;
            clearVignette = vignette.intensity.value;
            clearExposure = colorAdjustments.postExposure.value;
            return true;
        }

        IEnumerator FadeIn()
        {
            float elapsed = 0f;
            while (elapsed < fadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                SetSight(SightFadeState.Progress(elapsed, fadeSeconds));
                yield return null;
            }

            GameState.Set(SightFlag);
            player.RemoveLock(ControlLock);
            fade = null;
        }

        void SetSight(float progress)
        {
            if (depthOfField == null && !CacheProfile()) return;

            depthOfField.aperture.Override(Mathf.Lerp(1.4f, clearAperture, progress));
            vignette.intensity.Override(Mathf.Lerp(0.85f, clearVignette, progress));
            colorAdjustments.postExposure.Override(Mathf.Lerp(-3f, clearExposure, progress));
        }
    }
}
