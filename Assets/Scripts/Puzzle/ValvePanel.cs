using System;
using UnityEngine;
using UnityEngine.UI;

namespace Door13.Puzzle
{
    /// <summary>
    /// 阀门面板：三个阀门各循环 0~3 档（压力），确认校验目标读数。
    /// targets 形如 "2,1,3"。开关标签为阀门名（A/B/C）。
    /// </summary>
    public class ValvePanel : MonoBehaviour
    {
        public static ValvePanel Instance { get; private set; }

        public Text titleText;
        public Text gaugeText;
        public RectTransform slotRoot;

        int[] values = new int[3];
        int[] targets = new int[3];
        string[] names = { "A", "B", "C" };
        Action onCorrect;

        void Awake()
        {
            Instance = this;
            Bind("ConfirmButton", Confirm);
            Bind("CloseButton", Close);
            gameObject.SetActive(false);
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Open(string title, string valveNames, string targetCsv, Action onCorrect)
        {
            titleText.text = title;
            var n = valveNames.Split(',');
            if (n.Length == 3) names = n;
            var ts = targetCsv.Split(',');
            targets = new int[ts.Length];
            for (int i = 0; i < ts.Length; i++) targets[i] = int.Parse(ts[i].Trim());
            values = new int[targets.Length];
            this.onCorrect = onCorrect;
            Rebuild();
            Refresh();
            gameObject.SetActive(true);
        }

        public void Close() => gameObject.SetActive(false);

        public void Cycle(int index)
        {
            values[index] = (values[index] + 1) % 4;
            Audio.AudioDirector.Instance?.Click();
            Rebuild();
            Refresh();
        }

        public void Confirm()
        {
            for (int i = 0; i < targets.Length; i++)
                if (values[i] != targets[i])
                {
                    Audio.AudioDirector.Instance?.Buzz();
                    if (UI.Hud.Instance != null) UI.Hud.Instance.Toast("压力表在抖。读数不对。");
                    return;
                }
            var cb = onCorrect;
            Close();
            cb?.Invoke();
        }

        void Refresh()
        {
            if (gaugeText == null) return;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < values.Length; i++)
                sb.Append(names[i]).Append(": ").Append(values[i]).Append(" bar   ");
            gaugeText.text = sb.ToString();
        }

        void Rebuild()
        {
            foreach (Transform c in slotRoot) Destroy(c.gameObject);
            for (int i = 0; i < values.Length; i++)
            {
                var go = new GameObject("Valve_" + i, typeof(Image), typeof(Button), typeof(LayoutElement));
                go.transform.SetParent(slotRoot, false);
                var le = go.GetComponent<LayoutElement>();
                le.minWidth = 140; le.minHeight = 120;
                go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);

                var lg = new GameObject("L", typeof(Text));
                lg.transform.SetParent(go.transform, false);
                var t = lg.GetComponent<Text>();
                t.text = names[i] + " 阀门\n档位 " + values[i];
                t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                t.fontSize = 26; t.alignment = TextAnchor.MiddleCenter; t.color = Color.white; t.raycastTarget = false;
                var rt = (RectTransform)lg.transform;
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;

                int captured = i;
                go.GetComponent<Button>().onClick.AddListener(() => Cycle(captured));
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
