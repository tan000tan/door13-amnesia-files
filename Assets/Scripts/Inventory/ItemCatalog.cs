using System.Collections.Generic;
using UnityEngine;

namespace Door13.Inventory
{
    /// <summary>
    /// 物品目录与组合表（最终版：18件物品，6条组合）。
    /// 静态数据表：策划改这里即可，无需动UI/逻辑代码。
    /// </summary>
    public static class ItemCatalog
    {
        public struct ItemData
        {
            public string Id, Name, Desc;
            public Color Color;
            public ItemData(string id, string name, string desc, Color color)
            { Id = id; Name = name; Desc = desc; Color = color; }
        }

        public static readonly ItemData Keycard = new ItemData("keycard", "门禁卡",
            "研究员的门禁卡。背面手写着：B2-13。", new Color(0.92f, 0.85f, 0.45f));
        public static readonly ItemData Note = new ItemData("note", "撕下的便签",
            "『如果你醒了，先看墙上的数字。』", new Color(1f, 0.95f, 0.65f));
        public static readonly ItemData V2Card = new ItemData("v2card", "V2区门禁卡",
            "配电箱里掉出的卡。标签：实验区 V2。", new Color(0.6f, 0.8f, 0.95f));
        public static readonly ItemData ReagentA = new ItemData("reagent_a", "试剂A",
            "棕色瓶。标签：R-A／强腐蚀。瓶身有使用记录。", new Color(0.75f, 0.5f, 0.2f));
        public static readonly ItemData ReagentB = new ItemData("reagent_b", "试剂B",
            "透明瓶。标签：R-B／低温保存。", new Color(0.7f, 0.9f, 0.9f));
        public static readonly ItemData Solvent = new ItemData("solvent", "溶解剂",
            "A与B的混合液，正在缓慢放热。可以溶解低温粘合剂。", new Color(0.9f, 0.6f, 0.3f));
        public static readonly ItemData ArchiveCard = new ItemData("archive_card", "档案权限卡",
            "冷冻柜里的备份卡。标签：档案室·全权限。", new Color(0.9f, 0.7f, 0.85f));
        public static readonly ItemData TapeA = new ItemData("tape_a", "录音带A",
            "标签：最后一次团队会议。", new Color(0.3f, 0.3f, 0.35f));
        public static readonly ItemData TapeB = new ItemData("tape_b", "录音带B",
            "标签：装置运行日志·13号样本。", new Color(0.3f, 0.3f, 0.35f));
        public static readonly ItemData Flashlight = new ItemData("flashlight", "手电筒",
            "军用手电，电池还很足。地下区域没有照明。", new Color(0.85f, 0.85f, 0.7f));
        public static readonly ItemData CtrlCard = new ItemData("ctrl_card", "控制中心钥匙卡",
            "保险柜最深处。标签：控制中心·主管权限。", new Color(0.95f, 0.6f, 0.4f));
        public static readonly ItemData FuelCan = new ItemData("fuel_can", "空油桶",
            "发电机油桶，已经空了。", new Color(0.6f, 0.55f, 0.45f));
        public static readonly ItemData Fuel = new ItemData("fuel", "燃料罐",
            "标记：柴油。地下储藏格。", new Color(0.85f, 0.75f, 0.3f));
        public static readonly ItemData MixFuel = new ItemData("mix_fuel", "混合燃料",
            "加满的油桶。发电机可以用它启动。", new Color(0.8f, 0.7f, 0.35f));
        public static readonly ItemData Shard1 = new ItemData("shard_1", "记忆碎片·壹",
            "一片金属残片，摸到的瞬间脑中闪过画面：白色的走廊。刻着罗马数字Ⅰ。", new Color(0.75f, 0.78f, 0.85f));
        public static readonly ItemData Shard2 = new ItemData("shard_2", "记忆碎片·贰",
            "残片Ⅱ。闪过的画面：手术灯，和一句『记住你是谁』。", new Color(0.75f, 0.78f, 0.85f));
        public static readonly ItemData Shard12 = new ItemData("shard_12", "记忆碎片·壹贰",
            "两块残片咬合在一起，缺口指向第三块。", new Color(0.75f, 0.78f, 0.85f));
        public static readonly ItemData Shard3 = new ItemData("shard_3", "记忆碎片·叁",
            "残片Ⅲ。闪过的画面：十三扇门，一扇一扇排成一排，而你在最后一扇后面。", new Color(0.75f, 0.78f, 0.85f));
        public static readonly ItemData MasterKey = new ItemData("master_key", "十三号钥匙",
            "三块碎片咬合成的钥匙。冰凉，像一直在等你。", new Color(0.9f, 0.92f, 1f));
        public static readonly ItemData Badge = new ItemData("badge", "T-13工牌",
            "枕头下的工牌。照片是你。姓名栏被指甲反复刮过，仍能辨认：林澈。", new Color(0.8f, 0.82f, 0.88f));
        public static readonly ItemData PhotoOld = new ItemData("photo_old", "老照片",
            "冷冻柜里的照片：一群人围着生日蛋糕，蛋糕上写着『小澈 4·3』。", new Color(0.85f, 0.8f, 0.7f));

        public static readonly ItemData[] All =
        {
            Keycard, Note, V2Card, ReagentA, ReagentB, Solvent, ArchiveCard,
            TapeA, TapeB, Flashlight, CtrlCard, FuelCan, Fuel, MixFuel,
            Shard1, Shard12, Shard3, MasterKey, Badge, PhotoOld
        };

        public static ItemData Get(string id)
        {
            foreach (var d in All) if (d.Id == id) return d;
            return default;
        }

        // ---- 组合表（A+B → 结果）----
        public struct Combo { public string A, B, Result, Message, Special; }

        public static readonly Combo[] Combos =
        {
            new Combo{ A="reagent_a", B="reagent_b", Result="solvent",
                Message="试剂混合，瓶身微微发热——溶解剂完成了。" },
            new Combo{ A="fuel_can", B="fuel", Result="mix_fuel",
                Message="油桶加满了。发电机有救了。" },
            new Combo{ A="shard_1", B="shard_2", Result=null, Special="shard12",
                Message="两块残片咬合在一起，缺一角——还差一块。" },
            new Combo{ A="shard_12", B="shard_3", Result="master_key",
                Message="三块碎片拼合成一把完整的钥匙：十三号钥匙。" },
        };

        /// <summary>尝试组合两件物品。返回结果物品ID（null=失败），special 为中间产物。</summary>
        public static string TryCombo(string a, string b, out string message)
        {
            foreach (var c in Combos)
            {
                if ((c.A == a && c.B == b) || (c.A == b && c.B == a))
                {
                    message = c.Message;
                    if (c.Special == "shard12") return "shard_12";
                    return c.Result;
                }
            }
            message = "这两样东西组合不出什么。";
            return null;
        }
    }
}
