using UnityEngine;
using Door13.Save;

namespace Door13.Interaction
{
    /// <summary>电脑终端：打开白底文件系统（文件随循环解锁）。</summary>
    public class ComputerTerminal : Interactable
    {
        [TextArea(2, 4)] public string screenTitle = "终端 — 回廊7号内网";
        public ComputerPanel.FileEntry[] files;

        // 屏幕发光材质引用（用于启动动画）
        Material screenMat;
        bool isBooting = false;

        void Awake()
        {
            prompt = "使用电脑";
            // 查找屏幕 Renderer 获取材质
            var renderer = GetComponent<Renderer>();
            if (renderer != null) screenMat = renderer.sharedMaterial;
        }

        protected override void OnInteract(PlayerInteraction actor)
        {
            if (ComputerPanel.Instance != null)
            {
                // 启动动画：屏幕先闪一下（模拟系统唤醒）
                StartCoroutine(_BootFlash());
                // 打开电脑面板
                ComputerPanel.Instance.Open(screenTitle, files);
            }
        }

        /// <summary>屏幕启动闪烁效果：0.3秒内从暗到亮</summary>
        System.Collections.IEnumerator _BootFlash()
        {
            if (isBooting || screenMat == null) yield break;
            isBooting = true;
            float t = 0f;
            Color origColor = screenMat.GetColor("_BaseColor");
            Color dimColor = new Color(origColor.r * 0.2f, origColor.g * 0.2f, origColor.b * 0.2f, origColor.a);
            screenMat.SetColor("_EmissionColor", dimColor);
            while (t < 0.3f)
            {
                t += Time.unscaledDeltaTime;
                float f = Mathf.SmoothStep(0f, 1f, t / 0.3f);
                Color current = Color.Lerp(dimColor, origColor, f);
                screenMat.SetColor("_EmissionColor", current);
                yield return null;
            }
            screenMat.SetColor("_EmissionColor", origColor);
            isBooting = false;
        }
    }
}
