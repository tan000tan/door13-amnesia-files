using UnityEngine;
using UnityEngine.UI;
using Door13.Interaction;
using Door13.Inventory;
using Door13.Puzzle;
using Door13.Player;
using Door13.UI;

namespace Door13.EditorTools
{
    /// <summary>
    /// uGUI 全套界面生成（纯代码程序化视觉）：
    /// HUD（摇杆/交互/背包/菜单/准星/提示/目标/循环/旁白/淡入淡出）+
    /// 面板（检视/密码盘/顺序开关/调谐/排序/阀门/终端/背包）+
    /// 主菜单/暂停/设置。
    /// </summary>
    public static class UiBuilder
    {
        static Sprite circle;

        public static void Build()
        {
            circle = ProjectSetup.EnsureCircleSprite();

            var canvasGo = UICanvas();
            var safeRoot = SafeRoot(canvasGo.transform);
            BuildHud(safeRoot);
            BuildVignette(safeRoot);
            // 主菜单必须在所有功能面板之前创建：uGUI 兄弟节点越晚越上层，
            // 否则从主菜单打开设置时，菜单会绘制在设置面板之上（实测复现过）。
            BuildMainMenu(safeRoot);
            BuildInspect(safeRoot);
            BuildKeypad(safeRoot);
            BuildSwitch(safeRoot);
            BuildTuner(safeRoot);
            BuildSort(safeRoot);
            BuildValve(safeRoot);
            BuildTerminal(safeRoot);
            BuildComputer(safeRoot);
            BuildInventory(safeRoot);
            BuildPause(safeRoot);
            BuildSettings(safeRoot);
            BuildFade(safeRoot);
            BuildTouchDiagnostics(canvasGo.transform);
            canvasGo.AddComponent<Hud>();
        }

        /// <summary>
        /// 触摸诊断层：Canvas 直属（物理屏幕坐标系），显示触摸ID/坐标/归一化/接收者，
        /// 并在每个触摸位置画十字标记。全程 raycastTarget=false，绝不拦截真实触摸。
        /// </summary>
        static void BuildTouchDiagnostics(Transform canvas)
        {
            var root = new GameObject("TouchDiagRoot", typeof(RectTransform));
            root.transform.SetParent(canvas, false);
            Stretch((RectTransform)root.transform);

            var logGo = new GameObject("TouchDiagText", typeof(Text));
            logGo.transform.SetParent(root.transform, false);
            var log = logGo.GetComponent<Text>();
            log.font = ProjectSetup.GetBuiltinFont();
            log.fontSize = 24;
            log.color = new Color(0.5f, 1f, 0.65f, 0.95f);
            log.alignment = TextAnchor.UpperCenter;
            log.raycastTarget = false;
            var lrt = (RectTransform)logGo.transform;
            lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 1f);
            lrt.anchoredPosition = new Vector2(0f, -160f);
            lrt.sizeDelta = new Vector2(1600f, 150f);

            var dots = new Image[4];
            var dotLabels = new Text[4];
            for (int i = 0; i < 4; i++)
            {
                var dot = new GameObject("TouchDiagDot_" + i, typeof(Image));
                dot.transform.SetParent(root.transform, false);
                var img = dot.GetComponent<Image>();
                img.sprite = circle;
                img.color = new Color(1f, 0.45f, 0.2f, 0.9f);
                img.raycastTarget = false;
                var drt = (RectTransform)dot.transform;
                drt.anchorMin = drt.anchorMax = new Vector2(0.5f, 0.5f);
                drt.sizeDelta = new Vector2(56f, 56f);
                var lblGo = new GameObject("Label", typeof(Text));
                lblGo.transform.SetParent(dot.transform, false);
                var lbl = lblGo.GetComponent<Text>();
                lbl.font = ProjectSetup.GetBuiltinFont();
                lbl.fontSize = 22;
                lbl.color = Color.black;
                lbl.alignment = TextAnchor.MiddleCenter;
                lbl.raycastTarget = false;
                var lblRt = (RectTransform)lblGo.transform;
                lblRt.anchorMin = lblRt.anchorMax = new Vector2(0.5f, 0.5f);
                lblRt.sizeDelta = new Vector2(56f, 56f);
                dot.SetActive(false);
                dots[i] = img;
                dotLabels[i] = lbl;
            }

            var diagCtrl = new GameObject("TouchDiagController", typeof(RectTransform));
            diagCtrl.transform.SetParent(canvas, false);
            Stretch((RectTransform)diagCtrl.transform);
            var diag = diagCtrl.AddComponent<TouchDiagnostics>();
            diag.root = (RectTransform)root.transform;
            diag.logText = log;
            diag.dotImages = dots;
            diag.dotLabels = dotLabels;
        }

        /// <summary>安全区容器：所有UI内容放在其下（刘海/圆角/导航栏适配）</summary>
        static Transform SafeRoot(Transform canvas)
        {
            var go = new GameObject("SafeRoot", typeof(RectTransform));
            go.transform.SetParent(canvas, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.AddComponent<SafeArea>();
            return go.transform;
        }

        /// <summary>全屏径向暗角：不拦截点击，压在HUD之上、面板之下。</summary>
        static void BuildVignette(Transform canvas)
        {
            var vig = ProjectSetup.EnsureVignetteSprite();
            if (vig == null) return;
            var go = new GameObject("Vignette", typeof(Image));
            go.transform.SetParent(canvas, false);
            var img = go.GetComponent<Image>();
            img.sprite = vig;
            img.color = new Color(0f, 0f, 0f, 0.40f);
            img.raycastTarget = false;
            Stretch(img.rectTransform);
        }

        /// <summary>面板统一装饰：标题下的青灰饰条（研究所档案风格）。</summary>
        static void AccentLine(Transform panel, float yTopFraction, float widthFraction = 0.3f)
        {
            float half = widthFraction * 0.5f;
            Band(panel, panel.name + "_Accent",
                new Vector2(0.5f - half, yTopFraction), new Vector2(0.5f + half, yTopFraction + 0.006f),
                new Color(0.5f, 0.76f, 0.86f, 0.55f));
        }

        static GameObject UICanvas()
        {
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            canvasGo.AddComponent<TouchLookZone>();

            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            return canvasGo;
        }

        static void BuildHud(Transform canvas)
        {
            // 虚拟摇杆（左下）
            var baseGo = new GameObject("JoystickBase", typeof(Image));
            baseGo.transform.SetParent(canvas, false);
            var baseImg = baseGo.GetComponent<Image>();
            baseImg.sprite = circle;
            baseImg.color = new Color(1f, 1f, 1f, 0.28f);
            baseImg.raycastTarget = false;   // 关键：底座不可为射线目标，否则吞掉触摸导致摇杆永不激活
            var baseRt = (RectTransform)baseGo.transform;
            baseRt.anchorMin = baseRt.anchorMax = Vector2.zero;
            baseRt.anchoredPosition = new Vector2(240f, 240f);
            baseRt.sizeDelta = new Vector2(280f, 280f);
            var knobGo = new GameObject("Knob", typeof(Image));
            knobGo.transform.SetParent(baseGo.transform, false);
            var knobImg = knobGo.GetComponent<Image>();
            knobImg.sprite = circle;
            knobImg.color = new Color(1f, 1f, 1f, 0.5f);
            knobImg.raycastTarget = false;   // 圆钮同理
            var knobRt = (RectTransform)knobGo.transform;
            knobRt.anchorMin = knobRt.anchorMax = new Vector2(0.5f, 0.5f);
            knobRt.sizeDelta = new Vector2(130f, 130f);
            var joy = baseGo.AddComponent<VirtualJoystick>();
            joy.radius = 90f;

            // 按钮
            MakeButton(canvas, "InteractButton", "交互", 46, new Color(1f, 1f, 1f, 0.22f),
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-70f, 70f), new Vector2(190f, 190f));
            MakeButton(canvas, "InventoryButton", "背包", 30, new Color(1f, 1f, 1f, 0.20f),
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -40f), new Vector2(140f, 74f));
            MakeButton(canvas, "MenuButton", "菜单", 30, new Color(1f, 1f, 1f, 0.20f),
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -130f), new Vector2(140f, 74f));

            // 准星
            var crossGo = new GameObject("Crosshair", typeof(Image));
            crossGo.transform.SetParent(canvas, false);
            var cimg = crossGo.GetComponent<Image>();
            cimg.sprite = circle;
            cimg.color = new Color(1f, 1f, 1f, 0.55f);
            cimg.raycastTarget = false;
            var crt = (RectTransform)crossGo.transform;
            crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(12f, 12f);

            // 文本
            MakeLabel(canvas, "ObjectiveLabel", "", 30, new Color(0.65f, 0.85f, 1f, 0.95f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(1500f, 54f),
                TextAnchor.MiddleCenter, false);
            MakeLabel(canvas, "LoopLabel", "循环 1 / 4", 32, new Color(1f, 1f, 1f, 0.9f),
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -16f), new Vector2(300f, 50f),
                TextAnchor.UpperLeft, false);
            MakeLabel(canvas, "ToastLabel", "", 32, new Color(1f, 0.9f, 0.6f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(1200f, 60f),
                TextAnchor.MiddleCenter, false);
            MakeLabel(canvas, "PromptLabel", "", 34, new Color(1f, 1f, 1f, 0.95f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -100f), new Vector2(800f, 60f),
                TextAnchor.MiddleCenter, false);
            MakeLabel(canvas, "HintLabel",
#if UNITY_EDITOR || UNITY_STANDALONE
                "左下摇杆移动 ｜ 右侧拖动转视角 ｜ 轻点或【交互】按钮互动 ｜ 编辑器: WASD + 右键", 26,
#else
                "左下摇杆移动 ｜ 右侧拖动转视角 ｜ 轻点或【交互】按钮互动", 26,
#endif
                new Color(1f, 1f, 1f, 0.55f),
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f, 16f), new Vector2(1100f, 40f),
                TextAnchor.LowerLeft, false);

            // 旁白（背景+大字）
            var nbgGo = new GameObject("NarrationBG", typeof(Image));
            nbgGo.transform.SetParent(canvas, false);
            var nbg = nbgGo.GetComponent<Image>();
            nbg.color = new Color(0f, 0f, 0f, 0f);
            nbg.raycastTarget = false;
            Stretch(nbg.rectTransform);
            var nbgRT = nbg.rectTransform;
            nbgRT.anchorMin = new Vector2(0f, 0.3f);
            nbgRT.anchorMax = new Vector2(1f, 0.75f);
            nbgRT.sizeDelta = Vector2.zero;
            MakeLabel(canvas, "NarrationLabel", "", 40, new Color(0.95f, 0.95f, 0.92f, 1f),
                new Vector2(0.1f, 0.32f), new Vector2(0.9f, 0.73f), Vector2.zero, Vector2.zero,
                TextAnchor.MiddleCenter, false);

            // 注意：本方法收到的 canvas 参数是 SafeRoot，而 TouchLookZone 挂在 Canvas 根上——
            // 必须从根取，否则取出 null 存进场景，真机右侧滑动永远无法转视角（模拟器实测抓到此根因）。
            ProjectSetup.BindPlayerInput(joy, canvas.root.GetComponent<TouchLookZone>());
        }

        static void BuildInspect(Transform canvas)
        {
            var panel = NewPanel(canvas, "InspectPanel", new Color(0f, 0f, 0f, 0.9f));
            var title = MakeLabel(panel.transform, "TitleText", "", 40, Color.white,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(1400f, 70f),
                TextAnchor.UpperCenter, true);
            var body = MakeLabel(panel.transform, "BodyText", "", 32, new Color(0.92f, 0.92f, 0.9f, 1f),
                new Vector2(0.10f, 0.14f), new Vector2(0.90f, 0.86f), Vector2.zero, Vector2.zero,
                TextAnchor.UpperLeft, true);
            AccentLine(panel.transform, 0.905f, 0.26f);
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            MakeButton(panel.transform, "CloseButton", "关闭", 36, new Color(1f, 1f, 1f, 0.25f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(280f, 90f));
            var comp = panel.AddComponent<InspectPanel>();
            comp.titleText = title;
            comp.bodyText = body;
        }

        static void BuildKeypad(Transform canvas)
        {
            var panel = NewPanel(canvas, "KeypadPanel", new Color(0.08f, 0.09f, 0.12f, 0.97f), new Vector2(500f, 760f));
            var title = MakeLabel(panel.transform, "TitleText", "", 34, Color.white,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(460f, 60f),
                TextAnchor.MiddleCenter, true);
            var display = MakeLabel(panel.transform, "Display", "____", 46, new Color(0.7f, 1f, 0.8f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(440f, 70f),
                TextAnchor.MiddleCenter, true);

            AccentLine(panel.transform, 0.888f, 0.42f);

            var digits = new GameObject("Digits", typeof(RectTransform), typeof(GridLayoutGroup));
            digits.transform.SetParent(panel.transform, false);
            var drt = (RectTransform)digits.transform;
            drt.anchorMin = drt.anchorMax = new Vector2(0.5f, 0.5f);
            drt.anchoredPosition = new Vector2(0f, -70f);
            drt.sizeDelta = new Vector2(415f, 480f);
            var grid = digits.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(128f, 110f);
            grid.spacing = new Vector2(10f, 10f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            for (int i = 1; i <= 9; i++) GridButton(digits.transform, "Digit_" + i, i.ToString(), 40);
            GridButton(digits.transform, "Clear", "清空", 28);
            GridButton(digits.transform, "Digit_0", "0", 40);
            GridButton(digits.transform, "Enter", "确定", 30, new Color(0.5f, 0.8f, 0.6f, 0.35f));
            MakeButton(panel.transform, "CloseButton", "关闭", 30, new Color(1f, 1f, 1f, 0.15f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(240f, 70f));
            var comp = panel.AddComponent<KeypadPanel>();
            comp.titleText = title;
            comp.display = display;
        }

        static void BuildSwitch(Transform canvas)
        {
            var panel = NewPanel(canvas, "SwitchPanel", new Color(0.08f, 0.09f, 0.12f, 0.97f), new Vector2(760f, 560f));
            var title = MakeLabel(panel.transform, "TitleText", "", 34, Color.white,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(700f, 60f),
                TextAnchor.MiddleCenter, true);
            var progress = MakeLabel(panel.transform, "ProgressText", "0 / 0", 30, new Color(0.7f, 1f, 0.8f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(400f, 50f),
                TextAnchor.MiddleCenter, true);
            var slotGo = new GameObject("SlotRoot", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            slotGo.transform.SetParent(panel.transform, false);
            var srt = (RectTransform)slotGo.transform;
            srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(0f, 20f);
            AccentLine(panel.transform, 0.85f, 0.5f);
            srt.sizeDelta = new Vector2(680f, 130f);
            var hlg = slotGo.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 18f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            MakeButton(panel.transform, "ResetButton", "复位", 28, new Color(1f, 1f, 1f, 0.15f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-160f, 30f), new Vector2(200f, 74f));
            MakeButton(panel.transform, "CloseButton", "关闭", 30, new Color(1f, 1f, 1f, 0.15f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(160f, 30f), new Vector2(200f, 74f));
            var comp = panel.AddComponent<SwitchPanel>();
            comp.titleText = title;
            comp.progressText = progress;
            comp.slotRoot = srt;
        }

        static void BuildTuner(Transform canvas)
        {
            var panel = NewPanel(canvas, "TunerPanel", new Color(0.06f, 0.07f, 0.10f, 0.97f), new Vector2(640f, 560f));
            var title = MakeLabel(panel.transform, "TitleText", "", 34, Color.white,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(580f, 60f),
                TextAnchor.MiddleCenter, true);
            var hint = MakeLabel(panel.transform, "HintLabel", "", 26, new Color(0.8f, 0.85f, 0.9f, 0.9f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -105f), new Vector2(580f, 80f),
                TextAnchor.MiddleCenter, true);
            var freq = MakeLabel(panel.transform, "FreqText", "87.0 MHz", 52, new Color(1f, 1f, 1f, 0.85f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 70f), new Vector2(500f, 80f),
                TextAnchor.MiddleCenter, true);
            AccentLine(panel.transform, 0.85f, 0.45f);
            MakeButton(panel.transform, "Minus1", "-1.0", 32, new Color(1f, 1f, 1f, 0.18f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-230f, 150f), new Vector2(130f, 90f));
            MakeButton(panel.transform, "Minus01", "-0.1", 32, new Color(1f, 1f, 1f, 0.18f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-80f, 150f), new Vector2(130f, 90f));
            MakeButton(panel.transform, "Plus01", "+0.1", 32, new Color(1f, 1f, 1f, 0.18f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(80f, 150f), new Vector2(130f, 90f));
            MakeButton(panel.transform, "Plus1", "+1.0", 32, new Color(1f, 1f, 1f, 0.18f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(230f, 150f), new Vector2(130f, 90f));
            MakeButton(panel.transform, "CloseButton", "关闭", 30, new Color(1f, 1f, 1f, 0.15f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(240f, 74f));
            var comp = panel.AddComponent<TunerPanel>();
            comp.titleText = title;
            comp.freqText = freq;
            comp.hintLabel = hint;
        }

        static void BuildSort(Transform canvas)
        {
            var panel = NewPanel(canvas, "SortPanel", new Color(0.08f, 0.07f, 0.10f, 0.97f), new Vector2(980f, 620f));
            var title = MakeLabel(panel.transform, "TitleText", "", 34, Color.white,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(900f, 60f),
                TextAnchor.MiddleCenter, true);
            var hint = MakeLabel(panel.transform, "HintLabel", "", 26, new Color(0.85f, 0.85f, 0.9f, 0.9f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -108f), new Vector2(900f, 70f),
                TextAnchor.MiddleCenter, true);
            var slotGo = new GameObject("SlotRoot", typeof(RectTransform), typeof(GridLayoutGroup));
            slotGo.transform.SetParent(panel.transform, false);
            var srt = (RectTransform)slotGo.transform;
            srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(0f, 10f);
            srt.sizeDelta = new Vector2(880f, 220f);
            AccentLine(panel.transform, 0.865f, 0.45f);
            var grid = slotGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(200f, 200f);
            grid.spacing = new Vector2(20f, 10f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            MakeButton(panel.transform, "ConfirmButton", "确认排列", 30, new Color(0.5f, 0.8f, 0.6f, 0.3f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(300f, 80f));
            MakeButton(panel.transform, "CloseButton", "关闭", 30, new Color(1f, 1f, 1f, 0.15f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(240f, 70f));
            var comp = panel.AddComponent<SortPanel>();
            comp.titleText = title;
            comp.hintLabel = hint;
            comp.slotRoot = srt;
        }

        static void BuildValve(Transform canvas)
        {
            var panel = NewPanel(canvas, "ValvePanel", new Color(0.07f, 0.08f, 0.07f, 0.97f), new Vector2(820f, 600f));
            var title = MakeLabel(panel.transform, "TitleText", "", 34, Color.white,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(740f, 60f),
                TextAnchor.MiddleCenter, true);
            var gauge = MakeLabel(panel.transform, "GaugeText", "", 30, new Color(0.7f, 1f, 0.8f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(740f, 60f),
                TextAnchor.MiddleCenter, true);
            var slotGo = new GameObject("SlotRoot", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            slotGo.transform.SetParent(panel.transform, false);
            var srt = (RectTransform)slotGo.transform;
            srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(0f, 0f);
            srt.sizeDelta = new Vector2(700f, 170f);
            AccentLine(panel.transform, 0.86f, 0.45f);
            var hlg = slotGo.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 24f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            MakeButton(panel.transform, "ConfirmButton", "确认读数", 30, new Color(0.5f, 0.8f, 0.6f, 0.3f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(300f, 80f));
            MakeButton(panel.transform, "CloseButton", "关闭", 30, new Color(1f, 1f, 1f, 0.15f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(240f, 70f));
            var comp = panel.AddComponent<ValvePanel>();
            comp.titleText = title;
            comp.gaugeText = gauge;
            comp.slotRoot = srt;
        }

        static void BuildTerminal(Transform canvas)
        {
            var panel = NewPanel(canvas, "TerminalPanel", new Color(0.02f, 0.04f, 0.03f, 0.96f));
            var title = MakeLabel(panel.transform, "TitleText", "", 36, new Color(0.6f, 1f, 0.75f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(1400f, 70f),
                TextAnchor.UpperCenter, true);
            var body = MakeLabel(panel.transform, "BodyText", "", 30, new Color(0.75f, 0.95f, 0.8f, 1f),
                new Vector2(0.10f, 0.36f), new Vector2(0.90f, 0.88f), Vector2.zero, Vector2.zero,
                TextAnchor.UpperLeft, true);
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            var choiceGo = new GameObject("ChoiceRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
            choiceGo.transform.SetParent(panel.transform, false);
            var crt = (RectTransform)choiceGo.transform;
            crt.anchorMin = new Vector2(0.15f, 0.10f);
            crt.anchorMax = new Vector2(0.85f, 0.34f);
            crt.sizeDelta = Vector2.zero;
            var vlg = choiceGo.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 14f;
            AccentLine(panel.transform, 0.905f, 0.26f);
            MakeButton(panel.transform, "PrevButton", "◀ 上一页", 28, new Color(1f, 1f, 1f, 0.15f),
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(60f, 50f), new Vector2(240f, 74f));
            MakeButton(panel.transform, "NextButton", "下一页 ▶", 28, new Color(1f, 1f, 1f, 0.15f),
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-60f, 50f), new Vector2(240f, 74f));
            MakeButton(panel.transform, "CloseButton", "关闭", 30, new Color(1f, 1f, 1f, 0.15f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(240f, 74f));
            var comp = panel.AddComponent<TerminalPanel>();
            comp.titleText = title;
            comp.bodyText = body;
            comp.choiceRoot = crt;
        }

        static void BuildInventory(Transform canvas)
        {
            var panel = NewPanel(canvas, "InventoryPanel", new Color(0.02f, 0.02f, 0.03f, 0.90f));
            MakeLabel(panel.transform, "Title", "背 包", 40, Color.white,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(600f, 70f),
                TextAnchor.UpperCenter, true);
            var mode = MakeLabel(panel.transform, "ModeText", "点物品查看 / 点两件尝试组合", 26, new Color(0.7f, 0.8f, 1f, 0.9f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -115f), new Vector2(1200f, 50f),
                TextAnchor.MiddleCenter, true);
            var slotGo = new GameObject("SlotRoot", typeof(RectTransform), typeof(GridLayoutGroup));
            slotGo.transform.SetParent(panel.transform, false);
            var srt = (RectTransform)slotGo.transform;
            srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(0f, 90f);
            srt.sizeDelta = new Vector2(700f, 360f);
            AccentLine(panel.transform, 0.9f, 0.22f);
            var grid = slotGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(160f, 160f);
            grid.spacing = new Vector2(15f, 15f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            var detail = MakeLabel(panel.transform, "DetailText", "", 30, new Color(0.92f, 0.92f, 0.9f, 1f),
                new Vector2(0.08f, 0.10f), new Vector2(0.92f, 0.34f), Vector2.zero, Vector2.zero,
                TextAnchor.UpperLeft, true);
            detail.horizontalOverflow = HorizontalWrapMode.Wrap;
            MakeButton(panel.transform, "CloseButton", "关闭", 36, new Color(1f, 1f, 1f, 0.25f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(280f, 90f));
            var comp = panel.AddComponent<InventoryUI>();
            comp.slotRoot = srt;
            comp.detailText = detail;
            comp.modeText = mode;
        }

        static void BuildPause(Transform canvas)
        {
            var panel = NewPanel(canvas, "PausePanel", new Color(0.02f, 0.02f, 0.03f, 0.85f));
            MakeLabel(panel.transform, "Title", "已 暂 停", 44, Color.white,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -260f), new Vector2(600f, 80f),
                TextAnchor.UpperCenter, true);
            AccentLine(panel.transform, 0.68f, 0.3f);
            MakeButton(panel.transform, "BtnResume", "继续游戏", 34, new Color(1f, 1f, 1f, 0.2f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(420f, 96f));
            MakeButton(panel.transform, "BtnSettings", "设 置", 34, new Color(1f, 1f, 1f, 0.2f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(420f, 96f));
            MakeButton(panel.transform, "BtnMenu", "保存并回主菜单", 34, new Color(1f, 1f, 1f, 0.2f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -130f), new Vector2(420f, 96f));
            panel.AddComponent<PausePanel>();
        }

        static void BuildComputer(Transform canvas)
        {
            // 白底文档系统：左文件列表 + 右内容
            var panel = new GameObject("ComputerPanel", typeof(Image));
            panel.transform.SetParent(canvas, false);
            var img = panel.GetComponent<Image>();
            img.color = new Color(0.92f, 0.94f, 0.96f, 0.98f);
            Stretch(img.rectTransform);

            var title = MakeLabel(panel.transform, "TitleText", "", 38, new Color(0.1f, 0.15f, 0.25f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(1400f, 70f),
                TextAnchor.UpperCenter, true);

            AccentLine(panel.transform, 0.905f, 0.3f);

            var listGo = new GameObject("FileList", typeof(RectTransform), typeof(VerticalLayoutGroup));
            listGo.transform.SetParent(panel.transform, false);
            var lrt = (RectTransform)listGo.transform;
            lrt.anchorMin = new Vector2(0.03f, 0.08f);
            lrt.anchorMax = new Vector2(0.30f, 0.86f);
            lrt.sizeDelta = Vector2.zero;
            var vlg = listGo.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 10f;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = false;

            var body = MakeLabel(panel.transform, "BodyText", "", 30, new Color(0.08f, 0.1f, 0.16f, 1f),
                new Vector2(0.34f, 0.08f), new Vector2(0.97f, 0.86f), Vector2.zero, Vector2.zero,
                TextAnchor.UpperLeft, true);
            body.horizontalOverflow = HorizontalWrapMode.Wrap;

            MakeButton(panel.transform, "CloseButton", "关 闭", 34, new Color(0.2f, 0.3f, 0.45f, 0.9f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(300f, 80f));

            var comp = panel.AddComponent<Interaction.ComputerPanel>();
            comp.titleText = title;
            comp.bodyText = body;
            comp.fileList = lrt;
        }

        static void BuildSettings(Transform canvas)
        {
            // 900高：20:9横屏(2400x1080, scale≈1.118)下 900*1.118≈1006 < 1080，整屏内且四角均在屏内。
            // 内容纵向压缩排布：反转Y轴与触摸诊断两开关并排于底部（旧版850高内容排到-910，开关悬在面板外60px）。
            var panel = NewPanel(canvas, "SettingsPanel", new Color(0.06f, 0.07f, 0.10f, 0.96f), new Vector2(880f, 900f));
            MakeLabel(panel.transform, "Title", "设 置", 40, Color.white,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(600f, 62f),
                TextAnchor.UpperCenter, true);

            AccentLine(panel.transform, 0.905f, 0.4f);

            MakeLabel(panel.transform, "LookLabel", "视角灵敏度", 29, new Color(0.9f, 0.9f, 0.9f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(600f, 46f),
                TextAnchor.MiddleCenter, false);
            var lookState = MakeLabel(panel.transform, "LookState", "中", 29, new Color(0.7f, 1f, 0.8f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -148f), new Vector2(300f, 42f),
                TextAnchor.MiddleCenter, false);
            MakeButton(panel.transform, "LookLow", "低", 29, new Color(1f, 1f, 1f, 0.15f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-230f, -204f), new Vector2(200f, 68f));
            MakeButton(panel.transform, "LookMid", "中", 29, new Color(1f, 1f, 1f, 0.15f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -204f), new Vector2(200f, 68f));
            MakeButton(panel.transform, "LookHigh", "高", 29, new Color(1f, 1f, 1f, 0.15f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(230f, -204f), new Vector2(200f, 68f));

            MakeLabel(panel.transform, "SmoothLabel", "镜头平滑", 29, new Color(0.9f, 0.9f, 0.9f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -268f), new Vector2(600f, 46f),
                TextAnchor.MiddleCenter, false);
            var smoothState = MakeLabel(panel.transform, "SmoothState", "中", 29, new Color(0.7f, 1f, 0.8f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -306f), new Vector2(300f, 42f),
                TextAnchor.MiddleCenter, false);
            MakeButton(panel.transform, "SmoothLow", "低", 29, new Color(1f, 1f, 1f, 0.15f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-230f, -366f), new Vector2(200f, 68f));
            MakeButton(panel.transform, "SmoothMid", "中", 29, new Color(1f, 1f, 1f, 0.15f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -366f), new Vector2(200f, 68f));
            MakeButton(panel.transform, "SmoothHigh", "高", 29, new Color(1f, 1f, 1f, 0.15f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(230f, -366f), new Vector2(200f, 68f));

            MakeLabel(panel.transform, "MoveLabel", "摇杆灵敏度", 29, new Color(0.9f, 0.9f, 0.9f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -430f), new Vector2(600f, 46f),
                TextAnchor.MiddleCenter, false);
            var moveState = MakeLabel(panel.transform, "MoveState", "中", 29, new Color(0.7f, 1f, 0.8f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -468f), new Vector2(300f, 42f),
                TextAnchor.MiddleCenter, false);
            MakeButton(panel.transform, "MoveLow", "低", 29, new Color(1f, 1f, 1f, 0.15f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-230f, -528f), new Vector2(200f, 68f));
            MakeButton(panel.transform, "MoveMid", "中", 29, new Color(1f, 1f, 1f, 0.15f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -528f), new Vector2(200f, 68f));
            MakeButton(panel.transform, "MoveHigh", "高", 29, new Color(1f, 1f, 1f, 0.15f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(230f, -528f), new Vector2(200f, 68f));

            MakeLabel(panel.transform, "VolLabel", "主音量", 29, new Color(0.9f, 0.9f, 0.9f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -588f), new Vector2(300f, 46f),
                TextAnchor.MiddleCenter, false);
            var vol = MakeSlider(panel.transform, "VolumeSlider", new Vector2(0f, -632f));
            var volVal = MakeLabel(panel.transform, "VolumeValue", "80%", 27, new Color(0.7f, 1f, 0.8f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-170f, -672f), new Vector2(300f, 40f),
                TextAnchor.MiddleCenter, false);

            MakeLabel(panel.transform, "FovLabel", "视野角度", 29, new Color(0.9f, 0.9f, 0.9f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -726f), new Vector2(300f, 46f),
                TextAnchor.MiddleCenter, false);
            var fov = MakeSlider(panel.transform, "FovSlider", new Vector2(0f, -770f));
            var fovVal = MakeLabel(panel.transform, "FovValue", "65", 27, new Color(0.7f, 1f, 0.8f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-170f, -812f), new Vector2(300f, 40f),
                TextAnchor.MiddleCenter, false);

            // 底部三件套的命中区必须两两不交：开关行 y=-800（勾选框底沿高于关闭按钮顶沿），
            // 关闭按钮退到右下角。此前开关框与关闭按钮在屏幕上重叠，点开关会误触关闭（模拟器实测复现）。
            MakeToggle(panel.transform, "InvertToggle", "反转Y轴", new Vector2(-200f, -800f), 380f);
            MakeToggle(panel.transform, "TouchDebugToggle", "触摸诊断", new Vector2(200f, -800f), 380f);
            MakeButton(panel.transform, "CloseButton", "关 闭", 31, new Color(1f, 1f, 1f, 0.2f),
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-30f, 26f), new Vector2(270f, 68f));

            var comp = panel.AddComponent<SettingsPanel>();
            comp.volumeSlider = vol;
            comp.fovSlider = fov;
            comp.invertToggle = MakeToggleRef(panel.transform, "InvertToggle");
            comp.touchDebugToggle = MakeToggleRef(panel.transform, "TouchDebugToggle");
            comp.volumeValue = volVal;
            comp.fovValue = fovVal;
            comp.lookState = lookState;
            comp.smoothState = smoothState;
            comp.moveState = moveState;
        }

        static Toggle MakeToggleRef(Transform parent, string name)
        {
            var t = parent.Find(name);
            return t != null ? t.GetComponent<Toggle>() : null;
        }

        static void BuildMainMenu(Transform canvas)
        {
            // 背景由 MenuCamera 渲染的独立走廊小场景提供（与游戏完全分离）。
            // 这里仅一层轻遮罩保证文字可读——无横带分块，无黑白分界感。
            var panel = NewPanel(canvas, "MainMenu", new Color(0.01f, 0.015f, 0.03f, 0.30f));

            // 门牌徽记：铭牌 + "13"（与游戏内第十三扇门的门牌同源）
            var plateGo = new GameObject("DoorPlate", typeof(Image));
            plateGo.transform.SetParent(panel.transform, false);
            var plateImg = plateGo.GetComponent<Image>();
            plateImg.color = new Color(0.09f, 0.11f, 0.15f, 0.88f);
            plateImg.raycastTarget = false;
            var prt = (RectTransform)plateGo.transform;
            prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 1f);
            prt.anchoredPosition = new Vector2(0f, -108f);
            prt.sizeDelta = new Vector2(150f, 84f);
            MakeLabel(plateGo.transform, "PlateText", "13", 44, new Color(0.75f, 0.85f, 0.95f, 1f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(140f, 78f),
                TextAnchor.MiddleCenter, false);
            Band(plateGo.transform, "PlateLine",
                new Vector2(0.08f, 0.14f), new Vector2(0.92f, 0.175f), new Color(0.5f, 0.76f, 0.86f, 0.55f));

            var title = MakeLabel(panel.transform, "TitleText", "第十三扇门", 76, new Color(0.93f, 0.94f, 0.97f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -255f), new Vector2(1400f, 120f),
                TextAnchor.UpperCenter, false);
            MakeLabel(panel.transform, "SubText", "失 忆 档 案", 38, new Color(0.6f, 0.68f, 0.78f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -385f), new Vector2(1400f, 70f),
                TextAnchor.UpperCenter, false);
            var subtitle = MakeLabel(panel.transform, "SubtitleText", "", 27, new Color(0.58f, 0.63f, 0.72f, 0.92f),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -470f), new Vector2(1400f, 50f),
                TextAnchor.UpperCenter, false);

            MenuButton(panel.transform, "BtnNew", "新 游 戏", 36, new Vector2(0f, 120f));
            MenuButton(panel.transform, "BtnContinue", "继 续", 36, new Vector2(0f, 0f));
            MenuButton(panel.transform, "BtnSettings", "设 置", 36, new Vector2(0f, -120f));
            MenuButton(panel.transform, "BtnQuit", "退 出", 36, new Vector2(0f, -240f));

            MakeLabel(panel.transform, "VerText", "最终版 2.0 ｜ 回廊7号 · 记忆科学部", 24, new Color(0.55f, 0.6f, 0.67f, 0.9f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 38f), new Vector2(1200f, 40f),
                TextAnchor.LowerCenter, false);
            var comp = panel.AddComponent<MainMenu>();
            comp.titleText = title;
            comp.subtitleText = subtitle;
        }

        /// <summary>菜单主按钮：左侧青灰刻线（档案标签感）。</summary>
        static void MenuButton(Transform parent, string name, string label, int fontSize, Vector2 pos)
        {
            var b = MakeButton(parent, name, label, fontSize, new Color(0.88f, 0.92f, 1f, 0.24f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(440f, 96f));
            var tick = new GameObject("Tick", typeof(Image));
            tick.transform.SetParent(b.transform, false);
            var ti = tick.GetComponent<Image>();
            ti.color = new Color(0.5f, 0.78f, 0.9f, 0.65f);
            ti.raycastTarget = false;
            var trt = (RectTransform)tick.transform;
            trt.anchorMin = new Vector2(0f, 0.18f);
            trt.anchorMax = new Vector2(0f, 0.82f);
            trt.pivot = new Vector2(0f, 0.5f);
            trt.anchoredPosition = new Vector2(14f, 0f);
            trt.sizeDelta = new Vector2(6f, 0f);
        }

        /// <summary>菜单装饰带（纯色矩形，不拦截点击）。</summary>
        static void Band(Transform parent, string name, Vector2 aMin, Vector2 aMax, Color color)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            var rt = img.rectTransform;
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static void BuildFade(Transform canvas)
        {
            var fade = new GameObject("FadeOverlay", typeof(Image));
            fade.transform.SetParent(canvas, false);
            var fimg = fade.GetComponent<Image>();
            fimg.color = new Color(0f, 0f, 0f, 0f);
            fimg.raycastTarget = false;
            Stretch(fimg.rectTransform);
        }

        // ---------- 控件助手 ----------

        static GameObject NewPanel(Transform canvas, string name, Color color, Vector2? size = null)
        {
            var panel = new GameObject(name, typeof(Image));
            panel.transform.SetParent(canvas, false);
            var img = panel.GetComponent<Image>();
            img.color = color;
            var rt = img.rectTransform;
            if (size.HasValue)
            {
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = size.Value;
            }
            else Stretch(rt);
            return panel;
        }

        static Text MakeLabel(Transform parent, string name, string text, int fontSize, Color color,
            Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, TextAnchor align, bool raycast)
        {
            var go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = text;
            t.font = ProjectSetup.GetBuiltinFont();
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = raycast;
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return t;
        }

        static Button MakeButton(Transform parent, string name, string label, int fontSize, Color color,
            Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = circle;
            img.color = color;
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            AddLabel(go.transform, label, fontSize);
            return go.GetComponent<Button>();
        }

        static void GridButton(Transform parent, string name, string label, int fontSize)
            => GridButton(parent, name, label, fontSize, new Color(1f, 1f, 1f, 0.20f));

        static void GridButton(Transform parent, string name, string label, int fontSize, Color color)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().sprite = circle;
            go.GetComponent<Image>().color = color;
            AddLabel(go.transform, label, fontSize);
        }

        static void AddLabel(Transform parent, string label, int fontSize)
        {
            var labelGo = new GameObject("Label", typeof(Text));
            labelGo.transform.SetParent(parent, false);
            var t = labelGo.GetComponent<Text>();
            t.text = label;
            t.font = ProjectSetup.GetBuiltinFont();
            t.fontSize = fontSize;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.raycastTarget = false;
            var lrt = (RectTransform)labelGo.transform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.sizeDelta = Vector2.zero;
        }

        static Slider MakeSlider(Transform parent, string name, Vector2 pos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(620f, 44f);

            var bgGo = new GameObject("Background", typeof(Image));
            bgGo.transform.SetParent(go.transform, false);
            var bg = bgGo.GetComponent<Image>();
            bg.sprite = circle; bg.type = Image.Type.Sliced; bg.color = new Color(1f, 1f, 1f, 0.15f);
            var brt = (RectTransform)bgGo.transform;
            brt.anchorMin = new Vector2(0f, 0.25f); brt.anchorMax = new Vector2(1f, 0.75f); brt.sizeDelta = Vector2.zero;

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var fart = (RectTransform)fillArea.transform;
            fart.anchorMin = new Vector2(0f, 0.25f); fart.anchorMax = new Vector2(1f, 0.75f); fart.sizeDelta = Vector2.zero;
            var fillGo = new GameObject("Fill", typeof(Image));
            fillGo.transform.SetParent(fillArea.transform, false);
            var fill = fillGo.GetComponent<Image>();
            fill.sprite = circle; fill.type = Image.Type.Sliced; fill.color = new Color(0.5f, 0.85f, 0.6f, 0.8f);
            var frt = (RectTransform)fillGo.transform;
            frt.anchorMin = Vector2.zero; frt.anchorMax = new Vector2(0.6f, 1f); frt.sizeDelta = Vector2.zero;

            var handleArea = new GameObject("Handle Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            var hart = (RectTransform)handleArea.transform;
            hart.anchorMin = Vector2.zero; hart.anchorMax = Vector2.one; hart.sizeDelta = Vector2.zero;
            var handleGo = new GameObject("Handle", typeof(Image));
            handleGo.transform.SetParent(handleArea.transform, false);
            var handle = handleGo.GetComponent<Image>();
            handle.sprite = circle; handle.color = Color.white;
            var hrt = (RectTransform)handleGo.transform;
            hrt.sizeDelta = new Vector2(36f, 36f);

            var slider = go.GetComponent<Slider>();
            slider.fillRect = frt;
            slider.handleRect = hrt;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        static Toggle MakeToggle(Transform parent, string name, string label, Vector2 pos, float width = 420f)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(width, 50f);

            var bgGo = new GameObject("Background", typeof(Image));
            bgGo.transform.SetParent(go.transform, false);
            var bg = bgGo.GetComponent<Image>();
            bg.sprite = circle; bg.color = new Color(1f, 1f, 1f, 0.2f);
            var brt = (RectTransform)bgGo.transform;
            brt.anchorMin = new Vector2(0f, 0f); brt.anchorMax = new Vector2(0f, 1f);
            brt.pivot = new Vector2(0f, 0.5f);
            brt.anchoredPosition = new Vector2(25f, 0f);
            brt.sizeDelta = new Vector2(40f, 40f);

            var checkGo = new GameObject("Checkmark", typeof(Image));
            checkGo.transform.SetParent(bgGo.transform, false);
            var chk = checkGo.GetComponent<Image>();
            chk.sprite = circle; chk.color = new Color(0.5f, 0.9f, 0.6f, 1f);
            var ckrt = (RectTransform)checkGo.transform;
            ckrt.anchorMin = ckrt.anchorMax = new Vector2(0.5f, 0.5f);
            ckrt.sizeDelta = new Vector2(24f, 24f);

            var labelGo = new GameObject("Label", typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            var t = labelGo.GetComponent<Text>();
            t.text = label;
            t.font = ProjectSetup.GetBuiltinFont();
            t.fontSize = 28; t.color = Color.white; t.alignment = TextAnchor.MiddleLeft;
            var lrt = (RectTransform)labelGo.transform;
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.anchoredPosition = new Vector2(35f, 0f);
            lrt.sizeDelta = new Vector2(-60f, 0f);

            var tog = go.GetComponent<Toggle>();
            tog.graphic = chk;
            tog.targetGraphic = bg;
            return tog;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }
    }
}
