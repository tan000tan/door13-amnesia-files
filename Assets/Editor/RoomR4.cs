using UnityEngine;
using Door13.Audio;
using Door13.Interaction;
using Door13.Puzzle;
using Door13.Story;

namespace Door13.EditorTools
{
    /// <summary>R4 控制中心：断路器主电源/控制台登录/录音机×2/档案。x[-15,-3] z[-23,-16]。</summary>
    public static class RoomR4
    {
        public static void Build()
        {
            var room = new GameObject("Room_R4");
            var wall = RoomBuilder.Mat("M_Wall4", new Color(0.52f, 0.54f, 0.58f));
            var floor = RoomBuilder.Mat("M_Floor4", new Color(0.40f, 0.41f, 0.44f));
            var metal = RoomBuilder.Mat("M_Metal", new Color(0.44f, 0.47f, 0.51f));
            var paper = RoomBuilder.Mat("M_Paper", new Color(0.82f, 0.81f, 0.76f));
            var dark = RoomBuilder.Mat("M_Dark", new Color(0.08f, 0.08f, 0.10f));

            RoomBuilder.Shell(room.transform, -15, -3, -23, -16, 3.2f, floor, wall);
            RoomBuilder.WallX(room.transform, "N", -23.1f, -15.2f, -2.8f, 3.2f, wall);
            RoomBuilder.WallX(room.transform, "S", -15.9f, -15.2f, -2.8f, 3.2f, wall);
            RoomBuilder.WallZ(room.transform, "W", -15.1f, -23.2f, -15.8f, 3.2f, wall);
            RoomBuilder.WallZ(room.transform, "E", -2.9f, -23.2f, -15.8f, 3.2f, wall, -19f, 1.4f);

            // 地面污渍 + 管线
            RoomBuilder.Box(room.transform, "R4Stain1", RoomBuilder.V(-10f, 0.012f, -18f), RoomBuilder.V(1.0f, 0.01f, 0.7f),
                RoomBuilder.Mat("M_Stain", new Color(0.28f, 0.29f, 0.30f)));
            RoomBuilder.Box(room.transform, "R4Pipe1", RoomBuilder.V(-14.94f, 2.8f, -17f), RoomBuilder.V(0.06f, 0.06f, 5.0f),
                RoomBuilder.Mat("M_Pipe", new Color(0.4f, 0.42f, 0.45f)));

            // 断路器（5,2,4,1,3）→ 主电源 + 控制台屏幕亮
            var breaker = RoomBuilder.Box(room.transform, "BreakerR4", RoomBuilder.V(-14.85f, 1.6f, -20f), RoomBuilder.V(0.25f, 1.0f, 0.8f), metal);
            // 断路器机械细节：把手 + 状态指示
            var breakerHandle = RoomBuilder.Box(room.transform, "BreakerHandle", RoomBuilder.V(-14.7f, 1.8f, -19.58f), RoomBuilder.V(0.12f, 0.04f, 0.04f),
                RoomBuilder.Mat("M_BreakerHandle", new Color(0.9f, 0.2f, 0.15f)), false);
            var breakerInd = RoomBuilder.Box(room.transform, "BreakerInd", RoomBuilder.V(-14.7f, 2.0f, -19.58f), RoomBuilder.V(0.06f, 0.06f, 0.02f),
                RoomBuilder.Mat("M_IndRed", new Color(0.9f, 0.1f, 0.1f)), false);
            RoomBuilder.ReadableOn(breaker, "操作规程", StoryDocs.BreakerNoteR4, "查看规程");
            var pz = breaker.AddComponent<PuzzleInteractable>();
            pz.type = PuzzleType.Switch;
            pz.puzzleId = "breaker_r4";
            pz.panelTitle = "主配电 · 合闸";
            pz.solution = "5,2,4,1,3";
            pz.optionLabels = "①旁路,②照明,③备用,④设备,⑤总闸";
            pz.prompt = "操作主配电";
            pz.successMessage = "低沉的嗡鸣。控制台亮了。";
            pz.setFlag = Door13.Save.GameFlags.MainPower;
            var screen = RoomBuilder.Box(room.transform, "ConsoleScreenGlow", RoomBuilder.V(-5.05f, 1.3f, -20f), RoomBuilder.V(0.06f, 0.5f, 1.2f),
                RoomBuilder.Mat("M_ScreenGlow", new Color(0.2f, 0.55f, 0.4f)), false);
            screen.SetActive(false);
            pz.revealGo = screen;

            // 控制台（登录0403 → 日志+地下通道+磁带B）
            var desk = RoomBuilder.Box(room.transform, "ConsoleDesk", RoomBuilder.V(-5.5f, 0.8f, -20f), RoomBuilder.V(0.9f, 0.1f, 2.0f), metal);
            var console = RoomBuilder.Box(room.transform, "Console", RoomBuilder.V(-5.2f, 1.25f, -20f), RoomBuilder.V(0.3f, 0.4f, 1.4f), dark);
            // CRT 显示器边框 + 扫描线效果
            var crtFrame = RoomBuilder.Box(room.transform, "CrtFrame", RoomBuilder.V(-5.2f, 1.35f, -19.28f), RoomBuilder.V(0.35f, 0.45f, 0.04f),
                RoomBuilder.Mat("M_CrtFrame", new Color(0.2f, 0.22f, 0.25f)));
            var scanLine = RoomBuilder.Box(room.transform, "ScanLine", RoomBuilder.V(-5.2f, 1.35f, -19.25f), RoomBuilder.V(0.32f, 0.42f, 0.01f),
                RoomBuilder.Mat("M_ScanLine", new Color(0.15f, 0.4f, 0.3f)), false);
            var ct = console.AddComponent<ConsoleTerminal>();
            ct.password = "0403";
            ct.files = new[]
            {
                new ComputerPanel.FileEntry{ Title="控制日志 · 2004.11", Body=ComputerFiles.R4_Log, EvidenceFlag="ev_console" },
                new ComputerPanel.FileEntry{ Title="主管手记 · 关于『门』", Body=ComputerFiles.R4_Director },
                new ComputerPanel.FileEntry{ Title="附录：致后来的人", Body=ComputerFiles.R4_Postscript },
            };
            // 抽屉（登录后显现，磁带B）
            var drawer = new GameObject("ConsoleDrawer");
            drawer.transform.SetParent(room.transform);
            drawer.SetActive(false);
            var drawerBox = RoomBuilder.Box(drawer.transform, "DrawerBox", RoomBuilder.V(-5.5f, 0.55f, -19.2f), RoomBuilder.V(0.7f, 0.12f, 0.4f), metal);
            var tapeB = RoomBuilder.Box(drawer.transform, "TapeB", RoomBuilder.V(-5.5f, 0.65f, -19.2f), RoomBuilder.V(0.24f, 0.05f, 0.16f),
                RoomBuilder.Mat("M_Tape", new Color(0.3f, 0.3f, 0.35f)), false);
            RoomBuilder.PickupOn(tapeB, "tape_b", "拾取录音带B");
            var drawerNote = RoomBuilder.Box(drawer.transform, "DrawerNote", RoomBuilder.V(-5.5f, 0.68f, -19.55f), RoomBuilder.V(0.2f, 0.015f, 0.16f), paper);
            RoomBuilder.ReadableOn(drawerNote, "抽屉便签", StoryDocs.DrawerR4, "查看便签");
            var fr = drawer.AddComponent<FlagReveal>();
            fr.flag = "subway_unlock";
            ct.revealGo = drawer;

            // 控制台锁屏提示（未登录时也可读）
            var lockNote = RoomBuilder.Box(room.transform, "LockNote", RoomBuilder.V(-5.2f, 1.5f, -19.25f), RoomBuilder.V(0.22f, 0.015f, 0.2f), paper);
            RoomBuilder.ReadableOn(lockNote, "锁屏提示", StoryDocs.ConsoleLockR4, "查看提示");

            // 出生证明 / 死亡记录（证据）
            var birth = RoomBuilder.Box(room.transform, "BirthCertR4", RoomBuilder.V(-13f, 0.86f, -17.5f), RoomBuilder.V(0.3f, 0.02f, 0.4f), paper);
            RoomBuilder.ReadableOn(birth, "出生证明", StoryDocs.BirthCertR4, "查看文件", "ev_birth2");
            var death = RoomBuilder.Box(room.transform, "DeathRecord", RoomBuilder.V(-12.4f, 0.86f, -17.5f), RoomBuilder.V(0.3f, 0.02f, 0.4f), paper);
            RoomBuilder.ReadableOn(death, "死亡记录", StoryDocs.DeathRecordR4, "查看文件", "ev_death");

            // 录音机×2（A/B带）+ 面板细节
            var deckA = RoomBuilder.Box(room.transform, "DeckA", RoomBuilder.V(-8.5f, 0.85f, -22.4f), RoomBuilder.V(0.5f, 0.25f, 0.4f), metal, false);
            // 录音机磁带窗口
            var deckAWin = RoomBuilder.Box(room.transform, "DeckAWin", RoomBuilder.V(-8.5f, 0.85f, -22.18f), RoomBuilder.V(0.3f, 0.15f, 0.02f),
                RoomBuilder.Mat("M_DeckWin", new Color(0.15f, 0.15f, 0.18f)), false);
            var playerA = deckA.AddComponent<AudioLogPlayer>();
            playerA.tapeItem = "tape_a";
            playerA.tapeText = StoryDocs.TapeAText;
            playerA.evidenceFlag = "ev_tape_a";
            playerA.title = "录音机 · A舱";
            playerA.prompt = "播放录音带A";
            var deckB = RoomBuilder.Box(room.transform, "DeckB", RoomBuilder.V(-7.7f, 0.85f, -22.4f), RoomBuilder.V(0.5f, 0.25f, 0.4f), metal, false);
            var playerB = deckB.AddComponent<AudioLogPlayer>();
            playerB.tapeItem = "tape_b";
            playerB.tapeText = StoryDocs.TapeBText;
            playerB.evidenceFlag = "ev_tape_b";
            playerB.title = "录音机 · B舱";
            playerB.prompt = "播放录音带B";

            RoomBuilder.PointLight(room.transform, "R4Lamp", RoomBuilder.V(-9f, 2.9f, -20f), new Color(0.75f, 0.85f, 1f), 1.0f, 12f);
            var r4red = RoomBuilder.PointLight(room.transform, "R4Red", RoomBuilder.V(-5.2f, 2.2f, -20f), new Color(1f, 0.25f, 0.2f), 0.5f, 5f);
            r4red.gameObject.AddComponent<FlickerLight>().amount = 0.28f;

            RoomBuilder.Spawn("R4", RoomBuilder.V(-4.5f, 0.05f, -19f), -90f);
            RoomBuilder.Trigger(room.transform, "R4", RoomBuilder.V(-9f, 1.5f, -20f), RoomBuilder.V(10f, 3f, 5.5f),
                StoryTexts.Narr_R4, 0, null, 1);
        }
    }
}
