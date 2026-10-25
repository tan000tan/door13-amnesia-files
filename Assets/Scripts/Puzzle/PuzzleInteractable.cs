using UnityEngine;
using Door13.Save;

namespace Door13.Puzzle
{
    public enum PuzzleType { Keypad, Switch, Tuner, Sort, Valve }

    /// <summary>
    /// 谜题交互物：世界对象 → 打开对应面板 → 解开后登记旗标并施加世界效果
    /// （隐藏/显现/开关门）。已解状态在载入时自动补施加效果。
    /// </summary>
    public class PuzzleInteractable : Interaction.Interactable
    {
        public PuzzleType type = PuzzleType.Keypad;

        [Tooltip("谜题ID（旗标）")]
        public string puzzleId = "";

        [Header("配置")]
        [Tooltip("键盘:数字密码 / 开关:合闸顺序如3,1,4,2,5 / 调谐:频率 / 排序:正确槽位索引 / 阀门:目标读数")]
        public string solution = "";

        [Tooltip("面板标题")]
        public string panelTitle = "";

        [Tooltip("开关/排序/阀门的选项标签（逗号分隔）")]
        public string optionLabels = "";

        [Tooltip("调谐面板提示文本")]
        public string tunerHint = "";

        [Tooltip("解开时的提示（空=「解开了。」）")]
        public string successMessage = "";

        [Header("解开后的世界效果")]
        public GameObject hideOnSolve;
        public Interaction.Pickupable revealOnSolve;

        [Tooltip("显现任意物体（灯光/可读物等）")]
        public GameObject revealGo;

        public Interaction.DoorInteractable openDoor;
        public string setFlag = "";

        [Tooltip("交互前置旗标（空=无；如控制台需先供电）")]
        public string requiredFlag = "";

        void Awake()
        {
            if (string.IsNullOrEmpty(panelTitle)) panelTitle = name;
        }

        void Start()
        {
            // 存档恢复：已解谜题补施加世界效果（幂等）
            if (IsSolved) ApplyEffects();
            else if (revealOnSolve != null) revealOnSolve.gameObject.SetActive(false);
        }

        public bool IsSolved => !string.IsNullOrEmpty(puzzleId) && GameFlags.IsSolved(puzzleId);

        public override bool CanInteract => base.CanInteract && !IsSolved;

        public override string GateMessage()
        {
            if (IsSolved) return null;
            if (!string.IsNullOrEmpty(requiredFlag) && !GameFlags.Has(requiredFlag))
                return "它没有电。";
            return base.GateMessage();
        }

        protected override void OnInteract(Interaction.PlayerInteraction actor)
        {
            switch (type)
            {
                case PuzzleType.Keypad:
                    if (KeypadPanel.Instance != null)
                        KeypadPanel.Instance.Open(panelTitle, solution, Solved);
                    break;
                case PuzzleType.Switch:
                    if (SwitchPanel.Instance != null)
                        SwitchPanel.Instance.Open(panelTitle, solution, Split(optionLabels), Solved);
                    break;
                case PuzzleType.Tuner:
                    if (TunerPanel.Instance != null)
                        TunerPanel.Instance.Open(panelTitle, ParseFloat(solution), tunerHint, Solved);
                    break;
                case PuzzleType.Sort:
                    if (SortPanel.Instance != null)
                        SortPanel.Instance.Open(panelTitle, tunerHint, Split(optionLabels), ParseInts(solution), Solved);
                    break;
                case PuzzleType.Valve:
                    if (ValvePanel.Instance != null)
                        ValvePanel.Instance.Open(panelTitle, optionLabels, solution, Solved);
                    break;
            }
        }

        void Solved()
        {
            Audio.AudioDirector.Instance?.Clunk();
            ApplyEffects();
            PuzzleManager.Solve(puzzleId,
                string.IsNullOrEmpty(successMessage) ? "解开了。" : successMessage);
        }

        /// <summary>施加解开后的世界效果（幂等；测试/存档恢复共用）。</summary>
        public void ApplyEffects()
        {
            if (hideOnSolve != null) hideOnSolve.SetActive(false);
            if (revealOnSolve != null) revealOnSolve.gameObject.SetActive(true);
            if (revealGo != null) revealGo.SetActive(true);
            if (openDoor != null) openDoor.Open();
            if (!string.IsNullOrEmpty(setFlag)) GameFlags.SetOnce(setFlag);
        }

        /// <summary>测试/无障碍入口：直接判定解开。</summary>
        public void SolveNow()
        {
            if (!IsSolved) Solved();
            else ApplyEffects();
        }

        static string[] Split(string csv)
        {
            if (string.IsNullOrEmpty(csv)) return new string[0];
            return csv.Split(',');
        }

        static int[] ParseInts(string csv)
        {
            var parts = Split(csv);
            var arr = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++) int.TryParse(parts[i].Trim(), out arr[i]);
            return arr;
        }

        static float ParseFloat(string s)
        {
            float f; float.TryParse(s, out f); return f;
        }
    }
}
