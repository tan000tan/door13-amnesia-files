using System;
using System.IO;
using UnityEngine;

namespace Door13.Player
{
    /// <summary>
    /// 设置系统：灵敏度（低/中/高预设）/镜头平滑/音量（主音量+音乐+SFX）/Y轴反转/画质/FOV。JSON持久化。
    /// </summary>
    public static class GameSettings
    {
        const int Version = 4;
        static string FilePath =>
            System.IO.Path.Combine(Application.persistentDataPath, "door13_settings.json");

        [Serializable]
        public class Data
        {
            public int version = Version;
            public float lookSensitivity = 0.6f;   // 视角灵敏度（默认中档，偏低更稳）
            public float moveSensitivity = 1.0f;   // 摇杆灵敏度（移动速度倍率）
            public float lookSmoothing = 9f;       // 镜头平滑插值速度（0=关闭）
            public float masterVolume = 0.8f;      // 总音量
            public float musicVolume = 0.7f;       // 音乐音量（环境音）
            public float sfxVolume = 0.9f;         // 音效音量（交互/按键）
            public bool invertY = false;   // false=自然方向（上滑抬头）；翻转符号已由 PlayerController 承担
            public int graphicsQuality = 1;        // 0低 1中 2高
            public float fov = 65f;                // 视野角度
            public bool touchDebug = false;        // 触摸诊断模式（屏幕坐标级测试）
        }

        public static Data Cur { get; private set; } = new Data();
        public static event Action Changed;

        public static void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var d = JsonUtility.FromJson<Data>(File.ReadAllText(FilePath));
                    if (d != null && d.version == Version) Cur = d;
                }
            }
            catch (Exception e) { Debug.LogWarning("设置读取失败: " + e.Message); }
        }

        public static void Save()
        {
            try { File.WriteAllText(FilePath, JsonUtility.ToJson(Cur)); }
            catch (Exception e) { Debug.LogWarning("设置写入失败: " + e.Message); }
        }

        public static void Set(Action<Data> mutate)
        {
            mutate(Cur);
            Save();
            // 实时应用音量到 AudioListener
            if (AudioListener.volume != Cur.masterVolume)
                AudioListener.volume = Cur.masterVolume;
            // 实时应用FOV
            var mainCam = Camera.main;
            if (mainCam != null && Mathf.Abs(mainCam.fieldOfView - Cur.fov) > 0.1f)
                mainCam.fieldOfView = Cur.fov;
            Changed?.Invoke();
        }

        /// <summary>应用设置到运行时（启动/切换场景时调用）。</summary>
        public static void Apply()
        {
            AudioListener.volume = Cur.masterVolume;
            var mainCam = Camera.main;
            if (mainCam != null) mainCam.fieldOfView = Cur.fov;
        }

        /// <summary>低/中/高预设。index=灵敏度档，smooth=平滑档，move=摇杆档。</summary>
        public static readonly float[] LookPresets = { 0.4f, 0.6f, 0.9f };
        public static readonly float[] SmoothPresets = { 16f, 9f, 3f };
        public static readonly float[] MovePresets = { 0.8f, 1.0f, 1.2f };
        public static readonly string[] PresetNames = { "低", "中", "高" };

        public static int NearestIndex(float[] presets, float value)
        {
            float best = float.MaxValue; int idx = 1;
            for (int i = 0; i < presets.Length; i++)
            {
                float d = Mathf.Abs(presets[i] - value);
                if (d < best) { best = d; idx = i; }
            }
            return idx;
        }

        public static int NearestLookIndex() => NearestIndex(LookPresets, Cur.lookSensitivity);
        public static int NearestSmoothIndex() => NearestIndex(SmoothPresets, Cur.lookSmoothing);
        public static int NearestMoveIndex() => NearestIndex(MovePresets, Cur.moveSensitivity);
    }
}
