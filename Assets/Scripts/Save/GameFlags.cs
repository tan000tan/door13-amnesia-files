using System.Collections.Generic;

namespace Door13.Save
{
    /// <summary>剧情旗标与已解谜题的便捷读写（底层为存档列表）。</summary>
    public static class GameFlags
    {
        public static bool Has(string flag) => SaveManager.Data.flags.Contains(flag);

        public static bool SetOnce(string flag)
        {
            if (Has(flag)) return false;
            SaveManager.Data.flags.Add(flag);
            SaveManager.Save();
            return true;
        }

        public static void Clear(string flag) => SaveManager.Data.flags.Remove(flag);

        public static bool IsSolved(string puzzleId) => SaveManager.Data.solved.Contains(puzzleId);

        public static void MarkSolved(string puzzleId)
        {
            if (IsSolved(puzzleId)) return;
            SaveManager.Data.solved.Add(puzzleId);
            SaveManager.Save();
        }

        /// <summary>证据文件计数（结局判定用，旗标以 ev_ 开头）</summary>
        public static int EvidenceCount
        {
            get
            {
                int n = 0;
                var flags = SaveManager.Data.flags;
                for (int i = 0; i < flags.Count; i++)
                    if (flags[i].StartsWith("ev_")) n++;
                return n;
            }
        }

        public static bool AllEvidence(int need) => EvidenceCount >= need;

        // 常用旗标常量
        public const string Flashlight = "has_flashlight";
        public const string MainPower = "main_power";
        public const string DeviceReady = "device_ready";
        public const string Door13Open = "door13_open";
    }
}
