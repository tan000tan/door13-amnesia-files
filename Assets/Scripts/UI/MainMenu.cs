using UnityEngine;
using UnityEngine.UI;
using Door13.Player;
using Door13.Save;
using Door13.TimeLoop;

namespace Door13.UI
{
    /// <summary>
    /// 主菜单：新游戏 / 继续 / 设置 / 退出。
    /// 菜单与游戏场景完全分离——独立 MenuCamera 只渲染 MenuScene 层的走廊小场景，
    /// 期间主相机禁用（不可能看到游戏世界，无穿透）；输入由 InputLock 全局锁死；
    /// 玩法HUD隐藏。开始/继续游戏时相机切回、由 LoopManager 传送出生点。
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        public static MainMenu Instance { get; private set; }

        public Text titleText;
        public Text subtitleText;

        Camera menuCam;
        Camera gameCam;
        Vector3 camBasePos;
        Quaternion camBaseRot;

        void Awake()
        {
            Instance = this;
            GameSettings.Load();
            Bind("BtnNew", NewGame);
            Bind("BtnContinue", Continue);
            Bind("BtnSettings", OpenSettings);
            Bind("BtnQuit", Quit);
            var t = transform.Find("BtnContinue");
            if (t != null) t.gameObject.SetActive(SaveManager.HasSave);
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        void Start() => ShowMenu();

        void RefreshCams()
        {
            if (menuCam == null)
            {
                var mc = GameObject.Find("MenuCamera");
                menuCam = mc != null ? mc.GetComponent<Camera>() : null;
            }
            if (gameCam == null) gameCam = Camera.main;
        }

        public void ShowMenu()
        {
            if (Hud.Instance != null) Hud.Instance.GameOver = false;
            InputLock.Active = true;                       // 锁死移动/视角/交互
            if (Hud.Instance != null) Hud.Instance.SetHudVisible(false);
            gameObject.SetActive(true);                    // 先激活（启动 OnEnable 漂移协程）
            RefreshCams();
            if (menuCam != null) menuCam.enabled = true;   // 只渲染菜单小场景
            if (gameCam != null) gameCam.enabled = false;  // 游戏世界相机关闭 → 无穿透
            var t = transform.Find("BtnContinue");
            if (t != null) t.gameObject.SetActive(SaveManager.HasSave);
            if (subtitleText != null)
                subtitleText.text = SaveManager.HasSave ? "检测到存档 —— 你上次数到了第 " + SaveManager.Data.loop + " 次循环" : "回廊7号 · 记忆科学部";
        }

        /// <summary>开始游戏：相机切回游戏世界、解锁输入、恢复HUD。</summary>
        void StartPlay()
        {
            RefreshCams();
            if (menuCam != null) menuCam.enabled = false;
            if (gameCam != null) gameCam.enabled = true;
            InputLock.Active = false;
            if (Hud.Instance != null) Hud.Instance.SetHudVisible(true);
            gameObject.SetActive(false);
        }

        public void NewGame()
        {
            StartPlay();
            if (LoopManager.Instance != null) LoopManager.Instance.NewGame();
            if (Hud.Instance != null)
                Hud.Instance.StartCoroutine(Hud.Instance.OpeningGuidance());
        }

        void Continue()
        {
            StartPlay();
            // 存档已在 LoopManager.Awake 载入；恢复背包并回到上次房间
            var inv = Inventory.Inventory.Instance;
            if (inv != null) inv.Restore(SaveManager.Data.items);
            // 恢复循环次数（存档已在 LoopManager.Awake 载入，无需额外操作）
            // LoopManager.Start() 会正确从 SaveManager.Data.loop 读取
            var spawn = GameObject.Find("Spawn_" + SaveManager.Data.posRoom);
            var pc = FindObjectOfType<Player.PlayerController>();
            if (spawn != null && pc != null)
                pc.TeleportTo(spawn.transform.position, spawn.transform.rotation);
        }

        void OpenSettings()
        {
            if (SettingsPanel.Instance != null) SettingsPanel.Instance.Open();
        }

        void Quit()
        {
            Save.SaveManager.Save();
            Application.Quit();
        }

        void OnEnable()
        {
            RefreshCams();
            if (menuCam != null)
            {
                camBasePos = menuCam.transform.position;
                camBaseRot = menuCam.transform.rotation;
            }
            StartCoroutine(MenuPresence());
        }

        void OnDisable() => StopAllCoroutines();

        /// <summary>
        /// 菜单"活物感"：菜单相机在走廊机位上极缓慢漂移（呼吸视角），
        /// 标题轻微呼吸缩放，副标题偶发电灯式闪烁。
        /// </summary>
        System.Collections.IEnumerator MenuPresence()
        {
            var title = transform.Find("TitleText") as RectTransform;
            var sub = transform.Find("SubText");
            var subText = sub != null ? sub.GetComponent<Text>() : null;
            Color subBase = subText != null ? subText.color : Color.white;
            float t = 0f;
            while (true)
            {
                t += Time.unscaledDeltaTime;
                if (menuCam != null)
                {
                    menuCam.transform.position = camBasePos
                        + new Vector3(Mathf.Sin(t * 0.11f) * 0.14f, Mathf.Sin(t * 0.073f) * 0.05f, 0f);
                    menuCam.transform.rotation = camBaseRot * Quaternion.Euler(0f, Mathf.Sin(t * 0.13f) * 1.6f, 0f);
                }
                if (title != null)
                {
                    float s = 1f + Mathf.Sin(t * 0.7f) * 0.012f;
                    title.localScale = new Vector3(s, s, 1f);
                }
                if (subText != null)
                {
                    float a = subBase.a;
                    if (Mathf.PerlinNoise(t * 6.1f, 3.7f) > 0.93f) a *= 0.35f;
                    var c = subText.color; c.a = a; subText.color = c;
                }
                yield return null;
            }
        }

        void Bind(string name, System.Action action)
        {
            var t = transform.Find(name);
            var btn = t != null ? t.GetComponent<Button>() : null;
            if (btn != null) btn.onClick.AddListener(() => action());
        }
    }
}
