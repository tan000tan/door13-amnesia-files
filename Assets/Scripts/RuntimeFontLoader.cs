using UnityEngine;
using UnityEngine.UI;

namespace Door13
{
    /// <summary>
    /// 运行时字体兜底加载器：解决 Android 真机上字体引用失效导致文字空白的问题。
    /// 覆盖三类对象：
    /// ① 所有 uGUI Text —— 包括初始未激活的面板（检视/密码盘/电脑等，必须含未激活搜索）；
    /// ② 所有 TextMesh（墙字等世界文字），并补设 MeshRenderer 材质；
    /// ③ 字体来源依次：Resources/DefaultFont.ttf（随包字体）→ 内建字体 → 报错。
    /// 场景构建期（编辑器）已将各 Text 序列化引用指向 Resources/DefaultFont.ttf 资产本体，
    /// 本脚本仅作真机兜底与断言来源。
    /// </summary>
    public class RuntimeFontLoader : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            var f = ResolveFont();
            if (f == null)
            {
                Debug.LogError("[FontLoader] 所有字体来源均失败，文字将无法渲染！");
                return;
            }
            Door13.FontCache.Cur = f;

            int gui = 0, guiFixed = 0, mesh = 0, meshFixed = 0;
            var texts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var t in texts)
            {
                gui++;
                if (t.font == null || IsBuiltinRef(t.font))
                {
                    t.font = f;
                    t.SetAllDirty();   // 强制重建字体网格
                    guiFixed++;
                }
            }
            var meshes = FindObjectsByType<TextMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var tm in meshes)
            {
                mesh++;
                if (tm.font == null || IsBuiltinRef(tm.font))
                {
                    tm.font = f;
                    var mr = tm.GetComponent<MeshRenderer>();
                    if (mr != null) mr.sharedMaterial = f.material;
                    meshFixed++;
                }
            }
            Debug.Log($"[FontLoader] 字体={f.name}  Text总数={gui} 修复={guiFixed}  TextMesh总数={mesh} 修复={meshFixed}");
        }

        static Font ResolveFont()
        {
            Font f = Resources.Load<Font>("DefaultFont");
            if (f != null) { Debug.Log("[FontLoader] 使用随包字体 Resources/DefaultFont.ttf"); return f; }

            try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            if (f != null) { Debug.Log("[FontLoader] 使用内建 LegacyRuntime.ttf"); return f; }
            try { f = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
            if (f != null) { Debug.Log("[FontLoader] 使用内建 Arial.ttf"); return f; }
            return null;
        }

        /// <summary>内建引用（LegacyRuntime/Arial）在部分真机上解析失败，统一替换为随包字体。</summary>
        static bool IsBuiltinRef(Font font)
        {
            var n = font.name;
            return n == "LegacyRuntime" || n == "Arial" || n == "LegacyRuntime.ttf" || n == "Arial.ttf";
        }
    }

    /// <summary>字体缓存（运行时兜底结果；编辑器场景构建不依赖它）。</summary>
    public static class FontCache
    {
        public static Font Cur { get; set; }
    }
}
