using UnityEngine;

namespace Door13.UI
{
    /// <summary>
    /// 安全区适配：把节点 RectTransform 收缩到 Screen.safeArea 内。
    /// 挂在 Canvas 下的 SafeRoot 上，所有 HUD/面板内容放在其下，
    /// 避免刘海、圆角、导航栏遮挡与出屏。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        RectTransform rt;
        Rect last = Rect.zero;

        void Awake()
        {
            rt = (RectTransform)transform;
            Apply();
        }

        void Update()
        {
            var sa = Screen.safeArea;
            if (sa != last) Apply();
        }

        void Apply()
        {
            var sa = Screen.safeArea;
            last = sa;
            float w = Screen.width, h = Screen.height;
            if (w <= 0 || h <= 0) return;
            rt.anchorMin = new Vector2(sa.x / w, sa.y / h);
            rt.anchorMax = new Vector2((sa.x + sa.width) / w, (sa.y + sa.height) / h);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
