using UnityEngine;

namespace Door13.Player
{
    /// <summary>
    /// 全局输入汇聚点：移动向量与视角增量。场景中唯一实例，由 Player 持有。
    /// 编辑器/桌面下自动回退键鼠（WASD + 右键拖动视角），真机使用虚拟摇杆与触摸。
    /// </summary>
    public class MobileInput : MonoBehaviour
    {
        public static MobileInput Instance { get; private set; }

        [Tooltip("虚拟摇杆（输出移动向量）")]
        public VirtualJoystick joystick;

        [Tooltip("视角拖动区（输出视角增量）")]
        public TouchLookZone lookZone;

        /// <summary>摇杆输出：x=左右，y=前后，范围 -1..1</summary>
        public Vector2 Move { get; private set; }

        /// <summary>本帧视角增量（已含灵敏度）</summary>
        public Vector2 Look => lookZone != null ? lookZone.Delta : Vector2.zero;

        void Awake()
        {
            Instance = this;
            // 序列化兜底：若场景里引用缺失（历史版本曾把 lookZone 存成 null），运行时自愈。
            // 不做这层，右侧滑动转视角的链路会在本类处静默断裂且无任何报错。
            if (joystick == null) joystick = FindObjectOfType<VirtualJoystick>(true);
            if (lookZone == null) lookZone = FindObjectOfType<TouchLookZone>(true);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            Vector2 m = joystick != null ? joystick.Output : Vector2.zero;
#if UNITY_EDITOR || UNITY_STANDALONE
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            if (h != 0f || v != 0f)
                m = Vector2.ClampMagnitude(new Vector2(h, v), 1f);
#endif
            Move = m;
        }
    }
}
