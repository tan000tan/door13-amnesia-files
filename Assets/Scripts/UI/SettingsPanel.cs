using UnityEngine;
using UnityEngine.UI;
using Door13.Player;

namespace Door13.UI
{
    /// <summary>设置面板：视角灵敏度/镜头平滑/移动速度 + 主音量+Y轴反转/画质/FOV。</summary>
    public class SettingsPanel : MonoBehaviour
    {
        public static SettingsPanel Instance { get; private set; }

        public Slider volumeSlider;
        public Slider fovSlider;
        public Toggle invertToggle;
        public Toggle touchDebugToggle;
        public Text volumeValue;
        public Text lookState;
        public Text smoothState;
        public Text moveState;
        public Text fovValue;

        void Awake()
        {
            Instance = this;
            Bind("CloseButton", Close);
            BindLook(0, "LookLow"); BindLook(1, "LookMid"); BindLook(2, "LookHigh");
            BindSmooth(0, "SmoothLow"); BindSmooth(1, "SmoothMid"); BindSmooth(2, "SmoothHigh");
            BindMove(0, "MoveLow"); BindMove(1, "MoveMid"); BindMove(2, "MoveHigh");
            gameObject.SetActive(false);
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        void BindLook(int idx, string btn)
        {
            var t = transform.Find(btn);
            var b = t != null ? t.GetComponent<Button>() : null;
            if (b != null) b.onClick.AddListener(() =>
                GameSettings.Set(d => d.lookSensitivity = GameSettings.LookPresets[idx]));
        }

        void BindSmooth(int idx, string btn)
        {
            var t = transform.Find(btn);
            var b = t != null ? t.GetComponent<Button>() : null;
            if (b != null) b.onClick.AddListener(() =>
                GameSettings.Set(d => d.lookSmoothing = GameSettings.SmoothPresets[idx]));
        }

        void BindMove(int idx, string btn)
        {
            var t = transform.Find(btn);
            var b = t != null ? t.GetComponent<Button>() : null;
            if (b != null) b.onClick.AddListener(() =>
                GameSettings.Set(d => d.moveSensitivity = GameSettings.MovePresets[idx]));
        }

        public void Open()
        {
            if (volumeSlider != null)
            {
                volumeSlider.minValue = 0f; volumeSlider.maxValue = 1f;
                volumeSlider.value = GameSettings.Cur.masterVolume;
            }
            if (fovSlider != null)
            {
                fovSlider.minValue = 50f; fovSlider.maxValue = 90f;
                fovSlider.value = GameSettings.Cur.fov;
            }
            if (invertToggle != null) invertToggle.isOn = GameSettings.Cur.invertY;
            if (touchDebugToggle != null) touchDebugToggle.isOn = GameSettings.Cur.touchDebug;
            Refresh();
            gameObject.SetActive(true);
        }

        public void Close() => gameObject.SetActive(false);

        void Update()
        {
            if (!gameObject.activeSelf) return;

            // 音量滑块
            if (volumeSlider != null && !Mathf.Approximately(volumeSlider.value, GameSettings.Cur.masterVolume))
            {
                float v = volumeSlider.value;
                GameSettings.Set(d => d.masterVolume = v);
                Refresh();
            }
            // FOV滑块
            if (fovSlider != null && !Mathf.Approximately(fovSlider.value, GameSettings.Cur.fov))
            {
                float v = fovSlider.value;
                GameSettings.Set(d => d.fov = v);
                Refresh();
            }
            // Y轴反转
            if (invertToggle != null && invertToggle.isOn != GameSettings.Cur.invertY)
            {
                bool v = invertToggle.isOn;
                GameSettings.Set(d => d.invertY = v);
            }
            // 触摸诊断
            if (touchDebugToggle != null && touchDebugToggle.isOn != GameSettings.Cur.touchDebug)
            {
                bool v = touchDebugToggle.isOn;
                GameSettings.Set(d => d.touchDebug = v);
            }
        }

        void Refresh()
        {
            if (volumeValue != null) volumeValue.text = (GameSettings.Cur.masterVolume * 100).ToString("F0") + "%";
            if (fovValue != null) fovValue.text = ((int)GameSettings.Cur.fov).ToString();
            if (lookState != null) lookState.text = GameSettings.PresetNames[GameSettings.NearestLookIndex()];
            if (smoothState != null) smoothState.text = GameSettings.PresetNames[GameSettings.NearestSmoothIndex()];
            if (moveState != null) moveState.text = GameSettings.PresetNames[GameSettings.NearestMoveIndex()];
        }

        void Bind(string name, System.Action action)
        {
            var t = transform.Find(name);
            var btn = t != null ? t.GetComponent<Button>() : null;
            if (btn != null) btn.onClick.AddListener(() => action());
        }
    }
}
