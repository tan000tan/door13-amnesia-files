using UnityEngine;
using Door13.Player;
using Door13.UI;

namespace Door13.Interaction
{
    /// <summary>
    /// 准星交互：从相机中心发射球形射线（宽容瞄准），命中可交互物则更新提示。
    /// 触发途径：HUD交互按钮 / 轻点屏幕（TouchLookZone.TapInteract）。
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Tooltip("交互距离（米）")]
        public float range = 3.2f;

        [Tooltip("球形射线半径（越大越容易瞄准小物件）")]
        public float sphereRadius = 0.3f;

        /// <summary>当前准星指向的可交互物（可为 null）</summary>
        public Interactable Target { get; private set; }

        Camera cam;
        float tapCooldown;

        void Start()
        {
            cam = GetComponentInChildren<Camera>();
            if (Hud.Instance != null)
                Hud.Instance.InteractClicked += OnInteractPressed;
            var look = FindObjectOfType<Player.TouchLookZone>();
            if (look != null)
                look.TapInteract += OnInteractPressed;
        }

        void OnDestroy()
        {
            if (Hud.Instance != null)
                Hud.Instance.InteractClicked -= OnInteractPressed;
        }

        void Update()
        {
            Interactable hit = null;
            string prompt = string.Empty;

            if (cam != null)
            {
                var ray = new Ray(cam.transform.position, cam.transform.forward);
                if (Physics.SphereCast(ray, sphereRadius, out RaycastHit rh, range))
                    hit = rh.collider.GetComponentInParent<Interactable>();
                if (hit != null)
                    prompt = hit.DisplayPrompt;
            }

            if (hit != Target)
            {
                Target = hit;
                if (Hud.Instance != null)
                {
                    Hud.Instance.SetPrompt(prompt);
                    Hud.Instance.SetTargetHint(hit != null && hit.CanInteract);
                }
            }
            else if (hit == null && Hud.Instance != null && Hud.Instance.PromptText.Length > 0)
            {
                Hud.Instance.SetPrompt(string.Empty);
                Hud.Instance.SetTargetHint(false);
            }

            if (tapCooldown > 0f) tapCooldown -= Time.unscaledDeltaTime;
        }

        void OnInteractPressed()
        {
            if (InputLock.Active) return;   // 主菜单/结局机位：禁止交互
            if (tapCooldown > 0f) return;   // 防止按钮+轻点双触发
            tapCooldown = 0.25f;

            if (Target != null && Target.CanInteract)
            {
                if (Audio.AudioDirector.Instance != null) Audio.AudioDirector.Instance.Click();
                Target.Interact(this);
            }
            else if (Hud.Instance != null)
            {
                // 明确反馈：附近没有可交互物
                if (Target != null) Hud.Instance.Toast(Target.GateMessage() ?? "现在还做不到。");
            }
        }
    }
}
