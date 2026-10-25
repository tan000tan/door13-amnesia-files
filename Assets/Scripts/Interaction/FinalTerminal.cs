using UnityEngine;
using Door13.Puzzle;
using Door13.Save;
using Door13.Story;
using Door13.UI;

namespace Door13.Interaction
{
    /// <summary>终局终端（十三门后室）：三个选择 → 结局。
    /// ① 返回地面 → E2 逃离
    /// ② 销毁档案 → E3 尘归尘
    /// ③ 进入容器（需证据≥6）→ 进入R7容器室 → E4 归零（真结局）
    /// 证据不足时显示揭示性文本，而非简单拒绝。</summary>
    public class FinalTerminal : Interactable
    {
        static readonly string[] Choices =
        {
            "① 返回地面（电梯已供电）",
            "② 销毁全部档案",
            "③ 进入容器，完成实验"
        };

        void Awake() { prompt = "使用仪器"; }

        protected override void OnInteract(PlayerInteraction actor)
        {
            TerminalPanel.Instance?.Open(
                "第十三扇门 · 后室",
                StoryDocs.FinalChoiceBody,
                Choices,
                idx => EndingManager.Choose(idx));
        }
    }
}
