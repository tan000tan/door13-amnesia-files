using UnityEngine;
using UnityEngine.UI;

namespace Door13.Interaction
{
    /// <summary>
    /// 全屏检视面板：显示线索标题+正文。覆盖全屏时自然屏蔽移动与视角输入
    /// （触摸命中UI后不会进入视角区/摇杆）。
    /// </summary>
    public class InspectPanel : MonoBehaviour
    {
        public static InspectPanel Instance { get; private set; }

        public Text titleText;
        public Text bodyText;

        void Awake()
        {
            Instance = this;
            var close = transform.Find("CloseButton");
            if (close != null)
            {
                var btn = close.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(Close);
            }
            gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Show(string title, string body)
        {
            if (titleText != null) titleText.text = title;
            if (bodyText != null) bodyText.text = body;
            Audio.AudioDirector.Instance?.Paper();
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
            // 结局展示完毕 → 回主菜单
            if (UI.Hud.Instance != null && UI.Hud.Instance.GameOver && UI.MainMenu.Instance != null)
                UI.MainMenu.Instance.ShowMenu();
        }
    }
}
