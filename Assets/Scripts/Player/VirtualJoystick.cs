using UnityEngine;
using UnityEngine.UI;

namespace Door13.Player
{
    /// <summary>
    /// 虚拟摇杆（被动组件）：所有触摸由 TouchLookZone 统一路由（唯一输入入口）。
    /// 只负责移动向量——绝不影响视角。
    ///
    /// 坐标系铁律：视觉（底座/圆钮）与触控判定必须使用同一个 RectTransform——
    /// 即父节点（SafeRoot，安全区容器）。任何刘海/手势条内缩都同时作用于两者，
    /// 因此"显示在哪里，触摸区域就在哪里"在任意分辨率与任意机身上恒成立。
    /// </summary>
    public class VirtualJoystick : MonoBehaviour
    {
        [Tooltip("摇杆拖动半径（Canvas单位）")]
        public float radius = 110f;

        [Tooltip("左手区域：宽占比（相对安全区容器）")]
        public float zoneWidthRatio = 0.45f;

        [Tooltip("左手区域：高占比（自安全区底部起）")]
        public float zoneHeightRatio = 0.62f;

        [Tooltip("死区半径（0..1），低于此输出为零")]
        public float deadZone = 0.12f;

        /// <summary>输出：x=左右，y=前后（-1..1）</summary>
        public Vector2 Output { get; private set; }

        /// <summary>编辑器键鼠回退直接设置输出</summary>
        public void SetOutput(Vector2 v) => Output = v;

        RectTransform rect;
        RectTransform hostRect;   // 父节点（SafeRoot）：视觉与判定的统一坐标系
        RectTransform knob;
        Image baseImg;
        Vector2 restPos;
        Vector2 centerHost;

        void Awake()
        {
            rect = (RectTransform)transform;
            hostRect = transform.parent as RectTransform;
            restPos = rect.anchoredPosition;
            centerHost = restPos;
            knob = transform.Find("Knob") as RectTransform;
            baseImg = GetComponent<Image>();
        }

        /// <summary>屏幕点是否属于左手摇杆区（SafeRoot 空间判定）。</summary>
        public bool InZone(Vector2 screenPos)
        {
            if (hostRect == null) return false;
            Vector2 p = ToHost(screenPos);
            float w = hostRect.rect.width, h = hostRect.rect.height;
            return p.x >= 0f && p.y >= 0f && p.x < w * zoneWidthRatio && p.y < h * zoneHeightRatio;
        }

        /// <summary>触摸开始：底座浮动到按下点（SafeRoot 空间，与显示一致）。</summary>
        public void Begin(Vector2 screenPos)
        {
            if (hostRect == null) { Awake(); if (hostRect == null) return; }
            Vector2 p = ToHost(screenPos);
            float w = hostRect.rect.width, h = hostRect.rect.height;
            p.x = Mathf.Clamp(p.x, radius + 30f, w * zoneWidthRatio);
            p.y = Mathf.Clamp(p.y, radius + 30f, h * zoneHeightRatio);
            rect.anchoredPosition = p;
            centerHost = p;
            if (baseImg != null) baseImg.color = new Color(1f, 1f, 1f, 0.4f);
            Drag(screenPos);
        }

        /// <summary>触摸拖动：更新输出与圆钮。</summary>
        public void Drag(Vector2 screenPos)
        {
            if (hostRect == null) return;
            Vector2 v = Vector2.ClampMagnitude((ToHost(screenPos) - centerHost) / radius, 1f);
            // 死区：轻触不漂移；越过死区后线性重标定到满行程
            float mag = v.magnitude;
            if (mag < deadZone) v = Vector2.zero;
            else v = v / mag * ((mag - deadZone) / (1f - deadZone));
            Output = v;
            if (knob != null) knob.anchoredPosition = Vector2.ClampMagnitude(ToHost(screenPos) - centerHost, radius);
        }

        /// <summary>触摸结束：回中。</summary>
        public void End()
        {
            Output = Vector2.zero;
            rect.anchoredPosition = restPos;
            centerHost = restPos;
            if (knob != null) knob.anchoredPosition = Vector2.zero;
            if (baseImg != null) baseImg.color = new Color(1f, 1f, 1f, 0.28f);
        }

        /// <summary>屏幕像素 → SafeRoot 空间（左下原点）。Overlay 画布 camera=null 为正确用法。</summary>
        Vector2 ToHost(Vector2 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                hostRect, screenPos, null, out Vector2 local);
            return local - hostRect.rect.min;
        }
    }
}
