using UnityEngine;
using Door13.Audio;
using Door13.Puzzle;
using Door13.Story;

namespace Door13.EditorTools
{
    /// <summary>R1 回廊（枢纽）：配电箱/监控台谜题、四扇区域门、十三门。x[-3,3] z[-24,-5]。</summary>
    public static class RoomR1
    {
        public static void Build()
        {
            var room = new GameObject("Room_R1");
            var wall = RoomBuilder.Mat("M_Wall", new Color(0.52f, 0.54f, 0.58f));
            var floor = RoomBuilder.Mat("M_Floor", new Color(0.40f, 0.41f, 0.44f));
            var metal = RoomBuilder.Mat("M_Metal", new Color(0.44f, 0.47f, 0.51f));
            var dark = RoomBuilder.Mat("M_Dark", new Color(0.08f, 0.08f, 0.10f));
            var paper = RoomBuilder.Mat("M_Paper", new Color(0.82f, 0.81f, 0.76f));

            RoomBuilder.Shell(room.transform, -3, 3, -24, -5, 3.2f, floor, wall);
            RoomBuilder.WallX(room.transform, "N", -24.1f, -3.2f, 3.2f, 3.2f, wall, 0f, 1.4f);   // 十三门
            RoomBuilder.WallX(room.transform, "S", -4.9f, -3.2f, 3.2f, 3.2f, wall, 0f, 1.6f);    // 来自R0
            RoomBuilder.WallZ(room.transform, "W", -3.1f, -24.2f, -4.8f, 3.2f, wall, -10f, 1.4f, -19f, 1.4f);
            RoomBuilder.WallZ(room.transform, "E", 3.1f, -24.2f, -4.8f, 3.2f, wall, -10f, 1.4f, -19f, 1.4f);

            // ---- 环境细节：管线 + 墙皮脱落 + 地面污渍 ----
            // 沿北墙的管线（三根长条）
            var pipe = RoomBuilder.Mat("M_Pipe", new Color(0.5f, 0.42f, 0.35f));
            RoomBuilder.Box(room.transform, "Pipe_N1", RoomBuilder.V(-2f, 2.9f, -23.95f), RoomBuilder.V(4f, 0.08f, 0.08f), pipe);
            RoomBuilder.Box(room.transform, "Pipe_N2", RoomBuilder.V(-2f, 2.75f, -23.95f), RoomBuilder.V(4f, 0.08f, 0.08f), pipe);
            // 墙皮脱落痕迹（南墙入口附近）
            var peelMat = RoomBuilder.Mat("M_Peel", new Color(0.35f, 0.33f, 0.30f));
            RoomBuilder.Box(room.transform, "WallPeel", RoomBuilder.V(0f, 1.8f, -4.94f), RoomBuilder.V(1.2f, 0.8f, 0.02f), peelMat, false);
            // 地面污渍（配电箱附近）
            var stainMat = RoomBuilder.Mat("M_Stain", new Color(0.15f, 0.14f, 0.12f));
            RoomBuilder.Box(room.transform, "R1Stain", RoomBuilder.V(-2.5f, 0.012f, -10f), RoomBuilder.V(1.0f, 0.01f, 0.8f), stainMat, false);

            // 西墙门：R2实验室（V2卡）；R4控制中心（钥匙卡）
            RoomBuilder.MakeDoor(room.transform, "Door_R2", RoomBuilder.V(-3.0f, 1.1f, -10.7f), 90f,
                "v2card", "读卡器是灭的。需要V2区门禁卡。");
            RoomBuilder.MakeDoor(room.transform, "Door_R4", RoomBuilder.V(-3.0f, 1.1f, -19.7f), 90f,
                "ctrl_card", "需要控制中心钥匙卡。");
            // 东墙门：R3档案室（权限卡）；R5地下（控制台解锁）
            RoomBuilder.MakeDoor(room.transform, "Door_R3", RoomBuilder.V(3.0f, 1.1f, -10.7f), -90f,
                "archive_card", "需要档案权限卡。");
            RoomBuilder.MakeDoor(room.transform, "Door_R5", RoomBuilder.V(3.0f, 1.1f, -19.7f), -90f,
                "", "", "subway_unlock");

            // 北端：十三门（循环4+十三号钥匙）
            var door13 = RoomBuilder.MakeDoor(room.transform, "Door_13", RoomBuilder.V(-0.7f, 1.1f, -24.0f), 0f,
                "master_key", "门上有一道钥匙形凹槽，和一行小字：『带着你的全部记忆来。』", "", 4,
                RoomBuilder.Mat("M_Door13", new Color(0.06f, 0.06f, 0.09f)));
            var plate = RoomBuilder.Box(room.transform, "Plate_13", RoomBuilder.V(1.5f, 2.0f, -23.95f), RoomBuilder.V(1.0f, 0.5f, 0.06f), paper);
            RoomBuilder.ReadableOn(plate, "第十三扇门", StoryDocs.Door13SignR1, "查看铭牌");

            // 配电箱（合闸顺序 3,1,4,2,5）→ 走廊灯 + V2卡
            var breaker = RoomBuilder.Box(room.transform, "Breaker", RoomBuilder.V(-2.85f, 1.6f, -7.5f), RoomBuilder.V(0.25f, 0.9f, 0.7f), metal);
            // 配电箱机械细节：铰链 + 透明观察窗
            RoomBuilder.Box(room.transform, "BreakerHinge", RoomBuilder.V(-2.98f, 1.2f, -7.16f), RoomBuilder.V(0.03f, 0.03f, 0.03f), dark, false);
            RoomBuilder.Box(room.transform, "BreakerWindow", RoomBuilder.V(-2.85f, 1.9f, -7.16f), RoomBuilder.V(0.15f, 0.15f, 0.01f),
                RoomBuilder.Mat("M_BreakerWin", new Color(0.3f, 0.6f, 0.3f)), false);
            RoomBuilder.ReadableOn(breaker, "配电箱", StoryDocs.BreakerChartR1, "查看电路图");
            var pzBreaker = breaker.AddComponent<PuzzleInteractable>();
            pzBreaker.type = PuzzleType.Switch;
            pzBreaker.puzzleId = "breaker_r1";
            pzBreaker.panelTitle = "配电箱 · 合闸";
            pzBreaker.solution = "3,1,4,2,5";
            pzBreaker.optionLabels = "①,②,③,④,⑤";
            pzBreaker.prompt = "操作断路器";
            pzBreaker.successMessage = "走廊的灯亮了。箱底掉出一张卡。";
            var breakerReward = new GameObject("BreakerReward");
            breakerReward.transform.SetParent(room.transform);
            breakerReward.SetActive(false);
            var v2 = RoomBuilder.Box(breakerReward.transform, "V2Card", RoomBuilder.V(-2.6f, 0.15f, -7.5f), RoomBuilder.V(0.26f, 0.03f, 0.18f),
                RoomBuilder.Mat("M_V2Card", new Color(0.6f, 0.8f, 0.95f)), false);
            RoomBuilder.PickupOn(v2, "v2card", "拾取V2门禁卡");
            pzBreaker.revealGo = breakerReward;
            // 入口常亮暗灯（触发区闪烁用）
            var entryLampGo = RoomBuilder.PointLight(room.transform, "EntryLamp", RoomBuilder.V(0f, 2.9f, -6.5f), new Color(0.7f, 0.75f, 0.9f), 0.35f, 7f);
            entryLampGo.gameObject.AddComponent<FlickerLight>().amount = 0.08f;

            // 走廊灯（合闸后亮起，3盏沿走廊分布，循环3最深处那盏熄了）
            for (int i = 0; i < 3; i++)
            {
                var lamp = RoomBuilder.PointLight(breakerReward.transform, "CorridorLamp" + i,
                    RoomBuilder.V(0f, 3.0f, -7f - i * 6f), new Color(1f, 0.9f, 0.75f), 0.9f, 9f);
                lamp.gameObject.AddComponent<FlickerLight>().amount = 0.13f;
                if (i == 2)
                {
                    // 循环3起：走廊最深处那盏灯熄了——"它以前是亮的"
                    RoomBuilder.LoopRule(lamp.gameObject, "corridor_lamp2_dead", 3, true, false);
                }
            }

            // 监控台（密码 1204）→ 磁带A
            var console = RoomBuilder.Box(room.transform, "MonitorDesk", RoomBuilder.V(2.6f, 0.75f, -13.5f), RoomBuilder.V(0.7f, 0.1f, 1.6f), metal);
            var screens = RoomBuilder.Box(room.transform, "MonitorScreens", RoomBuilder.V(2.8f, 1.35f, -13.5f), RoomBuilder.V(0.15f, 0.6f, 1.4f), dark);
            // CRT屏幕发光效果（暗绿色，模拟旧监控显示器）
            var screenMat = RoomBuilder.Mat("M_MonitorScreen", new Color(0.15f, 0.35f, 0.25f));
            screenMat.EnableKeyword("_EMISSION");
            screenMat.SetColor("_EmissionColor", new Color(0.15f, 0.35f, 0.25f) * 0.6f);
            RoomBuilder.Box(room.transform, "MonitorCRT", RoomBuilder.V(2.8f, 1.35f, -13.44f), RoomBuilder.V(0.02f, 0.5f, 1.2f), screenMat);
            RoomBuilder.ReadableOn(screens, "监控台", StoryDocs.MonitorScreenR1, "查看监控");
            var monNote = RoomBuilder.Box(room.transform, "MonitorNote", RoomBuilder.V(2.45f, 0.85f, -13.1f), RoomBuilder.V(0.18f, 0.015f, 0.18f),
                RoomBuilder.Mat("M_Yellow", new Color(0.95f, 0.9f, 0.5f)));
            RoomBuilder.ReadableOn(monNote, "值班便签", StoryDocs.MonitorNoteR1, "查看便签");
            var pzMonitor = console.AddComponent<PuzzleInteractable>();
            pzMonitor.type = PuzzleType.Keypad;
            pzMonitor.puzzleId = "monitor_r1";
            pzMonitor.panelTitle = "监控台 · 门禁记录";
            pzMonitor.solution = "1204";
            pzMonitor.prompt = "使用监控台";
            pzMonitor.successMessage = "抽屉弹开。里面有一盘录音带。";
            var tapeReward = new GameObject("TapeAReward");
            tapeReward.transform.SetParent(room.transform);
            tapeReward.SetActive(false);
            var tapeA = RoomBuilder.Box(tapeReward.transform, "TapeA", RoomBuilder.V(2.6f, 0.85f, -13.8f), RoomBuilder.V(0.24f, 0.05f, 0.16f),
                RoomBuilder.Mat("M_Tape", new Color(0.3f, 0.3f, 0.35f)), false);
            RoomBuilder.PickupOn(tapeA, "tape_a", "拾取录音带A");
            pzMonitor.revealGo = tapeReward;

            // 海报
            var poster = RoomBuilder.Box(room.transform, "Poster", RoomBuilder.V(-2.95f, 1.7f, -13.5f), RoomBuilder.V(0.06f, 0.9f, 0.65f), paper);
            RoomBuilder.ReadableOn(poster, "褪色的公告", StoryDocs.CorridorPosterR1, "查看公告");

            // 墙裂缝（循环2出现）
            var crack = RoomBuilder.Box(room.transform, "Crack", RoomBuilder.V(2.97f, 1.5f, -21.5f), RoomBuilder.V(0.08f, 1.0f, 0.06f), dark);
            crack.GetComponent<Renderer>().enabled = false;
            var crackCol = crack.GetComponent<BoxCollider>();
            crackCol.size = RoomBuilder.V(0.3f, 1.0f, 0.3f);
            RoomBuilder.ReadableOn(crack, "墙上的裂缝", "裂缝里塞着一张纸条：\n\n『第七次循环时我把它藏在这里。\n 如果你也看到了裂缝——说明你也开始记住了。\n 十三门后面不是出口。是答案。』", "查看裂缝");
            crack.SetActive(false);
            RoomBuilder.LoopRule(crack, "crack_r1", 2, true, true);

            // 回廊铭牌（可读面朝南 = yaw180，文字沿-X展开）
            RoomBuilder.WallText(room.transform, RoomBuilder.V(1.7f, 2.5f, -23.9f), 180f, "回 廊 7 号", 42, 0.1f);

            RoomBuilder.Spawn("R1", RoomBuilder.V(0f, 0.05f, -6.5f), 180f);
            RoomBuilder.Trigger(room.transform, "R1", RoomBuilder.V(0f, 1.5f, -14f), RoomBuilder.V(5.5f, 3f, 18f),
                StoryTexts.Narr_R1, 2, entryLampGo, 1);
        }
    }
}
