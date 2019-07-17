using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    // 매칭 조건에 의해 생성되는 타일의 종류
    public enum RemoveType
    {
        Normal,
        Butterfly,
        HorizonRemover,
        VerticalRemover,
        Bomb,
        KindRemover
    }

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

    public enum MatchDir
    {
        None,
        Horizon,
        Vertical,
        Cross, // Hori && vertical
    }    

    // GamePanel::TrySwapTile Function Result
    public enum SwapTileResult
    {
        NoTile,
        Failure,
        Success,
    }
}