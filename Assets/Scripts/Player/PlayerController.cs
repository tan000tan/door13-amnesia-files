using UnityEngine;

namespace Door13.Player
{
    /// <summary>
    /// 第一人称玩家：CharacterController 平面移动 + 重力。
    /// 视角水平旋转作用于身体（改变移动朝向），俯仰作用于头部节点。
    /// 不含交互逻辑（交互见 Interaction 模块）。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Tooltip("头部节点（承载俯仰与相机）")]
        public Transform head;

        [Tooltip("行走速度 m/s")]
        public float walkSpeed = 2.4f;

        [Tooltip("俯仰限制（度）")]
        public float pitchLimit = 80f;

        [Tooltip("重力加速度")]
        public float gravity = -18f;

        CharacterController cc;
        float pitch;
        float vy;
        float stepDist;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
        }

        /// <summary>传送到指定位置（循环重置/新游戏用）。重置俯仰与垂直速度。</summary>
        public void TeleportTo(Vector3 pos, Quaternion rot)
        {
            if (cc == null) cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            transform.SetPositionAndRotation(pos, rot);
            vy = 0f;
            pitch = 0f;
            if (head != null) head.localRotation = Quaternion.identity;
            if (cc != null) cc.enabled = true;
        }

        void Update()
        {
            if (MobileInput.Instance == null || InputLock.Active) return;

            // 视角：水平转身体，俯仰转头；可选镜头平滑
            Vector2 look = MobileInput.Instance.Look;
            transform.Rotate(0f, look.x, 0f);
            // look.y 上滑为正 → pitch 增大（抬头）。invertY 开关在 TouchLookZone 中已做翻转
            pitch = Mathf.Clamp(pitch + look.y, -pitchLimit, pitchLimit);
            if (head != null)
            {
                float smooth = GameSettings.Cur.lookSmoothing;
                var target = Quaternion.Euler(pitch, 0f, 0f);
                head.localRotation = smooth > 0.1f
                    ? Quaternion.Slerp(head.localRotation, target, Mathf.Clamp01(Time.deltaTime * smooth))
                    : target;
            }

            // 移动（相对身体朝向）+ 重力；速度受设置倍率
            Vector2 m = MobileInput.Instance.Move;
            float speed = walkSpeed * GameSettings.Cur.moveSensitivity;
            Vector3 vel = (transform.right * m.x + transform.forward * m.y) * speed;
            if (cc.isGrounded && vy < 0f) vy = -2f;
            vy += gravity * Time.deltaTime;
            vel.y = vy;
            cc.Move(vel * Time.deltaTime);

            // 脚步声（按移动距离触发）
            if (cc.isGrounded && m.sqrMagnitude > 0.05f)
            {
                stepDist += vel.magnitude * Time.deltaTime;
                if (stepDist > 1.6f)
                {
                    stepDist = 0f;
                    if (Audio.AudioDirector.Instance != null)
                        Audio.AudioDirector.Instance.Footstep();
                }
            }
        }
    }
}
