using System;
using UnityEngine;
using UnityEngine.UI;

namespace Door13.Puzzle
{
    /// <summary>密码盘面板（通用）：任意长度数字密码，密码与标题由调用方配置。</summary>
    public class KeypadPanel : MonoBehaviour
    {
        public static KeypadPanel Instance { get; private set; }

        public Text titleText;
        public Text display;

        string code = "";
        string input = "";
        Action onCorrect;

        void Awake()
        {
            Instance = this;
            Wire("Digits/Digit_", 10, i => Press(i.ToString()));
            Bind("Digits/Clear", PressClear);
            Bind("Digits/Enter", PressEnter);
            Bind("CloseButton", Close);
            gameObject.SetActive(false);
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Open(string title, string correctCode, Action onCorrect)
        {
            code = correctCode;
            this.onCorrect = onCorrect;
            input = "";
            if (titleText != null) titleText.text = title;
            Refresh();
            gameObject.SetActive(true);
        }

        public void Close() => gameObject.SetActive(false);

        public void Press(string d)
        {
            if (input.Length < code.Length) input += d;
            Refresh();
            Audio.AudioDirector.Instance?.Blip(input.Length);   // 音调随位数升高
        }

        public void PressClear() { input = ""; Refresh(); }

        public void PressEnter() => EnterCode(input);

        /// <summary>校验入口（UI与自动化测试共用）。</summary>
        public void EnterCode(string attempt)
        {
            if (attempt == code)
            {
                Audio.AudioDirector.Instance?.Chime();
                var cb = onCorrect;
                Close();
                cb?.Invoke();
            }
            else
            {
                Audio.AudioDirector.Instance?.Buzz();
                if (UI.Hud.Instance != null) UI.Hud.Instance.Toast("密码错误。");
                input = "";
                Refresh();
            }
        }

        void Refresh()
        {
            if (display != null)
                display.text = input.PadRight(code.Length, '_');
        }

        void Wire(string prefix, int count, Action<int> action)
        {
            for (int i = 0; i < count; i++)
            {
                var t = transform.Find(prefix + i);
                var btn = t != null ? t.GetComponent<Button>() : null;
                if (btn != null)
                {
                    int captured = i;
                    btn.onClick.AddListener(() => action(captured));
                }
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
