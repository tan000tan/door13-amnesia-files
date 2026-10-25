using UnityEngine;

namespace Door13.Interaction
{
    /// <summary>
    /// 可拾取物：加入背包并隐藏自身。拾取状态由背包持有（跨循环保留）；
    /// 仅"新游戏"复原。拾取可附带旗标（如手电筒）。
    /// </summary>
    public class Pickupable : Interactable
    {
        [Tooltip("物品ID（见 ItemCatalog）")]
        public string itemId;

        [Tooltip("拾取时附加的旗标（如 has_flashlight），空=无")]
        public string pickupFlag = "";

        public bool Collected { get; private set; }

        void Reset() { prompt = "拾取"; }

        void Start()
        {
            // 存档恢复：物品已在背包 → 不再显示于世界
            var inv = Inventory.Inventory.Instance;
            if (inv != null && inv.Has(itemId))
            {
                Collected = true;
                gameObject.SetActive(false);
            }
        }

        void OnEnable()
        {
            // 循环规则可能重新激活物件；若已被拾取则保持隐藏
            if (Collected) gameObject.SetActive(false);
        }

        protected override void OnInteract(PlayerInteraction actor)
        {
            if (Collected) return;
            Collected = true;

            var def = Inventory.ItemCatalog.Get(itemId);
            Inventory.Inventory.Instance.Add(itemId);
            if (!string.IsNullOrEmpty(pickupFlag))
                Save.GameFlags.SetOnce(pickupFlag);
            if (UI.Hud.Instance != null) UI.Hud.Instance.Toast("拾取：" + def.Name);
            Audio.AudioDirector.Instance?.Pickup();
            Save.SaveManager.Save();
            gameObject.SetActive(false);
        }

        /// <summary>仅新游戏时调用：恢复可拾取状态。</summary>
        public void RestoreUnpicked()
        {
            Collected = false;
            gameObject.SetActive(true);
        }
    }
}
