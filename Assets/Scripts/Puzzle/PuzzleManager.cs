using UnityEngine;
using Door13.Save;
using Door13.UI;

namespace Door13.Puzzle
{
    /// <summary>谜题进度登记：解谜→旗标→存档→提示。全部谜题经此登记。</summary>
    public static class PuzzleManager
    {
        /// <summary>标记谜题已解（幂等）。返回是否为首次解开。</summary>
        public static bool Solve(string puzzleId, string toast = null)
        {
            bool first = !GameFlags.IsSolved(puzzleId);
            if (first)
            {
                GameFlags.MarkSolved(puzzleId);
                if (toast != null && Hud.Instance != null) Hud.Instance.Toast(toast);
                SaveManager.Save();
            }
            return first;
        }

        public static bool IsSolved(string puzzleId) => GameFlags.IsSolved(puzzleId);
    }
}
