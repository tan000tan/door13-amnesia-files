using System;
using UnityEngine;
using UnityEngine.UI;

namespace Door13.Puzzle
{
    /// <summary>
    /// 终端面板：正文分页（\f 分隔）+ 选项列表（登录改由 KeypadPanel 承担）。
    /// 用于控制中心日志、终局选择等。
    /// </summary>
    public class TerminalPanel : MonoBehaviour
    {
        public static TerminalPanel Instance { get; private set; }

        public Text titleText;
        public Text bodyText;
        public RectTransform choiceRoot;

        string[] pages = new string[0];
        int page;
        Action<int> onChoice;

        void Awake()
        {
            Instance = this;
            Bind("PrevButton", () => Flip(-1));
            Bind("NextButton", () => Flip(1));
            Bind("CloseButton", Close);
            gameObject.SetActive(false);
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Open(string title, string body, string[] choiceLabels, Action<int> onChoice)
        {
            titleText.text = title;
            this.onChoice = onChoice;
            pages = string.IsNullOrEmpty(body) ? new string[0] : body.Split('\f');
            page = 0;
            if (bodyText != null) bodyText.text = pages.Length > 0 ? pages[0] : "";

            if (choiceRoot != null)
            {
                foreach (Transform c in choiceRoot) Destroy(c.gameObject);
                for (int i = 0; i < (choiceLabels != null ? choiceLabels.Length : 0); i++)
                {
                    int idx = i;
                    var go = new GameObject("Choice_" + i, typeof(Image), typeof(Button), typeof(LayoutElement));
                    go.transform.SetParent(choiceRoot, false);
                    go.GetComponent<LayoutElement>().minHeight = 70;
                    go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.18f);
                    var lg = new GameObject("L", typeof(Text));
                    lg.transform.SetParent(go.transform, false);
                    var t = lg.GetComponent<Text>();
                    t.text = choiceLabels[i];
                    t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    t.fontSize = 30; t.alignment = TextAnchor.MiddleCenter; t.color = Color.white; t.raycastTarget = false;
                    var rt = (RectTransform)lg.transform;
                    rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
                    go.GetComponent<Button>().onClick.AddListener(() => Choose(idx));
                }
            }
            RefreshFlip();
            gameObject.SetActive(true);
        }

        public void Close() => gameObject.SetActive(false);

        public void Choose(int index)
        {
            var cb = onChoice;
            onChoice = null;
            Close();
            cb?.Invoke(index);
        }

        void Flip(int dir)
        {
            if (pages.Length == 0) return;
            page = Mathf.Clamp(page + dir, 0, pages.Length - 1);
            if (bodyText != null) bodyText.text = pages[page];
            RefreshFlip();
        }

        void RefreshFlip()
        {
            SetBtn("PrevButton", pages.Length > 1 && page > 0);
            SetBtn("NextButton", pages.Length > 1 && page < pages.Length - 1);
        }

        void SetBtn(string path, bool on)
        {
            var t = transform.Find(path);
            if (t != null) t.gameObject.SetActive(on);
        }

        void Bind(string path, Action action)
        {
            var t = transform.Find(path);
            var btn = t != null ? t.GetComponent<Button>() : null;
            if (btn != null) btn.onClick.AddListener(() => action());
        }
    }
}
