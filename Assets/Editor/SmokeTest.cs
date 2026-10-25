using System;
using System.Collections.Generic;
using System.IO;
using Door13.Interaction;
using Door13.Inventory;
using Door13.Puzzle;
using Door13.Save;
using Door13.Story;
using Door13.TimeLoop;
using Door13.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Door13.EditorTools
{
    /// <summary>
    /// batchmode 全流程验收测试（不要加 -quit）。
    /// 用法：Unity -batchmode -nographics -projectPath … -executeMethod Door13.EditorTools.SmokeTest.Run
    ///
    /// 剧本驱动：依次执行步骤（动作+等待条件+超时），覆盖——
    /// 新游戏 → 保险箱1998 → 门禁卡 → 配电箱/监控台 → 实验室(调频/组合/冷冻柜) →
    /// 档案室(409/排序/314) → 控制中心(断路器/登录0403) → 地下(阀门/加油/发电机) →
    /// 循环2/3/4（世界变化断言+碎片）→ 十三号钥匙 → 十三门 → 终局选择 → 隐藏结局 → 存档往返。
    /// 任何异常/断言失败/超时 → 非零码退出。
    /// </summary>
    [InitializeOnLoad]
    public static class SmokeTest
    {
        struct Step
        {
            public string Name;
            public Action Act;
            public Func<bool> Done;
            public float Timeout;
        }

        static int stepIdx = -1;
        static float stepStart;
        static float startTime;
        static bool finished;
        static readonly List<string> errors = new List<string>();
        static List<Step> steps;

        static SmokeTest()
        {
            if (SessionState.GetBool("SMOKE_RUNNING", false))
                Subscribe();
        }

        static void Subscribe()
        {
            startTime = (float)EditorApplication.timeSinceStartup;
            Application.logMessageReceived += OnLog;
            EditorApplication.update += OnUpdate;
        }

        public static void Run()
        {
            string full = Path.Combine(
                Directory.GetParent(Application.dataPath).FullName,
                ProjectSetup.ScenePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(full))
            {
                Debug.LogError("SMOKE_FAIL 场景不存在，请先执行 SetupOnly: " + ProjectSetup.ScenePath);
                EditorApplication.Exit(3);
                return;
            }

            EditorSceneManager.OpenScene(ProjectSetup.ScenePath);
            SessionState.SetBool("SMOKE_RUNNING", true);
            Subscribe();
            Debug.Log("SMOKE_START 全流程验收测试开始…");
            EditorApplication.isPlaying = true;
        }

        static void OnLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
                if (errors.Count < 50) errors.Add(condition);
        }

        static void OnUpdate()
        {
            float secs = (float)(EditorApplication.timeSinceStartup - startTime);

            if (!EditorApplication.isPlaying)
            {
                if (secs > 25f && !SessionState.GetBool("SMOKE_PLAYED", false))
                {
                    Debug.LogError("SMOKE_FAIL 播放模式未能启动");
                    Finish(secs);
                }
                return;
            }

            if (steps == null) BuildSteps();

            if (stepIdx < 0)
            {
                NextStep(secs);
                return;
            }

            if (stepIdx >= steps.Count)
            {
                Debug.Log("SMOKE_FUNC 全部步骤通过");
                Finish(secs);
                return;
            }

            var s = steps[stepIdx];
            try
            {
                if (s.Done())
                {
                    Debug.Log("SMOKE_FUNC ✔ " + s.Name);
                    NextStep(secs);
                    return;
                }
            }
            catch (Exception e)
            {
                errors.Add("SMOKE_CHECK 步骤异常 [" + s.Name + "]: " + e);
                Finish(secs);
                return;
            }

            if (secs - stepStart > s.Timeout)
            {
                errors.Add("SMOKE_CHECK 步骤超时: " + s.Name);
                Finish(secs);
            }
        }

        static void NextStep(float secs)
        {
            stepIdx++;
            if (stepIdx >= steps.Count)
            {
                Debug.Log("SMOKE_FUNC 全部步骤通过");
                Finish(secs);
                return;
            }
            stepStart = secs;
            var s = steps[stepIdx];
            try { s.Act?.Invoke(); }
            catch (Exception e)
            {
                errors.Add("SMOKE_CHECK 执行异常 [" + s.Name + "]: " + e);
                Finish(secs);
            }
        }

        // ---------- 对象查找 ----------

        static T Find<T>(string goName) where T : Component
        {
            var go = GameObject.Find(goName);
            return go != null ? go.GetComponent<T>() : null;
        }

        static void Pick(string goName, string itemId)
        {
            var go = GameObject.Find(goName);
            var p = go != null ? go.GetComponent<Pickupable>() : null;
            if (p == null) throw new Exception("找不到拾取物: " + goName);
            p.Interact(null);
        }

        static LoopManager LM { get { return LoopManager.Instance; } }
        static Door13.Inventory.Inventory Inv { get { return Door13.Inventory.Inventory.Instance; } }

        static bool Has(string id) { return Inv != null && Inv.Has(id); }

        // ---------- 剧本 ----------

        static void BuildSteps()
        {
            steps = new List<Step>
            {
                // ===== 主菜单：机位/输入锁/HUD绑定 =====
                new Step{ Name="主菜单机位与HUD绑定", Timeout=5,
                    Act=()=>{
                        if(UI.MainMenu.Instance==null) return; // MainMenu.Start 未跑完，下帧再验
                        if(!Player.InputLock.Active) throw new Exception("主菜单未锁定输入");
                        var hud = UI.Hud.Instance; if(hud==null) throw new Exception("缺HUD");
                        hud.SetPrompt("测试");
                        if(hud.PromptText!="测试") throw new Exception("HUD提示绑定失败(SafeRoot深度查找回归)");
                        if(GameObject.Find("JoystickBase")!=null) throw new Exception("主菜单期间摇杆应隐藏");
                    },
                    Done=()=> Player.InputLock.Active && UI.MainMenu.Instance!=null },
                // ===== 序章：苏醒室 =====
                new Step{ Name="新游戏", Timeout=5,
                    Act=()=>{ SaveManager.DeleteSave();
                        if(UI.MainMenu.Instance!=null) UI.MainMenu.Instance.NewGame(); else LM.NewGame(); },
                    Done=()=> LM!=null && LM.LoopCount==1 && !GameFlags.Has("safe_open")
                          && !Player.InputLock.Active && GameObject.Find("JoystickBase")!=null },
                new Step{ Name="字体覆盖断言(含未激活面板+TextMesh)", Timeout=5,
                    Act=()=>{
                        var bundled = UnityEngine.Resources.Load<Font>("DefaultFont");
                        if (bundled == null) throw new Exception("随包字体 Resources/DefaultFont.ttf 加载失败");
                        if (!bundled.HasCharacter('门')) throw new Exception("随包字体缺中文字形");
                        var texts = UnityEngine.Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                        if (texts.Length < 50) throw new Exception("Text数量异常: " + texts.Length);
                        foreach (var t in texts)
                            if (t.font == null) throw new Exception("Text缺字体: " + t.name);
                            else if (t.font.name != "DefaultFont") throw new Exception("Text字体非随包资产: " + t.name + " → " + t.font.name);
                        var meshes = UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                        foreach (var tm in meshes)
                            if (tm.font == null) throw new Exception("TextMesh缺字体: " + tm.name);
                            else if (tm.font.name != "DefaultFont") throw new Exception("TextMesh字体非随包资产: " + tm.name + " → " + tm.font.name);
                        Debug.Log("FONT_CHECK Text=" + texts.Length + " TextMesh=" + meshes.Length + " 全部指向随包字体");
                    },
                    Done=()=> true },
                new Step{ Name="保险箱开面板", Timeout=5,
                    Act=()=>{ var s=Find<SafeInteractable>("SafePivot"); if(s==null) throw new Exception("缺保险箱"); s.Interact(null); },
                    Done=()=> KeypadPanel.Instance!=null && KeypadPanel.Instance.gameObject.activeSelf },
                new Step{ Name="输入1998", Timeout=5,
                    Act=()=> KeypadPanel.Instance.EnterCode("1998"),
                    Done=()=> GameFlags.Has("safe_open") },
                new Step{ Name="门禁卡射线可达(回归:不得被箱体遮挡)", Timeout=5,
                    Act=()=>{
                        var kc = GameObject.Find("Keycard");
                        if (kc == null || !kc.activeSelf) throw new Exception("门禁卡未显现");
                        var pick = kc.GetComponent<Pickupable>();
                        var pc = UnityEngine.Object.FindObjectOfType<Player.PlayerController>();
                        if (pc == null) throw new Exception("缺玩家");
                        // 传送到保险箱正前方（真实站立位置），复刻 PlayerInteraction 的球形射线
                        pc.TeleportTo(RoomBuilder.V(-1.1f, 0.05f, -1.8f), Quaternion.LookRotation(Vector3.forward));
                        var cam = pc.GetComponentInChildren<Camera>();
                        if (cam == null) throw new Exception("缺玩家相机");
                        var dir = (kc.transform.position - cam.transform.position).normalized;
                        var hits = Physics.SphereCastAll(cam.transform.position, 0.3f, dir, 3.2f);
                        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                        Interactable first = null;
                        foreach (var h in hits)
                        {
                            var it = h.collider.GetComponentInParent<Interactable>();
                            if (it != null) { first = it; break; }
                        }
                        if (first != pick) throw new Exception(
                            "拾取射线未首先命中门禁卡（命中=" + (first != null ? first.name : "无可交互物") + "）");
                    },
                    Done=()=> true },
                new Step{ Name="拾取门禁卡", Timeout=5,
                    Act=()=> Pick("Keycard","keycard"),
                    Done=()=> Has("keycard") },
                new Step{ Name="电脑文件系统(白底文档+证据)", Timeout=5,
                    Act=()=>{ var c=Find<ComputerTerminal>("TerminalScreen"); c.Interact(null);
                        if(ComputerPanel.Instance==null || !ComputerPanel.Instance.gameObject.activeSelf) throw new Exception("电脑面板未打开");
                        ComputerPanel.Instance.OpenFile(1);
                        if(!GameFlags.Has("ev_terminal_r0")) throw new Exception("文件证据未登记");
                        ComputerPanel.Instance.Close(); },
                    Done=()=> GameFlags.Has("ev_terminal_r0") },
                new Step{ Name="R0门开", Timeout=5,
                    Act=()=>{ var d=Find<DoorInteractable>("Door_R0"); d.Interact(null); },
                    Done=()=> Find<DoorInteractable>("Door_R0").IsOpen },

                // ===== 回廊 =====
                new Step{ Name="配电箱合闸", Timeout=5,
                    Act=()=>{ var p=Find<PuzzleInteractable>("Breaker"); p.SolveNow(); },
                    Done=()=> GameFlags.IsSolved("breaker_r1") },
                new Step{ Name="走廊灯与V2卡显现", Timeout=5,
                    Act=()=>{ var r=GameObject.Find("BreakerReward"); if(r==null||!r.activeSelf) throw new Exception("奖励未显现"); },
                    Done=()=> GameObject.Find("BreakerReward")!=null && GameObject.Find("BreakerReward").activeSelf },
                new Step{ Name="拾取V2卡", Timeout=5,
                    Act=()=> Pick("V2Card","v2card"),
                    Done=()=> Has("v2card") },
                new Step{ Name="监控台密码1204", Timeout=5,
                    Act=()=>{ var p=Find<PuzzleInteractable>("MonitorDesk"); p.SolveNow(); },
                    Done=()=> GameFlags.IsSolved("monitor_r1") && GameObject.Find("TapeAReward").activeSelf },
                new Step{ Name="拾取磁带A", Timeout=5,
                    Act=()=> Pick("TapeA","tape_a"),
                    Done=()=> Has("tape_a") },

                // ===== 实验室 =====
                new Step{ Name="实验室门(V2卡)", Timeout=5,
                    Act=()=>{ var d=Find<DoorInteractable>("Door_R2"); d.Interact(null); },
                    Done=()=> Find<DoorInteractable>("Door_R2").IsOpen },
                new Step{ Name="收音机调频91.3", Timeout=5,
                    Act=()=>{ var p=Find<PuzzleInteractable>("Radio"); p.SolveNow(); },
                    Done=()=> GameFlags.IsSolved("radio_r2") },
                new Step{ Name="组合试剂→溶解剂", Timeout=5,
                    Act=()=>{ Inv.Add("reagent_a"); Inv.Add("reagent_b");
                        InventoryUI.Instance.TapItem("reagent_a"); InventoryUI.Instance.TapItem("reagent_b"); },
                    Done=()=> Has("solvent") },
                new Step{ Name="冷冻柜使用溶解剂", Timeout=5,
                    Act=()=>{ var u=Find<ItemSpotUse>("Freezer"); u.Interact(null); },
                    Done=()=> GameFlags.Has("freezer_r2") },
                new Step{ Name="拾取档案权限卡", Timeout=5,
                    Act=()=> Pick("ArchiveCard","archive_card"),
                    Done=()=> Has("archive_card") },

                // ===== 档案室 =====
                new Step{ Name="档案室门(权限卡)", Timeout=5,
                    Act=()=>{ var d=Find<DoorInteractable>("Door_R3"); d.Interact(null); },
                    Done=()=> Find<DoorInteractable>("Door_R3").IsOpen },
                new Step{ Name="档案柜409", Timeout=5,
                    Act=()=>{ var p=Find<PuzzleInteractable>("Cabinet"); p.SolveNow(); },
                    Done=()=> GameFlags.IsSolved("cabinet_r3") && GameObject.Find("CabinetLoot").activeSelf },
                new Step{ Name="照片排序→暗格", Timeout=5,
                    Act=()=>{ var p=Find<PuzzleInteractable>("PhotoBoardR3"); p.SolveNow(); },
                    Done=()=> GameFlags.IsSolved("photos_r3") },
                new Step{ Name="拾取碎片壹", Timeout=5,
                    Act=()=> Pick("Shard1","shard_1"),
                    Done=()=> Has("shard_1") },
                new Step{ Name="书架314保险柜", Timeout=5,
                    Act=()=>{ var p=Find<PuzzleInteractable>("SafeR3"); p.SolveNow(); },
                    Done=()=> GameFlags.IsSolved("safe_r3") && GameObject.Find("SafeLootR3").activeSelf },
                new Step{ Name="R3战利品射线可达(回归:不得嵌在柜体内)", Timeout=5,
                    Act=()=>{
                        var torch = GameObject.Find("Flashlight");
                        if (torch == null || !torch.activeSelf) throw new Exception("手电筒未显现");
                        var pick = torch.GetComponent<Pickupable>();
                        var pc = UnityEngine.Object.FindObjectOfType<Player.PlayerController>();
                        if (pc == null) throw new Exception("缺玩家");
                        pc.TeleportTo(RoomBuilder.V(4.2f, 0.05f, -5.2f), Quaternion.LookRotation(Vector3.back));
                        var cam = pc.GetComponentInChildren<Camera>();
                        if (cam == null) throw new Exception("缺玩家相机");
                        var dir = (torch.transform.position - cam.transform.position).normalized;
                        var hits = Physics.SphereCastAll(cam.transform.position, 0.3f, dir, 3.2f);
                        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                        Interactable first = null;
                        foreach (var h in hits)
                        {
                            var it = h.collider.GetComponentInParent<Interactable>();
                            if (it != null) { first = it; break; }
                        }
                        if (first != pick) throw new Exception(
                            "拾取射线未首先命中手电筒（命中=" + (first != null ? first.name : "无可交互物") + "）");
                    },
                    Done=()=> true },
                new Step{ Name="拾取手电筒+钥匙卡", Timeout=5,
                    Act=()=>{ Pick("Flashlight","flashlight"); Pick("CtrlCard","ctrl_card"); },
                    Done=()=> Has("flashlight") && Has("ctrl_card") && GameFlags.Has(GameFlags.Flashlight) },

                // ===== 控制中心 =====
                new Step{ Name="控制中心门", Timeout=5,
                    Act=()=>{ var d=Find<DoorInteractable>("Door_R4"); d.Interact(null); },
                    Done=()=> Find<DoorInteractable>("Door_R4").IsOpen },
                new Step{ Name="主断路器合闸", Timeout=5,
                    Act=()=>{ var p=Find<PuzzleInteractable>("BreakerR4"); p.SolveNow(); },
                    Done=()=> GameFlags.Has(GameFlags.MainPower) },
                new Step{ Name="控制台开登录面板", Timeout=5,
                    Act=()=>{ var c=Find<ConsoleTerminal>("Console"); c.Interact(null); },
                    Done=()=> KeypadPanel.Instance.gameObject.activeSelf },
                new Step{ Name="登录0403", Timeout=5,
                    Act=()=> KeypadPanel.Instance.EnterCode("0403"),
                    Done=()=> GameFlags.Has("subway_unlock") && GameFlags.Has("ev_console") },
                new Step{ Name="关闭日志+地下通道门", Timeout=5,
                    Act=()=>{ TerminalPanel.Instance.Close(); var d=Find<DoorInteractable>("Door_R5"); d.Interact(null); },
                    Done=()=> Find<DoorInteractable>("Door_R5").IsOpen },

                // ===== 地下区域 =====
                new Step{ Name="阀门压力2/1/3", Timeout=5,
                    Act=()=>{ var p=Find<PuzzleInteractable>("ValveRig"); p.SolveNow(); },
                    Done=()=> GameFlags.IsSolved("valves_r5") },
                new Step{ Name="组合燃料", Timeout=5,
                    Act=()=>{ Inv.Add("fuel_can"); Inv.Add("fuel");
                        InventoryUI.Instance.TapItem("fuel_can"); InventoryUI.Instance.TapItem("fuel"); },
                    Done=()=> Has("mix_fuel") },
                new Step{ Name="发电机加油", Timeout=5,
                    Act=()=>{ var u=Find<ItemSpotUse>("FuelSpot"); u.Interact(null); },
                    Done=()=> GameFlags.Has("gen_fueled") },
                new Step{ Name="拉启动索", Timeout=5,
                    Act=()=>{ var g=Find<GeneratorStart>("PullCord"); g.Interact(null); },
                    Done=()=> GameFlags.Has(GameFlags.DeviceReady) },

                // ===== 循环2 =====
                new Step{ Name="接触装置→循环2", Timeout=25,
                    Act=()=>{ var d=Find<DeviceInteractable>("DeviceSpot"); d.Interact(null); },
                    Done=()=> LM.LoopCount>=2 },
                new Step{ Name="循环2世界变化断言+碎片贰", Timeout=10,
                    Act=()=>{
                        var wt=GameObject.Find("WallText_Room_R0");
                        var tm=wt!=null?wt.GetComponent<TextMesh>():null;
                        if(tm==null||!tm.text.Contains("13")) throw new Exception("墙字未变13次: "+(tm!=null?tm.text:"null"));
                        var fig=GameObject.Find("Photo_Figure");
                        if(fig==null||!fig.activeSelf) throw new Exception("照片人影未出现");
                        var s2=GameObject.Find("Shard2");
                        if(s2==null||!s2.activeSelf) throw new Exception("碎片贰未出现");
                        Pick("Shard2","shard_2");
                    },
                    Done=()=> Has("shard_2") },

                new Step{ Name="循环2演出结束", Timeout=25,
                    Done=()=> LM==null || !LM.transitioning },
                // ===== 循环3 =====
                new Step{ Name="接触装置→循环3", Timeout=25,
                    Act=()=>{ var d=Find<DeviceInteractable>("DeviceSpot"); d.Interact(null); },
                    Done=()=> LM.LoopCount>=3 },
                new Step{ Name="循环3断言+碎片叁", Timeout=10,
                    Act=()=>{
                        var wt=GameObject.Find("WallText_Room_R0");
                        var tm=wt!=null?wt.GetComponent<TextMesh>():null;
                        if(tm==null||!tm.text.Contains("别再数了")) throw new Exception("墙字未变(循环3)");
                        var s3=GameObject.Find("Shard3");
                        if(s3==null||!s3.activeSelf) throw new Exception("碎片叁未出现");
                        Pick("Shard3","shard_3");
                    },
                    Done=()=> Has("shard_3") },

                new Step{ Name="循环3演出结束", Timeout=25,
                    Done=()=> LM==null || !LM.transitioning },
                // ===== 循环4 + 终局 =====
                new Step{ Name="接触装置→循环4", Timeout=25,
                    Act=()=>{ var d=Find<DeviceInteractable>("DeviceSpot"); d.Interact(null); },
                    Done=()=> LM.LoopCount>=4 },
                new Step{ Name="组合十三号钥匙", Timeout=5,
                    Act=()=>{ InventoryUI.Instance.TapItem("shard_1"); InventoryUI.Instance.TapItem("shard_2");
                        InventoryUI.Instance.TapItem("shard_12"); InventoryUI.Instance.TapItem("shard_3"); },
                    Done=()=> Has("master_key") },
                new Step{ Name="十三门开启", Timeout=5,
                    Act=()=>{ var d=Find<DoorInteractable>("Door_13"); d.Interact(null); },
                    Done=()=> Find<DoorInteractable>("Door_13").IsOpen },
                new Step{ Name="终局选择·证据不足被拒", Timeout=5,
                    Act=()=>{ var f=Find<FinalTerminal>("FinalTerminalBox"); f.Interact(null);
                        EndingManager.Choose(2); },
                    Done=()=> Hud.Instance!=null && !Hud.Instance.GameOver },
                new Step{ Name="集齐证据→进入容器室", Timeout=15,
                    Act=()=>{
                        string[] evs={"ev_terminal_r0","ev_log_lab","ev_card_t13","ev_birth","ev_death","ev_tape_a"};
                        foreach(var e in evs) GameFlags.SetOnce(e);
                        var f=Find<FinalTerminal>("FinalTerminalBox"); f.Interact(null);
                        EndingManager.Choose(2);
                    },
                    Done=()=> { var pc=UnityEngine.Object.FindObjectOfType<Player.PlayerController>();
                        return pc!=null && Vector3.Distance(pc.transform.position, new Vector3(6f,0.05f,-35f))<3f; } },
                new Step{ Name="打开容器→真结局", Timeout=8,
                    Act=()=>{ var c=Find<ContainerTerminal>("ContainerTerminal"); c.Interact(null); },
                    Done=()=> Hud.Instance!=null && Hud.Instance.GameOver && GameFlags.Has("ending_4") },

                // ===== 存档往返 =====
                new Step{ Name="存档往返校验", Timeout=5,
                    Act=()=>{ SaveManager.Save(); SaveManager.Load(); Debug.Log("ROUNDTRIP loop=" + SaveManager.Data.loop + " items=" + SaveManager.Data.items.Count + " flags=" + SaveManager.Data.flags.Count + " path=" + Application.persistentDataPath); },
                    Done=()=> SaveManager.Data.loop==4 && SaveManager.Data.items.Count>=7 },

                new Step{ Name="结局1可达性(装置确认流)", Timeout=5,
                    Act=()=>{ var d=Find<DeviceInteractable>("DeviceSpot");
                        d.Interact(null); // 第一次：确认提示
                        if(!d.DisplayPrompt.Contains("无法回头")) throw new Exception("L4确认流未触发");
                        // 不二次确认，避免污染结局4状态；仅验证提示
                    },
                    Done=()=> true },
            };
        }

        static void Finish(float secs)
        {
            if (finished) return;
            finished = true;

            SessionState.SetBool("SMOKE_RUNNING", false);
            SessionState.SetBool("SMOKE_PLAYED", false);
            Application.logMessageReceived -= OnLog;

            int passed = Mathf.Max(0, stepIdx);
            int total = steps != null ? steps.Count : 0;
            Debug.Log($"SMOKE_RESULT steps_passed={passed}/{total} seconds={secs:F1} errors={errors.Count}");
            foreach (var e in errors.ToArray())
                Debug.LogError("SMOKE_ERROR: " + e);
            EditorApplication.Exit(errors.Count > 0 ? 1 : 0);
        }
    }
}
