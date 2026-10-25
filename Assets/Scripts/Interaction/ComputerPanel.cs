using System;
using UnityEngine;
using UnityEngine.UI;
using Door13.TimeLoop;

namespace Door13.Interaction
{
    /// <summary>
    /// 电脑终端面板（白底文档系统）：左侧文件列表、右侧内容。
    /// 文件可带证据旗标与最低循环数（随循环解锁新档案，推动剧情）。
    /// </summary>
    public class ComputerPanel : MonoBehaviour
    {
        public static ComputerPanel Instance { get; private set; }

        [Serializable]
        public struct FileEntry
        {
            public string Title;
            [TextArea(3, 30)] public string Body;
            public string EvidenceFlag;
            public int MinLoop;
        }

        public Text titleText;
        public Text bodyText;
        public RectTransform fileList;

        FileEntry[] files = new FileEntry[0];
        int current = -1;

        void Awake()
        {
            Instance = this;
            Bind("CloseButton", Close);
            gameObject.SetActive(false);
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Open(string title, FileEntry[] entries)
        {
            titleText.text = title;
            files = entries ?? new FileEntry[0];
            current = -1;
            if (bodyText != null)
                bodyText.text = "\n\n\n请从左侧选择要查看的文件。";
            RebuildList();
            gameObject.SetActive(true);
        }

        public void Close() => gameObject.SetActive(false);

        void RebuildList()
        {
            if (fileList == null) return;
            foreach (Transform c in fileList) Destroy(c.gameObject);

            int loop = LoopManager.Instance != null ? LoopManager.Instance.LoopCount : 1;
            for (int i = 0; i < files.Length; i++)
            {
                var f = files[i];
                bool locked = f.MinLoop > loop;
                var go = new GameObject("File_" + i, typeof(Image), typeof(Button), typeof(LayoutElement));
                go.transform.SetParent(fileList, false);
                go.GetComponent<LayoutElement>().minHeight = 64;
                go.GetComponent<Image>().color = locked
                    ? new Color(0.75f, 0.75f, 0.78f, 0.6f)
                    : new Color(0.88f, 0.9f, 0.95f, 1f);

                var lg = new GameObject("L", typeof(Text));
                lg.transform.SetParent(go.transform, false);
                var t = lg.GetComponent<Text>();
                t.text = (locked ? "[加密]  " : "") + f.Title;
                t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                t.fontSize = 26;
                t.color = locked ? new Color(0.4f, 0.4f, 0.45f) : new Color(0.12f, 0.14f, 0.2f);
                t.alignment = TextAnchor.MiddleLeft;
                t.raycastTarget = false;
                var rt = (RectTransform)lg.transform;
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.anchoredPosition = new Vector2(14f, 0f);
                rt.sizeDelta = new Vector2(-28f, 0f);

                if (locked) continue; // 加密文件不可点
                int captured = i;
                go.GetComponent<Button>().onClick.AddListener(() => OpenFile(captured));
            }
        }

        public void OpenFile(int index)
        {
            if (index < 0 || index >= files.Length) return;
            current = index;
            var f = files[index];
            if (bodyText != null)
                bodyText.text = "【" + f.Title + "】\n\n" + f.Body;
            if (!string.IsNullOrEmpty(f.EvidenceFlag))
                Save.GameFlags.SetOnce(f.EvidenceFlag);
            Audio.AudioDirector.Instance?.Click();
        }

        void Bind(string path, Action action)
        {
            var t = transform.Find(path);
            var btn = t != null ? t.GetComponent<Button>() : null;
            if (btn != null) btn.onClick.AddListener(() => action());
        }
    }
}
