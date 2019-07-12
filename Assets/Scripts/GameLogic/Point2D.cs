using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Match3
{
    [Serializable]
    public struct Point2D
    {
        public int row;
        public int col;

        public Point2D(int x, int y) : this()
        {
            this.row = x;
            this.col = y;
        }
    }
}