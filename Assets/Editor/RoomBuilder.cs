using System.Collections.Generic;
using Door13.Interaction;
using Door13.TimeLoop;
using UnityEngine;

namespace Door13.EditorTools
{
    /// <summary>
    /// 场景编排：调用各房间生成器 + 共享构建助手（墙体开口/可读物/拾取物/墙字/触发区）。
    /// 布局：R0苏醒室(x-6..6,z-5..5) → R1回廊(x-3..3,z-24..-5) →
    /// 西侧 R2实验室/R4控制中心，东侧 R3档案室/R5地下区域 → 北端 R6十三门后室。
    /// </summary>
    public static class RoomBuilder
    {
        public static void Build()
        {
            RoomR0.Build();
            RoomR1.Build();
            RoomR2.Build();
            RoomR3.Build();
            RoomR4.Build();
            RoomR5R6.Build();
            BuildManagers();
        }

        static void BuildManagers()
        {
            var spawn = GameObject.Find("Spawn_R0");
            var lmGo = new GameObject("LoopManager");
            var lm = lmGo.AddComponent<Door13.TimeLoop.LoopManager>();
            if (spawn != null) lm.spawnPoint = spawn.transform;
            new GameObject("ObjectiveManager").AddComponent<Door13.Story.ObjectiveManager>();
            new GameObject("EndingManager").AddComponent<Door13.Story.EndingManager>();
        }

        // ---------- 材质 ----------
        public static Material Mat(string name, Color c)
            => ProjectSetup.EnsureMaterial("Assets/Materials/" + name + ".mat", c);

        // ---------- 几何 ----------
        public static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);

        public static GameObject Box(Transform parent, string name, Vector3 center, Vector3 size,
            Material mat, bool statik = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.isStatic = statik;
            go.transform.position = center;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (parent != null) go.transform.SetParent(parent, true);
            return go;
        }

        const float WallT = 0.2f;
        const float DoorH = 2.3f;

        /// <summary>沿X的墙（固定z），gaps 为 (中心,宽) 序列的门洞。</summary>
        public static void WallX(Transform p, string name, float z, float x0, float x1, float h,
            Material m, params float[] gaps)
            => Wall(p, name, true, z, x0, x1, h, m, gaps);

        /// <summary>沿Z的墙（固定x），gaps 为 (中心,宽) 序列的门洞。</summary>
        public static void WallZ(Transform p, string name, float x, float z0, float z1, float h,
            Material m, params float[] gaps)
            => Wall(p, name, false, x, z0, z1, h, m, gaps);

        static void Wall(Transform p, string name, bool alongX, float fixedC, float a0, float a1,
            float h, Material m, float[] gaps)
        {
            var list = new List<float[]>();
            for (int i = 0; i + 1 < gaps.Length; i += 2) list.Add(new[] { gaps[i], gaps[i + 1] });
            list.Sort((x, y) => x[0].CompareTo(y[0]));

            float cur = a0;
            int idx = 0;
            foreach (var g in list)
            {
                float gc = g[0], gw = g[1];
                float segEnd = gc - gw * 0.5f;
                if (segEnd > cur + 0.01f) Seg(p, name + idx, alongX, fixedC, cur, segEnd, h, m, false);
                Seg(p, name + "_lintel" + idx, alongX, fixedC, gc - gw * 0.5f, gc + gw * 0.5f,
                    h - DoorH, m, true);
                cur = gc + gw * 0.5f;
                idx++;
            }
            if (a1 > cur + 0.01f) Seg(p, name + idx, alongX, fixedC, cur, a1, h, m, false);
        }

        static void Seg(Transform p, string name, bool alongX, float fixedC, float a0, float a1,
            float h, Material m, bool lintel)
        {
            float len = a1 - a0;
            float mid = (a0 + a1) * 0.5f;
            Vector3 center = alongX ? V(mid, lintel ? (DoorH + h) * 0.5f : h * 0.5f, fixedC)
                                    : V(fixedC, lintel ? (DoorH + h) * 0.5f : h * 0.5f, mid);
            Vector3 size = alongX ? V(len, h, WallT) : V(WallT, h, len);
            Box(p, name, center, size, m);
        }

        public static void Shell(Transform room, float x0, float x1, float z0, float z1, float h,
            Material floor, Material wall, params float[] doorways)
        {
            // doorways: [z或x轴, 墙, 中心, 宽] → 每面墙单独调用更直观，这里仅铺地板/天花
            Box(room, "Floor", V((x0 + x1) / 2, -0.1f, (z0 + z1) / 2), V(x1 - x0, 0.2f, z1 - z0), floor);
            var ceil = Box(room, "Ceiling", V((x0 + x1) / 2, h + 0.1f, (z0 + z1) / 2), V(x1 - x0, 0.2f, z1 - z0), wall);
            ceil.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // ---------- 内容 ----------
        public static Readable ReadableOn(GameObject go, string title, string body,
            string prompt = "查看", string evidence = "")
        {
            var r = go.AddComponent<Readable>();
            r.title = title;
            r.body = body;
            r.prompt = prompt;
            r.evidenceFlag = evidence;
            return r;
        }

        public static Pickupable PickupOn(GameObject go, string itemId, string prompt = "拾取",
            string flag = "")
        {
            var p = go.AddComponent<Pickupable>();
            p.itemId = itemId;
            p.prompt = prompt;
            p.pickupFlag = flag;

            // 关键物品自发光：正常光线下也一眼可辨
            var r = go.GetComponent<Renderer>();
            if (r != null && r.sharedMaterial != null)
            {
                var mat = r.sharedMaterial;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", mat.color * 0.55f);
            }
            return p;
        }

        /// <summary>墙上文字（TextMesh）：yaw 调用方保证可读面（局部-Z）朝房内。</summary>
        public static TextMesh WallText(Transform parent, Vector3 pos, float yawDeg, string text,
            int fontSize = 48, float charSize = 0.12f)
        {
            var go = new GameObject("WallText_" + parent.name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, yawDeg, 0f);
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.font = ProjectSetup.GetBuiltinFont();
            tm.fontSize = fontSize;
            tm.characterSize = charSize;
            tm.color = new Color(0.12f, 0.12f, 0.14f);
            tm.anchor = TextAnchor.MiddleLeft;
            tm.alignment = TextAlignment.Left;
            go.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
            return tm;
        }

        /// <summary>房间触发区（首次进入：存档房间/旁白/惊吓/灯闪/环境音）。</summary>
        public static void Trigger(Transform room, string roomId, Vector3 center, Vector3 size,
            string narration = "", int scare = 0, Light flicker = null, int ambient = 0)
        {
            var go = new GameObject("Trigger_" + roomId);
            go.transform.SetParent(room);
            go.transform.position = center;
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = size;
            var tz = go.AddComponent<TriggerZone>();
            tz.roomId = roomId;
            tz.narrationBody = narration;
            tz.scareSound = scare;
            tz.flickerLight = flicker;
            tz.ambientType = ambient;
        }

        /// <summary>房间出生点（存档恢复/循环重置落点）。</summary>
        public static void Spawn(string roomId, Vector3 pos, float yaw)
        {
            var go = new GameObject("Spawn_" + roomId);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        public static Light PointLight(Transform parent, string name, Vector3 pos, Color c,
            float intensity, float range)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = c;
            l.intensity = intensity;
            l.range = range;
            l.shadows = LightShadows.Soft;
            return l;
        }

        /// <summary>制作一扇门（转轴+门扇+DoorInteractable），返回转轴。</summary>
        public static GameObject MakeDoor(Transform parent, string name, Vector3 pivotPos,
            float yawDeg, string requiredItem, string lockedMsg, string unlockedByFlag = "",
            int minLoop = 0, Material mat = null)
        {
            var pivot = new GameObject(name);
            pivot.transform.SetParent(parent);
            pivot.transform.position = pivotPos;
            pivot.transform.rotation = Quaternion.Euler(0f, yawDeg, 0f);
            var doorMat = mat != null ? mat : Mat("M_Door", new Color(0.30f, 0.31f, 0.34f));
            var leaf = ProjectSetup.Box("Leaf", Vector3.zero, Vector3.one, doorMat);
            leaf.transform.SetParent(pivot.transform, false);
            leaf.transform.localPosition = V(0.55f, 0f, 0f);
            leaf.transform.localScale = V(1.1f, 2.2f, 0.1f);
            leaf.isStatic = false;

            // 门框立柱（不随门转）+ 底槛 + 把手
            var frameMat = Mat("M_Frame", new Color(0.36f, 0.37f, 0.40f));
            var right = pivot.transform.right;
            var jambA = ProjectSetup.Box("Jamb_A", pivotPos - right * 0.07f, V(0.14f, 2.4f, 0.3f), frameMat);
            var jambB = ProjectSetup.Box("Jamb_B", pivotPos + right * 1.17f, V(0.14f, 2.4f, 0.3f), frameMat);
            var sill = ProjectSetup.Box("Sill", pivotPos + right * 0.55f, V(1.3f, 0.05f, 0.34f), frameMat);
            jambA.isStatic = false; jambB.isStatic = false; sill.isStatic = false;
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.name = "Handle";
            handle.transform.SetParent(leaf.transform, false);
            handle.transform.localPosition = V(0.85f / 1.1f, -0.12f / 2.2f, 0.55f);
            handle.transform.localScale = V(0.07f / 1.1f, 0.34f / 2.2f, 0.06f / 0.1f);
            handle.GetComponent<Renderer>().sharedMaterial = Mat("M_Handle", new Color(0.75f, 0.72f, 0.6f));
            handle.GetComponent<Collider>().enabled = false;
            handle.isStatic = false;

            var d = pivot.AddComponent<DoorInteractable>();
            d.doorLeaf = pivot.transform;
            d.requiredItemId = requiredItem ?? "";
            d.lockedMessage = string.IsNullOrEmpty(lockedMsg) ? "门被锁死了。" : lockedMsg;
            d.unlockedByFlag = unlockedByFlag ?? "";
            d.minLoop = minLoop;
            return pivot;
        }

        /// <summary>给物体挂循环规则（未激活的物体也能生效，LoopManager 全量扫描）。</summary>
        public static LoopObject LoopRule(GameObject go, string objectId, int fromLoop,
            bool setActive, bool activeValue, string setText = null)
        {
            var lo = go.GetComponent<LoopObject>();
            if (lo == null) lo = go.AddComponent<LoopObject>();
            lo.objectId = objectId;
            var rules = new List<LoopStateEntry>(lo.rules ?? new LoopStateEntry[0]);
            var e = new LoopStateEntry { fromLoop = fromLoop };
            if (setActive) { e.setActive = true; e.activeValue = activeValue; }
            if (setText != null) {
                e.setText = true;
                e.textValue = setText;
                if (lo.textTarget == null)
                    lo.textTarget = go.GetComponent<TextMesh>();
            }
            rules.Add(e);
            lo.rules = rules.ToArray();
            return lo;
        }
    }
}
