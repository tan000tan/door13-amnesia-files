using UnityEngine;
using UnityEngine.UI;

namespace Door13.Inventory
{
    /// <summary>
    /// 背包面板：网格槽位 + 详情 + 两步组合（先点A，再点B）。
    /// 组合表见 ItemCatalog.Combos。
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        public static InventoryUI Instance { get; private set; }

        [Tooltip("槽位容器（带 GridLayoutGroup）")]
        public RectTransform slotRoot;

        public Text detailText;
        public Text modeText;

        GameObject slotPrefab;
        string selectedId = "";

        bool subscribed;

        void Awake()
        {
            Instance = this;
            slotPrefab = BuildSlotPrefab();
            var close = transform.Find("CloseButton");
            if (close != null)
            {
                var btn = close.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(Close);
            }
            // 运行时 Awake 顺序不保证：Inventory.Instance 可能为 null
            // （NRE 曾中断 Awake → SetActive(false) 未执行 → 面板常驻遮住全部HUD按钮）
            if (Inventory.Instance != null) Subscribe();
            gameObject.SetActive(false);
            Refresh();
        }

        void Subscribe()
        {
            if (subscribed || Inventory.Instance == null) return;
            Inventory.Instance.Changed += Refresh;
            subscribed = true;
        }

        void OnDestroy()
        {
            if (subscribed && Inventory.Instance != null) Inventory.Instance.Changed -= Refresh;
            if (Instance == this) Instance = null;
        }

        public void Toggle()
        {
            Subscribe();   // Awake 期未订上的补订
            bool show = !gameObject.activeSelf;
            Debug.Log("[UI] InventoryToggle -> " + show);
            gameObject.SetActive(show);
            if (show) { selectedId = ""; Refresh(); }
        }

        public void Close() => gameObject.SetActive(false);

        public void Refresh()
        {
            if (slotRoot == null || Inventory.Instance == null) return;

            for (int i = slotRoot.childCount - 1; i >= 0; i--)
                Destroy(slotRoot.GetChild(i).gameObject);

            var items = Inventory.Instance.Items;
            for (int idx = 0; idx < items.Count; idx++)
            {
                string id = items[idx];
                var def = ItemCatalog.Get(id);
                var slot = Instantiate(slotPrefab, slotRoot);
                var img = slot.GetComponent<Image>();
                if (img != null)
                {
                    Color c = def.Color;
                    c.a = id == selectedId ? 1f : 0.85f;
                    img.color = id == selectedId ? Color.white : c;
                }
                var label = slot.GetComponentInChildren<Text>();
                if (label != null) label.text = def.Name;
                if (img != null && id == selectedId) img.color = new Color(1f, 0.85f, 0.4f, 1f);

                var btn = slot.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(() => TapItem(id));
            }

            if (modeText != null)
                modeText.text = selectedId == "" ? "点物品查看 / 点两件尝试组合" : "已选中：" + ItemCatalog.Get(selectedId).Name + "（再点一件组合，点自己取消）";
        }

        public void TapItem(string id)
        {
            if (selectedId == "")
            {
                selectedId = id;
                var d = ItemCatalog.Get(id);
                // 显示组合提示：这件物品可以和什么组合
                string comboHint = GetComboHint(id);
                if (detailText != null) detailText.text = "【" + d.Name + "】\n" + d.Desc + comboHint;
                Refresh();
                return;
            }
            if (selectedId == id)
            {
                selectedId = "";
                if (detailText != null) detailText.text = "";
                Refresh();
                return;
            }

            // 尝试组合
            string a = selectedId, b = id;
            var inv = Inventory.Instance;
            string msg;
            string result = ItemCatalog.TryCombo(a, b, out msg);
            if (result != null)
            {
                inv.Remove(a);
                inv.Remove(b);
                inv.Add(result);
                if (detailText != null) detailText.text = "【组合成功】\n" + msg;
                Audio.AudioDirector.Instance?.Click();
            }
            else
            {
                if (detailText != null) detailText.text = msg;
            }
            selectedId = "";
            Refresh();
        }

        /// <summary>获取物品的组合提示</summary>
        static string GetComboHint(string id)
        {
            foreach (var c in ItemCatalog.Combos)
            {
                if (c.A == id || c.B == id)
                {
                    string other = c.A == id ? c.B : c.A;
                    var otherData = ItemCatalog.Get(other);
                    return "\n\n💡 可与「" + otherData.Name + "」组合";
                }
            }
            return "";
        }

        GameObject BuildSlotPrefab()
        {
            var go = new GameObject("Slot", typeof(Image), typeof(Button), typeof(LayoutElement));
            var le = go.GetComponent<LayoutElement>();
            le.minWidth = 150f; le.minHeight = 150f;
            var img = go.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.9f);

            var labelGo = new GameObject("Label", typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            var label = labelGo.GetComponent<Text>();
            label.font = EditorFont != null ? EditorFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = 24;
            label.color = Color.black;
            var rt = (RectTransform)labelGo.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            return go;
        }

        /// <summary>编辑器生成的内置字体由场景构建时注入；运行时兜底用 LegacyRuntime。</summary>
        public static Font EditorFont { get; set; }
    }
}
