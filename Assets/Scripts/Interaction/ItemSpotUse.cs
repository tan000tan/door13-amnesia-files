using UnityEngine;

namespace Door13.Interaction
{
    /// <summary>
    /// 物品使用点：对准目标使用背包中指定物品（如溶解剂浇冷冻柜锁、燃料加油机）。
    /// 成功后登记旗标、可选消耗物品、可选显现拾取物。
    /// </summary>
    public class ItemSpotUse : Interactable, TimeLoop.ILoopResettable
    {
        [Tooltip("所需物品ID（基类 requiredItemId 同步使用）")]
        public string needItem = "";

        [Tooltip("使用后是否消耗该物品")]
        public bool consume = true;

        [Tooltip("成功旗标（幂等）")]
        public string solvedFlag = "";

        [Tooltip("成功后显现的拾取物（可选）")]
        public Pickupable reveal;

        [Tooltip("成功后隐藏的目标（如冰锁本体，可选）")]
        public GameObject hideOnUse;

        [Tooltip("成功提示")]
        public string successMessage = "用了上去。";

        public bool Used
        {
            get { return !string.IsNullOrEmpty(solvedFlag) && Save.GameFlags.Has(solvedFlag); }
        }

        void Awake()
        {
            TimeLoop.LoopManager.Register(this);
            requiredItemId = needItem;
        }

        void Start()
        {
            // 存档恢复：已使用状态 → 战利品保持显现
            if (Used && reveal != null && !reveal.Collected)
                reveal.gameObject.SetActive(true);
        }

        public override bool CanInteract => base.CanInteract && !Used;

        protected override void OnInteract(PlayerInteraction actor)
        {
            var inv = Inventory.Inventory.Instance;
            if (consume && inv != null && !string.IsNullOrEmpty(requiredItemId))
                inv.Remove(requiredItemId);

            if (!string.IsNullOrEmpty(solvedFlag))
                Save.GameFlags.SetOnce(solvedFlag);
            if (reveal != null) reveal.gameObject.SetActive(true);
            if (hideOnUse != null) hideOnUse.SetActive(false);

            Audio.AudioDirector.Instance?.Click();
            if (UI.Hud.Instance != null) UI.Hud.Instance.Toast(successMessage);
        }

        public void OnLoopReset(int loop)
        {
            // 已使用状态属于记忆，保留；未使用则保证初始遮蔽
            if (!Used && hideOnUse != null) hideOnUse.SetActive(true);
            if (!Used && reveal != null) reveal.gameObject.SetActive(false);
        }
    }
}
