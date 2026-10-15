using Game.Parts;
using Game.Player;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class InventoryUI : MonoBehaviour
    {
        const string ControlLock = "ui.inventory";
        [SerializeField] PlayerController player;
        [SerializeField] InputActionAsset controls;

        InputAction inventoryAction;
        GameObject panel;
        RectTransform partRows;
        Text slotsLabel;
        Image slotsFill;
        bool isOpen;

        void Awake()
        {
            if (player == null || controls == null)
            {
                Debug.LogError("InventoryUI requires a PlayerController and Controls input asset.", this);
                enabled = false;
                return;
            }

            inventoryAction = controls.FindAction("Player/Inventory", true);
            EnsureEventSystem();
            BuildPanel();
        }

        void OnEnable()
        {
            if (inventoryAction == null) return;
            inventoryAction.performed += Toggle;
            inventoryAction.Enable();
            player.Inventory.OnChanged += Refresh;
        }

        void OnDisable()
        {
            if (inventoryAction != null)
            {
                inventoryAction.performed -= Toggle;
                inventoryAction.Disable();
            }
            if (player != null && player.Inventory != null) player.Inventory.OnChanged -= Refresh;
            if (isOpen && player != null) player.RemoveLock(ControlLock);
            isOpen = false;
        }

        void Toggle(InputAction.CallbackContext _) => SetOpen(!isOpen);

        void SetOpen(bool open)
        {
            isOpen = open;
            panel.SetActive(open);
            if (open)
            {
                player.AddLock(ControlLock);
                Refresh();
            }
            else player.RemoveLock(ControlLock);
        }

        void BuildPanel()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("InventoryUI must be placed on or under a Canvas.", this);
                enabled = false;
                return;
            }

            panel = CreateElement("Parts Inventory", canvas.transform, new Vector2(560f, 420f), new Vector2(0.5f, 0.5f));
            panel.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.11f, 0.96f);
            AddText(panel.transform, "Parts", new Vector2(0f, 165f), new Vector2(380f, 46f), 30, TextAnchor.MiddleLeft);
            var close = CreateButton(panel.transform, "Close", new Vector2(210f, 165f), new Vector2(90f, 38f));
            close.onClick.AddListener(() => SetOpen(false));

            slotsLabel = AddText(panel.transform, "Slots", new Vector2(-5f, 112f), new Vector2(500f, 26f), 18, TextAnchor.MiddleLeft);
            var slotBar = CreateElement("Slot Bar", panel.transform, new Vector2(490f, 18f), new Vector2(0f, 82f));
            slotBar.GetComponent<Image>().color = new Color(0.2f, 0.22f, 0.23f, 1f);
            var fill = CreateElement("Fill", slotBar.transform, Vector2.zero, Vector2.zero);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            slotsFill = fill.GetComponent<Image>();
            slotsFill.type = Image.Type.Filled;
            slotsFill.fillMethod = Image.FillMethod.Horizontal;
            slotsFill.color = new Color(0.35f, 0.72f, 0.68f, 1f);

            var rows = CreateElement("Owned Parts", panel.transform, new Vector2(500f, 250f), new Vector2(0f, -55f));
            partRows = rows.GetComponent<RectTransform>();
            panel.SetActive(false);
        }

        void Refresh()
        {
            if (partRows == null) return;
            for (int i = partRows.childCount - 1; i >= 0; i--) Destroy(partRows.GetChild(i).gameObject);

            var loadout = player.Loadout;
            slotsLabel.text = $"Slots {loadout.UsedSlots} / {loadout.TotalSlots}";
            slotsFill.fillAmount = loadout.TotalSlots > 0 ? (float)loadout.UsedSlots / loadout.TotalSlots : 0f;
            for (int i = 0; i < player.Inventory.Owned.Count; i++)
            {
                var part = player.Inventory.Owned[i];
                var button = CreateButton(partRows, PartLabel(part), new Vector2(0f, -i * 58f), new Vector2(490f, 50f));
                button.onClick.AddListener(() =>
                {
                    if (loadout.Equipped.Contains(part)) player.Inventory.Unequip(part);
                    else player.Inventory.Equip(part);
                    Refresh();
                });
                button.interactable = loadout.Equipped.Contains(part) || loadout.CanEquip(part);
            }
        }

        string PartLabel(PartData part)
        {
            bool equipped = player.Loadout.Equipped.Contains(part);
            return $"{part.displayName}  |  {part.slotCost} slots  |  Speed x{part.speedMultiplier:0.0}  |  Drain x{part.powerDrainMultiplier:0.0}  |  {(equipped ? "Unequip" : "Equip")}";
        }

        static GameObject CreateElement(string name, Transform parent, Vector2 size, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return go;
        }

        static Text AddText(Transform parent, string value, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(value, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }

        static Button CreateButton(Transform parent, string label, Vector2 position, Vector2 size)
        {
            var go = CreateElement(label, parent, size, position);
            var image = go.GetComponent<Image>();
            image.color = new Color(0.2f, 0.24f, 0.25f, 1f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            AddText(go.transform, label, Vector2.zero, size - new Vector2(16f, 8f), 16, TextAnchor.MiddleCenter);
            return button;
        }

        static void EnsureEventSystem()
        {
            var eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem == null) eventSystem = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
            var module = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (module == null) module = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            if (module.actionsAsset == null) module.AssignDefaultActions();
        }
    }
}
