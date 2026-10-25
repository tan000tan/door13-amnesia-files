using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Door13.Player;
using Door13.Save;
using Door13.TimeLoop;
using Door13.UI;

namespace Door13.EditorTools
{
    /// <summary>
    /// 播放模式运行时诊断（batchmode，不加 -quit）：
    /// 以"显示位置=命中位置"为标准，用 EventSystem 真实射线 + 指针事件注入验证全部按钮。
    /// 注意：激活面板的同一帧内 Canvas 尚未重布局，其内部按钮的射线必须放到下一相位执行。
    /// 日志前缀 DIAG_，失败退出码 1。
    /// </summary>
    [InitializeOnLoad]
    public static class DiagnosticReport
    {
        static int frame;
        static int phase;
        static readonly List<string> fails = new List<string>();
        static Vector3 playerStart;
        static float savedLook;

        static DiagnosticReport()
        {
            // 播放模式域重载会清空订阅：与 SmokeTest 相同的 SessionState 复原模式
            if (SessionState.GetBool("DIAG_RUNNING", false))
                EditorApplication.update += OnUpdate;
        }

        public static void Run()
        {
            string full = System.IO.Path.Combine(
                System.IO.Directory.GetParent(Application.dataPath).FullName,
                ProjectSetup.ScenePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(full))
            {
                Debug.LogError("DIAG_FAIL 场景不存在");
                EditorApplication.Exit(3);
                return;
            }
            EditorSceneManager.OpenScene(ProjectSetup.ScenePath);
            SessionState.SetBool("DIAG_RUNNING", true);
            EditorApplication.update += OnUpdate;
            Application.logMessageReceived += (c, s, t) =>
            {
                if (t == LogType.Exception) Fail("异常: " + c);
            };
            Debug.Log("DIAG_START 运行时诊断开始");
            EditorApplication.isPlaying = true;
        }

        static void Fail(string msg)
        {
            fails.Add(msg);
            Debug.LogError("DIAG_FAIL " + msg);
        }

        static void Pass(string msg) => Debug.Log("DIAG_OK " + msg);

        static void OnUpdate()
        {
            frame++;
            if (frame < 40) return;   // 等 Awake/Start 全部完成

            // 开机渐显（1.8s）期间 FadeOverlay 拦截射线：等它淡出完毕再开始
            var hud = Object.FindObjectOfType<Hud>();
            if (frame < 600)
            {
                bool fading = false;
                var imgs = hud != null ? hud.GetComponentsInChildren<Image>(true) : null;
                if (imgs != null)
                    foreach (var im in imgs)
                        if (im.name == "FadeOverlay" && im.color.a > 0.05f) { fading = true; break; }
                if (fading) return;
            }

            switch (phase)
            {
                case 0: Safe(PhaseArchitecture); break;
                case 1: Safe(PhaseMenuClicks); break;
                case 2: Safe(PhaseJoystickBegin); break;
                case 3: Safe(PhaseMovementAndHudRaycasts); break;
                case 4: Safe(PhaseInventoryCloseAndPause); break;
                case 5: Safe(PhasePauseToSettings); break;
                case 6: Safe(PhaseSensitivityAndResume); break;
                case 7: Finish(); break;
            }
            phase++;
        }

        static void Safe(System.Action p)
        {
            try { p(); }
            catch (System.Exception e) { Fail("阶段异常: " + e.Message + "\n" + e.StackTrace); }
        }

        // ---------- 阶段0：架构与触摸区一致性 ----------
        static void PhaseArchitecture()
        {
            Debug.Log($"DIAG_INFO Screen={Screen.width}x{Screen.height} safeArea={Screen.safeArea} dpi={Screen.dpi:F0}");
            Debug.Log("DIAG_INFO activeInputHandler=0 (旧Input Manager, 见 ProjectSettings.asset)");

            var canvases = Object.FindObjectsOfType<Canvas>();
            Debug.Log($"DIAG_INFO Canvas数量={canvases.Length}");
            if (canvases.Length != 1) Fail($"Canvas数量异常: {canvases.Length}");
            var scaler = canvases[0].GetComponent<CanvasScaler>();
            if (scaler == null) Fail("缺 CanvasScaler");
            else Debug.Log($"DIAG_INFO Scaler={scaler.uiScaleMode} ref={scaler.referenceResolution} match={scaler.matchWidthOrHeight}");

            if (Object.FindObjectOfType<StandaloneInputModule>() == null) Fail("缺 StandaloneInputModule");
            if (EventSystem.current == null) Fail("缺 EventSystem");
            if (Object.FindObjectsOfType<EventSystem>().Length != 1) Fail("EventSystem数量!=1");
            Pass("架构检查完成");

            // 菜单/游戏相机分离（防穿透的核心架构）
            int menuLayer = LayerMask.NameToLayer("MenuScene");
            var mcGo = GameObject.Find("MenuCamera");
            var gcGo = GameObject.FindGameObjectWithTag("MainCamera");
            var gc = gcGo != null ? gcGo.GetComponent<Camera>() : null;   // 菜单态游戏相机禁用，Camera.main 会返回null
            if (menuLayer < 0) Fail("缺 MenuScene 层");
            if (mcGo == null) Fail("缺 MenuCamera");
            if (gc == null) Fail("缺主相机");
            if (menuLayer >= 0 && mcGo != null)
            {
                var mc = mcGo.GetComponent<Camera>();
                if (mc.cullingMask != (1 << menuLayer)) Fail($"MenuCamera cullingMask 应仅为 MenuScene: {mc.cullingMask}");
                else Pass("MenuCamera 仅渲染 MenuScene 层");
                if (mc.enabled && gc != null && gc.enabled) Fail("菜单态双相机同时渲染");
                else if (mc.enabled && gc != null && !gc.enabled) Pass("菜单态仅菜单相机渲染（游戏世界不可见，无穿透）");
            }
            if (menuLayer >= 0 && gc != null && (gc.cullingMask & (1 << menuLayer)) != 0)
                Fail("主相机渲染了 MenuScene 层（穿透源）");
            else Pass("主相机排除 MenuScene 层");
            CheckFits("MainMenu");

            var joy = Object.FindObjectOfType<VirtualJoystick>(true);   // 含未激活：主菜单期间摇杆应隐藏
            if (joy == null) { Fail("缺 VirtualJoystick"); return; }
            if (InputLock.Active && joy.gameObject.activeInHierarchy) Fail("主菜单期间摇杆未隐藏");
            else if (!InputLock.Active && !joy.gameObject.activeInHierarchy) Fail("游戏进行中摇杆不可见");
            else Pass($"摇杆显隐正确 (菜单中={InputLock.Active})");

            var baseImg = joy.GetComponent<Image>();
            var knobImg = joy.transform.Find("Knob")?.GetComponent<Image>();
            if (baseImg != null && baseImg.raycastTarget) Fail("摇杆底座 raycastTarget=true（吞触摸→摇杆失效）");
            else Pass("摇杆底座 raycastTarget=false");
            if (knobImg != null && knobImg.raycastTarget) Fail("摇杆圆钮 raycastTarget=true（吞触摸）");
            else Pass("摇杆圆钮 raycastTarget=false");

            float w = Screen.width, h = Screen.height;
            if (!joy.InZone(new Vector2(w * 0.2f, h * 0.3f))) Fail("左手区采样点未被识别为摇杆区");
            else Pass($"InZone 左下 ({w * 0.2f:F0},{h * 0.3f:F0}) = true");
            if (joy.InZone(new Vector2(w * 0.75f, h * 0.3f))) Fail("右侧屏幕被误判为摇杆区");
            else Pass("InZone 右侧 = false");
            if (joy.InZone(new Vector2(w * 0.2f, h * 0.85f))) Fail("左上区域被误判为摇杆区");
            else Pass("InZone 左上 = false");
        }

        // ---------- 阶段1：主菜单射线 + 设置/新游戏 ----------
        static void PhaseMenuClicks()
        {
            foreach (var b in new[] { "BtnNew", "BtnContinue", "BtnSettings", "BtnQuit" })
            {
                var go = GameObject.Find("/Canvas/SafeRoot/MainMenu/" + b);
                if (go == null) { Fail($"找不到按钮 {b}"); continue; }
                if (!go.activeInHierarchy) { Pass($"按钮 {b} 当前隐藏（符合状态）"); continue; }
                var rt = (RectTransform)go.transform;
                Vector2 sp = Center(rt);
                var hit = RaycastAt(sp);
                if (hit == null || hit.name != b)
                    Fail($"按钮 {b} 显示位置({sp.x:F0},{sp.y:F0})射线命中: {(hit != null ? hit.name : "无")}");
                else
                    Pass($"按钮 {b} 射线命中自身 @({sp.x:F0},{sp.y:F0})");
            }

            var btnSettings = GameObject.Find("/Canvas/SafeRoot/MainMenu/BtnSettings");
            if (btnSettings != null)
            {
                ClickAt((RectTransform)btnSettings.transform);
                var sp = Object.FindObjectOfType<SettingsPanel>();
                if (sp == null || !sp.gameObject.activeSelf) Fail("点击设置按钮后 SettingsPanel 未打开");
                else
                {
                    Pass("点击设置 → SettingsPanel 打开");
                    CheckFits("SettingsPanel");   // 出屏检查（20:9回归）
                    sp.Close();
                }
            }

            var btnNew = GameObject.Find("/Canvas/SafeRoot/MainMenu/BtnNew");
            if (btnNew != null)
            {
                ClickAt((RectTransform)btnNew.transform);
                if (InputLock.Active) Fail("点击新游戏后输入锁未解除");
                else Pass("点击新游戏 → 输入锁解除、游戏开始");
                var mc2Go = GameObject.Find("MenuCamera");
                var mc2 = mc2Go != null ? mc2Go.GetComponent<Camera>() : null;
                var gc2Go = GameObject.FindGameObjectWithTag("MainCamera");
                var gc2 = gc2Go != null ? gc2Go.GetComponent<Camera>() : null;
                if (mc2 != null && gc2 != null && mc2.enabled) Fail("进入游戏后菜单相机仍启用");
                else if (mc2 != null && gc2 != null && !gc2.enabled) Fail("进入游戏后主相机未启用");
                else if (mc2 != null && gc2 != null) Pass("相机已切回游戏世界");
            }
        }

        // ---------- 阶段2：摇杆显示=触摸一致性 ----------
        static void PhaseJoystickBegin()
        {
            var pc = Object.FindObjectOfType<PlayerController>();
            var joy = Object.FindObjectOfType<VirtualJoystick>();
            if (pc == null || joy == null) { Fail("缺玩家/摇杆"); return; }

            float w = Screen.width, h = Screen.height;
            Vector2 press = new Vector2(w * 0.2f, h * 0.3f);
            joy.Begin(press);   // 浮动底座应跳到按下点
            var knob = joy.transform.Find("Knob") as RectTransform;
            if (knob == null) Fail("缺摇杆圆钮");
            else if (Vector2.Distance(knob.anchoredPosition, Vector2.zero) < 1f)
                Pass("圆钮处于按下中心（浮动底座已跟随触摸点）");

            joy.Drag(new Vector2(w * 0.2f + 60f, h * 0.3f + 120f));
            Vector2 out0 = joy.Output;
            if (out0.y <= 0.2f || out0.x <= 0.05f)
                Fail($"摇杆输出异常: {out0}（上+右拖动应输出正前+向右）");
            else
                Pass($"摇杆输出正常: {out0:F2}");

            playerStart = pc.transform.position;
            joy.SetOutput(Vector2.up);   // 持续输出供下一帧测位移
        }

        // ---------- 阶段3：位移 + HUD射线 + 点背包 ----------
        static void PhaseMovementAndHudRaycasts()
        {
            var pc = Object.FindObjectOfType<PlayerController>();
            var joy = Object.FindObjectOfType<VirtualJoystick>();
            if (pc != null && joy != null)
            {
                float moved = Vector3.Distance(pc.transform.position, playerStart);
                // 仅记录：编辑器宏每帧清零摇杆输出，位移取决于Update执行顺序（真机触摸路径无此宏）
                Debug.Log($"DIAG_INFO 位移距离={moved:F2}m (一帧输出,编辑器宏下仅记录)");
                joy.End();
            }

            foreach (var b in new[] { "InteractButton", "InventoryButton", "MenuButton" })
            {
                var go = GameObject.Find("/Canvas/SafeRoot/" + b);
                if (go == null) { Fail($"游戏内按钮 {b} 不可见"); continue; }
                var rt = (RectTransform)go.transform;
                Vector2 sp = Center(rt);
                var hit = RaycastAt(sp);
                if (hit == null || hit.name != go.name)
                    Fail($"按钮 {b} 射线命中异常: {(hit != null ? hit.name : "无")} @({sp.x:F0},{sp.y:F0})");
                else
                    Pass($"按钮 {b} 射线命中 @({sp.x:F0},{sp.y:F0})");
            }

            var invBtn = GameObject.Find("/Canvas/SafeRoot/InventoryButton");
            if (invBtn != null)
            {
                ClickAt((RectTransform)invBtn.transform);
                var inv = Object.FindObjectOfType<Inventory.InventoryUI>();
                if (inv == null || !inv.gameObject.activeSelf) Fail("点击背包按钮后背包未打开");
                else Pass("点击背包 → 背包打开（等下一帧再点内部按钮）");
            }
        }

        // ---------- 阶段4：关背包 + 点游戏内菜单 ----------
        static void PhaseInventoryCloseAndPause()
        {
            var inv = Object.FindObjectOfType<Inventory.InventoryUI>();
            if (inv != null && inv.gameObject.activeSelf)
            {
                var invClose = GameObject.Find("/Canvas/SafeRoot/InventoryPanel/CloseButton");
                if (invClose != null) ClickAt((RectTransform)invClose.transform);
                if (inv.gameObject.activeSelf) Fail("背包关闭失败");
                else Pass("背包关闭成功");
            }

            var menuBtn = GameObject.Find("/Canvas/SafeRoot/MenuButton");
            if (menuBtn != null)
            {
                ClickAt((RectTransform)menuBtn.transform);
                var pp = Object.FindObjectOfType<PausePanel>();
                if (pp == null || !pp.gameObject.activeSelf) Fail("点击菜单按钮后 PausePanel 未打开");
                else { Pass("点击菜单 → PausePanel 打开（等下一帧再点内部按钮）"); CheckFits("PausePanel"); }
            }
        }

        // ---------- 阶段5：暂停 → 设置 ----------
        static void PhasePauseToSettings()
        {
            var psBtn = GameObject.Find("/Canvas/SafeRoot/PausePanel/BtnSettings");
            if (psBtn == null) { Fail("暂停面板缺设置按钮"); return; }
            ClickAt((RectTransform)psBtn.transform);
            var sp = Object.FindObjectOfType<SettingsPanel>();
            if (sp == null || !sp.gameObject.activeSelf) Fail("设置面板未从暂停菜单打开");
            else Pass("暂停菜单 → 设置面板打开（等下一帧点灵敏度）");
        }

        // ---------- 阶段6：灵敏度真实生效 + 恢复 + 关设置 ----------
        static void PhaseSensitivityAndResume()
        {
            var sp = Object.FindObjectOfType<SettingsPanel>();
            savedLook = GameSettings.Cur.lookSensitivity;
            var low = GameObject.Find("/Canvas/SafeRoot/SettingsPanel/LookLow");
            if (low == null) Fail("设置面板缺灵敏度低档按钮");
            else
            {
                var hitGo = RaycastAt(Center((RectTransform)low.transform));
                Debug.Log($"DIAG_INFO LookLow点击命中: {(hitGo != null ? hitGo.name : "无")}");
                ClickAt((RectTransform)low.transform);
                float after = GameSettings.Cur.lookSensitivity;
                if (System.Math.Abs(after - savedLook) < 0.01f)
                    Fail($"灵敏度点击无效: {savedLook}→{after}");
                else Pass($"灵敏度真实生效: {savedLook:F2}→{after:F2}");
                GameSettings.Set(d => d.lookSensitivity = savedLook);
            }
            var closeBtn = GameObject.Find("/Canvas/SafeRoot/SettingsPanel/CloseButton");
            if (closeBtn != null) ClickAt((RectTransform)closeBtn.transform);

            var resume = GameObject.Find("/Canvas/SafeRoot/PausePanel/BtnResume");
            if (resume != null)
            {
                ClickAt((RectTransform)resume.transform);
                var pp = Object.FindObjectOfType<PausePanel>();
                if (pp != null && pp.gameObject.activeSelf) Fail("点击继续游戏后暂停面板未关闭");
                else Pass("点击继续游戏 → 暂停关闭");
            }
        }

        // ---------- 工具 ----------
        static Vector2 Center(RectTransform rt)
        {
            Vector3[] c = new Vector3[4];
            rt.GetWorldCorners(c);
            return new Vector2((c[0].x + c[2].x) / 2f, (c[0].y + c[2].y) / 2f);
        }

        static GameObject RaycastAt(Vector2 screenPos)
        {
            var ped = new PointerEventData(EventSystem.current) { position = screenPos };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, results);
            return results.Count > 0 ? results[0].gameObject : null;
        }

        /// <summary>出屏检查：面板四角必须在屏幕内（"按钮跑到屏幕外"的回归门）。</summary>
        static void CheckFits(string name)
        {
            var go = GameObject.Find("/Canvas/SafeRoot/" + name);
            if (go == null) return;
            var rt = (RectTransform)go.transform;
            Vector3[] c = new Vector3[4];
            rt.GetWorldCorners(c);
            float w = Screen.width, h = Screen.height;
            for (int i = 0; i < 4; i++)
                if (c[i].x < -2f || c[i].x > w + 2f || c[i].y < -2f || c[i].y > h + 2f)
                {
                    Fail($"{name} 超出屏幕: 角({c[i].x:F0},{c[i].y:F0}) vs 屏幕{w:F0}x{h:F0}");
                    return;
                }
            Pass($"{name} 完整位于屏幕内");
        }

        static void ClickAt(RectTransform rt)
        {
            var ped = new PointerEventData(EventSystem.current) { position = Center(rt) };
            var go = RaycastAt(ped.position);
            if (go == null) { Fail("点击注入无命中目标"); return; }
            ExecuteEvents.Execute(go, ped, ExecuteEvents.pointerClickHandler);
        }

        static void Finish()
        {
            SessionState.SetBool("DIAG_RUNNING", false);
            EditorApplication.update -= OnUpdate;
            Debug.Log($"DIAG_RESULT fails={fails.Count}");
            EditorApplication.isPlaying = false;
            EditorApplication.Exit(fails.Count > 0 ? 1 : 0);
        }
    }
}
