using UnityEngine;

namespace Door13.Interaction
{
    /// <summary>
    /// 保险箱：交互打开密码盘（KeypadPanel）。开箱后显露指定拾取物并记旗标。
    /// 开启状态跨循环保留。
    /// </summary>
    public class SafeInteractable : Interactable, TimeLoop.ILoopResettable
    {
        [Tooltip("箱门转轴")]
        public Transform safeDoorPivot;

        [Tooltip("箱内物品（拾取物，初始隐藏）")]
        public Pickupable loot;

        [Tooltip("解开时的旗标")]
        public string solvedFlag = "safe_open";

        [Tooltip("密码")]
        public string code = "1998";

        [Tooltip("开箱后战利品的落点（世界坐标；零=原地显现）。物品必须落在实心箱体外，否则被箱体遮挡且射线不可达")]
        public Vector3 lootDropPosition;

        [Tooltip("开箱后禁用的箱体碰撞体（SafeBody），防止挡住拾取射线")]
        public Collider obstacleToDisable;

        public bool IsOpen
        {
            get { return !string.IsNullOrEmpty(solvedFlag) && Save.GameFlags.Has(solvedFlag); }
        }

        void Awake()
        {
            prompt = "查看保险箱";
            TimeLoop.LoopManager.Register(this);
        }

        void Start()
        {
            // 存档恢复：保险箱已开（但物品可能还没拾取）→ 补施加开箱状态
            if (IsOpen) ApplyOpenState();
        }

        public override bool CanInteract => base.CanInteract && !IsOpen;

        protected override void OnInteract(PlayerInteraction actor)
        {
            if (Puzzle.KeypadPanel.Instance != null)
                Puzzle.KeypadPanel.Instance.Open("保险箱", code, OnOpened);
        }

        void OnOpened()
        {
            Save.GameFlags.SetOnce(solvedFlag);
            ApplyOpenState();
            if (UI.Hud.Instance != null)
                UI.Hud.Instance.Toast(Story.StoryTexts.SafeOpened);
        }

        /// <summary>施加开箱状态（开门/禁用箱体碰撞/物品移到箱前空地）。幂等，存档恢复共用。</summary>
        void ApplyOpenState()
        {
            if (safeDoorPivot != null)
                safeDoorPivot.localRotation = Quaternion.Euler(0f, -100f, 0f);
            if (obstacleToDisable != null) obstacleToDisable.enabled = false;
            if (loot != null)
            {
                if (lootDropPosition != Vector3.zero) loot.transform.position = lootDropPosition;
                // 物品已在背包时 Pickupable 自身的 Start/OnEnable 会再隐藏，此处无碍
                loot.gameObject.SetActive(true);
            }
        }

        public void OnLoopReset(int loop)
        {
            if (IsOpen) return;
            if (safeDoorPivot != null) safeDoorPivot.localRotation = Quaternion.identity;
            if (obstacleToDisable != null) obstacleToDisable.enabled = true;
            if (loot != null) loot.gameObject.SetActive(false);
        }
    }
}
