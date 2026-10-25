using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Door13.Puzzle
{
    /// <summary>
    /// 顺序开关面板：按正确顺序按下编号开关（如断路器合闸 3-1-4-2-5）。
    /// 按错立即复位。solution 形如 "3,1,4,2,5"。
    /// </summary>
    public class SwitchPanel : MonoBehaviour
    {
        public static SwitchPanel Instance { get; private set; }

        public Text titleText;
        public Text progressText;
        public RectTransform slotRoot;

        string[] solution = new string[0];
        int progress;
        Action onCorrect;
        readonly List<Image> visuals = new List<Image>();

        void Awake()
        {
            Instance = this;
            Bind("CloseButton", Close);
            Bind("ResetButton", ResetProgress);
            gameObject.SetActive(false);
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Open(string title, string solutionCsv, string[] switchLabels, Action onCorrect)
        {
            titleText.text = title;
            solution = solutionCsv.Split(',');
            this.onCorrect = onCorrect;
            progress = 0;
            Rebuild(switchLabels);
            Refresh();
            gameObject.SetActive(true);
        }

        public void Close() => gameObject.SetActive(false);

        public void Press(int index)
        {
            if (progress >= solution.Length) return;
            if (solution[progress].Trim() == (index + 1).ToString())
            {
                progress++;
                Audio.AudioDirector.Instance?.Click();
                if (progress >= solution.Length)
                {
                    var cb = onCorrect;
                    Close();
                    cb?.Invoke();
                    return;
                }
            }
            else
            {
                progress = 0;
                Audio.AudioDirector.Instance?.Buzz();
                if (UI.Hud.Instance != null) UI.Hud.Instance.Toast("顺序错了。全部复位。");
            }
            Refresh();
        }

        public void ResetProgress() { progress = 0; Refresh(); }

        void Refresh()
        {
            if (progressText != null)
                progressText.text = progress + " / " + solution.Length;
            for (int i = 0; i < visuals.Count; i++)
                if (visuals[i] != null)
                    visuals[i].color = i < progress
                        ? new Color(0.4f, 0.85f, 0.5f, 0.9f)
                        : new Color(1f, 1f, 1f, 0.25f);
        }

        void Rebuild(string[] labels)
        {
            foreach (Transform c in slotRoot) Destroy(c.gameObject);
            visuals.Clear();
            int count = solution.Length;
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Sw_" + i, typeof(Image), typeof(Button), typeof(LayoutElement));
                go.transform.SetParent(slotRoot, false);
                var le = go.GetComponent<LayoutElement>();
                le.minWidth = 90; le.minHeight = 90;
                var img = go.GetComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0.25f);
                visuals.Add(img);

                string label = labels != null && i < labels.Length ? labels[i] : (i + 1).ToString();
                var lg = new GameObject("L", typeof(Text));
                lg.transform.SetParent(go.transform, false);
                var t = lg.GetComponent<Text>();
                t.text = label;
                t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                t.fontSize = 30; t.alignment = TextAnchor.MiddleCenter; t.color = Color.white; t.raycastTarget = false;
                var rt = (RectTransform)lg.transform;
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;

                int captured = i;
                go.GetComponent<Button>().onClick.AddListener(() => Press(captured));
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
