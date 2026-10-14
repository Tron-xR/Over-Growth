using Game.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class BatteryHud : MonoBehaviour
    {
        [SerializeField] Battery battery;
        [SerializeField] Image fill;

        void OnEnable()
        {
            if (battery == null || fill == null) { Debug.LogError("BatteryHud requires a Battery and fill Image.", this); enabled = false; return; }
            battery.OnChanged += UpdateBar;
            UpdateBar(battery.Current, battery.Max);
        }

        void OnDisable()
        {
            if (battery != null) battery.OnChanged -= UpdateBar;
        }

        void UpdateBar(float current, float max) => fill.fillAmount = max > 0f ? current / max : 0f;
    }
}
