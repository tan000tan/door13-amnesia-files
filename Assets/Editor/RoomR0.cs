using UnityEngine;
using Door13.Audio;
using Door13.Interaction;
using Door13.Story;

namespace Door13.EditorTools
{
    /// <summary>
    /// R0 苏醒实验室（体验版重制）：完整房间+丰富环境细节+电脑文件系统。
    /// x[-6,6] z[-5,5]。
    /// </summary>
    public static class RoomR0
    {
        public static void Build()
        {
            var room = new GameObject("Room_R0");
            var wall = RoomBuilder.Mat("M_Wall", new Color(0.52f, 0.54f, 0.58f));
            var floor = RoomBuilder.Mat("M_Floor", new Color(0.40f, 0.41f, 0.44f));
            var wood = RoomBuilder.Mat("M_Wood", new Color(0.52f, 0.40f, 0.28f));
            var metal = RoomBuilder.Mat("M_Metal", new Color(0.44f, 0.47f, 0.51f));
            var paper = RoomBuilder.Mat("M_Paper", new Color(0.82f, 0.81f, 0.76f));
            var yellow = RoomBuilder.Mat("M_Yellow", new Color(0.92f, 0.87f, 0.45f));
            var dark = RoomBuilder.Mat("M_Dark", new Color(0.08f, 0.08f, 0.10f));
            var pipe = RoomBuilder.Mat("M_Pipe", new Color(0.48f, 0.40f, 0.33f));

            RoomBuilder.Shell(room.transform, -6, 6, -5, 5, 3.2f, floor, wall);
            RoomBuilder.WallX(room.transform, "N", -5.1f, -6.2f, 6.2f, 3.2f, wall, 0f, 1.6f);
            RoomBuilder.WallX(room.transform, "S", 5.1f, -6.2f, 6.2f, 3.2f, wall);
            RoomBuilder.WallZ(room.transform, "W", -6.1f, -5.2f, 5.2f, 3.2f, wall);
            RoomBuilder.WallZ(room.transform, "E", 6.1f, -5.2f, 5.2f, 3.2f, wall);

            // 门（需门禁卡）→ 回廊
            RoomBuilder.MakeDoor(room.transform, "Door_R0", RoomBuilder.V(-0.8f, 1.1f, -5.0f), 0f,
                "keycard", StoryTexts.DoorLocked);
            // 门框加固：门后为什么打不开——门框有额外的金属加固条（暗示门从外部被锁死）
            RoomBuilder.Box(room.transform, "DoorFrameMetalL", RoomBuilder.V(-1.25f, 1.1f, -5.05f), RoomBuilder.V(0.06f, 2.1f, 0.06f), dark, false);
            RoomBuilder.Box(room.transform, "DoorFrameMetalR", RoomBuilder.V(-0.35f, 1.1f, -5.05f), RoomBuilder.V(0.06f, 2.1f, 0.06f), dark, false);
            RoomBuilder.Box(room.transform, "DoorFrameMetalTop", RoomBuilder.V(-0.8f, 2.15f, -5.05f), RoomBuilder.V(0.96f, 0.06f, 0.06f), dark, false);
            // 门后警示标识（门打不开的原因：外部封锁）
            var warningPlate = RoomBuilder.Box(room.transform, "DoorWarning", RoomBuilder.V(-0.8f, 1.8f, -5.08f), RoomBuilder.V(0.35f, 0.2f, 0.01f),
                RoomBuilder.Mat("M_Warning", new Color(0.85f, 0.75f, 0.15f)));
            RoomBuilder.ReadableOn(warningPlate, "门后警示牌",
                "观察区07室 · 外部封锁\n\n未经许可，严禁开启。\n—— 回廊7号安全委员会", "查看警示牌");

            // ---- 环境细节：管线 / 顶灯 / 文件柜 / 储物柜 / 废弃设备 ----
            // 沿西墙与北墙的管线（三根长条）
            RoomBuilder.Box(room.transform, "Pipe_W1", RoomBuilder.V(-5.95f, 2.9f, 0f), RoomBuilder.V(0.08f, 0.08f, 9.6f), pipe);
            RoomBuilder.Box(room.transform, "Pipe_W2", RoomBuilder.V(-5.95f, 2.7f, 0f), RoomBuilder.V(0.08f, 0.08f, 9.6f), pipe);
            RoomBuilder.Box(room.transform, "Pipe_N1", RoomBuilder.V(-4f, 2.9f, -5.95f), RoomBuilder.V(3.6f, 0.08f, 0.08f), pipe);
            // 顶灯灯具（自发光条）
            var lampMat = RoomBuilder.Mat("M_Lamp", new Color(0.95f, 0.95f, 0.9f));
            lampMat.EnableKeyword("_EMISSION");
            lampMat.SetColor("_EmissionColor", new Color(0.9f, 0.92f, 0.85f) * 1.1f);
            RoomBuilder.Box(room.transform, "CeilLamp_1", RoomBuilder.V(-2.5f, 3.08f, 0f), RoomBuilder.V(1.4f, 0.05f, 0.3f), lampMat);
            RoomBuilder.Box(room.transform, "CeilLamp_2", RoomBuilder.V(2.5f, 3.08f, 0f), RoomBuilder.V(1.4f, 0.05f, 0.3f), lampMat);
            // 文件柜（北墙西段）
            RoomBuilder.Box(room.transform, "FileCabinet", RoomBuilder.V(-4.3f, 0.9f, -4.6f), RoomBuilder.V(1.4f, 1.8f, 0.55f), metal);
            RoomBuilder.Box(room.transform, "CabinetDrawer1", RoomBuilder.V(-4.3f, 0.5f, -4.3f), RoomBuilder.V(1.2f, 0.5f, 0.06f), dark);
            RoomBuilder.Box(room.transform, "CabinetDrawer2", RoomBuilder.V(-4.3f, 1.1f, -4.3f), RoomBuilder.V(1.2f, 0.5f, 0.06f), dark);
            // 储物柜（东墙）
            RoomBuilder.Box(room.transform, "Locker1", RoomBuilder.V(5.7f, 1.0f, -2.5f), RoomBuilder.V(0.55f, 2.0f, 1.0f), metal);
            RoomBuilder.Box(room.transform, "Locker2", RoomBuilder.V(5.7f, 1.0f, -1.3f), RoomBuilder.V(0.55f, 2.0f, 1.0f), metal);
            // 倾倒的废弃设备（南角）
            var fallen = RoomBuilder.Box(room.transform, "FallenDevice", RoomBuilder.V(2.0f, 0.3f, 4.2f), RoomBuilder.V(1.0f, 0.6f, 0.7f), metal, false);
            fallen.transform.rotation = Quaternion.Euler(0f, 35f, 88f);
            // 地面散落文件与污渍
            for (int i = 0; i < 5; i++)
            {
                var p = RoomBuilder.Box(room.transform, "Scatter_" + i,
                    RoomBuilder.V(-2.2f + i * 1.1f, 0.015f, 1.4f - (i % 2) * 0.7f),
                    RoomBuilder.V(0.3f, 0.012f, 0.4f), paper, false);
                p.transform.rotation = Quaternion.Euler(0f, i * 47f, 0f);
            }
            var stainMat = RoomBuilder.Mat("M_Stain", new Color(0.15f, 0.14f, 0.12f));
            RoomBuilder.Box(room.transform, "Stain_1", RoomBuilder.V(-1.0f, 0.012f, 2.4f), RoomBuilder.V(1.2f, 0.01f, 0.8f), stainMat);
            RoomBuilder.Box(room.transform, "Stain_2", RoomBuilder.V(3.2f, 0.012f, -3.4f), RoomBuilder.V(0.8f, 0.01f, 1.1f), stainMat);

            // ---- 桌面：研究员曾在此工作（每件物品都有出处）----
            var mug = RoomBuilder.Mat("M_Mug", new Color(0.85f, 0.86f, 0.88f));
            RoomBuilder.Box(room.transform, "Mug", RoomBuilder.V(-4.38f, 0.87f, 1.0f), RoomBuilder.V(0.09f, 0.1f, 0.09f), mug, false);
            RoomBuilder.Box(room.transform, "MugHandle", RoomBuilder.V(-4.31f, 0.87f, 1.0f), RoomBuilder.V(0.05f, 0.05f, 0.02f), mug, false);
            RoomBuilder.Box(room.transform, "Pen", RoomBuilder.V(-4.52f, 0.835f, 0.2f), RoomBuilder.V(0.015f, 0.015f, 0.16f), dark, false);
            var notebook = RoomBuilder.Box(room.transform, "Notebook", RoomBuilder.V(-4.55f, 0.845f, 0.9f), RoomBuilder.V(0.24f, 0.015f, 0.32f), paper);
            RoomBuilder.ReadableOn(notebook, "实验记录本",
                "值班记录 · 11月2日\n\n0403 之后，全所权限暂停。\n地下通道封闭。\n\n——不要让 T-13 看到今晚的记录。", "翻开记录本");
            var frame = RoomBuilder.Box(room.transform, "PhotoFrame", RoomBuilder.V(-4.88f, 0.88f, -0.1f), RoomBuilder.V(0.13f, 0.17f, 0.02f),
                RoomBuilder.Mat("M_PhotoOld", new Color(0.85f, 0.8f, 0.7f)), false);
            RoomBuilder.ReadableOn(frame, "桌上的相框",
                "一群人站在研究所门口的合影。\n有人笑着比了手势。\n\n背面用铅笔写着：\n『明天见。』\n\n字迹被水洇开过。", "端详相框");

            // ---- 地面脚印：从床边走向保险箱——夜里有人来过 ----
            var printMat = RoomBuilder.Mat("M_Print", new Color(0.2f, 0.19f, 0.17f));
            float[] fpz = { 2.3f, 0.95f, -0.7f };
            float[] fpx = { 3.3f, 2.1f, 0.7f };
            for (int i = 0; i < 3; i++)
            {
                var p = RoomBuilder.Box(room.transform, "Footprint_" + i, RoomBuilder.V(fpx[i], 0.011f, fpz[i]),
                    RoomBuilder.V(0.14f, 0.006f, 0.3f), printMat, false);
                p.transform.rotation = Quaternion.Euler(0f, 42f + i * 5f, 0f);
            }

            // ---- 天花板：垂落的电线（坏掉的灯位）----
            RoomBuilder.Box(room.transform, "HangingWire", RoomBuilder.V(3.5f, 2.72f, -2.0f), RoomBuilder.V(0.02f, 0.72f, 0.02f), dark, false);
            RoomBuilder.Box(room.transform, "WireEnd", RoomBuilder.V(3.5f, 2.32f, -2.0f), RoomBuilder.V(0.06f, 0.09f, 0.06f), dark, false);

            // ---- 角落：待移交的纸箱 + 移交清单（剧情：缺少T-13）----
            var carton = RoomBuilder.Mat("M_Carton", new Color(0.6f, 0.55f, 0.45f));
            RoomBuilder.Box(room.transform, "Carton1", RoomBuilder.V(5.3f, 0.22f, -4.4f), RoomBuilder.V(0.6f, 0.44f, 0.5f), carton);
            var carton2 = RoomBuilder.Box(room.transform, "Carton2", RoomBuilder.V(5.3f, 0.64f, -4.35f), RoomBuilder.V(0.5f, 0.4f, 0.45f), carton, false);
            carton2.transform.rotation = Quaternion.Euler(0f, 18f, 0f);
            var manifest = RoomBuilder.Box(room.transform, "Manifest", RoomBuilder.V(5.3f, 0.9f, -4.35f), RoomBuilder.V(0.26f, 0.015f, 0.34f), paper);
            RoomBuilder.ReadableOn(manifest, "移交清单",
                "设备移交 · 观察区07室\n\n样本档案：12 份 ✓\n录像带：2 盘 ✓\n人身物品：6 件 ✓\n\n缺少：T-13。\n——找不到他的东西。", "查看清单");

            // ---- 医疗床 + 撕下的便签 ----
            RoomBuilder.Box(room.transform, "CotFrame", RoomBuilder.V(4.2f, 0.2f, 3.5f), RoomBuilder.V(2.0f, 0.4f, 0.95f), metal);
            RoomBuilder.Box(room.transform, "Mattress", RoomBuilder.V(4.2f, 0.46f, 3.5f), RoomBuilder.V(1.9f, 0.12f, 0.85f),
                RoomBuilder.Mat("M_Fabric", new Color(0.72f, 0.73f, 0.78f)));
            RoomBuilder.Box(room.transform, "Pillow", RoomBuilder.V(4.95f, 0.55f, 3.5f), RoomBuilder.V(0.40f, 0.10f, 0.50f),
                RoomBuilder.Mat("M_Pillow", new Color(0.85f, 0.85f, 0.87f)));
            var note = RoomBuilder.Box(room.transform, "NoteItem", RoomBuilder.V(4.0f, 0.55f, 3.3f), RoomBuilder.V(0.24f, 0.015f, 0.18f), yellow, false);
            RoomBuilder.PickupOn(note, "note");

            // 工牌（循环3出现在枕头下）
            var badge = RoomBuilder.Box(room.transform, "BadgeItem", RoomBuilder.V(4.9f, 0.56f, 3.6f), RoomBuilder.V(0.20f, 0.02f, 0.14f),
                RoomBuilder.Mat("M_Keycard", new Color(0.80f, 0.82f, 0.88f)), false);
            badge.AddComponent<Pickupable>().itemId = "badge";
            badge.SetActive(false);
            RoomBuilder.LoopRule(badge, "badge_spawn", 3, true, true);

            // 床边拖鞋（循环2出现，鞋尖朝墙——"昨夜有人站在那里，一直看着照片"）
            var slipMat = RoomBuilder.Mat("M_Slipper", new Color(0.2f, 0.22f, 0.3f));
            var slip1 = RoomBuilder.Box(room.transform, "SlipperL", RoomBuilder.V(3.55f, 0.03f, 2.62f), RoomBuilder.V(0.12f, 0.035f, 0.3f), slipMat, false);
            slip1.SetActive(false);
            RoomBuilder.LoopRule(slip1, "slipper_l", 2, true, true);
            var slip2 = RoomBuilder.Box(room.transform, "SlipperR", RoomBuilder.V(3.85f, 0.03f, 2.58f), RoomBuilder.V(0.12f, 0.035f, 0.3f), slipMat, false);
            slip2.SetActive(false);
            RoomBuilder.LoopRule(slip2, "slipper_r", 2, true, true);

            // ---- 桌台 + 电脑（白底文件系统）+ 便签 ----
            RoomBuilder.Box(room.transform, "DeskTop", RoomBuilder.V(-4.7f, 0.78f, 0.5f), RoomBuilder.V(1.0f, 0.08f, 1.9f), wood);
            RoomBuilder.Box(room.transform, "DeskLeg1", RoomBuilder.V(-4.3f, 0.38f, -0.35f), RoomBuilder.V(0.08f, 0.76f, 0.08f), wood);
            RoomBuilder.Box(room.transform, "DeskLeg2", RoomBuilder.V(-4.3f, 0.38f, 1.35f), RoomBuilder.V(0.08f, 0.76f, 0.08f), wood);
            RoomBuilder.Box(room.transform, "DeskLeg3", RoomBuilder.V(-5.1f, 0.38f, -0.35f), RoomBuilder.V(0.08f, 0.76f, 0.08f), wood);
            RoomBuilder.Box(room.transform, "DeskLeg4", RoomBuilder.V(-5.1f, 0.38f, 1.35f), RoomBuilder.V(0.08f, 0.76f, 0.08f), wood);
            RoomBuilder.Box(room.transform, "Monitor", RoomBuilder.V(-5.02f, 1.12f, 0.5f), RoomBuilder.V(0.10f, 0.44f, 0.62f), dark);
            var screenMat = RoomBuilder.Mat("M_Screen", new Color(0.85f, 0.9f, 0.95f));
            screenMat.EnableKeyword("_EMISSION");
            screenMat.SetColor("_EmissionColor", new Color(0.75f, 0.82f, 0.9f) * 0.9f);
            var terminal = RoomBuilder.Box(room.transform, "TerminalScreen", RoomBuilder.V(-4.96f, 1.14f, 0.5f), RoomBuilder.V(0.02f, 0.36f, 0.54f), screenMat);
            var pc = terminal.AddComponent<ComputerTerminal>();
            pc.screenTitle = "终端 — 回廊7号内网";
            pc.files = new[]
            {
                new ComputerPanel.FileEntry{ Title="通知 · 回廊7号", Body=ComputerFiles.R0_Notice },
                new ComputerPanel.FileEntry{ Title="应急协议 · 受试者查询", Body=ComputerFiles.R0_Emergency, EvidenceFlag="ev_terminal_r0" },
                new ComputerPanel.FileEntry{ Title="实验日志 · 第9~12次", Body=ComputerFiles.R0_Log9to12, MinLoop=2 },
                new ComputerPanel.FileEntry{ Title="异常报告 · 13号样本", Body=ComputerFiles.R0_Anomaly, EvidenceFlag="ev_anomaly", MinLoop=3 },
                new ComputerPanel.FileEntry{ Title="档案 T-13 · 完整版", Body=ComputerFiles.R0_Full, EvidenceFlag="ev_terminal_full", MinLoop=4 },
            };
            var sticky = RoomBuilder.Box(room.transform, "StickyNote", RoomBuilder.V(-4.5f, 0.83f, 1.2f), RoomBuilder.V(0.20f, 0.015f, 0.20f), yellow);
            RoomBuilder.ReadableOn(sticky, StoryTexts.StickyTitle, StoryDocs.StickyR0, "查看便签");

            // ---- 照片墙 + 人影（循环2出现）----
            var board = RoomBuilder.Box(room.transform, "PhotoBoard", RoomBuilder.V(-1.5f, 1.9f, -4.95f), RoomBuilder.V(2.8f, 1.4f, 0.06f), paper);
            RoomBuilder.ReadableOn(board, StoryTexts.PhotoWallTitle, StoryDocs.PhotoWallR0, "查看照片墙");
            // 图钉/夹子细节（每张照片的角上有固定物）
            var pinMat = RoomBuilder.Mat("M_Pin", new Color(0.45f, 0.45f, 0.48f));
            float[] px = { -2.55f, -1.85f, -1.15f, -0.45f };
            for (int i = 0; i < 4; i++)
            {
                // 照片底板
                RoomBuilder.Box(room.transform, "Photo_" + (i + 1), RoomBuilder.V(px[i], 1.9f, -4.90f), RoomBuilder.V(0.55f, 0.45f, 0.02f),
                    RoomBuilder.Mat("M_Photo" + (i + 1), new Color(0.55f + 0.05f * i, 0.57f + 0.05f * i, 0.58f + 0.04f * i)));
                // 四角图钉
                float cx = px[i], cy = 1.9f;
                float hs = 0.28f, vs = 0.22f;
                foreach (var (dx, dy) in new (float, float)[] { (-hs,-vs), (hs,-vs), (-hs,vs), (hs,vs) })
                {
                    RoomBuilder.Box(room.transform, "Pin_P" + (i+1) + "_" + dx.ToString("F1") + dy.ToString("F1"),
                        RoomBuilder.V(cx + dx, cy + dy, -4.875f), RoomBuilder.V(0.03f, 0.03f, 0.01f), pinMat, false);
                }
            }
            // 图钉孔（照片墙上的钉痕）
            var holeMat = RoomBuilder.Mat("M_Hole", new Color(0.35f, 0.33f, 0.30f));
            float[] extraHoles = { -3.0f, -0.1f, 0.3f };
            foreach (var hx in extraHoles)
                RoomBuilder.Box(room.transform, "Hole_" + hx.ToString("F1"),
                    RoomBuilder.V(hx, 1.2f, -4.93f), RoomBuilder.V(0.04f, 0.04f, 0.01f), holeMat, false);
            // 红色衬纸（循环3出现在空椅照片后——"这一张被谁用红笔圈了出来"）
            var redMark = RoomBuilder.Box(room.transform, "Photo4_RedMark", RoomBuilder.V(-0.45f, 1.9f, -4.905f), RoomBuilder.V(0.66f, 0.56f, 0.012f),
                RoomBuilder.Mat("M_RedMark", new Color(0.72f, 0.12f, 0.08f)), false);
            redMark.SetActive(false);
            RoomBuilder.LoopRule(redMark, "photo4_redmark", 3, true, true);
            var figure = RoomBuilder.Box(room.transform, "Photo_Figure", RoomBuilder.V(-0.45f, 1.9f, -4.88f), RoomBuilder.V(0.55f, 0.45f, 0.02f),
                RoomBuilder.Mat("M_PhotoFigure", new Color(0.14f, 0.14f, 0.18f)));
            figure.SetActive(false);
            RoomBuilder.LoopRule(figure, "photo_figure", 2, true, true);

            // ---- 墙字（西墙，可读面朝东=yaw -90；文字沿+Z展开）----
            var tm = RoomBuilder.WallText(room.transform, RoomBuilder.V(-5.87f, 2.05f, -1.5f), -90f, "你已经来过这里12次。");
            var textGo = tm.gameObject;
            RoomBuilder.LoopRule(textGo, "wall_text_r0", 1, false, false, "你已经来过这里12次。");
            RoomBuilder.LoopRule(textGo, "wall_text_r0", 2, false, false, "你已经来过这里13次。");
            RoomBuilder.LoopRule(textGo, "wall_text_r0", 3, false, false, "别再数了。");
            RoomBuilder.LoopRule(textGo, "wall_text_r0", 4, false, false, "欢迎回家。");
            var textHit = RoomBuilder.Box(room.transform, "WallTextHit", RoomBuilder.V(-5.80f, 2.05f, 0), RoomBuilder.V(0.06f, 0.7f, 3.2f), wall);
            textHit.GetComponent<Renderer>().enabled = false;
            RoomBuilder.ReadableOn(textHit, StoryTexts.WallTextTitle, StoryDocs.WallText1, "查看墙上的字");

            // ---- 墙面：房间编号喷涂 + 裂纹（废弃感）----
            var zoneTag = RoomBuilder.WallText(room.transform, RoomBuilder.V(2.6f, 2.25f, -4.93f), 0f, "07 室 · 观察区", 34, 0.1f);
            zoneTag.gameObject.name = "WallText_ZoneTag";   // 避免与循环计数墙字重名
            var crack0 = RoomBuilder.Box(room.transform, "WallCrack0", RoomBuilder.V(1.2f, 1.9f, -4.94f),
                RoomBuilder.V(0.035f, 1.15f, 0.02f), dark, false);
            crack0.transform.rotation = Quaternion.Euler(0f, 0f, 7f);

            // ---- 保险箱：机械感 + 归属感（保管员铭牌与实验日志记录员周明同名）----
            RoomBuilder.Box(room.transform, "SafeDial", RoomBuilder.V(-1.5f, 0.46f, -4.19f), RoomBuilder.V(0.17f, 0.17f, 0.03f), dark, false);
            RoomBuilder.Box(room.transform, "SafeHandle", RoomBuilder.V(-1.27f, 0.44f, -4.19f), RoomBuilder.V(0.1f, 0.035f, 0.03f), metal, false);
            // 铰链（箱门左侧）
            RoomBuilder.Box(room.transform, "SafeHinge1", RoomBuilder.V(-1.82f, 0.15f, -4.19f), RoomBuilder.V(0.04f, 0.04f, 0.03f), dark, false);
            RoomBuilder.Box(room.transform, "SafeHinge2", RoomBuilder.V(-1.82f, 0.65f, -4.19f), RoomBuilder.V(0.04f, 0.04f, 0.03f), dark, false);
            // 散热孔（箱门下方，一排小方孔）
            for (int i = 0; i < 5; i++)
                RoomBuilder.Box(room.transform, "SafeVent_" + i, RoomBuilder.V(-1.65f + i * 0.08f, 0.12f, -4.19f),
                    RoomBuilder.V(0.03f, 0.03f, 0.01f), dark, false);
            var safePlate = RoomBuilder.Box(room.transform, "SafePlate", RoomBuilder.V(-1.76f, 0.74f, -4.19f), RoomBuilder.V(0.15f, 0.09f, 0.012f), metal);
            RoomBuilder.ReadableOn(safePlate, "柜体铭牌",
                "T-13 · 人身物品柜\n\n保管员：周明\n注意事项：\n『里面那把钥匙，别交给他。』", "查看铭牌");

            // ---- 保险箱（1998）→ 门禁卡 ----
            // 注意：SafeBody 是实心立方体，战利品不能生成在其内部（会被遮挡且射线不可达）
            var safeBody = RoomBuilder.Box(room.transform, "SafeBody", RoomBuilder.V(-1.5f, 0.4f, -4.55f), RoomBuilder.V(0.72f, 0.8f, 0.66f), metal);
            var safePivot = new GameObject("SafePivot");
            safePivot.transform.SetParent(room.transform);
            safePivot.transform.position = RoomBuilder.V(-1.82f, 0.42f, -4.19f);
            var safeLeaf = ProjectSetup.Box("SafeDoor", Vector3.zero, Vector3.one, metal);
            safeLeaf.transform.SetParent(safePivot.transform, false);
            safeLeaf.transform.localPosition = RoomBuilder.V(0.34f, 0f, 0f);
            safeLeaf.transform.localScale = RoomBuilder.V(0.66f, 0.74f, 0.06f);
            safeLeaf.isStatic = false;
            var keycard = RoomBuilder.Box(room.transform, "Keycard", RoomBuilder.V(-1.5f, 0.24f, -4.6f), RoomBuilder.V(0.28f, 0.03f, 0.18f),
                RoomBuilder.Mat("M_Keycard", new Color(0.95f, 0.88f, 0.5f)), false);
            var keyPick = RoomBuilder.PickupOn(keycard, "keycard");
            keycard.SetActive(false);
            var safe = safePivot.AddComponent<SafeInteractable>();
            safe.safeDoorPivot = safePivot.transform;
            safe.loot = keyPick;
            safe.code = "1998";
            safe.solvedFlag = "safe_open";
            safe.obstacleToDisable = safeBody.GetComponent<Collider>();
            // 开箱落点：箱前地面。x=-1.1 是刻意选择：球形交互射线（半径0.3）不得
            // 触到旋转开启的箱门板（门板开启后占 x[-2.21,-1.55]），也不得触到任何实心体
            safe.lootDropPosition = RoomBuilder.V(-1.1f, 0.035f, -3.8f);

            // ---- 环境叙事道具：墙边刻痕计数 + 翻倒的椅子（"醒来时房间已被翻过"）----
            var chalk = RoomBuilder.Mat("M_Chalk", new Color(0.16f, 0.15f, 0.14f));
            for (int i = 0; i < 9; i++)
            {
                float zz = 1.9f + i * 0.09f;
                RoomBuilder.Box(room.transform, "Tally_" + i, RoomBuilder.V(-5.86f, 1.25f, zz),
                    RoomBuilder.V(0.012f, 0.16f, 0.02f), chalk, false);
                if (i % 5 == 4)   // 每五道一道斜划
                {
                    var slash = RoomBuilder.Box(room.transform, "TallySlash_" + i, RoomBuilder.V(-5.86f, 1.25f, zz - 0.18f),
                        RoomBuilder.V(0.012f, 0.16f, 0.02f), chalk, false);
                    slash.transform.localRotation = Quaternion.Euler(35f, 0f, 0f);
                }
            }
            var chair = RoomBuilder.Box(room.transform, "FallenChair", RoomBuilder.V(-2.9f, 0.18f, 2.1f),
                RoomBuilder.V(0.45f, 0.36f, 0.45f), wood, false);
            chair.transform.rotation = Quaternion.Euler(0f, 55f, 96f);
            RoomBuilder.Box(room.transform, "ChairLeg", RoomBuilder.V(-3.05f, 0.06f, 1.9f),
                RoomBuilder.V(0.05f, 0.12f, 0.05f), wood, false);

            // ---- 灯光（明亮但冷调，三层次：顶灯/台灯/屏幕冷光+出口红光）----
            // 台灯：暖光，照亮桌面区域（视觉焦点1：玩家醒来首先看到的工作区）
            var deskLamp = RoomBuilder.PointLight(room.transform, "DeskLamp", RoomBuilder.V(-4.6f, 2.2f, 0.5f), new Color(1f, 0.85f, 0.65f), 1.0f, 7f);
            deskLamp.gameObject.AddComponent<FlickerLight>().amount = 0.22f;
            // 顶灯：冷白，均匀照明（带供电不稳闪烁）
            var lampA = RoomBuilder.PointLight(room.transform, "R0LampA", RoomBuilder.V(-2.5f, 2.9f, 0f), new Color(1f, 0.97f, 0.9f), 0.70f, 9f);
            lampA.gameObject.AddComponent<FlickerLight>().amount = 0.12f;
            var lampB = RoomBuilder.PointLight(room.transform, "R0LampB", RoomBuilder.V(2.5f, 2.9f, 0f), new Color(1f, 0.97f, 0.9f), 0.70f, 9f);
            lampB.gameObject.AddComponent<FlickerLight>().amount = 0.12f;
            RoomBuilder.PointLight(room.transform, "R0LampC", RoomBuilder.V(0f, 2.6f, -4f), new Color(0.9f, 0.95f, 1f), 0.50f, 7f);
            // 电脑屏幕的冷光（视觉焦点2：剧情核心节点，青蓝色光吸引玩家注意）
            RoomBuilder.PointLight(room.transform, "ScreenGlow0", RoomBuilder.V(-4.5f, 1.25f, 0.8f), new Color(0.55f, 0.75f, 0.95f), 0.65f, 4f);
            // 照片墙照明（视觉焦点3：墙上的照片是第一个异常来源）
            RoomBuilder.PointLight(room.transform, "PhotoGlow", RoomBuilder.V(-1.5f, 2.6f, -4.2f), new Color(0.9f, 0.9f, 0.95f), 0.35f, 3f);
            // 门框上方的应急指示（红色微光：出口的方向感）
            RoomBuilder.PointLight(room.transform, "ExitGlow0", RoomBuilder.V(-0.8f, 2.5f, -4.5f), new Color(1f, 0.25f, 0.2f), 0.3f, 2.6f);

            RoomBuilder.Spawn("R0", RoomBuilder.V(0f, 0.05f, 3.5f), 180f);
            RoomBuilder.Trigger(room.transform, "R0", RoomBuilder.V(0f, 1.5f, 0f), RoomBuilder.V(11f, 3f, 9f), ambient: 0);
        }
    }
}
