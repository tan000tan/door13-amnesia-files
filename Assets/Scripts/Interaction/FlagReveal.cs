using UnityEngine;

namespace Door13.Interaction
{
    /// <summary>旗标联动显现（通用小组件）：旗标存在→激活自身，否则保持隐藏。</summary>
    public class FlagReveal : MonoBehaviour
    {
        public string flag;
        bool last;

        void Start() { Apply(); }

        void Update()
        {
            bool on = !string.IsNullOrEmpty(flag) && Save.GameFlags.Has(flag);
            if (on != last) Apply();
        }

        public void Apply()
        {
            last = !string.IsNullOrEmpty(flag) && Save.GameFlags.Has(flag);
            if (gameObject.activeSelf != last)
                gameObject.SetActive(last);
        }
    }
}
