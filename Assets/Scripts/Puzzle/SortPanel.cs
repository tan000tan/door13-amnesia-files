using System;
using UnityEngine;
using UnityEngine.UI;

namespace Door13.Puzzle
{
    /// <summary>
    /// 排序面板：点击两张照片交换位置，确认校验顺序。
    /// items[i] 的正确排列由 correctOrder 给出（显示顺序→物品索引）。
    /// </summary>
    public class SortPanel : MonoBehaviour
    {
        public static SortPanel Instance { get; private set; }

        public Text titleText;
        public Text hintLabel;
        public RectTransform slotRoot;

        string[] labels = new string[0];
        int[] slots = new int[0];       // 槽位→物品索引
        int[] correct = new int[0];     // 正确排列
        int selected = -1;
        Action onCorrect;

        void Awake()
        {
            Instance = this;
            Bind("ConfirmButton", Confirm);
            Bind("CloseButton", Close);
            gameObject.SetActive(false);
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Open(string title, string hint, string[] itemLabels, int[] correctOrder, Action onCorrect)
        {
            titleText.text = title;
            hintLabel.text = hint;
            labels = itemLabels;
            correct = correctOrder;
            this.onCorrect = onCorrect;
            slots = new int[labels.Length];
            // 打乱初始顺序（Fisher-Yates），确保玩家必须实际排序
            for (int i = 0; i < slots.Length; i++) slots[i] = i;
            var rng = new System.Random(42); // 固定种子，确保可复现
            for (int i = slots.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                int tmp = slots[i]; slots[i] = slots[j]; slots[j] = tmp;
            }
            // 确保不是正确顺序（极小概率事件）
            bool allCorrect = true;
            for (int i = 0; i < slots.Length; i++) if (slots[i] != i) { allCorrect = false; break; }
            if (allCorrect && slots.Length > 1) { int tmp2 = slots[0]; slots[0] = slots[1]; slots[1] = tmp2; }
            selected = -1;
            Rebuild();
            gameObject.SetActive(true);
        }

        public void Close() => gameObject.SetActive(false);

        public void Tap(int slot)
        {
            if (selected < 0) { selected = slot; Highlight(); return; }
            if (selected == slot) { selected = -1; Highlight(); return; }
            int tmp = slots[selected]; slots[selected] = slots[slot]; slots[slot] = tmp;
            selected = -1;
            Audio.AudioDirector.Instance?.Click();
            Rebuild();
        }

        public void Confirm()
        {
            for (int i = 0; i < correct.Length; i++)
                if (slots[i] != correct[i])
                {
                    Audio.AudioDirector.Instance?.Buzz();
                    if (UI.Hud.Instance != null) UI.Hud.Instance.Toast("顺序不对。再想想时间线。");
                    return;
                }
            var cb = onCorrect;
            Close();
            cb?.Invoke();
        }

        void Highlight() => Rebuild();

        void Rebuild()
        {
            foreach (Transform c in slotRoot) Destroy(c.gameObject);
            for (int i = 0; i < slots.Length; i++)
            {
                var go = new GameObject("Slot_" + i, typeof(Image), typeof(Button), typeof(LayoutElement));
                go.transform.SetParent(slotRoot, false);
                var le = go.GetComponent<LayoutElement>();
                le.minWidth = 150; le.minHeight = 170;
                var img = go.GetComponent<Image>();
                bool sel = i == selected;
                img.color = sel ? new Color(1f, 0.9f, 0.4f, 0.9f) : new Color(1f, 1f, 1f, 0.25f);

                var lg = new GameObject("L", typeof(Text));
                lg.transform.SetParent(go.transform, false);
                var t = lg.GetComponent<Text>();
                t.text = labels[slots[i]];
                t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                t.fontSize = 24; t.alignment = TextAnchor.MiddleCenter; t.color = Color.white; t.raycastTarget = false;
                var rt = (RectTransform)lg.transform;
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;

                int captured = i;
                go.GetComponent<Button>().onClick.AddListener(() => Tap(captured));
            }
        }

        void Bind(string path, Action action)
        {
            var t = transform.Find(path);
            var btn = t != null ? t.GetComponent<Button>() : null;
            if (btn != null) btn.onClick.AddListener(() => action());
        }
    }
}
