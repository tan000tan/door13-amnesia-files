using System.IO;
using Door13.Audio;
using Door13.Interaction;
using Door13.Inventory;
using Door13.Player;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering.Universal;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Door13.EditorTools
{
    /// <summary>
    /// 工程一键配置：玩家设置 + URP资产 + 场景全量重建。
    /// 场景由代码生成（纯自制资源原则），每次 EnsureScene 全量重建（杜绝重复对象）。
    /// </summary>
    public static class ProjectSetup
    {
        public const string ScenePath = "Assets/Scenes/Main.unity";
        const string CirclePng = "Assets/UI/Tex_Circle.png";
        const string RendererPath = "Assets/Settings/URP_Renderer.asset";
        const string PipelinePath = "Assets/Settings/URP_Pipeline.asset";
        public const string MenuLayer = "MenuScene";

        [MenuItem("Door13/1-工程配置+生成场景")]
        public static void SetupAll()
        {
            ApplyPlayerSettings();
            EnsureURP();
            EnsureScene();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Door13/2-仅重新生成场景")]
        public static void RegenerateScene()
        {
            EnsureScene();
            AssetDatabase.SaveAssets();
        }

        /// <summary>程序化径向暗角纹理：中心透明，边缘暗——低成本"胶片摄影感"。</summary>
        public static Sprite EnsureVignetteSprite()
        {
            const string Png = "Assets/UI/Tex_Vignette.png";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(Png);
            if (existing != null) return existing;

            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float c = (size - 1) * 0.5f;
            float maxD = c * Mathf.Sqrt(2f);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c)) / maxD;
                    float a = Mathf.Clamp01((d - 0.42f) / 0.58f);
                    a = a * a;                       // 平方衰减：中心更干净
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            tex.Apply();
            File.WriteAllBytes(Application.dataPath + Png.Substring("Assets".Length), tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(Png);
            var imp = (TextureImporter)AssetImporter.GetAtPath(Png);
            imp.textureType = TextureImporterType.Sprite;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(Png);
        }

        /// <summary>安卓玩家设置（上项目验证过的稳定组合）</summary>
        public static void ApplyPlayerSettings()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);

            PlayerSettings.companyName = "Door13 Studio";
            PlayerSettings.productName = "第十三扇门：失忆档案";
            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.Android, "com.door13.amnesia");
            PlayerSettings.bundleVersion = "2.1";
            PlayerSettings.Android.bundleVersionCode = 13;

            PlayerSettings.SetScriptingBackend(
                NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(
                NamedBuildTarget.Android, ManagedStrippingLevel.Medium);
            PlayerSettings.colorSpace = ColorSpace.Gamma;

            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,
                new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });

            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.optimizedFramePacing = true;

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
            EditorUserBuildSettings.buildAppBundle = false;
        }

        /// <summary>URP 资产与全局管线设置（MSAA/阴影在此配置，替代 QualitySettings）</summary>
        public static void EnsureURP()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                AssetDatabase.CreateFolder("Assets", "Settings");

            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (urp == null)
            {
                var rd = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rd, RendererPath);
                urp = UniversalRenderPipelineAsset.Create(rd);
                AssetDatabase.CreateAsset(urp, PipelinePath);
            }

            urp.msaaSampleCount = 4;          // 4x MSAA（上一版画质反馈）
            urp.shadowDistance = 20f;
            urp.supportsHDR = false;          // 低多边形+Gamma，省带宽

            // 软阴影开关在 Renderer 资产上（URP14 管线资产无此属性）
            var rdAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(RendererPath);
            if (rdAsset != null)
            {
                var so = new SerializedObject(rdAsset);
                var soft = so.FindProperty("m_SupportsSoftShadows");
                if (soft != null) { soft.boolValue = true; so.ApplyModifiedProperties(); }
            }

            if (GraphicsSettings.defaultRenderPipeline != urp)
                GraphicsSettings.defaultRenderPipeline = urp;

            // 内置管线时代的旧材质全部失效：删除，场景重建时按 URP/Lit 重新生成
            foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets/Materials" }))
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
        }

        /// <summary>场景全量重建：灯光+玩家+苏醒室+UI。可重复执行。</summary>
        public static void EnsureScene()
        {
            EnsureLayer(MenuLayer);
            EnsureFolders();
            EnsureCircleSprite();
            InventoryUI.EditorFont = GetBuiltinFont();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildLighting();
            BuildPlayer();
            RoomBuilder.Build();
            BuildMenuDiorama();
            UiBuilder.Build();

            // 运行时字体加载器：解决 Android IL2CPP 上字体加载失败导致文字不渲染
            var fontLoaderGo = new GameObject("RuntimeFontLoader");
            fontLoaderGo.AddComponent<Door13.RuntimeFontLoader>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log("SCENE_OK 场景已全量重建: " + ScenePath);
        }

        /// <summary>注册自定义层（主菜单独立场景层，与游戏渲染完全隔离）。</summary>
        static void EnsureLayer(string layerName)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0) return;
            var tm = new SerializedObject(assets[0]);
            var layers = tm.FindProperty("layers");
            for (int i = 8; i < 32; i++)
            {
                var slot = layers.GetArrayElementAtIndex(i);
                if (slot.stringValue == layerName) return;
                if (string.IsNullOrEmpty(slot.stringValue))
                {
                    slot.stringValue = layerName;
                    tm.ApplyModifiedProperties();
                    return;
                }
            }
        }

        /// <summary>
        /// 主菜单独立小场景（MenuScene 层，y+200 与游戏世界物理隔离）：
        /// 一条走廊尽头立着漆黑的第十三扇门，吊灯供电不稳地闪烁。
        /// 仅由 MenuCamera 渲染——主菜单与游戏场景完全分离，无穿透。
        /// </summary>
        static void BuildMenuDiorama()
        {
            int layer = LayerMask.NameToLayer(MenuLayer);
            var off = new Vector3(0f, 200f, 0f);

            var wall = RoomBuilder.Mat("M_Wall", new Color(0.50f, 0.52f, 0.56f));
            var floor = RoomBuilder.Mat("M_Floor", new Color(0.36f, 0.37f, 0.40f));
            var metal = RoomBuilder.Mat("M_Metal", new Color(0.42f, 0.45f, 0.50f));
            var dark = RoomBuilder.Mat("M_Dark", new Color(0.05f, 0.05f, 0.07f));
            var paper = RoomBuilder.Mat("M_Paper", new Color(0.72f, 0.70f, 0.65f));

            var root = new GameObject("MenuDiorama");

            GameObject B(string n, Vector3 localCenter, Vector3 size, Material m)
            {
                var g = RoomBuilder.Box(root.transform, n, localCenter + off, size, m, false);
                g.layer = layer;
                var col = g.GetComponent<Collider>();
                if (col != null) Object.DestroyImmediate(col);
                return g;
            }

            // 走廊外壳：4宽 × 14长 × 3.2高，尽头墙在 -Z
            B("MFloor", new Vector3(0f, -0.05f, 0f), new Vector3(4f, 0.1f, 14f), floor);
            B("MCeiling", new Vector3(0f, 3.2f, 0f), new Vector3(4f, 0.1f, 14f), wall);
            B("MWallL", new Vector3(-2.05f, 1.6f, 0f), new Vector3(0.1f, 3.2f, 14f), wall);
            B("MWallR", new Vector3(2.05f, 1.6f, 0f), new Vector3(0.1f, 3.2f, 14f), wall);
            B("MWallFar", new Vector3(0f, 1.6f, -7f), new Vector3(4.2f, 3.2f, 0.1f), wall);

            // 第十三扇门：门框 + 漆黑门扇 + 铭牌
            B("MDoorPostL", new Vector3(-0.72f, 1.15f, -6.88f), new Vector3(0.18f, 2.3f, 0.14f), metal);
            B("MDoorPostR", new Vector3(0.72f, 1.15f, -6.88f), new Vector3(0.18f, 2.3f, 0.14f), metal);
            B("MDoorLintel", new Vector3(0f, 2.38f, -6.88f), new Vector3(1.62f, 0.18f, 0.14f), metal);
            B("MDoorLeaf", new Vector3(0f, 1.13f, -6.93f), new Vector3(1.26f, 2.26f, 0.06f), dark);
            B("MDoorPlate", new Vector3(0f, 1.95f, -6.84f), new Vector3(0.52f, 0.26f, 0.03f), metal);
            var tm = RoomBuilder.WallText(root.transform, off + new Vector3(-0.18f, 1.86f, -6.82f), 180f, "13", 30, 0.1f);
            tm.color = new Color(0.75f, 0.82f, 0.9f);
            tm.gameObject.layer = layer;

            // 吊灯（自发光条 + 暖光，供电不稳闪烁——菜单的"活物感"）
            var lampMat = RoomBuilder.Mat("M_MenuLamp", new Color(0.95f, 0.92f, 0.85f));
            lampMat.EnableKeyword("_EMISSION");
            lampMat.SetColor("_EmissionColor", new Color(0.9f, 0.85f, 0.7f) * 1.15f);
            B("MLampBar", new Vector3(0f, 2.92f, -2f), new Vector3(0.9f, 0.06f, 0.18f), lampMat);
            B("MLampCord", new Vector3(0f, 3.06f, -2f), new Vector3(0.03f, 0.24f, 0.03f), dark);

            Light L(string n, Vector3 localPos, Color c, float inten, float range)
            {
                var l = RoomBuilder.PointLight(root.transform, n, localPos + off, c, inten, range);
                l.gameObject.layer = layer;
                l.cullingMask = 1 << layer;   // 菜单灯绝不影响游戏世界
                return l;
            }
            var lamp = L("MLampLight", new Vector3(0f, 2.7f, -2f), new Color(1f, 0.88f, 0.72f), 1.1f, 8f);
            var fl = lamp.gameObject.AddComponent<Audio.FlickerLight>();
            fl.amount = 0.34f; fl.speed = 1.3f;
            L("MDoorRim", new Vector3(0f, 1.6f, -5.4f), new Color(0.35f, 0.5f, 0.75f), 0.55f, 5f);
            L("MNearFill", new Vector3(0f, 2.8f, 3.4f), new Color(0.9f, 0.93f, 1f), 0.32f, 6f);

            // 环境细节：散落文件 + 倾倒椅子（与游戏内R0呼应）
            for (int i = 0; i < 4; i++)
            {
                var p = B("MPaper" + i, new Vector3(-1.4f + i * 0.9f, 0.012f, 2.2f - (i % 2) * 1.1f),
                    new Vector3(0.3f, 0.012f, 0.4f), paper);
                p.transform.rotation = Quaternion.Euler(0f, i * 41f, 0f);
            }
            var chair = B("MChair", new Vector3(1.4f, 0.2f, 0.6f), new Vector3(0.45f, 0.36f, 0.45f), metal);
            chair.transform.rotation = Quaternion.Euler(0f, 30f, 96f);

            // 菜单专用相机：仅渲染 MenuScene 层；初始禁用，MainMenu 打开时启用
            var mcGo = new GameObject("MenuCamera");
            mcGo.transform.position = off + new Vector3(0f, 1.62f, 5.2f);
            mcGo.transform.rotation = Quaternion.Euler(0f, 180f, 0f);   // 望向尽头的门
            var mc = mcGo.AddComponent<Camera>();
            mc.clearFlags = CameraClearFlags.SolidColor;
            mc.backgroundColor = new Color(0.012f, 0.018f, 0.032f);
            mc.cullingMask = 1 << layer;
            mc.fieldOfView = 58f;
            mc.nearClipPlane = 0.05f;
            mc.farClipPlane = 60f;
            mc.depth = 5f;
            mc.enabled = false;   // 无 AudioListener：监听者始终只有玩家身上那个
        }

        static void EnsureFolders()
        {
            string[] folders = { "Scenes", "Materials", "UI", "Prefabs", "Models", "Audio", "Data", "Settings" };
            foreach (var f in folders)
                if (!AssetDatabase.IsValidFolder("Assets/" + f))
                    AssetDatabase.CreateFolder("Assets", f);
        }

        /// <summary>URP/Lit 材质；按名称自动挂程序化纹理（地面砖/墙板/金属），消除平涂测试感</summary>
        public static Material EnsureMaterial(string path, Color color)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                mat = new Material(shader != null ? shader : Shader.Find("Standard"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetColor("_BaseColor", color);

            string n = System.IO.Path.GetFileNameWithoutExtension(path);
            // 统一材质风格：低多边形 + 程序化纹理 + 统一粗糙度/金属度体系
            // 地面：中粗糙度，无金属感，砖缝纹理
            if (n.Contains("Floor") || n.Contains("Floor2"))
            {
                mat.SetFloat("_Smoothness", 0.6f);
                mat.SetFloat("_Metallic", 0f);
                mat.mainTexture = EnsureTex("Tex_FloorTiles", 256, FloorTilePix);
                mat.mainTextureScale = new Vector2(4f, 3f);
            }
            // 墙面：较粗糙，无金属感，板缝+霉斑纹理
            else if (n.Contains("Wall") || n.Contains("Wall2"))
            {
                mat.SetFloat("_Smoothness", 0.75f);
                mat.SetFloat("_Metallic", 0f);
                mat.mainTexture = EnsureTex("Tex_WallPanel", 256, WallPanelPix);
                mat.mainTextureScale = new Vector2(5f, 2f);
            }
            // 金属：光滑+高金属度，锈迹纹理
            else if (n.Contains("Metal") || n.Contains("Door") || n.Contains("Pipe") || n.Contains("Handle") || n.Contains("Frame") || n.Contains("Plate") || n.Contains("Safe") || n.Contains("Lock") || n.Contains("Keycard") || n.Contains("Badge") || n.Contains("V2Card") || n.Contains("CtrlCard") || n.Contains("ArchiveCard"))
            {
                mat.SetFloat("_Smoothness", 0.3f);
                mat.SetFloat("_Metallic", 0.8f);
                mat.mainTexture = EnsureTex("Tex_Metal", 128, MetalPix);
                mat.mainTextureScale = new Vector2(2f, 2f);
            }
            // 木头：中等粗糙度，无金属感
            else if (n.Contains("Wood") || n.Contains("Desk") || n.Contains("Chair") || n.Contains("Carton") || n.Contains("Mug") || n.Contains("Pen") || n.Contains("Notebook") || n.Contains("Cabinet") || n.Contains("Locker") || n.Contains("Slipper") || n.Contains("Shoe"))
            {
                mat.SetFloat("_Smoothness", 0.5f);
                mat.SetFloat("_Metallic", 0f);
            }
            // 屏幕/灯具：低粗糙度，无金属感（自发光）
            else if (n.Contains("Screen") || n.Contains("Lamp") || n.Contains("CRT") || n.Contains("Monitor") || n.Contains("Terminal"))
            {
                mat.SetFloat("_Smoothness", 0.1f);
                mat.SetFloat("_Metallic", 0f);
            }
            // 纸张/便签/布料：高粗糙度，无金属感
            else if (n.Contains("Paper") || n.Contains("Note") || n.Contains("Sticky") || n.Contains("Fabric") || n.Contains("Pillow") || n.Contains("Mattress") || n.Contains("Poster") || n.Contains("Tape") || n.Contains("Manifest") || n.Contains("Photo") || n.Contains("Board") || n.Contains("Pin") || n.Contains("Hole") || n.Contains("RedMark") || n.Contains("Figure"))
            {
                mat.SetFloat("_Smoothness", 0.85f);
                mat.SetFloat("_Metallic", 0f);
            }
            // 深色/暗色材质：统一为中粗糙度
            else if (n.Contains("Dark") || n.Contains("Stain") || n.Contains("Print") || n.Contains("Chalk") || n.Contains("Crack") || n.Contains("Wire") || n.Contains("Cable") || n.Contains("Peel") || n.Contains("Shadow"))
            {
                mat.SetFloat("_Smoothness", 0.7f);
                mat.SetFloat("_Metallic", 0f);
            }
            // 黄色/警示材质：中等粗糙度
            else if (n.Contains("Yellow") || n.Contains("Warning"))
            {
                mat.SetFloat("_Smoothness", 0.6f);
                mat.SetFloat("_Metallic", 0f);
            }
            // 玩家/角色：中粗糙度
            else if (n.Contains("Player") || n.Contains("Body") || n.Contains("Head"))
            {
                mat.SetFloat("_Smoothness", 0.6f);
                mat.SetFloat("_Metallic", 0f);
            }
            // 默认：统一中粗糙度+无金属
            else
            {
                mat.SetFloat("_Smoothness", 0.6f);
                mat.SetFloat("_Metallic", 0f);
            }
            return mat;
        }

        // ---- 程序化纹理（灰度变化，由材质 _BaseColor 染色）----
        public static Texture2D EnsureTex(string name, int size, System.Func<int, int, Color> gen)
        {
            string path = "Assets/Textures/" + name + ".png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null) return existing;

            if (!AssetDatabase.IsValidFolder("Assets/Textures"))
                AssetDatabase.CreateFolder("Assets", "Textures");
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            var rand = new System.Random(2026);
            Color c;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    c = gen(x, y);
                    c.r = Mathf.Clamp01(c.r + (float)rand.NextDouble() * 0.04f - 0.02f);
                    c.g = c.b = c.r;
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            System.IO.File.WriteAllBytes(Application.dataPath + "/Textures/" + name + ".png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.wrapMode = TextureWrapMode.Repeat;
            imp.mipmapEnabled = true;
            imp.mipmapFilter = TextureImporterMipFilter.BoxFilter;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Color FloorTilePix(int x, int y)
        {
            if (x % 64 < 2 || y % 64 < 2) return new Color(0.45f, 0.45f, 0.45f);   // 砖缝（更深）
            float cell = (((x / 64) * 7 + (y / 64) * 13) % 10) / 10f;
            // 污渍：随机深色斑块
            float stain = Mathf.PerlinNoise(x * 0.02f, y * 0.02f);
            float v = 0.82f + cell * 0.12f - stain * 0.12f;
            return new Color(v, v * 0.98f, v * 0.95f);
        }

        static Color WallPanelPix(int x, int y)
        {
            float v = 0.92f;
            if (y % 64 < 2) v = 0.75f;                                              // 横向板缝（更深）
            if (x % 128 < 2) v = 0.80f;                                             // 竖向接缝
            if ((x + y / 8) % 97 < 3) v -= 0.10f;                                   // 划痕
            // 水渍/霉斑：低频率Perlin噪声
            float mold = Mathf.PerlinNoise(x * 0.008f + 3.7f, y * 0.008f + 1.2f);
            if (mold > 0.7f) v -= (mold - 0.7f) * 0.3f;                            // 霉斑区域
            // 随机小划痕
            if ((x * 17 + y * 31) % 47 < 1) v -= 0.08f;
            return new Color(v, v * 0.97f, v * 0.94f);
        }

        static Color MetalPix(int x, int y)
        {
            float v = 0.88f + ((x * 31 + y * 17) % 13) / 13f * 0.10f;
            if ((x * 3 + y) % 53 < 2) v -= 0.18f;                                   // 磨损（更深）
            // 锈迹：低频率噪声 + 暖色调
            float rust = Mathf.PerlinNoise(x * 0.015f + 7.3f, y * 0.015f + 2.1f);
            if (rust > 0.65f)
            {
                float r = rust * 0.25f;
                v = v * (1f - r) + r * 0.55f;                                       // 锈迹偏暖
            }
            return new Color(v, v * 0.96f, v * 0.92f);
        }

        /// <summary>程序化圆形贴图（UI 用）</summary>
        public static Sprite EnsureCircleSprite()
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePng);
            if (sprite != null) return sprite;

            const int size = 96;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float r = 44f, c = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01((r - d) / 2f)));
                }
            tex.Apply();
            File.WriteAllBytes(Application.dataPath + CirclePng.Substring("Assets".Length), tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(CirclePng);
            var imp = (TextureImporter)AssetImporter.GetAtPath(CirclePng);
            imp.textureType = TextureImporterType.Sprite;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(CirclePng);
        }

        public static Font GetBuiltinFont()
        {
            // 场景构建期字体策略：优先随包字体资产（Assets/Resources/DefaultFont.ttf）。
            // 项目资产引用会被打进 APK 且真机必然可解析；内建 LegacyRuntime/Arial 引用
            // 在部分 Android 真机上解析失败（文字全空白），仅作最后兜底。
            var f = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/DefaultFont.ttf");
            if (f != null) return f;
            try { f = Resources.Load<Font>("DefaultFont"); } catch { }
            if (f != null) return f;
            try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            if (f != null) return f;
            try { f = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
            return f;
        }

        /// <summary>创建立方体基本体（静态+纯色材质）</summary>
        public static GameObject Box(string name, Vector3 center, Vector3 size, Material mat, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.isStatic = true;
            go.transform.position = center;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (parent != null) go.transform.SetParent(parent, true);
            return go;
        }

        static void BuildLighting()
        {
            var sunGo = new GameObject("DirLight");
            var l = sunGo.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(0.88f, 0.92f, 1f);
            l.intensity = 1.4f;
            l.shadows = LightShadows.Soft;
            sunGo.transform.rotation = Quaternion.Euler(45f, 305f, 0f);

            // 明亮废弃研究所：恐怖来自未知而非黑屏——保证看清一切结构与物品
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            // 统一环境光：偏冷色调，与场景蓝灰色调一致
            RenderSettings.ambientLight = new Color(0.55f, 0.57f, 0.63f);

            // 雾效：增强空间深度感，远处渐隐——与暗角配合形成"胶片摄影感"
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.18f, 0.20f, 0.26f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 20f;
            RenderSettings.fogEndDistance = 60f;

            // 全局环境光探针：提升间接光照质量
            var probeGo = new GameObject("GlobalLightProbe");
            probeGo.AddComponent<LightProbeProxyVolume>();
        }

        static void BuildPlayer()
        {
            var player = new GameObject("Player");
            player.transform.position = new Vector3(0f, 0.05f, 3.5f);
            player.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // 面向北墙

            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.7f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0f, 0.85f, 0f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(player.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            body.transform.localScale = new Vector3(0.55f, 0.85f, 0.55f);
            body.GetComponent<Collider>().enabled = false;
            body.GetComponent<Renderer>().sharedMaterial =
                EnsureMaterial("Assets/Materials/M_Player.mat", new Color(0.15f, 0.15f, 0.18f));

            var head = new GameObject("Head");
            head.transform.SetParent(player.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.55f, 0f);

            var camGo = new GameObject("MainCamera");
            camGo.transform.SetParent(head.transform, false);
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 60f;
            cam.fieldOfView = 65f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.10f);
            int ml = LayerMask.NameToLayer(MenuLayer);
            if (ml >= 0) cam.cullingMask &= ~(1 << ml);   // 主相机永不渲染菜单层（无穿透）
            camGo.AddComponent<AudioListener>();

            var pc = player.AddComponent<PlayerController>();
            pc.head = head.transform;

            player.AddComponent<MobileInput>();
            player.AddComponent<PlayerInteraction>();
            player.AddComponent<Door13.Inventory.Inventory>();
            player.AddComponent<AudioDirector>();
            player.AddComponent<FlashlightController>();

            var mi = player.GetComponent<MobileInput>();
            _playerInput = mi;
        }

        static MobileInput _playerInput;

        /// <summary>供 UiBuilder 绑定摇杆与视角区（场景构建时调用）</summary>
        public static void BindPlayerInput(VirtualJoystick joy, TouchLookZone zone)
        {
            if (_playerInput != null)
            {
                _playerInput.joystick = joy;
                _playerInput.lookZone = zone;
            }
        }
    }
}
