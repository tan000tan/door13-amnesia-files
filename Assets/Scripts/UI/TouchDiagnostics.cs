using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Door13.UI
{
    /// <summary>
    /// 触摸诊断模式（屏幕坐标级测试工具，设置面板可开关，随设置持久化）。
    /// 开启后实时显示：每个触摸的 Touch ID / 屏幕坐标 / 归一化坐标 / 接收者
    /// （具体UI对象名 / 摇杆区 / 视角区 / 无），并在触摸位置画出十字标记。
    /// 标记画在 Canvas 根坐标系（物理屏幕位置），与 SafeRoot 无关——
    /// 因此"标记的位置 = 手指的位置"恒成立，可直接对照接收者文本验证路由。
    /// 全部节点 raycastTarget=false，绝不拦截真实触摸。
    /// </summary>
    public class TouchDiagnostics : MonoBehaviour
    {
        public static TouchDiagnostics Instance { get; private set; }

        public RectTransform root;        // 诊断层根（Canvas 直属）
        public Text logText;              // 触摸清单
        public Image[] dotImages;         // 触摸点标记
        public Text[] dotLabels;          // 触摸点 ID 标签

        RectTransform canvasRect;
        Player.VirtualJoystick joy;
        readonly List<RaycastResult> rayResults = new List<RaycastResult>();

        void Awake()
        {
            Instance = this;
            // 本组件挂在常驻控制器节点上；root 是它关照的"视觉层"兄弟节点。
            // 绝不能把组件挂在 root 上再自行 SetActive(false)——那会连 Update/Start 一起停掉，
            // 导致诊断模式永远无法开启（模拟器实测复现过）。
            canvasRect = transform.parent as RectTransform;
            if (root != null) root.gameObject.SetActive(false);
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        void Start()
        {
            joy = FindObjectOfType<Player.VirtualJoystick>(true);   // 含未激活：菜单期间摇杆是隐藏的
            Player.GameSettings.Changed += ApplyState;
            ApplyState();
        }

        void ApplyState()
        {
            bool on = Player.GameSettings.Cur.touchDebug;
            if (root.gameObject.activeSelf != on) root.gameObject.SetActive(on);
        }

        void Update()
        {
            if (!root.gameObject.activeSelf) return;

            int n = Mathf.Min(Input.touchCount, dotImages.Length);
            var sb = new StringBuilder();
            sb.AppendLine("── 触摸诊断 ──");
            sb.AppendLine($"灵敏度={Player.GameSettings.Cur.lookSensitivity:F2} 输入锁={Player.InputLock.Active}");

            var cam = Camera.main;
            if (cam != null) sb.AppendLine($"相机Yaw={cam.transform.eulerAngles.y:F1} Pitch={cam.transform.eulerAngles.x:F1}");
            var pc = FindObjectOfType<Player.PlayerController>();
            if (pc != null)
            {
                var p = pc.transform.position;
                sb.AppendLine($"玩家坐标=({p.x:F1},{p.y:F1},{p.z:F1})");
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                string receiver = Resolve(t.position, t.fingerId);
                sb.AppendLine($"T{t.fingerId} {t.phase} X={t.position.x:F0} Y={t.position.y:F0} " +
                              $"归一=({t.position.x / Screen.width:F2},{t.position.y / Screen.height:F2}) → {receiver}");
                if (t.phase == TouchPhase.Began)
                    Debug.Log($"[TOUCH] began id={t.fingerId} X={t.position.x:F0} Y={t.position.y:F0} 接收={receiver}");

                if (i < n)
                {
                    var img = dotImages[i];
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvasRect, t.position, null, out Vector2 local);
                    img.rectTransform.anchoredPosition = local;
                    img.gameObject.SetActive(true);
                    if (dotLabels[i] != null) dotLabels[i].text = "T" + t.fingerId;
                }
            }

            for (int i = n; i < dotImages.Length; i++)
                if (dotImages[i] != null) dotImages[i].gameObject.SetActive(false);

            if (Input.touchCount == 0) sb.Append("（无触摸）");
            if (logText != null) logText.text = sb.ToString();
        }

        /// <summary>判定接收者：UI对象 > 摇杆区 > 视角区 > 左半屏无。</summary>
        public string Resolve(Vector2 screenPos, int fingerId)
        {
            var es = EventSystem.current;
            if (es != null)
            {
                var ped = new PointerEventData(es) { position = screenPos };
                rayResults.Clear();
                es.RaycastAll(ped, rayResults);
                if (rayResults.Count > 0) return "UI:" + rayResults[0].gameObject.name;
            }
            if (joy != null && joy.InZone(screenPos)) return "摇杆区";
            if (screenPos.x >= Screen.width * 0.5f) return "视角区";
            return "无(左半屏)";
        }
    }
}
