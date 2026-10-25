using UnityEngine;

namespace Door13.Interaction
{
    /// <summary>
    /// 房间触发区：玩家首次进入→登记房间（存档）、更新目标、可选一次性惊吓
    /// （音效+灯光闪烁）、可选一次性旁白。
    /// </summary>
    public class TriggerZone : MonoBehaviour
    {
        [Tooltip("房间标识（存档用，如 R1）")]
        public string roomId = "";

        [Tooltip("首次进入的旁白标题/正文（空=无）")]
        public string narrationTitle = "";
        [Multiline] public string narrationBody = "";

        [Tooltip("惊吓音效类型：0无 1重击 2金属 3耳语")]
        public int scareSound = 0;

        [Tooltip("房间环境音：0默认低鸣 1电流 2机器 3深处")]
        public int ambientType = 0;

        [Tooltip("闪烁的灯（可选，仅首次进入时闪几下）")]
        public Light flickerLight;

        bool fired;

        void Reset()
        {
            var c = GetComponent<BoxCollider>();
            if (c == null) gameObject.AddComponent<BoxCollider>();
            if (c != null) c.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (fired) return;
            if (other.GetComponentInParent<Player.PlayerController>() == null) return;
            fired = true;

            if (Audio.AudioDirector.Instance != null)
                Audio.AudioDirector.Instance.SetRoomAmbient(ambientType);

            if (!string.IsNullOrEmpty(roomId))
            {
                Save.SaveManager.RecordRoom(roomId);
            }
            if (!string.IsNullOrEmpty(narrationBody) && UI.Hud.Instance != null)
                StartCoroutine(UI.Hud.Instance.ShowNarration(narrationBody, 3.2f));
            if (scareSound > 0)
                Audio.AudioDirector.Instance?.Scare(scareSound);
            if (flickerLight != null)
                StartCoroutine(Flicker());
        }

        System.Collections.IEnumerator Flicker()
        {
            float t = 0f;
            float baseI = flickerLight != null ? flickerLight.intensity : 0f;
            while (t < 1.2f)
            {
                t += Time.deltaTime;
                if (flickerLight != null)
                    flickerLight.intensity = Random.value > 0.5f ? baseI : baseI * 0.15f;
                yield return null;
            }
            if (flickerLight != null) flickerLight.intensity = baseI;
        }

        /// <summary>存档恢复/循环后调用：已触发过的房间不再重复演出。</summary>
        public void MarkFired() { fired = true; }

        /// <summary>新游戏时重置演出状态。</summary>
        public void ResetZone() { fired = false; }
    }
}
