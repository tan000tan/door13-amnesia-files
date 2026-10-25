using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Door13.Save
{
    /// <summary>
    /// JSON 存档 v3：循环、旗标、已解谜题、背包、所在房间、章节进度、自动存档时间。
    /// v1 旧档自动迁移（safeOpened → flag）。坏档自动重置，绝不崩溃。
    /// 自动存档：房间切换/谜题解开/拾取物品/循环推进时触发。
    /// 手动存档：暂停菜单"保存"按钮。
    /// 章节记录：记录每章首次到达的循环次数。
    /// </summary>
    public static class SaveManager
    {
        const int CurrentVersion = 3;

        static string FilePath =>
            System.IO.Path.Combine(Application.persistentDataPath, "door13_save.json");

        // 自动存档节流：0.5秒内不重复写盘
        static float _lastSaveTime = -999f;
        const float AutoSaveCooldown = 0.5f;

        [Serializable]
        public class SaveData
        {
            public int saveVersion = CurrentVersion;

            // 循环进度
            public int loop = 1;

            // 当前位置
            public string posRoom = "R0";

            // 章节首次到达记录（key=房间名, value=首次到达循环次数）
            public Dictionary<string, int> chapterFirstVisit = new Dictionary<string, int>();

            // v1 兼容
            public bool safeOpened;             // v1 兼容
            public bool demoEndShown;           // v1 兼容（废弃）

            // 剧情旗标
            public List<string> flags = new List<string>();

            // 已解谜题
            public List<string> solved = new List<string>();

            // 背包物品
            public List<string> items = new List<string>();

            // 结局记录
            public List<string> endings = new List<string>();

            // 游戏时长（秒）
            public float playTime = 0f;
        }

        public static SaveData Data { get; private set; } = new SaveData();

        public static void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var loaded = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
                    if (loaded != null)
                    {
                        // v1 迁移
                        if (loaded.saveVersion == 1)
                        {
                            loaded.saveVersion = CurrentVersion;
                            if (loaded.safeOpened && !loaded.flags.Contains("safe_open"))
                                loaded.flags.Add("safe_open");
                        }
                        // v2 → v3：新增 chapterFirstVisit / endings / playTime 字段
                        if (loaded.saveVersion == 2)
                        {
                            loaded.saveVersion = CurrentVersion;
                            if (loaded.chapterFirstVisit == null)
                                loaded.chapterFirstVisit = new Dictionary<string, int>();
                            if (loaded.endings == null)
                                loaded.endings = new List<string>();
                        }
                        if (loaded.saveVersion != CurrentVersion)
                        {
                            Debug.LogWarning("存档版本不匹配，已重置。");
                            loaded = new SaveData();
                        }
                        // 防御性修复：任何版本的坏档/缺字段集合都不允许为 null（"绝不崩溃"承诺）
                        if (loaded.flags == null) loaded.flags = new List<string>();
                        if (loaded.solved == null) loaded.solved = new List<string>();
                        if (loaded.items == null) loaded.items = new List<string>();
                        if (loaded.endings == null) loaded.endings = new List<string>();
                        if (loaded.chapterFirstVisit == null)
                            loaded.chapterFirstVisit = new Dictionary<string, int>();
                        Data = loaded;
                        return;
                    }
                }
                Data = new SaveData();
            }
            catch (Exception e)
            {
                Debug.LogWarning("存档读取失败，已重置: " + e.Message);
                Data = new SaveData();
            }
        }

        /// <summary>保存存档（带节流，防止频繁写盘）。</summary>
        public static void Save()
        {
            // 节流：0.5秒内不重复写盘
            float now = Time.realtimeSinceStartup;
            if (now - _lastSaveTime < AutoSaveCooldown) return;
            _lastSaveTime = now;

            try { File.WriteAllText(FilePath, JsonUtility.ToJson(Data)); }
            catch (Exception e) { Debug.LogWarning("存档写入失败: " + e.Message); }
        }

        /// <summary>强制立即保存（无视节流），用于手动保存按钮）。</summary>
        public static void SaveForced()
        {
            _lastSaveTime = -999f; // 清除节流
            Save();
        }

        public static void DeleteSave()
        {
            Data = new SaveData();
            try { if (File.Exists(FilePath)) File.Delete(FilePath); }
            catch { /* 删除失败不致命 */ }
        }

        public static bool HasSave => File.Exists(FilePath);

        /// <summary>记录玩家到达的房间（自动存档触发点）。</summary>
        public static void RecordRoom(string roomName)
        {
            if (string.IsNullOrEmpty(roomName)) return;
            Data.posRoom = roomName;
            // 记录首次到达循环
            if (!Data.chapterFirstVisit.ContainsKey(roomName))
                Data.chapterFirstVisit[roomName] = Data.loop;
            Save();
        }

        /// <summary>记录游戏时长（每帧调用）。</summary>
        public static void AddPlayTime(float seconds)
        {
            Data.playTime += seconds;
        }

        /// <summary>记录结局。</summary>
        public static void RecordEnding(string endingId)
        {
            if (!Data.endings.Contains(endingId))
                Data.endings.Add(endingId);
            Save();
        }

        /// <summary>章节进度摘要（供UI显示）。</summary>
        public static string GetChapterProgressSummary()
        {
            if (Data.chapterFirstVisit == null || Data.chapterFirstVisit.Count == 0)
                return "尚未开始探索";

            var sb = new System.Text.StringBuilder();
            sb.Append("探索进度：");
            foreach (var kv in Data.chapterFirstVisit)
                sb.Append(kv.Key).Append("(循环").Append(kv.Value).Append(") ");
            return sb.ToString();
        }

        /// <summary>已解锁章节数。</summary>
        public static int UnlockedRooms => Data.chapterFirstVisit != null ? Data.chapterFirstVisit.Count : 0;

        /// <summary>总章节数。</summary>
        public static int TotalRooms => 7; // R0-R6
    }
}
