using UnityEngine;
using Door13.Puzzle;
using Door13.Save;

namespace Door13.Interaction
{
    /// <summary>
    /// 控制中心主控台：需主电源；密码登录（经 KeypadPanel，0403）后
    /// 打开白底文件系统（控制日志等），解锁地下通道、显现抽屉里的录音带B。
    /// </summary>
    public class ConsoleTerminal : Interactable
    {
        [Tooltip("登录密码")]
        public string password = "0403";

        [Tooltip("登录成功旗标")]
        public string unlockedFlag = "subway_unlock";

        [Tooltip("登录后显现的物体（抽屉/磁带B）")]
        public GameObject revealGo;

        [Tooltip("登录证据旗标")]
        public string evidenceFlag = "ev_console";

        public ComputerPanel.FileEntry[] files;

        public bool LoggedIn => GameFlags.Has(unlockedFlag);

        void Awake() { prompt = "使用控制台"; }

        public override bool CanInteract => base.CanInteract && !LoggedIn;

        public override string GateMessage()
        {
            if (LoggedIn) return null;
            if (!GameFlags.Has(GameFlags.MainPower)) return "屏幕黑着。先恢复主电源。";
            return base.GateMessage();
        }

        protected override void OnInteract(PlayerInteraction actor)
        {
            if (KeypadPanel.Instance != null)
                KeypadPanel.Instance.Open("控制台登录（主管生日）", password, OnLogin);
        }

        void OnLogin()
        {
            GameFlags.SetOnce(unlockedFlag);
            if (!string.IsNullOrEmpty(evidenceFlag)) GameFlags.SetOnce(evidenceFlag);
            if (revealGo != null) revealGo.SetActive(true);
            SaveManager.Save();
            if (UI.Hud.Instance != null) UI.Hud.Instance.Toast("身份确认。日志已解密，地下通道开放。");
            if (ComputerPanel.Instance != null && files != null)
                ComputerPanel.Instance.Open("控制中心 · 主控台", files);
        }
    }
}
