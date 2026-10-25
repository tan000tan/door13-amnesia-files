namespace Door13.TimeLoop
{
    /// <summary>
    /// 可随循环重置的对象（门/机关等）。由 LoopManager 在循环重置时回调。
    /// </summary>
    public interface ILoopResettable
    {
        void OnLoopReset(int loop);
    }
}
