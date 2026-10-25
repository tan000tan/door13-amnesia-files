using UnityEngine;
using Door13.Save;

namespace Door13.Interaction
{
    /// <summary>发电机启动索：需已加油（旗标），拉动后装置通电（旗标 device_ready）。</summary>
    public class GeneratorStart : Interactable
    {
        [Tooltip("启动成功旗标")]
        public string readyFlag = GameFlags.DeviceReady;

        public bool Started
        {
            get { return !string.IsNullOrEmpty(readyFlag) && GameFlags.Has(readyFlag); }
        }

        void Awake() { prompt = "拉启动索"; }

        public override bool CanInteract => base.CanInteract && !Started;

        public override string GateMessage()
        {
            if (Started) return null;
            if (!GameFlags.Has("gen_fueled")) return "油桶是空的。先加满柴油。";
            return base.GateMessage();
        }

        protected override void OnInteract(PlayerInteraction actor)
        {
            GameFlags.SetOnce(readyFlag);
            Save.SaveManager.Save();
            Audio.AudioDirector.Instance?.Scare(1);
            if (UI.Hud.Instance != null)
                UI.Hud.Instance.Toast("发电机轰鸣起来。主装置的指示灯亮了——");
        }
    }
}
