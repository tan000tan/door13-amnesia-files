using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Door13.UI
{
    /// <summary>
    /// HUD：准星提示 / 循环计数 / 气泡提示 / 交互与背包按钮 / 全屏淡入淡出。
    /// 按钮点击通过事件暴露；各系统在 Start 订阅（避免 Awake 顺序问题）。
    /// 子节点按命名约定查找：InteractButton / InventoryButton / PromptLabel /
    /// LoopLabel / ToastLabel / FadeOverlay。
    /// </summary>
    public class Hud : MonoBehaviour
    {
        public static Hud Instance { get; private set; }

        public event Action InteractClicked;
        public event Action InventoryClicked;
        public event Action MenuClicked;

        Text promptLabel;
        Text loopLabel;
        Text toastLabel;
        Text objectiveLabel;
        Text narrationLabel;
        Image narrationBG;
        Image fadeOverlay;
        Image crosshair;
        Image interactBtnImg;
        Coroutine toastCo;
        Coroutine narrationCo;
        float pulse;
        bool targetHint;

        /// <summary>结局展示后置位：检视面板关闭时回到主菜单。</summary>
        public bool GameOver { get; set; }

        static readonly string[] gameplayNodes =
            { "JoystickBase", "InteractButton", "InventoryButton", "MenuButton", "Crosshair",
              "ObjectiveLabel", "LoopLabel", "ToastLabel", "PromptLabel", "HintLabel",
              "NarrationBG", "NarrationLabel" };

        /// <summary>游戏玩法HUD整体显隐（主菜单/剧情机位时隐藏，开始游戏后恢复）。</summary>
        public void SetHudVisible(bool on)
        {
            foreach (var n in gameplayNodes)
            {
                var t = DeepFind(n);
                if (t != null) t.gameObject.SetActive(on);
            }
            if (on && promptLabel != null) promptLabel.text = "";
        }

        public string PromptText => promptLabel != null ? promptLabel.text : string.Empty;

        void Awake()
        {
            Instance = this;

            promptLabel = FindText("PromptLabel");
            loopLabel = FindText("LoopLabel");
            toastLabel = FindText("ToastLabel");
            objectiveLabel = FindText("ObjectiveLabel");
            narrationLabel = FindText("NarrationLabel");
            narrationBG = FindImage("NarrationBG");
            fadeOverlay = FindImage("FadeOverlay");

            BindButton("InteractButton", () => InteractClicked?.Invoke());
            // 背包/菜单直调兜底：面板组件 Awake 即自禁用，其 Start 订阅不可靠（历史bug）。
            // Hud.Awake 在所有面板 Awake 之后运行，Instance 必已就绪。
            BindButton("InventoryButton", () =>
            {
                Audio.AudioDirector.Instance?.Click();
                InventoryClicked?.Invoke();
                Inventory.InventoryUI.Instance?.Toggle();
            });
            BindButton("MenuButton", () =>
            {
                Audio.AudioDirector.Instance?.Click();
                MenuClicked?.Invoke();
                PausePanel.Instance?.Open();
            });

            if (toastLabel != null)
            {
                toastLabel.text = "";
                var c = toastLabel.color; c.a = 1f; toastLabel.color = c;
            }
            if (objectiveLabel != null) objectiveLabel.text = "";
            if (narrationBG != null)
            {
                var c = narrationBG.color; c.a = 0f; narrationBG.color = c;
                narrationBG.raycastTarget = false;
            }
            if (narrationLabel != null) narrationLabel.text = "";
            if (fadeOverlay != null)
            {
                var c = fadeOverlay.color; c.a = 0f; fadeOverlay.color = c;
                fadeOverlay.raycastTarget = false;
            }
            crosshair = FindImage("Crosshair");
            var ib = DeepFind("InteractButton");
            interactBtnImg = ib != null ? ib.GetComponent<Image>() : null;
        }

        void Start()
        {
            // 开机渐显：从黑暗中醒来进入菜单（制造"入戏"的第一眼）
            if (fadeOverlay != null)
            {
                var c = fadeOverlay.color; c.a = 1f; fadeOverlay.color = c;
                StartCoroutine(Fade(0f, 1.8f));
            }
        }

        void Update()
        {
            if (!targetHint) return;
            pulse += Time.unscaledDeltaTime;
            float k = 0.5f + 0.5f * Mathf.Sin(pulse * 5.2f);
            if (interactBtnImg != null)
            {
                var c = interactBtnImg.color;
                c.a = Mathf.Lerp(0.22f, 0.5f, k);
                interactBtnImg.color = c;
            }
        }

        /// <summary>准星指向可交互物时：准星放大变暖、交互按钮呼吸提示。</summary>
        public void SetTargetHint(bool on)
        {
            if (targetHint == on) return;
            targetHint = on;
            if (on) pulse = 0f;
            if (interactBtnImg != null && !on)
            {
                var c = interactBtnImg.color;
                c.a = 0.22f;
                interactBtnImg.color = c;
            }
            if (crosshair != null)
            {
                var c = crosshair.color;
                c = on ? new Color(1f, 0.85f, 0.55f, 0.95f) : new Color(1f, 1f, 1f, 0.55f);
                crosshair.color = c;
                var rt = crosshair.rectTransform;
                rt.sizeDelta = on ? new Vector2(20f, 20f) : new Vector2(12f, 12f);
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SetPrompt(string s)
        {
            if (promptLabel != null) promptLabel.text = s;
        }

        public void SetLoop(int n)
        {
            if (loopLabel != null) loopLabel.text = "循环 " + n + " / 4";
        }

        public void SetObjective(string s)
        {
            if (objectiveLabel != null) objectiveLabel.text = s;
        }

        /// <summary>屏幕中央旁白（淡入-停留-淡出），用于循环演出/首次进入房间。</summary>
        public IEnumerator ShowNarration(string text, float hold)
        {
            if (narrationLabel == null || narrationBG == null) yield break;
            if (narrationCo != null) StopCoroutine(narrationCo);
            narrationLabel.text = text;
            narrationCo = StartCoroutine(NarrationFade(hold));
        }

        IEnumerator NarrationFade(float hold)
        {
            float t = 0f;
            Color bg = narrationBG.color;
            Color fg = narrationLabel.color;
            fg.a = 0f;
            while (t < 0.5f) { t += Time.deltaTime; bg.a = fg.a = t / 0.5f; narrationBG.color = bg; narrationLabel.color = fg; yield return null; }
            yield return new WaitForSeconds(hold);
            t = 0f;
            while (t < 0.5f) { t += Time.deltaTime; bg.a = fg.a = 1f - t / 0.5f; narrationBG.color = bg; narrationLabel.color = fg; yield return null; }
            narrationLabel.text = "";
        }

        /// <summary>结局展示（检视面板承载，关闭后回主菜单）。</summary>
        public void ShowEnding(string title, string body)
        {
            GameOver = true;
            if (Interaction.InspectPanel.Instance != null)
                Interaction.InspectPanel.Instance.Show(title, body);
        }

        /// <summary>开局引导：开场旁白 + 分段操作/目标提示。</summary>
        public System.Collections.IEnumerator OpeningGuidance()
        {
            yield return ShowNarration(
                "你在冰冷的操作台上醒来。\n头脑空空。墙上的刻字、桌上的电脑、照片墙……\n这里的一切都在暗示：你来过。", 3.4f);
            yield return new WaitForSeconds(0.6f);
            Toast("目标：调查房间，寻找离开的方法");
            yield return new WaitForSeconds(4.5f);
            Toast("提示：墙上有字，桌上有电脑——它们说的好像不是同一件事");
            yield return new WaitForSeconds(4.5f);
            Toast("提示：靠近物品后，轻点屏幕或按【交互】查看");
        }

        public void Toast(string s)
        {
            if (toastLabel == null) return;
            if (toastCo != null) StopCoroutine(toastCo);
            toastLabel.text = s;
            var c = toastLabel.color; c.a = 1f; toastLabel.color = c;
            toastCo = StartCoroutine(FadeToast());
        }

        /// <summary>全屏淡入淡出：to=目标透明度（1=全黑），dur=秒。</summary>
        public IEnumerator Fade(float to, float dur)
        {
            if (fadeOverlay == null) yield break;
            fadeOverlay.raycastTarget = to > 0.5f; // 黑屏期间拦截输入
            float from = fadeOverlay.color.a;
            for (float t = 0f; t < dur; t += Time.deltaTime)
            {
                var c = fadeOverlay.color;
                c.a = Mathf.Lerp(from, to, Mathf.Clamp01(t / dur));
                fadeOverlay.color = c;
                yield return null;
            }
            var end = fadeOverlay.color;
            end.a = to;
            fadeOverlay.color = end;
            fadeOverlay.raycastTarget = to > 0.5f;
        }

        IEnumerator FadeToast()
        {
            yield return new WaitForSeconds(2.0f);
            float a = 1f;
            while (a > 0f)
            {
                a -= Time.deltaTime * 2f;
                var c = toastLabel.color;
                c.a = Mathf.Max(0f, a);
                toastLabel.color = c;
                yield return null;
            }
            toastLabel.text = "";
            var r = toastLabel.color;
            r.a = 1f;
            toastLabel.color = r;
        }

        /// <summary>深度查找（含未激活节点）：HUD节点在 SafeRoot 之下，非直接子级。</summary>
        Transform DeepFind(string name)
        {
            var all = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
                if (all[i].name == name) return all[i];
            return null;
        }

        Text FindText(string name)
        {
            var t = DeepFind(name);
            return t != null ? t.GetComponent<Text>() : null;
        }

        Image FindImage(string name)
        {
            var t = DeepFind(name);
            return t != null ? t.GetComponent<Image>() : null;
        }

        void BindButton(string name, Action onClick)
        {
            var t = DeepFind(name);
            if (t == null) return;
            var btn = t.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(() => onClick());
        }
    }
}
