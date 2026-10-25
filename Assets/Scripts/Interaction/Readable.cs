using UnityEngine;

namespace Door13.Interaction
{
    /// <summary>
    /// 可阅读物（便签/终端/照片墙/档案）：交互后打开检视面板显示文本。
    /// evidenceFlag 非空时，首次阅读登记证据旗标（结局判定）。
    /// </summary>
    public class Readable : Interactable
    {
        public string title = "线索";

        [Multiline]
        public string body = "";

        [Tooltip("证据旗标（如 ev_birth），空=非证据")]
        public string evidenceFlag = "";

        void Reset() { prompt = "查看"; }

        protected override void OnInteract(PlayerInteraction actor)
        {
            if (InspectPanel.Instance != null)
                InspectPanel.Instance.Show(title, body);
            if (!string.IsNullOrEmpty(evidenceFlag))
                Save.GameFlags.SetOnce(evidenceFlag);
        }
    }
}
