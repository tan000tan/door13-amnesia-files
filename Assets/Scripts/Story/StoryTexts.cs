namespace Door13.Story
{
    /// <summary>系统/UI 层文本：目标、旁白、结局、提示（正文见 StoryDocs）。</summary>
    public static class StoryTexts
    {
        // ---- 目标（按循环） ----
        public static string Objective(int loop)
        {
            switch (loop)
            {
                case 2: return "目标：你记得一切。找出第一次没能发现的东西";
                case 3: return "目标：收集记忆碎片（3块），逼近真相";
                case 4: return "目标：打开第十三扇门，面对答案";
                default: return "目标：逃出苏醒室，调查这座研究所";
            }
        }

        // ---- 循环旁白 ----
        public static string LoopNarration(int newLoop)
        {
            switch (newLoop)
            {
                case 2: return "……你再次在医疗床上醒来。\n墙上写着：你已经来过这里13次。\n——你记得毒气。你记得一切。";
                case 3: return "墙上的字换了：『别再数了。』\n照片里的人离镜头更近了。\n有什么东西在裂缝里发光。";
                case 4: return "这是最后一次循环。\n走廊尽头，第十三扇门后面的灯亮了。\n它在等你回家。";
                default: return "";
            }
        }

        public static string DeviceTouch(int newLoop)
        {
            switch (newLoop)
            {
                case 2: return "装置苏醒了。世界在你眼前倒带。";
                case 3: return "倒带。这一次，你睁开了另一只眼睛。";
                case 4: return "最后一次倒带。走廊尽头有灯亮起。";
                default: return "";
            }
        }

        // ---- 固定提示 ----
        public const string DoorLocked = "门被锁死了。";
        public const string SafeOpened = "保险箱开了。";
        public const string EndingBlocked = "容器拒绝了你。证据不足——你还不知道自己是谁。";

        // ---- 房间首次进入旁白 ----
        public const string Narr_R1 = "长长的回廊。两侧的门牌锈得看不清，只有编号还亮着：01、02、03……到12。尽头没有13。";
        public const string Narr_R2 = "实验室。手术灯还亮着一盏，台面上的器械摆得整整齐齐——像有人刚刚离开。";
        public const string Narr_R3 = "档案室。成排的柜子上贴着编号：T-01 到 T-12。第十三排是空的，标签写着『预留』。";
        public const string Narr_R4 = "控制中心。整面墙的屏幕都黑着，只有主控台下面一颗红色待机灯在呼吸。";
        public const string Narr_R5 = "地下区域。黑暗浓得像水。你的手电是这里唯一的光。";
        public const string Narr_R6 = "门后不是房间。是一面墙，墙上有一扇更小的门，和一把椅子。";
        public const string Narr_R7 = "白色的容器室。\n\n中央有一个发光的球体——记忆容器。\n\n它等着装下你记住的一切。\n不只是你的记忆。\n还有那十二个孩子的。\n\n他们唯一剩下的东西。";

        // ---- 循环推进特殊旁白 ----
        public const string Loop2Special = "……你再次在医疗床上醒来。\n墙上写着：你已经来过这里13次。\n——你记得毒气。你记得一切。\n\n那些数字不是别人的。\n是你自己刻的。";

        public const string Loop3Special = "墙上的字换了：『别再数了。』\n照片里的人离镜头更近了。\n有什么东西在裂缝里发光。\n\n你开始记起一些碎片——\n不是你的记忆。\n是别人的。";

        public const string Loop4Special = "这是最后一次循环。\n走廊尽头，第十三扇门后面的灯亮了。\n它在等你回家。\n\n你记得所有十二个孩子。\n你记得他们每个人的名字。\n你答应过不会忘记。";

        // ---- 房间特殊旁白（循环2+） ----
        public const string Narr_R2_Loop2 = "实验室。和上次一样，又不一样。\n冷冻柜的门微微开着，里面有什么在发光。\n你走过去——是一块记忆碎片。";

        public const string Narr_R3_Loop3 = "档案室。照片墙上的照片位置变了。\n它们在等你把它们排对。\n暗格开了，里面有一块碎片。\n你握住它的时候，记起了一个孩子的脸。";

        public const string Narr_R5_Loop4 = "地下区域。黑暗比之前更深了。\n墙上的名字在发光——十二个孩子。\n他们在问你：你还记得我们吗？\n\n主装置表面的凹槽在等你放入碎片。\n第十三扇门在等你推开门。";

        // ---- 结局 ----
        public static string EndingTitle(int id)
        {
            switch (id)
            {
                case 1: return "结局一 · 循环终点";
                case 2: return "结局二 · 逃离";
                case 3: return "结局三 · 尘归尘";
                default: return "隐藏结局 · 归零";
            }
        }

        public static string EndingBody(int id)
        {
            switch (id)
            {
                case 1:
                    return "你把手放回装置上。\n毒气涌入的瞬间，你甚至觉得熟悉。\n\n第十三次循环开始了。\n你在医疗床上睁开眼。\n墙上写着：你已经来过这里13次。\n\n——而这一次，你什么都不记得了。\n\n十二个孩子的名字，从你的记忆里消失了。\n墙上的数字又变回了12。\n\n（达成：循环终点）";

                case 2:
                    return "电梯上行了很久。\n门开时是清晨，废弃的研究所入口，阳光刺眼。\n\n你回头看了眼研究所的铁门。\n记忆完整、真相完整，但你把它们全部锁在了地下。\n\n十二个孩子的名字，你记住了。\n但你知道，没有人会相信。\n\n偶尔，在梦里，你还会数门。\n1、2、3……13。\n\n然后醒来。\n\n（达成：逃离）";

                case 3:
                    return "你把每一份档案扔进焚化炉。\nT-01 到 T-13，连同你自己的名字一起烧成灰。\n\n火光照亮了地下区域的墙壁。\n那些刻在墙上的名字——\nT-01 到 T-12——\n在火光中一个一个地淡去了。\n\n『至少不会再有第十四个。』\n\n火光里，你仿佛看见十三个孩子排成一排，\n对你点了点头，然后散去。\n\n你叫林澈。你19岁。你什么都记得。\n但你选择了忘记。\n\n（达成：尘归尘）";

                default:
                    return "你走进第十三扇门。\n\n门后是一间小小的白房间，只有一把椅子和一台仪器。\n仪器屏幕上写着：\n\n『受试者 T-13：记忆穿透实验——成功。\n  感谢参与。实验结束。』\n\n你坐下来，笑了。\n\n原来你从来不是调查员，也不只是实验对象——\n你是唯一一个带着记忆走完十三扇门的人。\n\n你记得十二个孩子的名字。\nT-01 到 T-12。\n你记得他们每个人的脸。\n\n容器打开了。\n你把所有记忆放了进去——\n不只是你的，还有他们的。\n那是他们唯一剩下的东西。\n\n灯灭了。\n\n这一次，你在自己的名字里醒来：林澈。\n19岁。\n你记得一切。\n\n但你不再害怕了。\n\n因为你知道——\n\n那些孩子已经不在了。\n但他们被记住了。\n\n这就够了。\n\n（达成：归零——隐藏真结局）";
            }
        }

        // ---- Demo 结束卡（保留兼容） ----
        public const string DemoEnd = "";
        public const string WallTextTitle = "墙上的字";
        public const string StickyTitle = "桌上的便签";
        public const string TerminalTitle = "终端";
        public const string PhotoWallTitle = "照片墙";
    }
}
