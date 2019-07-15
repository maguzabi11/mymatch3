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
    FindMatchInfo findinfo = new FindMatchInfo();

    List<FindMatchInfo> matchInfos = new List<FindMatchInfo>(); // 추가 정보를 포함한 매치 리스트
    

    GamePanel _gamepanel;

    [Inject]
    public void Constructor(GamePanel gamepanel)
    {
        _gamepanel = gamepanel;
    }

    public List<FindMatchInfo> GetMatchInfo()
    {
        return matchInfos;
    }

    public int FindMatches(Point2D[] poslist)
    {
        ResetSearch();

        for (int i = 0; i < poslist.Length; i++)
            FindMatchingTiles(poslist[i].row, poslist[i].col, findinfo);

        return matchInfos.Count;
    }

    public int FindAllMatches()
    {
        ResetSearch();
        
        for (int i = 0; i < _gamepanel.NumRow; i++)
            for (int j = 0; j < _gamepanel.NumCol; j++)
                FindMatchingTiles(i, j, findinfo);

        return matchInfos.Count;
    }

    private void ResetSearch()
    {
        matchInfos.Clear();
        _gamepanel.ResetSearch();
        findinfo.Reset();
    }

    
    /*
    재검토
    - 패턴 데이터 검사
    - 결과는 매치와 매칭의 종류(new)
     */
    private void FindMatchingTiles(int row, int col, FindMatchInfo findinfo)
    {
        Tile baseTile = _gamepanel.tiles[row, col];
        if (baseTile.IsChecked)
            return;
        else
            baseTile.IsChecked = true;

        // 검사 기준을 먼저 둔다
        findinfo.AddTilePosition(row, col);

        FindMatch( row, col, findinfo, FindDirection.Horizon);
        FindMatch( row, col, findinfo, FindDirection.Vertical);

        if( findinfo.isMatch )
            matchInfos.Add(new FindMatchInfo(findinfo));

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

        List<Tile> matchCandidates = new List<Tile>(); // 매치 확인을 위해 임시로 저장
        bool bStopLeft = false;
        bool bStopRight = false;
        for (int i = 0; i < num; i++)
        {
            if(!bStopLeft && compareTile1(row, col, i))
                matchCandidates.Add(getTile1(row, col, i));
            else if(bStopRight)
                break;
            else
                bStopLeft = true;
 
            if(!bStopRight && compareTile2(row, col, i))
                matchCandidates.Add(getTile2(row, col, i));
            else if(bStopLeft)
                break;
            else   
                bStopRight = true;
        } 
        
        if(matchCandidates.Count >= 2)
        {
            findinfo.isMatch = true;
            foreach (var tile in matchCandidates)
            {
                if (tile.IsMatched == false)
                {
                    findinfo.AddTilePosition(tile.GetLocation());
                    tile.MarkSearch();
                }
            }

            // working
            CheckRemover(findinfo, dir);
        }
        matchCandidates.Clear();
    }

    private static void CheckRemover(FindMatchInfo findinfo, FindDirection dir)
    {
        // cross 매칭 확인
        if (dir == FindDirection.Horizon)
            findinfo.isHorizonMatch = true;
        else
            findinfo.isVerticalMatch = true;

        if (findinfo.isCrossMatch)
        {
            findinfo.Remover = RemoveType.Bomb;
            Debug.LogFormat($"[Remover] {RemoveType.Bomb.ToString()}생성");
        }
        else
        {
            if (findinfo.Length == 4)
            {
                if (findinfo.isHorizonMatch)
                {
                    findinfo.Remover = RemoveType.VerticalRemover;
                    Debug.LogFormat($"[Remover]{RemoveType.VerticalRemover.ToString()}");
                }
                else if (findinfo.isVerticalMatch)
                {
                    findinfo.Remover = RemoveType.HorizonRemover;
                    Debug.LogFormat($"[Remover]{RemoveType.HorizonRemover.ToString()}");
                }
            }
            else if (findinfo.Length == 5) // cross확인?
            {
                findinfo.Remover = RemoveType.KindRemover;
                Debug.LogFormat($"[Remover]타입지우기");
            }
        }
    }

    public void OutputMatches()
    {
        StringBuilder str = new StringBuilder(64);
        for(int i=0; i<matchInfos.Count; i++)
        {
            MatchList list = matchInfos[i].matchlist;
            Debug.LogFormat("match[{0}]", i);

            foreach(Point2D pos in list)
                str.AppendFormat($"[{pos.row},{pos.col}] ");
            str.AppendLine();
            Debug.Log(str.ToString());
            str.Clear();
        }
    }
}

public class FindMatchInfo
{
    public MatchList matchlist;

    public bool isMatch = false;

    public bool isHorizonMatch;
    public bool isVerticalMatch;

    public bool isCrossMatch
    { get { return isHorizonMatch && isVerticalMatch;}}

    // remover 생성 정보 필요
    public RemoveType Remover;

    public int Length { get { return matchlist.Count; } }

    public void Reset()
    {
        isHorizonMatch = false;
        isVerticalMatch = false;
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

    public FindMatchInfo()
    {
        matchlist = new MatchList();
    }

    public FindMatchInfo(FindMatchInfo info)
    {
        matchlist = new MatchList(info.matchlist);
        isMatch = info.isMatch;
        isHorizonMatch = info.isHorizonMatch;
        isVerticalMatch = info.isVerticalMatch;   
        Remover = info.Remover;
    }
} 


}