using System.Collections;
using UnityEngine;
using Door13.Save;
using Door13.TimeLoop;
using Door13.UI;

namespace Door13.Story
{
    /// <summary>
    /// 结局系统。最终终端三个选项：
    /// ① 返回地面 → E2 逃离；② 销毁档案 → E3 尘归尘；
    /// ③ 进入容器（需证据≥6）→ R7容器室 → E4 归零（真结局）；证据不足则显示揭示文本。
    /// E1 循环终点：L4 后再次接触主装置（需二次确认）。
    /// </summary>
    public class EndingManager : MonoBehaviour
    {
        public static EndingManager Instance { get; private set; }

        public const int EvidenceNeed = 6;

        void Awake() { Instance = this; }
        void OnDestroy() { if (Instance == this) Instance = null; }

        /// <summary>结局编号 1~4。</summary>
        public void ShowEnding(int id)
        {
            GameFlags.SetOnce("ending_" + id);
            SaveManager.RecordEnding("ending_" + id);
            SaveManager.Save();
            StartCoroutine(Play(id));
        }

        IEnumerator Play(int id)
        {
            if (Audio.AudioDirector.Instance != null) Audio.AudioDirector.Instance.Stinger();
            if (Hud.Instance != null)
            {
                yield return Hud.Instance.Fade(1f, 1.2f);
                Hud.Instance.ShowEnding(StoryTexts.EndingTitle(id), StoryTexts.EndingBody(id));
                yield return Hud.Instance.Fade(0f, 1.2f);
            }
        }

        /// <summary>终局选择入口（终端按钮调用）。</summary>
        public static void Choose(int option)
        {
            switch (option)
            {
                case 0: // 返回地面 → E2 逃离
                    Instance?.ShowEnding(2);
                    break;
                case 1: // 销毁档案 → E3 尘归尘
                    Instance?.ShowEnding(3);
                    break;
                case 2: // 进入容器 → 需证据≥6 → R7 → E4 归零
                    if (GameFlags.AllEvidence(EvidenceNeed))
                    {
                        // 进入容器室（R7）
                        if (Instance != null)
                            Instance.StartCoroutine(Instance.EnterContainer());
                    }
                    else
                    {
                        // 证据不足：显示揭示性文本
                        if (Hud.Instance != null)
                            Hud.Instance.ShowNarration(
                                "容器没有打开。\n\n你还没有记住所有事。\n\n你还不知道他们是谁。\n你还不知道——\n你自己是谁。",
                                4f);
                    }
                    break;
            }
        }

        /// <summary>进入容器室（R7）：真结局的前置场景。</summary>
        IEnumerator EnterContainer()
        {
            if (Hud.Instance != null)
            {
                // 淡出
                yield return Hud.Instance.Fade(1f, 1.5f);

                // 容器室旁白
                var containerNarration = "你走进了第十三扇门。\n\n门后不是房间。\n是一间白色的容器室。\n\n中央有一个发光的球体——\n记忆容器。\n\n它等着装下你记住的一切。\n不只是你的记忆。\n还有那十二个孩子的。\n\n他们唯一剩下的东西。";
                yield return Hud.Instance.ShowNarration(containerNarration, 5f);

                // 淡入
                yield return Hud.Instance.Fade(0f, 1.5f);

                // 传送玩家到R7
                var spawn = GameObject.Find("Spawn_R7");
                var pc = FindObjectOfType<Player.PlayerController>();
                if (spawn != null && pc != null)
                    pc.TeleportTo(spawn.transform.position, spawn.transform.rotation);

                // 标记已到达R7
                SaveManager.RecordRoom("R7");

                // 切换环境音为温暖氛围
                var audio = Audio.AudioDirector.Instance;
                if (audio != null) audio.SetRoomAmbient(0); // 温暖低鸣
            }
        }
    }
}
