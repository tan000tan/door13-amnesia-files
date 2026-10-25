using UnityEngine;
using Door13.Save;
using Door13.Story;
using Door13.TimeLoop;

namespace Door13.Interaction
{
    /// <summary>
    /// 主装置（循环引擎）：接触推进循环。L4 时提供两个终局选择：
    /// 再次接触（二次确认）→ E1 循环终点；或前往十三门。
    /// </summary>
    public class DeviceInteractable : Interactable
    {
        bool confirming;

        void Awake() { prompt = "接触装置"; }

        public override string GateMessage()
        {
            var lm = LoopManager.Instance;
            if (lm != null && lm.LoopCount >= LoopManager.MaxLoop && !confirming)
                return null; // L4：允许接触（进入终局确认）
            if (!Save.GameFlags.Has(GameFlags.DeviceReady))
                return "装置没有电。指示灯是灭的。";
            return base.GateMessage();
        }

        public override string DisplayPrompt
        {
            get
            {
                var lm = TimeLoop.LoopManager.Instance;
                if (lm != null && lm.LoopCount >= TimeLoop.LoopManager.MaxLoop)
                    return confirming ? "再次接触——无法回头" : "接触装置（它不再回应…？）";
                return prompt;
            }
        }

        protected override void OnInteract(PlayerInteraction actor)
        {
            var lm = LoopManager.Instance;
            if (lm == null) return;

            if (lm.LoopCount >= LoopManager.MaxLoop)
            {
                // L4 终局分支
                if (!confirming)
                {
                    confirming = true;
                    if (UI.Hud.Instance != null)
                        UI.Hud.Instance.Toast("装置深处传来倒计时。再接触一次，循环将不再重来。");
                    return;
                }
                EndingManager.Instance?.ShowEnding(1);
                return;
            }

            confirming = false;
            if (lm.AdvanceLoop() && UI.Hud.Instance != null)
                UI.Hud.Instance.Toast(StoryTexts.DeviceTouch(lm.LoopCount));
        }
    }
}
