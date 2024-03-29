﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Match3
{
    using MatchList = List<Match3.Point2D>;
    
public class MatchInfo
{
    public MatchList matchlist;

    public bool isMatch = false;

    public MatchDir direction;

    public bool isCrossMatch
    { get { return direction == MatchDir.Cross;}}

    // remover 생성 정보 필요
    public MatchType matchType; // 적절한 이름 고민 중

    public Point2D creationPos;

    public bool isLinearMatch
    {
        get { return (matchType == MatchType.Normal || matchType == MatchType.Horizon4 || matchType == MatchType.Vertical4);}
    }

    public bool isCreationTile
    {
        get { return (matchType != MatchType.Normal);}
    }

    public int Length { get { return matchlist.Count; } }

    public void Reset()
    {
        direction = MatchDir.None;
        matchlist.Clear();
        isMatch = false;
        matchType = MatchType.Normal;
    }

    public void SetCreationPos(int x, int y)
    {
        AddTilePosition(x, y);
        creationPos.row = x;
        creationPos.col = y;        
    }

    public void AddTilePosition(int x, int y)
    {
        matchlist.Add(new Point2D(x, y));
    }

    public void AddTilePosition(Point2D pos)
    {
        matchlist.Add(pos);
    }

    public void AddTilePosition(MatchInfo info)
    {
        matchlist.AddRange(info.matchlist);
    }    

    public void RemoveLast()
    {
        matchlist.RemoveAt(matchlist.Count-1);
    }

    public void CopyMatchList( MatchList input )
    {
        input = matchlist; // 내용을 복사
    }

    public MatchInfo()
    {
        matchlist = new MatchList();
    }

    public MatchInfo(MatchInfo info)
    {
        matchlist = new MatchList(info.matchlist);
        isMatch = info.isMatch;
        direction = info.direction;
        matchType = info.matchType;
        creationPos = info.creationPos;
    }

    public bool Find(int row, int col)
    {
        for( int i=0; i<matchlist.Count; i++)
        {
            if(matchlist[i].row == row && matchlist[i].col == col)
                return true;
        }
        return false;
    }
}

}