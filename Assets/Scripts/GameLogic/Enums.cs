using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public enum TileMovement
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum FindDirection
    {
        Horizon,
        Vertical
    }

    // GamePanel::TrySwapTile Function Result
    public enum SwapTileResult
    {
        NoTile,
        Failure,
        Success,
    }
}