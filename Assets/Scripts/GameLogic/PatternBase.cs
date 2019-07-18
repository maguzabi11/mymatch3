using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{

public abstract class PatternBase
{
    protected abstract void Init();

    protected List<List<Point2D>> matchlist = new List<List<Point2D>>();

    public int GetPatternLength(int pattern)
    {
        return matchlist[pattern].Count; 
    }

    public Point2D GetPatternPos(int pattern, int i, int row, int col)
    {
        var calculatedPos = new Point2D(matchlist[pattern][i].row + row, matchlist[pattern][i].col + col);
        return calculatedPos;
    }  

    public int Length
    {
        get { return matchlist.Count; }
    }    
}

}