namespace Door13.Player
{
    /// <summary>
    /// 全局输入锁：主菜单/结局展示等"剧情机位"状态下禁用玩家移动、视角与交互。
    /// 由 MainMenu 持有生命周期；PlayerController / TouchLookZone / PlayerInteraction 统一检查。
    /// </summary>
    public static class InputLock
    {
        public static bool Active { get; set; }
    }
}
