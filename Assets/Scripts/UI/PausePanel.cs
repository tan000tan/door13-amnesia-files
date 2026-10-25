using UnityEngine;
using UnityEngine.UI;
using Door13.Save;
using Door13.Player;

namespace Door13.UI
{
    /// <summary>暂停菜单：继续 / 保存 / 设置 / 回主菜单 / 退出。</summary>
    public class PausePanel : MonoBehaviour
    {
        public static PausePanel Instance { get; private set; }

        void Awake()
        {
            Instance = this;
            Bind("BtnResume", Close);
            Bind("BtnSave", ManualSave);
            Bind("BtnSettings", OpenSettings);
            Bind("BtnMenu", BackToMenu);
            Bind("BtnQuit", Quit);
            gameObject.SetActive(false);
            // 注意：不要在 Start 里订阅 Hud.MenuClicked——本面板 Awake 即自禁用，
            // Start 永不运行（曾导致游戏内"菜单"按钮完全失效）。
            // 打开入口由 Hud 直接调用 PausePanel.Instance.Open()。
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Open()
        {
            Debug.Log("[UI] PauseOpen");
            InputLock.Active = true;   // 暂停期间禁用移动/视角/交互
            gameObject.SetActive(true);
        }
        public void Close()
        {
            // 循环切换演出期间由 LoopManager 持有输入锁，暂停面板不得提前解锁
            bool transitionHeld = TimeLoop.LoopManager.Instance != null && TimeLoop.LoopManager.Instance.transitioning;
            InputLock.Active = transitionHeld;
            gameObject.SetActive(false);
        }

        /// <summary>手动保存（强制写盘，无视节流）。</summary>
        void ManualSave()
        {
            SaveManager.SaveForced();
            if (Hud.Instance != null)
                Hud.Instance.Toast("游戏已保存。");
        }

        void OpenSettings()
        {
            if (SettingsPanel.Instance != null) SettingsPanel.Instance.Open();
        }

        void BackToMenu()
        {
            SaveManager.SaveForced();
            Close();
            if (MainMenu.Instance != null) MainMenu.Instance.ShowMenu();  // ShowMenu 会重新锁定输入
        }

        void Quit()
        {
            SaveManager.SaveForced();
            GameSettings.Save();
            Close();
            if (MainMenu.Instance != null) MainMenu.Instance.ShowMenu();  // ShowMenu 会重新锁定输入
        }

        void Bind(string name, System.Action action)
        {
            var t = transform.Find(name);
            var btn = t != null ? t.GetComponent<Button>() : null;
            if (btn != null) btn.onClick.AddListener(() => action());
        }
    }
}
