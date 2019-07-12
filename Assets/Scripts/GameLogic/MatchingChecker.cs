using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Match3
{
    using MatchList = List<Point2D>;

/*
   - Gamepanel에서 역할 분리로 시작.
   
   정리할 것
   - 어느 class와 연동할 것인가?
 */
public class MatchingChecker
{
    List<MatchList> matches = new List<MatchList>();

    GamePanel _gamepanel;

    [Inject]
    public void Constructor(GamePanel gamepanel)
    {
        _gamepanel = gamepanel;
    }

    public List<MatchList> GetMatchInfo()
    {
        return matches;
    }    

    public int FindMatches(Point2D[] poslist)
    {
        ResetSearch();
        FindMatchInfo findinfo = new FindMatchInfo();

        for (int i = 0; i < poslist.Length; i++)
            FindMatchingTiles(poslist[i].row, poslist[i].col, findinfo);

        return matches.Count;
    }

    public int FindAllMatches()
    {
        ResetSearch();
        FindMatchInfo findinfo = new FindMatchInfo();

        for (int i = 0; i < _gamepanel.NumRow; i++)
            for (int j = 0; j < _gamepanel.NumCol; j++)
                FindMatchingTiles(i, j, findinfo);

        return matches.Count;
    }

    private void ResetSearch()
    {
        // 검사 속성 리셋
        matches.Clear();
        _gamepanel.ResetSearch();
    }

    
    /*
    재검토
    - 패턴 데이터 검사
    - 결과는 매치와 매칭의 종류(new)
     */
    private void FindMatchingTiles(int row, int col, FindMatchInfo findinfo)
    {
        Tile baseTile = _gamepanel.tiles[row, col]; // 액세스 방법 정하기.
        if (baseTile.IsChecked)
            return;
        else
            baseTile.IsChecked = true;

        findinfo.AddTilePosition(row, col);
        
        FindMatch( row, col, findinfo, FindDirection.Horizon);
        FindMatch( row, col, findinfo, FindDirection.Vertical);

        if( findinfo.isMatch )
            matches.Add(new MatchList(findinfo.matchlist));

        findinfo.Reset();
    }

    delegate bool compareTile(int row, int col, int offset); // isSame~
    delegate  Tile getNeighborTile(int row, int col, int offset);

    void FindMatch(int row, int col, FindMatchInfo findinfo, FindDirection dir)
    {
        int num = 0;
        compareTile compareTile1;
        compareTile compareTile2;
        getNeighborTile getTile1;
        getNeighborTile getTile2;            
        if( dir == FindDirection.Horizon)
        {
            num = _gamepanel.NumCol-1;
            compareTile1 = _gamepanel.IsSameLeft;
            compareTile2 = _gamepanel.IsSameRight;
            getTile1 = _gamepanel.GetLeftTile;
            getTile2 = _gamepanel.GetRightTile;
        }
        else
        {
            num = _gamepanel.NumRow-1;
            compareTile1 = _gamepanel.IsSameUp;
            compareTile2 = _gamepanel.IsSameDown;
            getTile1 = _gamepanel.GetUpTile;
            getTile2 = _gamepanel.GetDownTile;
        }

        List<Tile> matchCandidates = new List<Tile>();
        bool bStopLeft = false;
        bool bStopRight = false;
        for (int i = 0; i < num; i++)
        {
            if(!bStopLeft && compareTile1(row, col, i))
            {
                var tile = getTile1(row, col, i);
                matchCandidates.Add(tile);

            }
            else
            {
                if(bStopRight)
                    break;
                bStopLeft = true;
            }

            if(!bStopRight && compareTile2(row, col, i))
            {
                var tile = getTile2(row, col, i);
                matchCandidates.Add(tile);

            }
            else
            {
                if(bStopLeft)
                    break;
                bStopRight = true;
            }
        }   
        
        if(matchCandidates.Count >= 2)
        {
            findinfo.isMatch = true;
            foreach( var tile in matchCandidates)
            {
                if( tile.IsMatched == false )
                {
                    findinfo.AddTilePosition(tile.GetLocation());
                    tile.MarkSearch();
                }
            }
        }
        matchCandidates.Clear();
    }   

    public void OutputMatches()
    {
        StringBuilder str = new StringBuilder(64);
        for(int i=0; i<matches.Count; i++)
        {
            MatchList list = matches[i];
            Debug.LogFormat("match[{0}]", i);

            foreach(Point2D pos in list)
                str.AppendFormat($"[{pos.row},{pos.col}] ");
            str.AppendLine();
            Debug.Log(str.ToString());
            str.Clear();
        }
    }
}

class FindMatchInfo
{
    public MatchList matchlist = new MatchList();

    public bool isMatch = false;

    public int Length { get { return matchlist.Count; } }

    public void Reset()
    {
        matchlist.Clear();
        isMatch = false;
    }

    public void AddTilePosition(int x, int y)
    {
        matchlist.Add(new Point2D(x, y));
    }

    public void AddTilePosition(Point2D pos)
    {
        matchlist.Add(pos);
    }

    public void RemoveLast()
    {
        matchlist.RemoveAt(matchlist.Count-1);
    }

    public void CopyMatchList( MatchList input )
    {
        input = matchlist; // 내용을 복사
    }
} 


}