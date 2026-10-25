using UnityEngine;
using Door13.Save;

namespace Door13.Player
{
    /// <summary>手电筒：拾取后自动点亮（头顶聚光灯），照亮地下区域。</summary>
    public class FlashlightController : MonoBehaviour
    {
        Light spot;
        bool lit;

        void Start()
        {
            var head = GetComponentInChildren<Camera>() != null
                ? GetComponentInChildren<Camera>().transform.parent
                : transform;
            var go = new GameObject("FlashlightLight");
            go.transform.SetParent(head, false);
            go.transform.localPosition = new Vector3(0.1f, -0.05f, 0f);
            spot = go.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.spotAngle = 62f;
            spot.range = 15f;
            spot.intensity = 1.7f;
            spot.color = new Color(1f, 0.96f, 0.85f);
            spot.shadows = LightShadows.Hard;
            spot.enabled = false;
        }

        void Update()
        {
            bool has = GameFlags.Has(GameFlags.Flashlight);
            if (has != lit)
            {
                lit = has;
                if (spot != null) spot.enabled = lit;
            }
        }
    }
}
