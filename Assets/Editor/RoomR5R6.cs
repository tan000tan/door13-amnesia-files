using UnityEngine;
using Door13.Audio;
using Door13.Interaction;
using Door13.Save;
using Door13.Puzzle;
using Door13.Story;

namespace Door13.EditorTools
{
    /// <summary>R5 地下区域（黑暗）+ R6 十三门后室。R5 x[3,15] z[-23,-16]，R6 x[5,13] z[-30,-23.5]。</summary>
    public static class RoomR5R6
    {
        public static void Build()
        {
            var room = new GameObject("Room_R5");
            var wall = RoomBuilder.Mat("M_Wall5", new Color(0.20f, 0.20f, 0.23f));
            var floor = RoomBuilder.Mat("M_Floor5", new Color(0.16f, 0.16f, 0.18f));
            var metal = RoomBuilder.Mat("M_Metal", new Color(0.44f, 0.47f, 0.51f));
            var paper = RoomBuilder.Mat("M_Paper", new Color(0.82f, 0.81f, 0.76f));
            var dark = RoomBuilder.Mat("M_Dark", new Color(0.06f, 0.06f, 0.08f));

            // ---- R5 地下区域 ----
            RoomBuilder.Shell(room.transform, 3, 15, -23, -16, 3.2f, floor, wall);
            RoomBuilder.WallX(room.transform, "N", -23.1f, 2.8f, 15.2f, 3.2f, wall, 9f, 1.4f);  // 十三门
            RoomBuilder.WallX(room.transform, "S", -15.9f, 2.8f, 15.2f, 3.2f, wall);
            RoomBuilder.WallZ(room.transform, "W", 2.9f, -23.2f, -15.8f, 3.2f, wall, -19f, 1.4f);
            RoomBuilder.WallZ(room.transform, "E", 15.1f, -23.2f, -15.8f, 3.2f, wall);

            // 沿北墙管道 + 蒸汽泄漏
            RoomBuilder.Box(room.transform, "R5PipeN1", RoomBuilder.V(5f, 2.8f, -22.94f), RoomBuilder.V(6.0f, 0.1f, 0.08f),
                RoomBuilder.Mat("M_Pipe", new Color(0.35f, 0.37f, 0.40f)));
            RoomBuilder.Box(room.transform, "R5PipeN2", RoomBuilder.V(10f, 2.6f, -22.94f), RoomBuilder.V(4.0f, 0.08f, 0.08f),
                RoomBuilder.Mat("M_Pipe", new Color(0.35f, 0.37f, 0.40f)));
            // 管道锈迹
            var rust1 = RoomBuilder.Box(room.transform, "R5Rust1", RoomBuilder.V(7f, 2.85f, -22.88f), RoomBuilder.V(0.5f, 0.06f, 0.06f),
                RoomBuilder.Mat("M_Rust", new Color(0.6f, 0.3f, 0.15f)));

            // 红色应急灯（唯一的常亮光，供电不稳地闪烁）
            var redGo = RoomBuilder.PointLight(room.transform, "RedEmergency", RoomBuilder.V(13.5f, 2.6f, -22f),
                new Color(1f, 0.2f, 0.15f), 0.7f, 6f);
            redGo.gameObject.AddComponent<FlickerLight>().amount = 0.3f;

            // 阀门组（2/1/3）→ 蒸汽停
            var valveRig = RoomBuilder.Box(room.transform, "ValveRig", RoomBuilder.V(13.5f, 1.2f, -18f), RoomBuilder.V(0.6f, 2.4f, 0.5f), metal);
            RoomBuilder.ReadableOn(valveRig, "管道铭牌", StoryDocs.ValvePlateR5, "查看铭牌");
            var pzValve = valveRig.AddComponent<PuzzleInteractable>();
            pzValve.type = PuzzleType.Valve;
            pzValve.puzzleId = "valves_r5";
            pzValve.panelTitle = "泄压阀 · 压力";
            pzValve.solution = "2,1,3";
            pzValve.optionLabels = "A管,B管,C管";
            pzValve.prompt = "操作阀门";
            pzValve.successMessage = "嘶——蒸汽停了。管道安静下来。";
            var steam = RoomBuilder.Box(room.transform, "SteamBlock", RoomBuilder.V(12.5f, 1.5f, -18f), RoomBuilder.V(1.6f, 3.0f, 0.1f),
                RoomBuilder.Mat("M_Steam", new Color(0.8f, 0.85f, 0.9f)), false);
            steam.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            pzValve.hideOnSolve = steam;

            // 发电机：先加油（组合产物）再拉索 + 铭牌
            var generator = RoomBuilder.Box(room.transform, "Generator", RoomBuilder.V(5.2f, 0.7f, -21.5f), RoomBuilder.V(1.6f, 1.4f, 1.0f), dark);
            var genNameplate = RoomBuilder.Box(room.transform, "GenNameplate", RoomBuilder.V(5.2f, 1.3f, -20.94f), RoomBuilder.V(0.5f, 0.12f, 0.02f),
                RoomBuilder.Mat("M_Nameplate", new Color(0.55f, 0.55f, 0.58f)));
            RoomBuilder.ReadableOn(genNameplate, "发电机铭牌", "型号：DG-4000\n功率：4kW\n注意：定期维护", "查看铭牌");
            RoomBuilder.ReadableOn(generator, "发电机", StoryDocs.GeneratorNoteR5, "查看操作牌");
            var fuelSpotGo = RoomBuilder.Box(room.transform, "FuelSpot", RoomBuilder.V(6.2f, 0.5f, -21.5f), RoomBuilder.V(0.1f, 0.5f, 0.5f), metal, false);
            var fuelSpot = fuelSpotGo.AddComponent<ItemSpotUse>();
            fuelSpot.requiredItemId = "mix_fuel";
            fuelSpot.lockedMessage = "发电机需要一桶加满的柴油。";
            fuelSpot.successMessage = "柴油灌进去了。";
            fuelSpot.solvedFlag = "gen_fueled";
            fuelSpot.consume = true;
            fuelSpot.prompt = "加油";
            var pullGo = RoomBuilder.Box(room.transform, "PullCord", RoomBuilder.V(5.2f, 1.1f, -20.9f), RoomBuilder.V(0.08f, 0.3f, 0.08f),
                RoomBuilder.Mat("M_Rope", new Color(0.6f, 0.5f, 0.35f)), false);
            pullGo.AddComponent<GeneratorStart>();

            // 储藏格：空油桶 + 柴油 + 刻字
            var storage = RoomBuilder.Box(room.transform, "StorageBox", RoomBuilder.V(14.2f, 0.5f, -21.8f), RoomBuilder.V(1.0f, 1.0f, 1.2f), metal);
            RoomBuilder.ReadableOn(storage, "储藏格", StoryDocs.StorageR5, "查看储藏格");
            var can = RoomBuilder.Box(room.transform, "FuelCan", RoomBuilder.V(14.0f, 1.1f, -21.8f), RoomBuilder.V(0.3f, 0.3f, 0.3f),
                RoomBuilder.Mat("M_Can", new Color(0.6f, 0.55f, 0.45f)), false);
            RoomBuilder.PickupOn(can, "fuel_can", "拾取空油桶");
            var fuel = RoomBuilder.Box(room.transform, "Fuel", RoomBuilder.V(14.4f, 1.1f, -21.8f), RoomBuilder.V(0.24f, 0.34f, 0.24f),
                RoomBuilder.Mat("M_Fuel", new Color(0.85f, 0.75f, 0.3f)), false);
            RoomBuilder.PickupOn(fuel, "fuel", "拾取燃料罐");

            // 主装置（循环引擎）+ 发光环
            var device = RoomBuilder.Box(room.transform, "MainDevice", RoomBuilder.V(9f, 1.3f, -22.3f), RoomBuilder.V(2.2f, 2.6f, 0.9f), dark);
            // 装置发光环（循环激活时亮起）
            var glowRing = RoomBuilder.Box(room.transform, "DeviceGlowRing", RoomBuilder.V(9f, 2.0f, -21.84f), RoomBuilder.V(1.8f, 0.04f, 0.04f),
                RoomBuilder.Mat("M_DeviceGlow", new Color(0.3f, 0.6f, 0.9f)), false);
            glowRing.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var glowFlag = glowRing.gameObject.AddComponent<FlagReveal>();
            glowFlag.flag = GameFlags.DeviceReady;
            glowRing.gameObject.SetActive(false);
            RoomBuilder.ReadableOn(device, "主装置", StoryDocs.DeviceNoteR5, "查看装置");
            var devGo = new GameObject("DeviceSpot");
            devGo.transform.SetParent(room.transform);
            devGo.transform.position = RoomBuilder.V(9f, 1.3f, -21.8f);
            var devCol = devGo.AddComponent<BoxCollider>();
            devCol.size = RoomBuilder.V(2.2f, 2.4f, 0.4f);
            devGo.AddComponent<DeviceInteractable>();
            var deviceLight = RoomBuilder.PointLight(room.transform, "DeviceLight", RoomBuilder.V(9f, 2.8f, -21.6f),
                new Color(0.4f, 0.8f, 1f), 0.8f, 8f);
            var dlFlag = deviceLight.gameObject.AddComponent<FlagReveal>();
            dlFlag.flag = GameFlags.DeviceReady;
            deviceLight.gameObject.SetActive(false);

            // 十三门旁：碎片3（循环3出现）
            var shard3 = RoomBuilder.Box(room.transform, "Shard3", RoomBuilder.V(10.3f, 0.06f, -22.6f), RoomBuilder.V(0.24f, 0.02f, 0.18f),
                RoomBuilder.Mat("M_Shard", new Color(0.75f, 0.78f, 0.85f)), false);
            RoomBuilder.PickupOn(shard3, "shard_3", "拾取记忆碎片");
            shard3.SetActive(false);
            RoomBuilder.LoopRule(shard3, "shard3_spawn", 3, true, true);

            // 低语（证据氛围）
            var whisper = RoomBuilder.Box(room.transform, "WhisperSpot", RoomBuilder.V(7f, 1.4f, -17.5f), RoomBuilder.V(0.1f, 0.1f, 0.1f), dark);
            whisper.GetComponent<Renderer>().enabled = false;
            RoomBuilder.ReadableOn(whisper, "黑暗深处", StoryDocs.WhisperR5, "凝神细听");

            RoomBuilder.PointLight(room.transform, "R5Dim1", RoomBuilder.V(6f, 2.7f, -17.5f), new Color(0.5f, 0.6f, 0.8f), 0.45f, 8f);


            RoomBuilder.PointLight(room.transform, "R5Dim2", RoomBuilder.V(12f, 2.7f, -21f), new Color(0.6f, 0.5f, 0.5f), 0.4f, 8f);


            RoomBuilder.Spawn("R5", RoomBuilder.V(4.5f, 0.05f, -19f), 90f);
            RoomBuilder.Trigger(room.transform, "R5", RoomBuilder.V(9f, 1.5f, -19.5f), RoomBuilder.V(10f, 3f, 5.5f),
                StoryTexts.Narr_R5, 3, redGo, 2);

            // ---- R6 十三门后室 ----
            var room6 = new GameObject("Room_R6");
            RoomBuilder.Shell(room6.transform, 5, 13, -30, -23.5f, 3.0f, dark,
                RoomBuilder.Mat("M_WallWhite", new Color(0.82f, 0.82f, 0.84f)));
            RoomBuilder.WallX(room6.transform, "N", -30.1f, 4.8f, 13.2f, 3.0f, RoomBuilder.Mat("M_WallWhite", new Color(0.82f, 0.82f, 0.84f)));
            RoomBuilder.WallX(room6.transform, "S", -23.4f, 4.8f, 13.2f, 3.0f, RoomBuilder.Mat("M_WallWhite", new Color(0.82f, 0.82f, 0.84f)), 9f, 1.4f);
            RoomBuilder.WallZ(room6.transform, "W", 4.9f, -30.2f, -23.3f, 3.0f, RoomBuilder.Mat("M_WallWhite", new Color(0.82f, 0.82f, 0.84f)));
            RoomBuilder.WallZ(room6.transform, "E", 13.1f, -30.2f, -23.3f, 3.0f, RoomBuilder.Mat("M_WallWhite", new Color(0.82f, 0.82f, 0.84f)));

            // 白房间：椅子 + 终局终端 + 顶光
            var chair = RoomBuilder.Box(room6.transform, "Chair", RoomBuilder.V(7.5f, 0.45f, -27f), RoomBuilder.V(0.5f, 0.9f, 0.5f), metal, false);
            var chairBar = RoomBuilder.Box(room6.transform, "ChairBar", RoomBuilder.V(7.5f, 0.75f, -27.26f), RoomBuilder.V(0.4f, 0.03f, 0.03f), metal);
            // 终端底座 + 线缆
            var terminalBase = RoomBuilder.Box(room6.transform, "TerminalBase", RoomBuilder.V(10.5f, 0.85f, -28.5f), RoomBuilder.V(0.9f, 0.05f, 0.4f), dark);
            var terminalCable = RoomBuilder.Box(room6.transform, "TerminalCable", RoomBuilder.V(10.5f, 0.02f, -28.3f), RoomBuilder.V(0.04f, 0.04f, 0.3f),
                RoomBuilder.Mat("M_Cable", new Color(0.2f, 0.2f, 0.22f)));
            var terminal = RoomBuilder.Box(room6.transform, "FinalTerminalBox", RoomBuilder.V(10.5f, 0.9f, -28.8f), RoomBuilder.V(1.0f, 1.8f, 0.5f), dark);
            var ft = terminal.AddComponent<FinalTerminal>();
            var screenMat = RoomBuilder.Mat("M_ScreenFinal", new Color(0.85f, 0.9f, 0.95f));
            screenMat.EnableKeyword("_EMISSION");
            screenMat.SetColor("_EmissionColor", new Color(0.7f, 0.8f, 0.9f) * 1.2f);
            RoomBuilder.Box(room6.transform, "FinalScreen", RoomBuilder.V(10.5f, 1.4f, -28.5f), RoomBuilder.V(0.7f, 0.5f, 0.05f), screenMat);
            RoomBuilder.PointLight(room6.transform, "R6Lamp", RoomBuilder.V(9f, 2.8f, -27f), new Color(0.9f, 0.93f, 1f), 1.0f, 10f);

            RoomBuilder.Spawn("R6", RoomBuilder.V(9f, 0.05f, -25f), 0f);
            RoomBuilder.Trigger(room6.transform, "R6", RoomBuilder.V(9f, 1.5f, -27f), RoomBuilder.V(7f, 3f, 5.5f),
                StoryTexts.Narr_R6, 3, null, 3);

            // ---- R7 容器室（隐藏终章，循环4+所有证据解锁） ----
            // 通过FinalTerminal选择"进入容器"后进入
            var room7 = new GameObject("Room_R7");
            var whiteWall = RoomBuilder.Mat("M_WallR7", new Color(0.95f, 0.95f, 0.96f));
            var whiteFloor = RoomBuilder.Mat("M_FloorR7", new Color(0.92f, 0.92f, 0.93f));
            RoomBuilder.Shell(room7.transform, 4, 8, -38, -33, 3.5f, whiteFloor, whiteWall);
            RoomBuilder.WallX(room7.transform, "N", -38.1f, 5.0f, 8.2f, 3.5f, whiteWall);
            RoomBuilder.WallX(room7.transform, "S", -32.9f, 5.0f, 8.2f, 3.5f, whiteWall);
            RoomBuilder.WallZ(room7.transform, "W", 3.9f, -38.2f, -32.8f, 3.5f, whiteWall);
            RoomBuilder.WallZ(room7.transform, "E", 8.1f, -38.2f, -32.8f, 3.5f, whiteWall);

            // 容器（记忆容器，发光的半透明球体）
            var container = RoomBuilder.Box(room7.transform, "MemoryContainer", RoomBuilder.V(6f, 1.2f, -36f), RoomBuilder.V(1.5f, 2.4f, 1.5f),
                RoomBuilder.Mat("M_Container", new Color(0.6f, 0.8f, 1f)));
            container.GetComponent<Renderer>().material.SetFloat("_Smoothness", 0.05f);
            container.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var containerLight = RoomBuilder.PointLight(room7.transform, "ContainerGlow", RoomBuilder.V(6f, 2.5f, -36f),
                new Color(0.5f, 0.8f, 1f), 0.8f, 10f);

            // 容器说明牌
            var containerNote = RoomBuilder.Box(room7.transform, "ContainerNote", RoomBuilder.V(6f, 2.2f, -35.1f), RoomBuilder.V(0.8f, 0.3f, 0.02f), paper);
            RoomBuilder.ReadableOn(containerNote, "记忆容器", StoryDocs.UndergroundContainerNote, "阅读说明");

            // 容器终端（打开容器触发真结局）
            var containerTerm = RoomBuilder.Box(room7.transform, "ContainerTerminal", RoomBuilder.V(6f, 0.6f, -34.5f), RoomBuilder.V(0.4f, 0.5f, 0.1f), dark);
            var ct = containerTerm.AddComponent<ContainerTerminal>();

            // 12个孩子名字的刻痕（地板）
            for (int i = 0; i < 12; i++)
            {
                float x = 4.5f + i * 0.4f;
                var namePlate = RoomBuilder.Box(room7.transform, "ChildName_" + i, RoomBuilder.V(x, 0.005f, -34f), RoomBuilder.V(0.3f, 0.005f, 0.15f),
                    RoomBuilder.Mat("M_NamePlate", new Color(0.7f, 0.75f, 0.8f)));
            }

            // 顶光（温暖白色，象征释然）
            RoomBuilder.PointLight(room7.transform, "R7Lamp", RoomBuilder.V(6f, 3.3f, -35f), new Color(1f, 0.98f, 0.95f), 1.5f, 12f);

            // R7入口（从R6的终端选择进入）
            // 由FinalTerminal.cs控制解锁

            RoomBuilder.Spawn("R7", RoomBuilder.V(6f, 0.05f, -35f), 0f);
        }
    }
}
