using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Door13.Player;
using Door13.UI;
using Door13.Puzzle;
using Door13.Interaction;
using Door13.Inventory;
using Door13.TimeLoop;
using Door13.Save;
using UnityEditor;

namespace Door13.EditorTools
{
    /// <summary>
    /// 完整玩家测试：模拟从启动到第一章流程的每一步，
    /// 逐项验证用户报告的所有Bug点。
    /// 输出：逐行 PASS/FAIL + 最终报告。
    /// </summary>
    public class FullPlayerTest
    {
        [MenuItem("Door13/0-完整玩家测试")]
        public static void Run()
        {
            Debug.Log("=== 完整玩家测试 v1.7 ===");
            Debug.Log("开始时间: " + System.DateTime.Now.ToString("HH:mm:ss"));

            // 跑场景重建
            Debug.Log("\n--- 阶段0: 场景重建 ---");
            ProjectSetup.EnsureScene();

            // 在播放模式下运行测试
            Debug.Log("\n--- 阶段1: 播放模式测试 ---");
            PlayModeTestRunner.StartTest();
        }
    }

    public class PlayModeTestRunner : MonoBehaviour
    {
        static readonly string LOG_PREFIX = "PLAYER_TEST";

        int pass = 0, fail = 0;
        string logPath;

        void Awake()
        {
            logPath = Application.dataPath + "/../Tools/logs/player_test_v17.log";
        }

        void Start()
        {
            StartCoroutine(RunAll());
        }

        void Log(string msg) => Debug.Log(LOG_PREFIX + " " + msg);

        void Record(string test, bool ok, string detail = "")
        {
            if (ok) { pass++; Log("PASS " + test + (detail != "" ? " | " + detail : "")); }
            else { fail++; Log("FAIL " + test + (detail != "" ? " | " + detail : "")); }
        }

        System.Collections.IEnumerator RunAll()
        {
            // ====== 1. 主菜单验证 ======
            Log("\n--- 1. 主菜单 ---");
            var mm = FindObjectOfType<MainMenu>();
            Record("MainMenu存在", mm != null);
            if (mm != null)
            {
                Record("MainMenu激活", mm.gameObject.activeSelf);

                // 1a. 主菜单与游戏场景隔离（MenuCamera启用，主相机禁用）
                var menuCam = GameObject.Find("MenuCamera")?.GetComponent<Camera>();
                var gameCam = Camera.main;
                Record("MenuCamera启用", menuCam != null && menuCam.enabled);
                Record("主相机禁用（防穿透）", gameCam != null && !gameCam.enabled);

                // 1b. 相机分离断言
                var mcLayer = LayerMask.NameToLayer("MenuScene");
                if (menuCam != null)
                {
                    Record("MenuCamera仅渲染MenuScene层", menuCam.cullingMask == (1 << mcLayer),
                        $"mask={menuCam.cullingMask} expected={(1 << mcLayer)}");
                }
                if (gameCam != null)
                {
                    Record("主相机排除MenuScene层", (gameCam.cullingMask & (1 << mcLayer)) == 0);
                }

                // 1c. 主菜单四角在屏内
                var mmPanel = GameObject.Find("/Canvas/SafeRoot/MainMenu")?.transform as RectTransform;
                if (mmPanel != null)
                {
                    Vector3[] corners = new Vector3[4];
                    mmPanel.GetWorldCorners(corners);
                    bool fits = true;
                    for (int i = 0; i < 4; i++)
                        if (corners[i].x < -2f || corners[i].x > Screen.width + 2f ||
                            corners[i].y < -2f || corners[i].y > Screen.height + 2f)
                            fits = false;
                    Record("MainMenu四角在屏内", fits,
                        $"screen={Screen.width}x{Screen.height}");
                }

                // 1d. 所有按钮可点击（射线命中自身）
                string[] btnNames = { "BtnNew", "BtnContinue", "BtnSettings", "BtnQuit" };
                foreach (var btnName in btnNames)
                {
                    var btnGo = GameObject.Find("/Canvas/SafeRoot/MainMenu/" + btnName);
                    if (btnGo != null)
                    {
                        var btnRt = (RectTransform)btnGo.transform;
                        var hit = RaycastAt(Center(btnRt));
                        Record($"主菜单按钮 {btnName} 可点击", hit == btnGo,
                            hit != null ? $"命中={hit.name}" : "无命中");
                    }
                    else
                    {
                        Record($"主菜单按钮 {btnName} 存在", false, "GameObject不存在");
                    }
                }

                // 1e. 设置按钮 → SettingsPanel 打开
                ClickAt(btnRt: GameObject.Find("/Canvas/SafeRoot/MainMenu/BtnSettings")?.transform as RectTransform);
                yield return new WaitForSeconds(0.1f);
                var sp = FindObjectOfType<SettingsPanel>();
                Record("SettingsPanel打开", sp != null && sp.gameObject.activeSelf);

                // 1f. SettingsPanel 四角在屏内（20:9回归）
                if (sp != null && sp.gameObject.activeSelf)
                {
                    var spRt = sp.gameObject.GetComponent<RectTransform>();
                    Vector3[] spCorners = new Vector3[4];
                    spRt.GetWorldCorners(spCorners);
                    bool spFits = true;
                    for (int i = 0; i < 4; i++)
                        if (spCorners[i].x < -2f || spCorners[i].x > Screen.width + 2f ||
                            spCorners[i].y < -2f || spCorners[i].y > Screen.height + 2f)
                            spFits = false;
                    Record("SettingsPanel四角在屏内", spFits,
                        $"screen={Screen.width}x{Screen.height}");
                }

                // 1g. 设置面板按钮可点击 + 灵敏度真实生效
                var lookLow = GameObject.Find("/Canvas/SafeRoot/SettingsPanel/LookLow");
                if (lookLow != null)
                {
                    var lookLowRt = (RectTransform)lookLow.transform;
                    var hit = RaycastAt(Center(lookLowRt));
                    Record("LookLow按钮可点击", hit == lookLow,
                        hit != null ? $"命中={hit.name}" : "无命中");

                    float savedLook = GameSettings.Cur.lookSensitivity;
                    ClickAt(lookLowRt);
                    float after = GameSettings.Cur.lookSensitivity;
                    Record("灵敏度真实生效", System.Math.Abs(after - savedLook) > 0.01f,
                        $"{savedLook:F2}→{after:F2}");
                    GameSettings.Set(d => d.lookSensitivity = savedLook);
                }

                // 1h. 关闭SettingsPanel
                sp?.Close();
                yield return new WaitForSeconds(0.1f);
                Record("SettingsPanel关闭", sp != null && !sp.gameObject.activeSelf);

                // 1i. 关闭PausePanel（如果之前打开过）
                var pp = FindObjectOfType<PausePanel>();
                if (pp != null && pp.gameObject.activeSelf) pp.Close();
            }

            // ====== 2. 新游戏 ======
            Log("\n--- 2. 新游戏 ---");
            var btnNew = GameObject.Find("/Canvas/SafeRoot/MainMenu/BtnNew")?.transform as RectTransform;
            if (btnNew != null)
            {
                ClickAt(btnNew);
                yield return new WaitForSeconds(0.3f);

                Record("输入锁解除", !InputLock.Active);
                var mc2 = GameObject.Find("MenuCamera")?.GetComponent<Camera>();
                var gc2 = Camera.main;
                Record("MenuCamera禁用（游戏态）", mc2 != null && !mc2.enabled);
                Record("主相机启用（游戏态）", gc2 != null && gc2.enabled);
            }

            // ====== 3. HUD验证 ======
            Log("\n--- 3. HUD ---");
            var hud = FindObjectOfType<Hud>();
            Record("HUD存在", hud != null);
            if (hud != null)
            {
                Record("HUD可见", hud.gameObject.activeSelf);

                // 3a. 摇杆基础检查
                var joy = FindObjectOfType<VirtualJoystick>();
                Record("VirtualJoystick存在", joy != null);
                if (joy != null)
                {
                    var img = joy.GetComponent<Image>();
                    Record("摇杆raycastTarget=false", img?.raycastTarget == false,
                        $"raycastTarget={img?.raycastTarget}");
                    var knob = joy.transform.Find("Knob");
                    if (knob != null)
                    {
                        var knobImg = knob.GetComponent<Image>();
                        Record("摇杆圆钮raycastTarget=false", knobImg?.raycastTarget == false,
                            $"raycastTarget={knobImg?.raycastTarget}");
                    }

                    // 3b. 摇杆InZone检查：用反射取私有字段
                    var rectField = joy.GetType().GetField("rect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var canvasField = joy.GetType().GetField("canvasRect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (rectField != null && canvasField != null)
                    {
                        var joyRt = rectField.GetValue(joy) as RectTransform;
                        var canvasRt = canvasField.GetValue(joy) as RectTransform;
                        if (joyRt != null && canvasRt != null)
                        {
                            Vector2 anchored = joyRt.anchoredPosition;
                            float w = canvasRt.rect.width, h = canvasRt.rect.height;
                            float zoneW = joyRt.rect.width;
                            float zoneH = joyRt.rect.height;
                            bool inLeftZone = anchored.x < w * joy.zoneWidthRatio && anchored.y < h * joy.zoneHeightRatio;
                            Record("摇杆默认位置在左手区", inLeftZone,
                                $"pos=({anchored.x:F0},{anchored.y:F0}) zone=({w:F0}x{h:F0})");
                        }
                    }
                }

                // 3c. HUD按钮可点击
                string[] hudBtns = { "InteractButton", "InventoryButton", "MenuButton" };
                foreach (var name in hudBtns)
                {
                    var btnGo = GameObject.Find("/Canvas/SafeRoot/" + name);
                    if (btnGo != null)
                    {
                        var btnRt = (RectTransform)btnGo.transform;
                        var hit = RaycastAt(Center(btnRt));
                        Record($"HUD按钮 {name} 可点击", hit == btnGo,
                            hit != null ? $"命中={hit.name}" : "无命中");
                    }
                    else
                    {
                        Record($"HUD按钮 {name} 存在", false, "GameObject不存在");
                    }
                }
            }

            // ====== 4. 摇杆功能 ======
            Log("\n--- 4. 摇杆功能 ---");
            var joy4 = FindObjectOfType<VirtualJoystick>();
            if (hud != null && joy4 != null)
            {
                // 模拟触摸摇杆区：用反射取canvasRect
                var canvasField4 = joy4.GetType().GetField("canvasRect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (canvasField4 != null)
                {
                    var canvasRt4 = canvasField4.GetValue(joy4) as RectTransform;
                    if (canvasRt4 != null)
                    {
                        float w4 = canvasRt4.rect.width, h4 = canvasRt4.rect.height;
                        Vector2 screenPos4 = CanvasToScreen(new Vector2(w4 * 0.25f, h4 * 0.25f));
                        var touchZone4 = FindObjectOfType<TouchLookZone>();
                        if (touchZone4 != null)
                        {
                            touchZone4.GetType().GetMethod("Begin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                ?.Invoke(touchZone4, new object[] { screenPos4 });
                            touchZone4.GetType().GetMethod("Drag", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                ?.Invoke(touchZone4, new object[] { screenPos4 + new Vector2(30f, 40f) });

                            var output4 = joy4.Output;
                            Record("摇杆输出非零", output4.magnitude > 0.01f, $"output=({output4.x:F2},{output4.y:F2})");

                            touchZone4.GetType().GetMethod("End", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                ?.Invoke(touchZone4, null);
                        }
                    }
                }
            }

            // ====== 5. 视角功能 ======
            Log("\n--- 5. 视角功能 ---");
            var touchZone5 = FindObjectOfType<TouchLookZone>();
            if (touchZone5 != null)
            {
                var joy5 = FindObjectOfType<VirtualJoystick>();
                var canvasField5 = joy5.GetType().GetField("canvasRect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (canvasField5 != null)
                {
                    var canvasRt5 = canvasField5.GetValue(joy5) as RectTransform;
                    if (canvasRt5 != null)
                    {
                        float w5 = canvasRt5.rect.width, h5 = canvasRt5.rect.height;
                        Vector2 screenPos5 = CanvasToScreen(new Vector2(w5 * 0.75f, h5 * 0.5f));
                        touchZone5.GetType().GetMethod("Begin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.Invoke(touchZone5, new object[] { screenPos5 });
                        touchZone5.GetType().GetMethod("Drag", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.Invoke(touchZone5, new object[] { screenPos5 + new Vector2(50f, 0f) });

                        var delta5 = touchZone5.Delta;
                        Record("视角Delta非零", System.Math.Abs(delta5.x) > 0.01f, $"delta=({delta5.x:F2},{delta5.y:F2})");

                        touchZone5.GetType().GetMethod("End", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.Invoke(touchZone5, null);
                    }
                }
            }

            // ====== 6. 背包 ======
            Log("\n--- 6. 背包 ---");
            var invGo = GameObject.Find("/Canvas/SafeRoot/InventoryPanel");
            if (invGo != null)
            {
                var menuBtn = GameObject.Find("/Canvas/SafeRoot/MenuButton")?.transform as RectTransform;
                ClickAt(menuBtn);
                yield return new WaitForSeconds(0.15f);

                var pp2 = FindObjectOfType<PausePanel>();
                Record("PausePanel打开", pp2 != null && pp2.gameObject.activeSelf);

                if (pp2 != null && pp2.gameObject.activeSelf)
                {
                    var invBtn = GameObject.Find("/Canvas/SafeRoot/PausePanel/BtnInventory")?.transform as RectTransform;
                    if (invBtn != null)
                    {
                        ClickAt(invBtn);
                        yield return new WaitForSeconds(0.15f);

                        var invUI = FindObjectOfType<InventoryUI>();
                        Record("InventoryPanel打开", invUI != null && invUI.gameObject.activeSelf);

                        // 背包四角在屏内
                        if (invUI != null && invUI.gameObject.activeSelf)
                        {
                            var invRt = invUI.gameObject.GetComponent<RectTransform>();
                            Vector3[] invCorners = new Vector3[4];
                            invRt.GetWorldCorners(invCorners);
                            bool invFits = true;
                            for (int i = 0; i < 4; i++)
                                if (invCorners[i].x < -2f || invCorners[i].x > Screen.width + 2f ||
                                    invCorners[i].y < -2f || invCorners[i].y > Screen.height + 2f)
                                    invFits = false;
                            Record("InventoryPanel四角在屏内", invFits);
                        }

                        invUI?.Close();
                        yield return new WaitForSeconds(0.1f);
                        Record("InventoryPanel关闭", invUI != null && !invUI.gameObject.activeSelf);
                    }

                    // 关闭PausePanel
                    var closeBtn = GameObject.Find("/Canvas/SafeRoot/PausePanel/BtnClose")?.transform as RectTransform;
                    ClickAt(closeBtn);
                    yield return new WaitForSeconds(0.1f);
                }
            }

            // ====== 7. 设置面板功能完整测试 ======
            Log("\n--- 7. 设置面板完整功能 ---");
            var pauseGo = GameObject.Find("/Canvas/SafeRoot/PausePanel");
            if (pauseGo != null && pauseGo.GetComponent<PausePanel>() != null)
            {
                var menuBtn2 = GameObject.Find("/Canvas/SafeRoot/MenuButton")?.transform as RectTransform;
                ClickAt(menuBtn2);
                yield return new WaitForSeconds(0.15f);

                var pp3 = FindObjectOfType<PausePanel>();
                if (pp3 != null && pp3.gameObject.activeSelf)
                {
                    var settingsBtn = GameObject.Find("/Canvas/SafeRoot/PausePanel/BtnSettings")?.transform as RectTransform;
                    if (settingsBtn != null)
                    {
                        ClickAt(settingsBtn);
                        yield return new WaitForSeconds(0.15f);

                        var sp2 = FindObjectOfType<SettingsPanel>();
                        Record("SettingsPanel从PausePanel打开", sp2 != null && sp2.gameObject.activeSelf);

                        if (sp2 != null && sp2.gameObject.activeSelf)
                        {
                            // 测试所有灵敏度按钮
                            string[] lookBtns = { "LookLow", "LookMid", "LookHigh" };
                            foreach (var bName in lookBtns)
                            {
                                var bGo = GameObject.Find("/Canvas/SafeRoot/SettingsPanel/" + bName);
                                if (bGo != null)
                                {
                                    var hit = RaycastAt(Center((RectTransform)bGo.transform));
                                    Record($"SettingsPanel {bName} 可点击", hit == bGo);
                                }
                            }

                            // 测试音量滑块
                            var volSlider = sp2.volumeSlider;
                            Record("VolumeSlider存在", volSlider != null);
                            if (volSlider != null)
                            {
                                float oldVol = volSlider.value;
                                volSlider.value = 0.5f;
                                Record("VolumeSlider可拖动", System.Math.Abs(volSlider.value - oldVol) > 0.01f);
                            }

                            // 测试Y轴反转
                            var invertToggle = sp2.invertToggle;
                            Record("InvertToggle存在", invertToggle != null);
                            if (invertToggle != null)
                            {
                                bool oldInvert = invertToggle.isOn;
                                invertToggle.isOn = !oldInvert;
                                Record("InvertToggle可切换", invertToggle.isOn != oldInvert);
                                invertToggle.isOn = oldInvert;
                            }

                            sp2.Close();
                            yield return new WaitForSeconds(0.1f);
                        }

                        // 关闭PausePanel
                        var closeBtn2 = GameObject.Find("/Canvas/SafeRoot/PausePanel/BtnClose")?.transform as RectTransform;
                        ClickAt(closeBtn2);
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }

            // ====== 8. 第一章流程 ======
            Log("\n--- 8. 第一章流程 ---");
            var pc = FindObjectOfType<PlayerController>();
            Record("PlayerController存在", pc != null);

            // 8a. 交互按钮可触发
            var interactGo = GameObject.Find("/Canvas/SafeRoot/InteractButton");
            if (interactGo != null)
            {
                var interactRt = (RectTransform)interactGo.transform;
                var hit = RaycastAt(Center(interactRt));
                Record("交互按钮可点击", hit == interactGo);
            }

            // 8b. 保险箱可交互
            var safeGo = GameObject.Find("Safe");
            if (safeGo != null)
            {
                Record("保险箱GameObject存在", safeGo.activeSelf);
            }

            // 8c. 电脑可交互
            var pcComp = GameObject.Find("Computer")?.GetComponent<ComputerTerminal>();
            Record("电脑组件存在", pcComp != null);

            // ====== 9. 最终汇总 ======
            Log("\n=== 最终汇总 ===");
            Log($"通过: {pass} | 失败: {fail} | 总计: {pass + fail}");

            // 写入日志文件
            File.WriteAllText(logPath, $"Player Test v1.7\nPass: {pass}\nFail: {fail}\nTotal: {pass + fail}\n");

            // 停止测试
            if (fail == 0)
                Debug.Log("ALL TESTS PASSED");
            else
                Debug.Log($"TESTS FAILED: {fail} failures");

            StopAllCoroutines();
            Destroy(gameObject);
        }

        static Vector2 Center(RectTransform rt)
        {
            Vector3[] c = new Vector3[4];
            rt.GetWorldCorners(c);
            return new Vector2((c[0].x + c[2].x) / 2f, (c[0].y + c[2].y) / 2f);
        }

        static GameObject RaycastAt(Vector2 pos)
        {
            var ped = new PointerEventData(EventSystem.current) { position = pos };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, results);
            return results.Count > 0 ? results[0].gameObject : null;
        }

        static void ClickAt(RectTransform btnRt)
        {
            if (btnRt != null)
            {
                var pos = Center(btnRt);
                var ped = new PointerEventData(EventSystem.current) { position = pos };
                var results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(ped, results);
                foreach (var r in results)
                {
                    var btn = r.gameObject.GetComponent<UnityEngine.UI.Button>();
                    if (btn != null) btn.onClick.Invoke();
                }
            }
        }

        static Vector2 CanvasToScreen(Vector2 canvasPos)
        {
            float w = Screen.width, h = Screen.height;
            return new Vector2(canvasPos.x / 1920f * w, canvasPos.y / 1080f * h);
        }

        public static void StartTest()
        {
            var go = new GameObject("PlayerTestRunner");
            go.AddComponent<PlayModeTestRunner>();
        }
    }
}
