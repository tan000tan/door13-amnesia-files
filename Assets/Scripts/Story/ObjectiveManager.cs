using UnityEngine;
using Door13.UI;

namespace Door13.Story
{
    /// <summary>目标系统：按循环给出当前目标，显示于HUD。</summary>
    public class ObjectiveManager : MonoBehaviour
    {
        public static ObjectiveManager Instance { get; private set; }

        void Awake() { Instance = this; }
        void OnDestroy() { if (Instance == this) Instance = null; }

        public void ApplyLoop(int loop)
        {
            if (Hud.Instance != null)
                Hud.Instance.SetObjective(StoryTexts.Objective(loop));
        }
    }
}
