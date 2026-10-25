using UnityEngine;
using Door13.Puzzle;
using Door13.Save;
using Door13.Story;

namespace Door13.EditorTools
{
    /// <summary>R3 档案室：档案柜409/照片排序暗格/书架314保险柜。x[3,15] z[-14,-6]。</summary>
    public static class RoomR3
    {
        public static void Build()
        {
            var room = new GameObject("Room_R3");
            var wall = RoomBuilder.Mat("M_Wall3", new Color(0.52f, 0.54f, 0.58f));
            var floor = RoomBuilder.Mat("M_Floor3", new Color(0.40f, 0.41f, 0.44f));
            var metal = RoomBuilder.Mat("M_Metal", new Color(0.44f, 0.47f, 0.51f));
            var paper = RoomBuilder.Mat("M_Paper", new Color(0.82f, 0.81f, 0.76f));
            var wood = RoomBuilder.Mat("M_Wood", new Color(0.52f, 0.40f, 0.28f));
            var dark = RoomBuilder.Mat("M_Dark", new Color(0.08f, 0.08f, 0.10f));

            RoomBuilder.Shell(room.transform, 3, 15, -14, -6, 3.2f, floor, wall);
            RoomBuilder.WallX(room.transform, "N", -14.1f, 2.8f, 15.2f, 3.2f, wall);
            RoomBuilder.WallX(room.transform, "S", -5.9f, 2.8f, 15.2f, 3.2f, wall);
            RoomBuilder.WallZ(room.transform, "W", 2.9f, -14.2f, -5.8f, 3.2f, wall, -10f, 1.4f);
            RoomBuilder.WallZ(room.transform, "E", 15.1f, -14.2f, -5.8f, 3.2f, wall);

            // 地面污渍 + 墙皮脱落
            RoomBuilder.Box(room.transform, "R3Stain1", RoomBuilder.V(8f, 0.012f, -10f), RoomBuilder.V(0.8f, 0.01f, 0.6f),
                RoomBuilder.Mat("M_Stain", new Color(0.35f, 0.33f, 0.30f)));
            RoomBuilder.Box(room.transform, "R3WallPeel", RoomBuilder.V(12f, 1.5f, -13.94f), RoomBuilder.V(0.5f, 0.4f, 0.02f),
                RoomBuilder.Mat("M_WallPeel", new Color(0.65f, 0.62f, 0.55f)));

            // 档案柜（409）→ 出生证明 + 老照片
            var cabinet = RoomBuilder.Box(room.transform, "Cabinet", RoomBuilder.V(6f, 1.0f, -13.3f), RoomBuilder.V(2.2f, 2.0f, 0.8f), metal);
            // 档案柜编号牌 + 铰链细节
            var cabTag = RoomBuilder.Box(room.transform, "CabinetTag", RoomBuilder.V(6f, 2.5f, -12.85f), RoomBuilder.V(0.6f, 0.08f, 0.02f), paper);
            RoomBuilder.ReadableOn(cabTag, "档案柜编号", "档案柜 T-409 · 记忆实验记录\n权限：三级", "查看编号牌");
            var cabHinge1 = RoomBuilder.Box(room.transform, "CabinetHinge1", RoomBuilder.V(7.05f, 0.5f, -12.85f), RoomBuilder.V(0.06f, 0.12f, 0.04f), metal);
            var cabHinge2 = RoomBuilder.Box(room.transform, "CabinetHinge2", RoomBuilder.V(7.05f, 1.5f, -12.85f), RoomBuilder.V(0.06f, 0.12f, 0.04f), metal);
            var pzCab = cabinet.AddComponent<PuzzleInteractable>();
            pzCab.type = PuzzleType.Keypad;
            pzCab.puzzleId = "cabinet_r3";
            pzCab.panelTitle = "档案柜 · 三位密码";
            pzCab.solution = "409";
            pzCab.prompt = "使用档案柜";
            pzCab.successMessage = "柜门弹开，一叠卡片滑了出来。";
            var cabLoot = new GameObject("CabinetLoot");
            cabLoot.transform.SetParent(room.transform);
            cabLoot.SetActive(false);
            // 柜门弹开后物品落在柜前地面（柜体正面 z=-12.9，物品须完全在柜体外）
            var cert = RoomBuilder.Box(cabLoot.transform, "BirthCert", RoomBuilder.V(5.5f, 0.012f, -12.65f), RoomBuilder.V(0.3f, 0.02f, 0.4f), paper);
            RoomBuilder.ReadableOn(cert, "出生证明", StoryDocs.BirthCertR4, "查看文件", "ev_birth");
            var photo = RoomBuilder.Box(cabLoot.transform, "PhotoOld", RoomBuilder.V(6.4f, 0.012f, -12.68f), RoomBuilder.V(0.26f, 0.02f, 0.2f),
                RoomBuilder.Mat("M_PhotoOld", new Color(0.85f, 0.8f, 0.7f)), false);
            RoomBuilder.PickupOn(photo, "photo_old", "拾取老照片");
            pzCab.revealGo = cabLoot;

            // 三张档案卡（可读，T13为证据）
            var c04 = RoomBuilder.Box(room.transform, "Card04", RoomBuilder.V(7.4f, 1.0f, -13.0f), RoomBuilder.V(0.22f, 0.3f, 0.02f), paper);
            RoomBuilder.ReadableOn(c04, "档案卡 T-04", StoryDocs.CardT04, "查看档案卡");
            var c09 = RoomBuilder.Box(room.transform, "Card09", RoomBuilder.V(7.7f, 1.0f, -13.0f), RoomBuilder.V(0.22f, 0.3f, 0.02f), paper);
            RoomBuilder.ReadableOn(c09, "档案卡 T-09", StoryDocs.CardT09, "查看档案卡");
            var c13 = RoomBuilder.Box(room.transform, "Card13", RoomBuilder.V(8.0f, 1.0f, -13.0f), RoomBuilder.V(0.22f, 0.3f, 0.02f), paper);
            RoomBuilder.ReadableOn(c13, "档案卡 T-13", StoryDocs.CardT13, "查看档案卡", "ev_card_t13");
            var cabNote = RoomBuilder.Box(room.transform, "CabinetNote", RoomBuilder.V(6f, 2.15f, -12.85f), RoomBuilder.V(0.4f, 0.015f, 0.3f), paper);
            RoomBuilder.ReadableOn(cabNote, "柜子便签", StoryDocs.CabinetNoteR3, "查看便签");

            // 照片墙（排序）→ 暗格（碎片1）
            var board = RoomBuilder.Box(room.transform, "PhotoBoardR3", RoomBuilder.V(11f, 1.7f, -13.6f), RoomBuilder.V(3.0f, 1.6f, 0.08f), paper);
            var pzSort = board.AddComponent<PuzzleInteractable>();
            pzSort.type = PuzzleType.Sort;
            pzSort.puzzleId = "photos_r3";
            pzSort.panelTitle = "照片墙 · 时间线";
            pzSort.solution = "0,1,2,3";
            pzSort.optionLabels = "1996·合影,1997·实验,1998·生日,2001·空椅";
            pzSort.tunerHint = "按时间顺序排列，暗格自开。";
            pzSort.prompt = "查看照片墙";
            pzSort.successMessage = "咔哒。照片墙后的暗格开了。";
            float[] px = { 10.0f, 10.75f, 11.5f, 12.25f };
            for (int i = 0; i < 4; i++)
            {
                var ph = RoomBuilder.Box(room.transform, "PhotoR3_" + (i + 1), RoomBuilder.V(px[i], 1.7f, -13.52f), RoomBuilder.V(0.6f, 0.5f, 0.03f), paper, false);
                RoomBuilder.ReadableOn(ph, "照片 " + (i + 1),
                    new[] { StoryDocs.Photo1R3, StoryDocs.Photo2R3, StoryDocs.Photo3R3, StoryDocs.Photo4R3 }[i], "查看照片");
                // 图钉/夹子细节
                var pin = RoomBuilder.Box(room.transform, "PhotoPin_" + (i + 1), RoomBuilder.V(px[i], 2.0f, -13.55f), RoomBuilder.V(0.04f, 0.04f, 0.02f),
                    RoomBuilder.Mat("M_Pin", new Color(0.45f, 0.45f, 0.48f)), false);
            }
            var cover = RoomBuilder.Box(room.transform, "DarkCover", RoomBuilder.V(11f, 0.9f, -13.55f), RoomBuilder.V(0.8f, 0.8f, 0.05f), wall, false);
            pzSort.hideOnSolve = cover;
            var shard1 = RoomBuilder.Box(room.transform, "Shard1", RoomBuilder.V(11f, 0.9f, -13.45f), RoomBuilder.V(0.24f, 0.02f, 0.18f),
                RoomBuilder.Mat("M_Shard", new Color(0.75f, 0.78f, 0.85f)), false);
            RoomBuilder.PickupOn(shard1, "shard_1", "拾取记忆碎片");
            var darkNote = RoomBuilder.Box(room.transform, "DarkNote", RoomBuilder.V(11.6f, 0.9f, -13.45f), RoomBuilder.V(0.24f, 0.015f, 0.18f), paper);
            RoomBuilder.ReadableOn(darkNote, "暗格里的纸", StoryDocs.DrawerNoteR3, "查看纸条");

            // 书架（索书号 3-1-4）
            var shelf = RoomBuilder.Box(room.transform, "Shelf", RoomBuilder.V(15.0f - 0.15f, 1.3f, -10f), RoomBuilder.V(0.5f, 2.2f, 3.0f), wood);
            // 书架索书号标签
            var shelfLabel = RoomBuilder.Box(room.transform, "ShelfLabel", RoomBuilder.V(14.85f, 2.6f, -8.4f), RoomBuilder.V(0.3f, 0.06f, 0.02f), paper);
            RoomBuilder.ReadableOn(shelfLabel, "索书号标签", "314 · 记忆实验档案\nT-01 ~ T-15", "查看标签");
            var book = RoomBuilder.Box(room.transform, "BrightBook", RoomBuilder.V(14.7f, 1.3f, -9.2f), RoomBuilder.V(0.18f, 0.4f, 0.3f),
                RoomBuilder.Mat("M_Book", new Color(0.55f, 0.35f, 0.25f)), false);
            RoomBuilder.ReadableOn(book, "被摸亮的书", StoryDocs.BookR3, "查看书脊");
            var shelfNote = RoomBuilder.Box(room.transform, "ShelfNote", RoomBuilder.V(14.7f, 2.2f, -10.8f), RoomBuilder.V(0.2f, 0.015f, 0.2f), paper);
            RoomBuilder.ReadableOn(shelfNote, "书架便签", StoryDocs.ShelfNoteR3, "查看便签");

            // 保险柜（314）→ 手电筒 + 控制中心钥匙卡
            var safe = RoomBuilder.Box(room.transform, "SafeR3", RoomBuilder.V(4.2f, 0.45f, -7.2f), RoomBuilder.V(0.9f, 0.9f, 0.9f), dark);
            var pzSafe = safe.AddComponent<PuzzleInteractable>();
            pzSafe.type = PuzzleType.Keypad;
            pzSafe.puzzleId = "safe_r3";
            pzSafe.panelTitle = "档案室保险柜";
            pzSafe.solution = "314";
            pzSafe.prompt = "使用保险柜";
            pzSafe.successMessage = "柜门开了。手电筒和一张卡掉在了地上。";
            var safeNote = RoomBuilder.Box(room.transform, "SafeNoteR3", RoomBuilder.V(4.2f, 1.0f, -7.2f), RoomBuilder.V(0.3f, 0.015f, 0.24f), paper);
            RoomBuilder.ReadableOn(safeNote, "保险柜标签", StoryDocs.SafeRoomNoteR3, "查看标签");
            var safeLoot = new GameObject("SafeLootR3");
            safeLoot.transform.SetParent(room.transform);
            safeLoot.SetActive(false);
            // 保险柜是实心立方体（且谜题组件挂在其上）——战利品必须落在柜前地面，否则永久不可拾取
            var torch = RoomBuilder.Box(safeLoot.transform, "Flashlight", RoomBuilder.V(4.0f, 0.06f, -6.45f), RoomBuilder.V(0.12f, 0.12f, 0.35f),
                RoomBuilder.Mat("M_Flashlight", new Color(0.85f, 0.85f, 0.7f)), false);
            RoomBuilder.PickupOn(torch, "flashlight", "拾取手电筒", GameFlags.Flashlight);
            var ctrl = RoomBuilder.Box(safeLoot.transform, "CtrlCard", RoomBuilder.V(4.45f, 0.045f, -6.45f), RoomBuilder.V(0.26f, 0.03f, 0.18f),
                RoomBuilder.Mat("M_CtrlCard", new Color(0.95f, 0.6f, 0.4f)), false);
            RoomBuilder.PickupOn(ctrl, "ctrl_card", "拾取控制中心钥匙卡");
            pzSafe.revealGo = safeLoot;

            RoomBuilder.PointLight(room.transform, "R3Lamp", RoomBuilder.V(9f, 2.9f, -10f), new Color(1f, 0.92f, 0.78f), 1.15f, 11f);
            RoomBuilder.PointLight(room.transform, "R3Lamp2", RoomBuilder.V(6f, 2.8f, -13f), new Color(0.7f, 0.65f, 0.55f), 0.4f, 6f);

            RoomBuilder.Spawn("R3", RoomBuilder.V(4.5f, 0.05f, -10f), 90f);
            RoomBuilder.Trigger(room.transform, "R3", RoomBuilder.V(9f, 1.5f, -10f), RoomBuilder.V(10f, 3f, 6f),
                StoryTexts.Narr_R3, 0);
        }
    }
}
