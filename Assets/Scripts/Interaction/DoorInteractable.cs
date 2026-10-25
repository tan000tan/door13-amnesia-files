using UnityEngine;

namespace Door13.Interaction
{
    /// <summary>
    /// 房门：门禁（所需物品/循环）由基类处理，这里负责开门动画与状态。
    /// 每次循环重置为关闭（门是"世界状态"的一部分）。
    /// </summary>
    public class DoorInteractable : Interactable, TimeLoop.ILoopResettable
    {
        [Tooltip("门扇转轴")]
        public Transform doorLeaf;

        [Tooltip("已解谜题旗标替代物品门禁（如电子锁已解），空=无")]
        public string unlockedByFlag = "";

        public bool IsOpen { get; private set; }

        void Awake()
        {
            prompt = "开门";
            TimeLoop.LoopManager.Register(this);
        }

        public override bool CanInteract => base.CanInteract && !IsOpen;

        protected override void OnInteract(PlayerInteraction actor)
        {
            Open();
        }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            Audio.AudioDirector.Instance?.DoorCreak();
            if (doorLeaf != null)
            {
                opening = Swing(doorLeaf.localRotation,
                    Quaternion.Euler(0f, -100f, 0f));
                StartCoroutine(opening);
            }
        }

        System.Collections.IEnumerator opening;

        private System.Collections.IEnumerator Swing(Quaternion from, Quaternion to)
        {
            float t = 0f;
            const float dur = 0.45f;
            while (t < dur)
            {
                t += Time.deltaTime;
                if (doorLeaf != null)
                    doorLeaf.localRotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0f, 1f, t / dur));
                yield return null;
            }
            if (doorLeaf != null) doorLeaf.localRotation = to;
        }

        public void OnLoopReset(int loop)
        {
            IsOpen = false;
            if (opening != null) { StopCoroutine(opening); opening = null; }
            if (doorLeaf != null)
                doorLeaf.localRotation = Quaternion.identity;
        }

        /// <summary>电子锁型门：谜题旗标已满足即视为门禁通过。</summary>
        public override string GateMessage()
        {
            if (!string.IsNullOrEmpty(unlockedByFlag) && Save.GameFlags.Has(unlockedByFlag))
                return null;
            return base.GateMessage();
        }
    }
}
