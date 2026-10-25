using System;
using UnityEngine;
using UnityEngine.UI;

namespace Door13.Puzzle
{
    /// <summary>调谐面板：把收音机调到目标频率（步进0.1），静态噪音→清晰人声。</summary>
    public class TunerPanel : MonoBehaviour
    {
        public static TunerPanel Instance { get; private set; }

        public Text titleText;
        public Text freqText;
        public Text hintLabel;

        float value = 87.0f;
        float target = 91.3f;
        Action onCorrect;
        bool solved;

        const float Min = 87.0f, Max = 108.0f;

        void Awake()
        {
            Instance = this;
            Bind("Minus1", () => Step(-1f));
            Bind("Minus01", () => Step(-0.1f));
            Bind("Plus01", () => Step(0.1f));
            Bind("Plus1", () => Step(1f));
            Bind("CloseButton", Close);
            gameObject.SetActive(false);
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Open(string title, float targetFreq, string hint, Action onCorrect)
        {
            titleText.text = title;
            target = targetFreq;
            this.onCorrect = onCorrect;
            hintLabel.text = hint;
            value = Min;
            solved = false;
            Refresh();
            gameObject.SetActive(true);
        }

        public void Close() => gameObject.SetActive(false);

        public void Step(float delta)
        {
            if (solved) return;
            value = Mathf.Clamp(Mathf.Round((value + delta) * 10f) / 10f, Min, Max);
            Refresh();
            Audio.AudioDirector.Instance?.Click();
            if (Mathf.Abs(value - target) < 0.05f)
            {
                solved = true;
                var cb = onCorrect;
                Close();
                cb?.Invoke();
            }
        }

        void Refresh()
        {
            if (freqText != null)
            {
                freqText.text = value.ToString("F1") + " MHz";
                bool near = Mathf.Abs(value - target) < 0.3f;
                freqText.color = near ? new Color(0.5f, 1f, 0.6f) : new Color(1f, 1f, 1f, 0.8f);
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
