using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Door13.Save;
using Door13.Story;
using Door13.UI;

namespace Door13.TimeLoop
{
    /// <summary>
    /// 循环总控（最终版）：循环上限4次，由主装置接触推进（剧情制）。
    /// 每次推进：旁白 → 淡出 → 应用循环规则+复位 → 目标更新 → 存档 → 淡入。
    /// 跨循环保留：背包/旗标/已解谜题（记忆穿透）；世界按 LoopObject 规则变化。
    /// </summary>
    public class LoopManager : MonoBehaviour
    {
        public static LoopManager Instance { get; private set; }

        public const int MaxLoop = 4;

        /// <summary>当前循环次数（1~4）</summary>
        public int LoopCount { get; private set; } = 1;

        [Tooltip("玩家出生点")]
        public Transform spawnPoint;

        static readonly List<ILoopResettable> resettables = new List<ILoopResettable>();
        static readonly List<LoopObject> loopObjects = new List<LoopObject>();

        /// <summary>循环推进演出进行中（公开只读，供测试与UI防抖）</summary>
        public bool transitioning { get; private set; }

        void Awake()
        {
            Instance = this;
            resettables.Clear();
            loopObjects.Clear();
            SaveManager.Load();
        }

        void Start()
        {
            // 应用设置（音量/灵敏度）
            Player.GameSettings.Load();
            AudioListener.volume = Player.GameSettings.Cur.masterVolume;

            foreach (var o in FindObjectsOfType<LoopObject>(true))
                RegisterLoopObject(o);

            LoopCount = Mathf.Clamp(SaveManager.Data.loop, 1, MaxLoop);
            ApplyAllStates(LoopCount);
            foreach (var r in resettables) r.OnLoopReset(LoopCount);
            ObjectiveManager.Instance?.ApplyLoop(LoopCount);
            if (Hud.Instance != null) Hud.Instance.SetLoop(LoopCount);
        }

        void Update()
        {
            // 累计游戏时长
            SaveManager.AddPlayTime(Time.deltaTime);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static void Register(ILoopResettable r)
        {
            if (!resettables.Contains(r)) resettables.Add(r);
        }

        public static void RegisterLoopObject(LoopObject o)
        {
            if (!loopObjects.Contains(o)) loopObjects.Add(o);
        }

        /// <summary>主装置接触：推进循环（L4时不再推进）。</summary>
        public bool AdvanceLoop()
        {
            if (transitioning || LoopCount >= MaxLoop) return false;
            transitioning = true;
            StartCoroutine(DoAdvance());
            return true;
        }

        IEnumerator DoAdvance()
        {
            if (Audio.AudioDirector.Instance != null) Audio.AudioDirector.Instance.Stinger();
            if (Hud.Instance != null)
            {
                yield return Hud.Instance.Fade(1f, 1.0f);
                yield return Hud.Instance.ShowNarration(StoryTexts.LoopNarration(LoopCount + 1), 2.6f);
            }
            // 淡黑屏期间锁定玩家输入，防止在切换过程中移动
            Player.InputLock.Active = true;
            ApplyReset();
            if (Hud.Instance != null) yield return Hud.Instance.Fade(0f, 1.0f);
            Player.InputLock.Active = false;
            transitioning = false;
        }

        /// <summary>立即执行循环推进核心流程（演出之外，供流程与测试复用）。</summary>
        public void ApplyReset()
        {
            LoopCount = Mathf.Min(LoopCount + 1, MaxLoop);
            SaveManager.Data.loop = LoopCount;

            foreach (var r in resettables) r.OnLoopReset(LoopCount);
            ApplyAllStates(LoopCount);
            TeleportPlayer();

            SaveManager.Save();
            ObjectiveManager.Instance?.ApplyLoop(LoopCount);
            if (Hud.Instance != null)
            {
                Hud.Instance.SetLoop(LoopCount);
                Hud.Instance.Toast("第 " + LoopCount + " 次循环。");
            }
        }

        void ApplyAllStates(int loop)
        {
            foreach (var o in loopObjects) o.Apply(loop);
        }

        void TeleportPlayer()
        {
            var spawn = spawnPoint != null ? spawnPoint : GameObject.Find("PlayerSpawn")?.transform;
            if (spawn == null) return;
            var pc = FindObjectOfType<Player.PlayerController>();
            if (pc != null) pc.TeleportTo(spawn.position, spawn.rotation);
        }

        /// <summary>新游戏：清存档、回循环1、清背包、全量复位。</summary>
        public void NewGame()
        {
            SaveManager.DeleteSave();
            LoopCount = 1;

            if (Inventory.Inventory.Instance != null) Inventory.Inventory.Instance.Clear();
            foreach (var r in resettables) r.OnLoopReset(1);
            ApplyAllStates(1);
            foreach (var p in FindObjectsOfType<Interaction.Pickupable>(true))
                p.RestoreUnpicked();
            TeleportPlayer();

            ObjectiveManager.Instance?.ApplyLoop(1);
            if (Hud.Instance != null) Hud.Instance.SetLoop(1);
        }
    }
}
