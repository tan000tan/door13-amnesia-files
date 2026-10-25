using UnityEngine;

namespace Door13.Audio
{
    /// <summary>
    /// 灯光微闪烁：强度围绕基准做平滑噪声摆动，偶发短促骤暗。
    /// 营造"供电不稳的废弃研究所"氛围；不改变平均亮度方向。
    /// </summary>
    public class FlickerLight : MonoBehaviour
    {
        [Range(0f, 1f)] public float amount = 0.16f;
        public float speed = 1.6f;

        Light l;
        float baseIntensity;
        float seed;
        bool captured;

        void Awake()
        {
            seed = Random.value * 100f;
        }

        void Update()
        {
            if (l == null)
            {
                l = GetComponent<Light>();
                if (l == null) return;
            }
            if (!captured) { baseIntensity = l.intensity; captured = true; }

            float sway = Mathf.PerlinNoise(seed + Time.time * speed, seed * 0.73f);
            float spike = Mathf.PerlinNoise(seed * 1.7f + Time.time * speed * 4.3f, 7.31f);
            float dip = spike > 0.86f ? (spike - 0.86f) * 6f : 0f;
            l.intensity = baseIntensity * (1f - amount * (0.55f * sway + 0.45f * dip));
        }
    }
}
