using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Door13.Player
{
    /// <summary>
    /// 唯一触摸路由器：所有屏幕触摸在此一处分配，三区互斥——
    /// ① UI（EventSystem 命中）→ 交给按钮，本脚本忽略；
    /// ② 左下摇杆区（SafeRoot 空间判定）→ 交给 VirtualJoystick：只移动，绝不转视角；
    /// ③ 右半屏 → 视角拖动 + 轻点交互：只转镜头，绝不影响移动。
    /// 左半屏摇杆区之外：无任何作用。
    /// 区域以"触摸按下点"为路由依据，手指越界不改路由（多指完全独立）。
    /// </summary>
    public class TouchLookZone : MonoBehaviour
    {
        [Tooltip("编辑器鼠标灵敏度倍率")]
        public float mouseSensitivity = 10f;

        /// <summary>本帧视角增量（已含灵敏度）</summary>
        public Vector2 Delta { get; private set; }

        /// <summary>轻点右半屏（非UI/非摇杆区）→ 直接交互</summary>
        public event Action TapInteract;

        VirtualJoystick joystick;
        Vector2 deltaSum;
        int moveFinger = -1;

        class LookTouch { public Vector2 start; public float time; public float move; public Vector2 sum; }
        readonly Dictionary<int, LookTouch> lookTouches = new Dictionary<int, LookTouch>();

        void Start()
        {
            // 含未激活：主菜单开局即隐藏摇杆（SetHudVisible(false)），
            // 若 Start 时摇杆已隐藏而不带 true，此处将得到 null → 摇杆区彻底失灵（真机"无法移动"级隐患）
            joystick = FindObjectOfType<VirtualJoystick>(true);
        }

        void Update()
        {
            if (InputLock.Active) { Delta = Vector2.zero; lookTouches.Clear(); moveFinger = -1; return; }

            deltaSum = Vector2.zero;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                var es = EventSystem.current;
                bool overUI = es != null && es.IsPointerOverGameObject(t.fingerId);

                switch (t.phase)
                {
                    case TouchPhase.Began:
                        if (overUI) break;                          // UI 自己处理（按钮所见即所点）
                        if (moveFinger == -1 && joystick != null && joystick.InZone(t.position))
                        {
                            moveFinger = t.fingerId;                // 左下摇杆区 → 只移动
                            joystick.Begin(t.position);
                            Debug.Log($"[INPUT] 摇杆接管 id={t.fingerId} 屏幕坐标=({t.position.x:F0},{t.position.y:F0})");
                            break;
                        }
                        if (t.position.x >= Screen.width * 0.5f)    // 右半屏 → 只视角
                        {
                            lookTouches[t.fingerId] = new LookTouch
                                { start = t.position, time = Time.unscaledTime, move = 0f, sum = Vector2.zero };
                            Debug.Log($"[INPUT] 视角接管 id={t.fingerId} 屏幕坐标=({t.position.x:F0},{t.position.y:F0})");
                        }
                        // 左半屏非摇杆区：无任何作用
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (t.fingerId == moveFinger) { joystick.Drag(t.position); break; }
                        if (overUI) break;
                        if (lookTouches.TryGetValue(t.fingerId, out var tr))
                        {
                            tr.move += t.deltaPosition.magnitude;
                            tr.sum += t.deltaPosition;
                            deltaSum += t.deltaPosition;            // 只转视角
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (t.fingerId == moveFinger)
                        {
                            moveFinger = -1;
                            Debug.Log($"[INPUT] 摇杆释放 id={t.fingerId} 输出归零");
                            if (joystick != null) joystick.End();
                            break;
                        }
                        if (lookTouches.TryGetValue(t.fingerId, out var end))
                        {
                            // 轻点 = 快速且几乎没移动 → 直接交互（仅右半屏）
                            if (!overUI && Time.unscaledTime - end.time < 0.3f && end.move < 28f)
                                TapInteract?.Invoke();
                            Debug.Log($"[INPUT] 视角释放 id={t.fingerId} 累计位移=({end.sum.x:F0},{end.sum.y:F0})");
                            lookTouches.Remove(t.fingerId);
                        }
                        break;
                }
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButton(1))
                deltaSum += new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSensitivity;
            if (Input.GetMouseButtonDown(0) &&
                (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
                TapInteract?.Invoke();
            float kh = Input.GetAxisRaw("Horizontal");
            float kv = Input.GetAxisRaw("Vertical");
            if ((kh != 0f || kv != 0f) && joystick != null)
                joystick.SetOutput(Vector2.ClampMagnitude(new Vector2(kh, kv), 1f));
            else if (joystick != null && moveFinger == -1)
                joystick.SetOutput(Vector2.zero);
#endif

            float sens = GameSettings.Cur.lookSensitivity;
            Delta = new Vector2(deltaSum.x * sens, deltaSum.y * (GameSettings.Cur.invertY ? -1f : 1f) * sens);
        }
    }
}
