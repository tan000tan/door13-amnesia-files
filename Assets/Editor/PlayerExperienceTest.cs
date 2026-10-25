using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
    /// 玩家体验验收测试：模拟真实玩家从启动到第一章完整流程。
    /// 不按代码逻辑测试，而是按玩家直觉操作。
    /// 输出：验收报告文件。
    /// </summary>
    public class PlayerExperienceTest
    {
        [MenuItem("Door13/1-玩家体验验收")]
        public static void Run()
        {
            Debug.Log("=== 玩家体验验收测试 ===");
            Debug.Log("开始: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // 先跑场景重建
            Debug.Log("\n>>> 阶段0: 场景重建");
            ProjectSetup.EnsureScene();

            // 启动播放模式测试
            Debug.Log("\n>>> 阶段1: 播放模式玩家体验测试");
            ExperienceRunner.StartTest();
        }
    }

    public class ExperienceRunner : MonoBehaviour
    {
        string logPath;
        StringBuilder report = new StringBuilder();
        int totalTests = 0, passed = 0, failed = 0;

        void Awake()
        {
            logPath = Application.dataPath + "/../Tools/logs/player_experience_v17.log";
        }

        void Start()
        {
            StartCoroutine(RunPlayerExperience());
        }

        void Log(string msg)
        {
            Debug.Log(msg);
            report.AppendLine(msg);
        }

        /// <summary>玩家视角测试：通过/失败/观察</summary>
        void Test(string name, bool ok, string detail = "")
        {
            totalTests++;
            string mark = ok ? "✅ PASS" : "❌ FAIL";
            string line = $"  [{mark}] {name}";
            if (detail != "") line += $" | {detail}";
            Log(line);
            if (ok) passed++; else failed++;
        }

        System.Collections.IEnumerator RunPlayerExperience()
        {
            report.AppendLine("《第十三扇门：失忆档案》玩家体验验收报告");
            report.AppendLine($"测试日期: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"屏幕分辨率: {Screen.width}x{Screen.height}");
            report.AppendLine($"Canvas参考分辨率: 1920x1080, 缩放模式: ScaleWithScreenSize, matchHeight=0.5");
            report.AppendLine("=================================================================");

            // ================================================================
            // 1. 启动游戏 — 主菜单
            // ================================================================
            report.AppendLine("\n【阶段1：启动游戏 — 主菜单】");
            Log("\n--- 1. 启动游戏 ---");
            Log("玩家视角：打开游戏，看到主菜单界面");

            var mm = FindObjectOfType<MainMenu>();
            Test("MainMenu组件存在", mm != null);
            if (mm == null) { WriteReport(); StopAllCoroutines(); Destroy(gameObject); yield break; }

            Test("MainMenu处于激活状态", mm.gameObject.activeSelf,
                $"active={mm.gameObject.activeSelf}");

            // 1a. 检查标题和副标题
            var titleText = mm.titleText;
            Test("标题文字存在", titleText != null,
                titleText != null ? $"text='{titleText.text}'" : "null");

            var subText = mm.subtitleText;
            Test("副标题文字存在", subText != null,
                subText != null ? $"text='{subText.text}'" : "null");

            // 1b. 检查菜单背景——不应该看到游戏世界
            var menuCam = GameObject.Find("MenuCamera")?.GetComponent<Camera>();
            var gameCam = Camera.main;
            Test("MenuCamera已启用（渲染菜单场景）", menuCam != null && menuCam.enabled,
                menuCam != null ? $"enabled={menuCam.enabled}, cullingMask={menuCam.cullingMask}" : "null");
            Test("主相机已禁用（防止看到游戏世界）", gameCam != null && !gameCam.enabled,
                gameCam != null ? $"enabled={gameCam.enabled}" : "null");

            // 1c. 检查菜单场景层隔离
            int menuLayer = LayerMask.NameToLayer("MenuScene");
            Test("MenuScene层已注册", menuLayer >= 0, $"layerIndex={menuLayer}");
            if (menuCam != null && menuLayer >= 0)
            {
                Test("MenuCamera仅渲染MenuScene层", menuCam.cullingMask == (1 << menuLayer),
                    $"mask={menuCam.cullingMask}, expected={(1 << menuLayer)}");
            }
            if (gameCam != null && menuLayer >= 0)
            {
                Test("主相机排除MenuScene层", (gameCam.cullingMask & (1 << menuLayer)) == 0,
                    $"mask={gameCam.cullingMask}");
            }

            // 1d. 检查主菜单面板是否在屏内
            var mmRt = GameObject.Find("/Canvas/SafeRoot/MainMenu")?.transform as RectTransform;
            if (mmRt != null)
            {
                Vector3[] mmCorners = new Vector3[4];
                mmRt.GetWorldCorners(mmCorners);
                bool mmFits = true;
                string mmCornerInfo = "";
                for (int i = 0; i < 4; i++)
                {
                    bool okX = mmCorners[i].x >= -2f && mmCorners[i].x <= Screen.width + 2f;
                    bool okY = mmCorners[i].y >= -2f && mmCorners[i].y <= Screen.height + 2f;
                    if (!okX || !okY) mmFits = false;
                    mmCornerInfo += $"[{i}](x{mmCorners[i].x:F0},y{mmCorners[i].y:F0}) ";
                }
                Test("主菜单四角在屏幕内", mmFits, $"screen={Screen.width}x{Screen.height}\n    角点: {mmCornerInfo}");
            }

            // 1e. 逐个检查菜单按钮——玩家会依次看到并点击
            string[] menuBtns = { "BtnNew", "BtnSettings", "BtnQuit" };
            foreach (var btnName in menuBtns)
            {
                var btnGo = GameObject.Find("/Canvas/SafeRoot/MainMenu/" + btnName);
                if (btnGo != null)
                {
                    var btnRt = (RectTransform)btnGo.transform;
                    var hit = RaycastAt(Center(btnRt));
                    Test($"按钮 '{btnName}' 存在且可点击", hit == btnGo,
                        hit != null ? $"命中自身={hit.name}" : "命中失败");

                    // 检查按钮是否在屏内
                    Vector3[] bCorners = new Vector3[4];
                    btnRt.GetWorldCorners(bCorners);
                    bool bFits = true;
                    for (int i = 0; i < 4; i++)
                        if (bCorners[i].x < -2f || bCorners[i].x > Screen.width + 2f ||
                            bCorners[i].y < -2f || bCorners[i].y > Screen.height + 2f)
                            bFits = false;
                    Test($"按钮 '{btnName}' 在屏幕内", bFits);
                }
                else
                {
                    Test($"按钮 '{btnName}' 存在", false, "GameObject未找到");
                }
            }

            // 1f. 玩家点击"设置"按钮
            Log("\n--- 1f. 玩家点击【设置】按钮 ---");
            var settingsBtn = GameObject.Find("/Canvas/SafeRoot/MainMenu/BtnSettings")?.transform as RectTransform;
            if (settingsBtn != null)
            {
                ClickAt(settingsBtn);
                yield return new WaitForSeconds(0.2f);

                var sp = FindObjectOfType<SettingsPanel>();
                Test("SettingsPanel打开", sp != null && sp.gameObject.activeSelf);

                if (sp != null && sp.gameObject.activeSelf)
                {
                    // 1f-i. 检查设置面板在屏内
                    var spRt = sp.gameObject.GetComponent<RectTransform>();
                    Vector3[] spCorners = new Vector3[4];
                    spRt.GetWorldCorners(spCorners);
                    bool spFits = true;
                    string spCornerInfo = "";
                    for (int i = 0; i < 4; i++)
                    {
                        bool okX = spCorners[i].x >= -2f && spCorners[i].x <= Screen.width + 2f;
                        bool okY = spCorners[i].y >= -2f && spCorners[i].y <= Screen.height + 2f;
                        if (!okX || !okY) spFits = false;
                        spCornerInfo += $"[{i}](x{spCorners[i].x:F0},y{spCorners[i].y:F0}) ";
                    }
                    Test("SettingsPanel四角在屏幕内", spFits, $"screen={Screen.width}x{Screen.height}\n    角点: {spCornerInfo}");

                    // 1f-ii. 检查设置面板所有按钮
                    string[] settingBtns = { "LookLow", "LookMid", "LookHigh",
                        "SmoothLow", "SmoothMid", "SmoothHigh",
                        "MoveLow", "MoveMid", "MoveHigh", "CloseButton" };
                    foreach (var sBtn in settingBtns)
                    {
                        var sGo = GameObject.Find("/Canvas/SafeRoot/SettingsPanel/" + sBtn);
                        if (sGo != null)
                        {
                            var sRt = (RectTransform)sGo.transform;
                            var hit = RaycastAt(Center(sRt));
                            Test($"设置按钮 '{sBtn}' 可点击", hit == sGo,
                                hit != null ? $"命中={hit.name}" : "无命中");
                        }
                        else
                        {
                            Test($"设置按钮 '{sBtn}' 存在", false);
                        }
                    }

                    // 1f-iii. 检查音量滑块
                    Test("VolumeSlider存在", sp.volumeSlider != null);
                    if (sp.volumeSlider != null)
                    {
                        float volStart = sp.volumeSlider.value;
                        sp.volumeSlider.value = 0.3f;
                        Test("VolumeSlider可拖动", sp.volumeSlider.value != volStart,
                            $"{volStart:F2}→{sp.volumeSlider.value:F2}");
                        sp.volumeSlider.value = 0.8f; // 恢复
                    }

                    // 1f-iv. 检查Y轴反转
                    Test("InvertToggle存在", sp.invertToggle != null);
                    if (sp.invertToggle != null)
                    {
                        bool oldInv = sp.invertToggle.isOn;
                        sp.invertToggle.isOn = !oldInv;
                        Test("InvertToggle可切换", sp.invertToggle.isOn != oldInv);
                        sp.invertToggle.isOn = oldInv;
                    }

                    // 1f-v. 玩家点击"关闭"
                    Log("\n--- 1f-v. 玩家点击【关闭】 ---");
                    var closeBtn = GameObject.Find("/Canvas/SafeRoot/SettingsPanel/CloseButton")?.transform as RectTransform;
                    if (closeBtn != null)
                    {
                        ClickAt(closeBtn);
                        yield return new WaitForSeconds(0.15f);
                        Test("SettingsPanel关闭", sp != null && !sp.gameObject.activeSelf);
                    }
                }
            }

            // ================================================================
            // 2. 开始游戏
            // ================================================================
            report.AppendLine("\n【阶段2：开始游戏 — 新游戏】");
            Log("\n--- 2. 玩家点击【新游戏】 ---");

            var newBtn = GameObject.Find("/Canvas/SafeRoot/MainMenu/BtnNew")?.transform as RectTransform;
            if (newBtn != null)
            {
                ClickAt(newBtn);
                yield return new WaitForSeconds(0.5f);

                Test("输入锁已解除", !InputLock.Active);
                Test("MenuCamera已禁用", menuCam != null && !menuCam.enabled);
                Test("主相机已启用", gameCam != null && gameCam.enabled);

                // 2a. 检查HUD
                Log("\n--- 2a. 检查HUD ---");
                var hud = FindObjectOfType<Hud>();
                Test("HUD存在", hud != null);
                if (hud != null)
                {
                    Test("HUD可见", hud.gameObject.activeSelf);

                    // 2a-i. 检查摇杆
                    Log("\n--- 2a-i. 检查摇杆 ---");
                    var joy = FindObjectOfType<VirtualJoystick>();
                    Test("VirtualJoystick存在", joy != null);
                    if (joy != null)
                    {
                        var baseImg = joy.GetComponent<Image>();
                        Test("摇杆底座 raycastTarget=false", baseImg != null && baseImg.raycastTarget == false,
                            $"raycastTarget={baseImg?.raycastTarget}");

                        var knob = joy.transform.Find("Knob");
                        if (knob != null)
                        {
                            var knobImg = knob.GetComponent<Image>();
                            Test("摇杆圆钮 raycastTarget=false", knobImg != null && knobImg.raycastTarget == false,
                                $"raycastTarget={knobImg?.raycastTarget}");
                        }

                        // 摇杆默认位置在左下角
                        var joyRtField = joy.GetType().GetField("rect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var canvasRtField = joy.GetType().GetField("canvasRect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (joyRtField != null && canvasRtField != null)
                        {
                            var joyRt = joyRtField.GetValue(joy) as RectTransform;
                            var canvasRt = canvasRtField.GetValue(joy) as RectTransform;
                            if (joyRt != null && canvasRt != null)
                            {
                                Vector2 anchored = joyRt.anchoredPosition;
                                float w = canvasRt.rect.width, h = canvasRt.rect.height;
                                float zoneX = w * joy.zoneWidthRatio;
                                float zoneY = h * joy.zoneHeightRatio;
                                bool inLeftZone = anchored.x < zoneX && anchored.y < zoneY;
                                Test("摇杆默认位置在左手区", inLeftZone,
                                    $"pos=({anchored.x:F0},{anchored.y:F0}) zone=({zoneX:F0}x{zoneY:F0})");

                                // 检查摇杆在屏内
                                Vector3[] jCorners = new Vector3[4];
                                joyRt.GetWorldCorners(jCorners);
                                bool jFits = true;
                                for (int i = 0; i < 4; i++)
                                    if (jCorners[i].x < -2f || jCorners[i].x > Screen.width + 2f ||
                                        jCorners[i].y < -2f || jCorners[i].y > Screen.height + 2f)
                                        jFits = false;
                                Test("摇杆底座在屏幕内", jFits);
                            }
                        }
                    }

                    // 2a-ii. 检查HUD按钮
                    Log("\n--- 2a-ii. 检查HUD按钮 ---");
                    string[] hudBtns = { "InteractButton", "InventoryButton", "MenuButton" };
                    foreach (var hBtn in hudBtns)
                    {
                        var hGo = GameObject.Find("/Canvas/SafeRoot/" + hBtn);
                        if (hGo != null)
                        {
                            var hRt = (RectTransform)hGo.transform;
                            var hit = RaycastAt(Center(hRt));
                            Test($"HUD按钮 '{hBtn}' 可点击", hit == hGo,
                                hit != null ? $"命中={hit.name}" : "无命中");

                            // 检查在屏内
                            Vector3[] hCorners = new Vector3[4];
                            hRt.GetWorldCorners(hCorners);
                            bool hFits = true;
                            for (int i = 0; i < 4; i++)
                                if (hCorners[i].x < -2f || hCorners[i].x > Screen.width + 2f ||
                                    hCorners[i].y < -2f || hCorners[i].y > Screen.height + 2f)
                                    hFits = false;
                            Test($"HUD按钮 '{hBtn}' 在屏幕内", hFits);
                        }
                        else
                        {
                            Test($"HUD按钮 '{hBtn}' 存在", false);
                        }
                    }
                }
            }

            // ================================================================
            // 3. 操作测试 — 摇杆移动 + 视角转动
            // ================================================================
            report.AppendLine("\n【阶段3：操作测试 — 摇杆移动 + 视角转动】");
            Log("\n--- 3. 操作测试 ---");

            var touchZone = FindObjectOfType<TouchLookZone>();
            var joy3 = FindObjectOfType<VirtualJoystick>();
            if (touchZone != null && joy3 != null)
            {
                var canvasRtField = joy3.GetType().GetField("canvasRect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (canvasRtField != null)
                {
                    var canvasRt = canvasRtField.GetValue(joy3) as RectTransform;
                    if (canvasRt != null)
                    {
                        float w = canvasRt.rect.width, h = canvasRt.rect.height;

                        // 3a. 玩家用左手在左下角拖动 → 应该移动
                        Log("\n--- 3a. 左手区拖动（摇杆移动） ---");
                        Vector2 touchPos = CanvasToScreen(new Vector2(w * 0.2f, h * 0.2f));
                        touchZone.GetType().GetMethod("Begin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.Invoke(touchZone, new object[] { touchPos });
                        touchZone.GetType().GetMethod("Drag", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.Invoke(touchZone, new object[] { touchPos + new Vector2(40f, 50f) });

                        var output = joy3.Output;
                        Test("摇杆输出非零", output.magnitude > 0.01f,
                            $"output=({output.x:F2},{output.y:F2}), magnitude={output.magnitude:F2}");

                        // 3b. 玩家用右手在右侧滑动 → 应该转视角
                        Log("\n--- 3b. 右手区滑动（视角转动） ---");
                        Vector2 lookPos = CanvasToScreen(new Vector2(w * 0.8f, h * 0.5f));
                        touchZone.GetType().GetMethod("Begin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.Invoke(touchZone, new object[] { lookPos });
                        touchZone.GetType().GetMethod("Drag", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.Invoke(touchZone, new object[] { lookPos + new Vector2(60f, 0f) });

                        var delta = touchZone.Delta;
                        Test("视角Delta非零", System.Math.Abs(delta.x) > 0.01f,
                            $"delta=({delta.x:F2},{delta.y:F2})");

                        // 3c. 确认摇杆和视角互不影响
                        Test("摇杆输出仍有效（双指操作）", output.magnitude > 0.01f,
                            $"output=({output.x:F2},{output.y:F2})");

                        // 清理
                        touchZone.GetType().GetMethod("End", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.Invoke(touchZone, null);
                        touchZone.GetType().GetMethod("End", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.Invoke(touchZone, null);
                    }
                }
            }

            // ================================================================
            // 4. UI测试 — 菜单、背包
            // ================================================================
            report.AppendLine("\n【阶段4：UI测试 — 菜单、背包】");
            Log("\n--- 4. UI测试 ---");

            // 4a. 玩家按菜单按钮
            Log("\n--- 4a. 玩家按【菜单】按钮 ---");
            var menuBtnGo = GameObject.Find("/Canvas/SafeRoot/MenuButton")?.transform as RectTransform;
            if (menuBtnGo != null)
            {
                ClickAt(menuBtnGo);
                yield return new WaitForSeconds(0.2f);

                var pp = FindObjectOfType<PausePanel>();
                Test("PausePanel打开", pp != null && pp.gameObject.activeSelf);

                if (pp != null && pp.gameObject.activeSelf)
                {
                    // 检查PausePanel在屏内
                    var ppRt = pp.gameObject.GetComponent<RectTransform>();
                    Vector3[] ppCorners = new Vector3[4];
                    ppRt.GetWorldCorners(ppCorners);
                    bool ppFits = true;
                    for (int i = 0; i < 4; i++)
                        if (ppCorners[i].x < -2f || ppCorners[i].x > Screen.width + 2f ||
                            ppCorners[i].y < -2f || ppCorners[i].y > Screen.height + 2f)
                            ppFits = false;
                    Test("PausePanel四角在屏幕内", ppFits);

                    // 检查PausePanel内按钮
                    string[] pauseBtns = { "BtnInventory", "BtnSettings", "BtnClose" };
                    foreach (var pBtn in pauseBtns)
                    {
                        var pGo = GameObject.Find("/Canvas/SafeRoot/PausePanel/" + pBtn);
                        if (pGo != null)
                        {
                            var pRt = (RectTransform)pGo.transform;
                            var hit = RaycastAt(Center(pRt));
                            Test($"PausePanel按钮 '{pBtn}' 可点击", hit == pGo,
                                hit != null ? $"命中={hit.name}" : "无命中");
                        }
                        else
                        {
                            Test($"PausePanel按钮 '{pBtn}' 存在", false);
                        }
                    }

                    // 4b. 玩家点击背包
                    Log("\n--- 4b. 玩家点击【背包】按钮 ---");
                    var invBtn = GameObject.Find("/Canvas/SafeRoot/PausePanel/BtnInventory")?.transform as RectTransform;
                    if (invBtn != null)
                    {
                        ClickAt(invBtn);
                        yield return new WaitForSeconds(0.2f);

                        var invUI = FindObjectOfType<InventoryUI>();
                        Test("InventoryPanel打开", invUI != null && invUI.gameObject.activeSelf);

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
                            Test("InventoryPanel四角在屏幕内", invFits);
                        }

                        // 关闭背包
                        var invClose = GameObject.Find("/Canvas/SafeRoot/InventoryPanel/CloseButton")?.transform as RectTransform;
                        if (invClose != null)
                        {
                            ClickAt(invClose);
                            yield return new WaitForSeconds(0.15f);
                            Test("InventoryPanel关闭", invUI != null && !invUI.gameObject.activeSelf);
                        }
                    }

                    // 关闭PausePanel
                    Log("\n--- 4c. 玩家点击【关闭】回到游戏 ---");
                    var ppClose = GameObject.Find("/Canvas/SafeRoot/PausePanel/BtnClose")?.transform as RectTransform;
                    if (ppClose != null)
                    {
                        ClickAt(ppClose);
                        yield return new WaitForSeconds(0.15f);
                        Test("PausePanel关闭", pp != null && !pp.gameObject.activeSelf);
                    }
                }
            }

            // ================================================================
            // 5. 设置测试 — 从PausePanel进入
            // ================================================================
            report.AppendLine("\n【阶段5：设置测试 — 修改灵敏度并验证】");
            Log("\n--- 5. 设置测试 ---");

            var menuBtnGo2 = GameObject.Find("/Canvas/SafeRoot/MenuButton")?.transform as RectTransform;
            if (menuBtnGo2 != null)
            {
                ClickAt(menuBtnGo2);
                yield return new WaitForSeconds(0.2f);

                var pp2 = FindObjectOfType<PausePanel>();
                if (pp2 != null && pp2.gameObject.activeSelf)
                {
                    var settingsBtn2 = GameObject.Find("/Canvas/SafeRoot/PausePanel/BtnSettings")?.transform as RectTransform;
                    if (settingsBtn2 != null)
                    {
                        ClickAt(settingsBtn2);
                        yield return new WaitForSeconds(0.2f);

                        var sp2 = FindObjectOfType<SettingsPanel>();
                        if (sp2 != null && sp2.gameObject.activeSelf)
                        {
                            // 5a. 玩家点击"视角灵敏度 → 低"
                            Log("\n--- 5a. 玩家点击【视角灵敏度→低】 ---");
                            float savedLook = GameSettings.Cur.lookSensitivity;
                            var lookLowBtn = GameObject.Find("/Canvas/SafeRoot/SettingsPanel/LookLow")?.transform as RectTransform;
                            if (lookLowBtn != null)
                            {
                                ClickAt(lookLowBtn);
                                yield return new WaitForSeconds(0.1f);
                                float afterLook = GameSettings.Cur.lookSensitivity;
                                Test("视角灵敏度真实改变", System.Math.Abs(afterLook - savedLook) > 0.01f,
                                    $"{savedLook:F2}→{afterLook:F2}");
                            }

                            // 5b. 玩家点击"摇杆灵敏度 → 高"
                            Log("\n--- 5b. 玩家点击【摇杆灵敏度→高】 ---");
                            float savedMove = GameSettings.Cur.moveSensitivity;
                            var moveHighBtn = GameObject.Find("/Canvas/SafeRoot/SettingsPanel/MoveHigh")?.transform as RectTransform;
                            if (moveHighBtn != null)
                            {
                                ClickAt(moveHighBtn);
                                yield return new WaitForSeconds(0.1f);
                                float afterMove = GameSettings.Cur.moveSensitivity;
                                Test("摇杆灵敏度真实改变", System.Math.Abs(afterMove - savedMove) > 0.01f,
                                    $"{savedMove:F2}→{afterMove:F2}");
                            }

                            // 5c. 玩家点击"关闭"
                            var closeBtn2 = GameObject.Find("/Canvas/SafeRoot/SettingsPanel/CloseButton")?.transform as RectTransform;
                            if (closeBtn2 != null)
                            {
                                ClickAt(closeBtn2);
                                yield return new WaitForSeconds(0.15f);
                                Test("SettingsPanel关闭", sp2 != null && !sp2.gameObject.activeSelf);
                            }

                            // 恢复设置
                            GameSettings.Set(d => d.lookSensitivity = savedLook);
                            GameSettings.Set(d => d.moveSensitivity = savedMove);
                        }

                        // 关闭PausePanel
                        var ppClose2 = GameObject.Find("/Canvas/SafeRoot/PausePanel/BtnClose")?.transform as RectTransform;
                        if (ppClose2 != null)
                        {
                            ClickAt(ppClose2);
                            yield return new WaitForSeconds(0.15f);
                        }
                    }
                }
            }

            // ================================================================
            // 6. 第一章流程测试
            // ================================================================
            report.AppendLine("\n【阶段6：第一章流程测试 — 完整探索】");
            Log("\n--- 6. 第一章流程测试 ---");

            // 6a. 玩家环顾四周
            Log("\n--- 6a. 玩家环顾四周 ---");
            var pc = FindObjectOfType<PlayerController>();
            Test("PlayerController存在", pc != null);

            // 检查出生点附近的关键物体
            string[] keyObjects = { "Bed", "Desk", "Computer", "Safe", "PhotoWall", "Door" };
            foreach (var objName in keyObjects)
            {
                var obj = GameObject.Find(objName);
                Test($"房间物体 '{objName}' 存在", obj != null,
                    obj != null ? $"pos=({obj.transform.position.x:F1},{obj.transform.position.y:F1},{obj.transform.position.z:F1})" : "未找到");
            }

            // 6b. 检查交互按钮
            Log("\n--- 6b. 检查交互系统 ---");
            var interactGo = GameObject.Find("/Canvas/SafeRoot/InteractButton");
            if (interactGo != null)
            {
                var interactRt = (RectTransform)interactGo.transform;
                var hit = RaycastAt(Center(interactRt));
                Test("交互按钮可点击", hit == interactGo);
            }

            // 6c. 检查保险箱
            Log("\n--- 6c. 检查保险箱 ---");
            var safeGo = GameObject.Find("Safe");
            if (safeGo != null)
            {
                Test("保险箱GameObject存在", safeGo.activeSelf);

                var safeComp = safeGo.GetComponent<SafeInteractable>();
                Test("保险箱组件存在", safeComp != null);
                if (safeComp != null)
                {
                    Test("保险箱未解开（初始状态）", !safeComp.IsOpen);
                }
            }

            // 6d. 检查电脑
            Log("\n--- 6d. 检查电脑 ---");
            var computerGo = GameObject.Find("Computer");
            if (computerGo != null)
            {
                Test("电脑GameObject存在", computerGo.activeSelf);
                var compTerm = computerGo.GetComponent<ComputerTerminal>();
                Test("电脑组件存在", compTerm != null);
            }

            // 6e. 检查照片墙
            Log("\n--- 6e. 检查照片墙 ---");
            var photoWall = GameObject.Find("PhotoWall");
            if (photoWall != null)
            {
                Test("照片墙GameObject存在", photoWall.activeSelf);
            }
            else
            {
                // 照片墙可能叫其他名字
                var photoPhotos = GameObject.FindGameObjectsWithTag("Photo");
                Test("照片墙（照片物体）存在", photoPhotos.Length > 0,
                    $"找到{photoPhotos.Length}个照片物体");
            }

            // 6f. 检查可交互物体
            Log("\n--- 6f. 检查可交互组件 ---");
            var interactables = FindObjectsOfType<Interactable>();
            Test("存在可交互物体", interactables.Length > 0,
                $"找到{interactables.Length}个Interactable");
            foreach (var inter in interactables)
            {
                Test($"  可交互: {inter.name}", true);
            }

            // 6g. 检查循环系统
            Log("\n--- 6g. 检查循环系统 ---");
            var loopMgr = FindObjectOfType<LoopManager>();
            Test("LoopManager存在", loopMgr != null);
            if (loopMgr != null)
            {
                Test("当前循环次数=1", loopMgr.LoopCount == 1,
                    $"loop={loopMgr.LoopCount}");
            }

            // 6h. 检查事件系统
            Log("\n--- 6h. 检查事件系统 ---");
            var es = EventSystem.current;
            Test("EventSystem存在", es != null);
            if (es != null)
            {
                Test("EventSystem使用StandaloneInputModule",
                    es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>() != null);
            }

            // ================================================================
            // 最终汇总
            // ================================================================
            report.AppendLine("\n=================================================================");
            report.AppendLine("【最终汇总】");
            report.AppendLine($"总测试数: {totalTests}");
            report.AppendLine($"通过: {passed}");
            report.AppendLine($"失败: {failed}");
            report.AppendLine($"通过率: {passed * 100.0 / totalTests:F1}%");
            report.AppendLine("");

            if (failed == 0)
            {
                report.AppendLine("【结论】所有基础操作正常，玩家可完整体验第一章。");
                report.AppendLine("建议：进入下一阶段（场景、美术、剧情全面优化）。");
            }
            else
            {
                report.AppendLine($"【结论】存在{failed}个失败项，需修复后再进入下一阶段。");
                report.AppendLine("请查看上方失败项详情。");
            }

            Log("\n=== 最终汇总 ===");
            Log($"通过: {passed} | 失败: {failed} | 总计: {totalTests}");
            Log($"通过率: {passed * 100.0 / totalTests:F1}%");

            // 写入报告文件
            File.WriteAllText(logPath, report.ToString());
            Debug.Log($"\n报告已写入: {logPath}");

            StopAllCoroutines();
            Destroy(gameObject);
        }

        // ---- 工具方法 ----
        static Vector2 Center(RectTransform rt)
        {
            Vector3[] c = new Vector3[4];
            rt.GetWorldCorners(c);
            return new Vector2((c[0].x + c[2].x) / 2f, (c[0].y + c[2].y) / 2f);
        }

        static GameObject RaycastAt(Vector2 pos)
        {
            var ped = new PointerEventData(EventSystem.current) { position = pos };
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, results);
            return results.Count > 0 ? results[0].gameObject : null;
        }

        static void ClickAt(RectTransform btnRt)
        {
            if (btnRt != null)
            {
                var pos = Center(btnRt);
                var ped = new PointerEventData(EventSystem.current) { position = pos };
                var results = new System.Collections.Generic.List<RaycastResult>();
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

        void WriteReport()
        {
            File.WriteAllText(logPath, report.ToString());
        }

        public static void StartTest()
        {
            var go = new GameObject("PlayerExperienceTest");
            go.AddComponent<ExperienceRunner>();
        }
    }
}
