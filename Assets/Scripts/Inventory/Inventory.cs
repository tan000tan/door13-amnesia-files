using System;
using System.Collections.Generic;
using UnityEngine;

namespace Door13.Inventory
{
    /// <summary>
    /// 背包数据模型：持有物品ID列表（不含UI）。存档恢复走 Restore。
    /// UI 通过 Changed 事件刷新。
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        public static Inventory Instance { get; private set; }

        public event Action Changed;

        readonly List<string> items = new List<string>();

        /// <summary>当前物品ID（只读）</summary>
        public IReadOnlyList<string> Items => items;

        void Awake()
        {
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public bool Has(string id) => items.Contains(id);

        /// <summary>把当前背包同步进存档数据（写入方；读取方为 Restore）。</summary>
        void Sync()
        {
            var d = Save.SaveManager.Data;
            if (d == null) return;
            d.items.Clear();
            d.items.AddRange(items);
        }

        public void Add(string id)
        {
            if (items.Contains(id)) return;
            items.Add(id);
            Sync();
            Changed?.Invoke();
        }

        public bool Remove(string id)
        {
            if (items.Remove(id))
            {
                Sync();
                Changed?.Invoke();
                return true;
            }
            return false;
        }

        public void Clear()
        {
            if (items.Count == 0) return;
            items.Clear();
            Sync();
            Changed?.Invoke();
        }

        /// <summary>从存档恢复（整体替换）。</summary>
        public void Restore(IEnumerable<string> ids)
        {
            items.Clear();
            if (ids != null) items.AddRange(ids);
            Sync();
            Changed?.Invoke();
        }
    }
}
