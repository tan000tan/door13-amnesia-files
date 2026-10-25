using UnityEngine;

namespace Door13.TimeLoop
{
    [System.Serializable]
    public struct LoopStateEntry
    {
        [Tooltip("从第几次循环开始生效（含本循环，累计生效）")]
        public int fromLoop;

        public bool setActive;
        public bool activeValue;

        public bool setText;
        public string textValue;

        public bool setColor;
        public Color colorValue;
    }

    /// <summary>
    /// 受时间循环影响的场景对象：规则表序列化在组件上（数据驱动，无硬编码），
    /// LoopManager 每次循环按当前次数应用所有规则（fromLoop ≤ 当前循环 的累计生效）。
    /// </summary>
    public class LoopObject : MonoBehaviour
    {
        [Tooltip("对象标识（存档/调试用）")]
        public string objectId = "";

        [Tooltip("循环状态规则表")]
        public LoopStateEntry[] rules;

        [Tooltip("规则作用的目标对象（缺省=自身）")]
        public GameObject target;

        [Tooltip("setText 的目标（可选）")]
        public TextMesh textTarget;

        [Tooltip("setColor 的目标（可选）")]
        public Renderer colorTarget;

        void Awake()
        {
            LoopManager.RegisterLoopObject(this);
        }

        public void Apply(int loop)
        {
            GameObject go = target != null ? target : gameObject;
            foreach (var r in rules)
            {
                if (loop < r.fromLoop) continue;
                if (r.setActive) go.SetActive(r.activeValue);
                if (r.setText && textTarget != null) textTarget.text = r.textValue;
                if (r.setColor && colorTarget != null) colorTarget.material.color = r.colorValue;
            }
        }
    }
}
