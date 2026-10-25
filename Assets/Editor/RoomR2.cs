using UnityEngine;
using Door13.Interaction;
using Door13.Puzzle;
using Door13.Story;

namespace Door13.EditorTools
{
    /// <summary>R2 实验室：收音机调频/试剂组合/冷冻柜。x[-15,-3] z[-14,-6]。</summary>
    public static class RoomR2
    {
        public static void Build()
        {
            var room = new GameObject("Room_R2");
            var wall = RoomBuilder.Mat("M_Wall2", new Color(0.52f, 0.54f, 0.58f));
            var floor = RoomBuilder.Mat("M_Floor2", new Color(0.40f, 0.41f, 0.44f));
            var metal = RoomBuilder.Mat("M_Metal", new Color(0.44f, 0.47f, 0.51f));
            var paper = RoomBuilder.Mat("M_Paper", new Color(0.82f, 0.81f, 0.76f));
            var wood = RoomBuilder.Mat("M_Wood", new Color(0.52f, 0.40f, 0.28f));

            RoomBuilder.Shell(room.transform, -15, -3, -14, -6, 3.2f, floor, wall);
            RoomBuilder.WallX(room.transform, "N", -14.1f, -15.2f, -2.8f, 3.2f, wall);
            RoomBuilder.WallX(room.transform, "S", -5.9f, -15.2f, -2.8f, 3.2f, wall);
            RoomBuilder.WallZ(room.transform, "W", -15.1f, -14.2f, -5.8f, 3.2f, wall);
            RoomBuilder.WallZ(room.transform, "E", -2.9f, -14.2f, -5.8f, 3.2f, wall, -10f, 1.4f);

            // ---- 环境细节：地面污渍 + 墙皮脱落 + 管线 ----
            var stainMat = RoomBuilder.Mat("M_Stain2", new Color(0.15f, 0.14f, 0.12f));
            RoomBuilder.Box(room.transform, "R2Stain1", RoomBuilder.V(-9f, 0.012f, -12f), RoomBuilder.V(1.0f, 0.01f, 0.6f), stainMat, false);
            RoomBuilder.Box(room.transform, "R2Stain2", RoomBuilder.V(-13f, 0.012f, -7f), RoomBuilder.V(0.8f, 0.01f, 0.5f), stainMat, false);
            // 墙皮脱落（南墙）
            var peelMat = RoomBuilder.Mat("M_Peel2", new Color(0.35f, 0.33f, 0.30f));
            RoomBuilder.Box(room.transform, "R2WallPeel", RoomBuilder.V(-9f, 1.8f, -5.94f), RoomBuilder.V(1.5f, 0.8f, 0.02f), peelMat, false);
            // 沿东墙管线
            var pipe = RoomBuilder.Mat("M_Pipe", new Color(0.5f, 0.42f, 0.35f));
            RoomBuilder.Box(room.transform, "Pipe_E1", RoomBuilder.V(-2.95f, 2.9f, -10f), RoomBuilder.V(0.08f, 0.08f, 6f), pipe);
            RoomBuilder.Box(room.transform, "Pipe_E2", RoomBuilder.V(-2.95f, 2.7f, -10f), RoomBuilder.V(0.08f, 0.08f, 6f), pipe);

            // 实验台 + 收音机（调频 91.3）
            RoomBuilder.Box(room.transform, "LabTable", RoomBuilder.V(-9f, 0.75f, -10f), RoomBuilder.V(3.0f, 0.1f, 1.0f), wood);
            RoomBuilder.Box(room.transform, "LabLeg1", RoomBuilder.V(-10.3f, 0.35f, -10f), RoomBuilder.V(0.1f, 0.7f, 0.8f), wood);
            RoomBuilder.Box(room.transform, "LabLeg2", RoomBuilder.V(-7.7f, 0.35f, -10f), RoomBuilder.V(0.1f, 0.7f, 0.8f), wood);
            // 实验台额外设备：烧杯、量筒
            var glassMat = RoomBuilder.Mat("M_Glass", new Color(0.6f, 0.75f, 0.8f));
            RoomBuilder.Box(room.transform, "Beaker", RoomBuilder.V(-8.2f, 0.88f, -9.7f), RoomBuilder.V(0.08f, 0.12f, 0.08f), glassMat, false);
            RoomBuilder.Box(room.transform, "GraduatedCylinder", RoomBuilder.V(-7.5f, 0.88f, -10.3f), RoomBuilder.V(0.06f, 0.15f, 0.06f), glassMat, false);
            var radio = RoomBuilder.Box(room.transform, "Radio", RoomBuilder.V(-9.8f, 0.95f, -10f), RoomBuilder.V(0.5f, 0.3f, 0.3f), metal, false);
            var pzRadio = radio.AddComponent<PuzzleInteractable>();
            pzRadio.type = PuzzleType.Tuner;
            pzRadio.puzzleId = "radio_r2";
            pzRadio.panelTitle = "收音机 · 调频";
            pzRadio.solution = "91.3";
            pzRadio.tunerHint = "纸条上写着：最后一次广播，91.3。";
            pzRadio.prompt = "使用收音机";
            pzRadio.successMessage = "杂音退去，一个声音开始念数字。";
            // 广播记录（解开后出现在收音机旁）
            var broadcast = RoomBuilder.Box(room.transform, "BroadcastNote", RoomBuilder.V(-9.2f, 0.86f, -9.6f), RoomBuilder.V(0.25f, 0.015f, 0.2f), paper);
            RoomBuilder.ReadableOn(broadcast, "广播记录", StoryDocs.RadioBroadcastR2, "查看记录");
            broadcast.SetActive(false);
            pzRadio.revealGo = broadcast;

            // 试剂台：试剂A/B + 标签
            RoomBuilder.Box(room.transform, "ReagentShelf", RoomBuilder.V(-13.5f, 1.2f, -10f), RoomBuilder.V(0.5f, 0.08f, 2.4f), wood);
            // 试剂架层板
            RoomBuilder.Box(room.transform, "ReagentShelf2", RoomBuilder.V(-13.5f, 0.8f, -10f), RoomBuilder.V(0.4f, 0.04f, 2.0f), wood, false);
            var ra = RoomBuilder.Box(room.transform, "ReagentA", RoomBuilder.V(-13.5f, 1.34f, -9.3f), RoomBuilder.V(0.14f, 0.2f, 0.14f),
                RoomBuilder.Mat("M_ReagentA", new Color(0.75f, 0.5f, 0.2f)), false);
            RoomBuilder.PickupOn(ra, "reagent_a", "拾取试剂A");
            var rb = RoomBuilder.Box(room.transform, "ReagentB", RoomBuilder.V(-13.5f, 1.34f, -10.7f), RoomBuilder.V(0.14f, 0.2f, 0.14f),
                RoomBuilder.Mat("M_ReagentB", new Color(0.7f, 0.9f, 0.9f)), false);
            RoomBuilder.PickupOn(rb, "reagent_b", "拾取试剂B");
            var rlabel = RoomBuilder.Box(room.transform, "ReagentLabel", RoomBuilder.V(-13.5f, 1.32f, -11.2f), RoomBuilder.V(0.2f, 0.015f, 0.2f), paper);
            RoomBuilder.ReadableOn(rlabel, "试剂柜标签", StoryDocs.ReagentLabelR2, "查看标签");

            // 冷冻柜（用溶解剂）+ 冰霜效果
            var freezer = RoomBuilder.Box(room.transform, "Freezer", RoomBuilder.V(-6f, 0.7f, -12.8f), RoomBuilder.V(1.4f, 1.4f, 1.0f), metal);
            // 冷冻柜冰霜层（半透明蓝色覆盖）
            var iceFrost = RoomBuilder.Box(room.transform, "FreezerFrost", RoomBuilder.V(-6f, 0.7f, -12.24f), RoomBuilder.V(1.3f, 1.3f, 0.01f),
                RoomBuilder.Mat("M_Frost", new Color(0.6f, 0.8f, 0.95f)), false);
            iceFrost.GetComponent<Renderer>().material.SetFloat("_Smoothness", 0.1f);
            var iceLock = RoomBuilder.Box(room.transform, "IceLock", RoomBuilder.V(-5.6f, 0.9f, -12.25f), RoomBuilder.V(0.3f, 0.4f, 0.12f),
                RoomBuilder.Mat("M_Ice", new Color(0.7f, 0.85f, 0.95f)), false);
            var use = freezer.AddComponent<ItemSpotUse>();
            use.requiredItemId = "solvent";
            use.lockedMessage = "冷冻柜被低温粘合剂封死了。";
            use.successMessage = "粘合剂嘶嘶作响，柜门松开了。";
            use.hideOnUse = iceLock;
            use.solvedFlag = "freezer_r2";
            use.prompt = "使用溶解剂";
            // 柜内战利品（解开同步显现）
            var card = RoomBuilder.Box(room.transform, "ArchiveCard", RoomBuilder.V(-6f, 0.5f, -12.7f), RoomBuilder.V(0.26f, 0.03f, 0.18f),
                RoomBuilder.Mat("M_ArcCard", new Color(0.9f, 0.7f, 0.85f)), false);
            var cardPick = RoomBuilder.PickupOn(card, "archive_card", "拾取档案权限卡");
            card.SetActive(false);
            use.reveal = cardPick;

            var fnote = RoomBuilder.Box(room.transform, "FreezerNote", RoomBuilder.V(-5.3f, 1.45f, -12.7f), RoomBuilder.V(0.24f, 0.015f, 0.2f), paper);
            RoomBuilder.ReadableOn(fnote, "处理单", StoryDocs.FreezerNoteR2, "查看处理单");

            // 实验日志（证据）
            var log = RoomBuilder.Box(room.transform, "LabLog", RoomBuilder.V(-9f, 0.86f, -9.4f), RoomBuilder.V(0.3f, 0.02f, 0.4f), paper);
            RoomBuilder.ReadableOn(log, "实验台日志", StoryDocs.LabLogR2, "查看日志", "ev_log_lab");

            // 抽屉（循环2出现）：碎片2
            var drawerR2 = RoomBuilder.Box(room.transform, "ShardDrawer", RoomBuilder.V(-13.5f, 0.5f, -7.0f), RoomBuilder.V(0.8f, 0.5f, 0.8f), metal, false);
            var shard2 = RoomBuilder.Box(room.transform, "Shard2", RoomBuilder.V(-13.5f, 0.8f, -7.0f), RoomBuilder.V(0.24f, 0.02f, 0.18f),
                RoomBuilder.Mat("M_Shard", new Color(0.75f, 0.78f, 0.85f)), false);
            RoomBuilder.PickupOn(shard2, "shard_2", "拾取记忆碎片");
            shard2.SetActive(false);
            RoomBuilder.LoopRule(shard2, "shard2_spawn", 2, true, true);
            RoomBuilder.LoopRule(drawerR2, "drawer_r2", 2, true, true);
            drawerR2.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            RoomBuilder.PointLight(room.transform, "LabLamp1", RoomBuilder.V(-9f, 2.9f, -10f), new Color(0.85f, 0.9f, 1f), 1.15f, 10f);
            RoomBuilder.PointLight(room.transform, "LabLamp2", RoomBuilder.V(-6.5f, 2.6f, -12.5f), new Color(0.9f, 0.8f, 0.7f), 0.8f, 6f);

            RoomBuilder.Spawn("R2", RoomBuilder.V(-4.5f, 0.05f, -10f), -90f);
            RoomBuilder.Trigger(room.transform, "R2", RoomBuilder.V(-9f, 1.5f, -10f), RoomBuilder.V(10f, 3f, 6f),
                StoryTexts.Narr_R2, 0, null, 1);
        }
    }
}
