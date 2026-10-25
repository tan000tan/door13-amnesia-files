using UnityEngine;

namespace Door13.Interaction
{
    /// <summary>
    /// 录音机：放入指定录音带播放（文本+证据旗标），不消耗磁带。
    /// </summary>
    public class AudioLogPlayer : Interactable
    {
        [Tooltip("需要的磁带物品ID")]
        public string tapeItem = "tape_a";

        [Multiline] public string tapeText = "";

        [Tooltip("证据旗标")]
        public string evidenceFlag = "";

        [Tooltip("面板标题")]
        public string title = "录音机";

        void Awake()
        {
            prompt = "播放录音带";
            requiredItemId = tapeItem;
            lockedMessage = "录音机是空的。需要对应的录音带。";
        }

        protected override void OnInteract(PlayerInteraction actor)
        {
            if (InspectPanel.Instance != null)
                InspectPanel.Instance.Show(title, tapeText);
            if (!string.IsNullOrEmpty(evidenceFlag))
                Save.GameFlags.SetOnce(evidenceFlag);
            Audio.AudioDirector.Instance?.Scare(3);
        }
    }
}
