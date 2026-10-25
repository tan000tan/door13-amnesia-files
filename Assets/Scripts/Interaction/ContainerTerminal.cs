using UnityEngine;
using Door13.Save;
using Door13.Story;
using Door13.UI;

namespace Door13.Interaction
{
    /// <summary>容器室终端（R7）：真结局的最终交互。</summary>
    public class ContainerTerminal : Interactable
    {
        void Awake() { prompt = "打开容器"; }

        protected override void OnInteract(PlayerInteraction actor)
        {
            if (GameFlags.AllEvidence(EndingManager.EvidenceNeed))
            {
                // 播放真结局
                var em = FindObjectOfType<Story.EndingManager>();
                em?.ShowEnding(4);
            }
            else
            {
                // 证据不足，显示揭示性文本
                if (Hud.Instance != null)
                    Hud.Instance.ShowNarration(
                        "容器没有打开。\n\n你还没有记住所有事。\n你还不知道他们是谁。\n你还不知道——\n\n你自己是谁。",
                        4f);
            }
        }
    }
}
