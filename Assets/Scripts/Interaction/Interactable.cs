using UnityEngine;
using Door13.Save;

namespace Door13.Interaction
{
    /// <summary>
    /// 可交互物基类（模板方法）：统一处理门禁（所需物品/最低循环），
    /// 子类实现 OnInteract。由 PlayerInteraction 通过相机中心射线发现。
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        [Tooltip("准星提示，如：查看 / 拾取 / 开门")]
        public string prompt = "交互";

        [Tooltip("不可交互时的提示")]
        public string disabledPrompt = "无法使用";

        [Tooltip("需要背包中的物品（空=无要求）")]
        public string requiredItemId = "";

        [Tooltip("缺少所需物品时的提示")]
        public string lockedMessage = "它锁着。需要别的东西。";

        [Tooltip("最低循环次数（0=不限；低于该循环时无法交互）")]
        public int minLoop = 0;

        /// <summary>当前是否可交互</summary>
        public virtual bool CanInteract => isActiveAndEnabled;

        /// <summary>门禁检查后的提示文本（null=通过）。子类可扩展门禁条件。</summary>
        public virtual string GateMessage()
        {
            if (minLoop > 0 && TimeLoop.LoopManager.Instance != null &&
                TimeLoop.LoopManager.Instance.LoopCount < minLoop)
                return "现在还不是时候。";
            if (!string.IsNullOrEmpty(requiredItemId))
            {
                var inv = Inventory.Inventory.Instance;
                if (inv == null || !inv.Has(requiredItemId)) return lockedMessage;
            }
            return null;
        }

        public virtual string DisplayPrompt
        {
            get
            {
                if (!CanInteract) return disabledPrompt;
                return GateMessage() != null ? lockedMessage : prompt;
            }
        }

        /// <summary>玩家触发交互（含门禁）。actor 可能为 null（自动化测试）。</summary>
        public void Interact(PlayerInteraction actor)
        {
            if (!CanInteract)
            {
                if (UI.Hud.Instance != null) UI.Hud.Instance.Toast(disabledPrompt);
                return;
            }
            string gate = GateMessage();
            if (gate != null)
            {
                if (UI.Hud.Instance != null) UI.Hud.Instance.Toast(gate);
                return;
            }
            OnInteract(actor);
        }

        /// <summary>门禁通过后的实际行为（子类实现）。</summary>
        protected abstract void OnInteract(PlayerInteraction actor);
    }
}
