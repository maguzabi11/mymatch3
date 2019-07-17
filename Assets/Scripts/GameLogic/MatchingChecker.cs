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
    MatchInfo findinfo = new MatchInfo();

    List<MatchInfo> matchInfos = new List<MatchInfo>(); // 추가 정보를 포함한 매치 리스트
    

    GamePanel _gamepanel;

    [Inject]
    public void Constructor(GamePanel gamepanel)
    {
        _gamepanel = gamepanel;
    }

    public List<MatchInfo> GetMatchInfo()
    {
        return matchInfos;
    }

    public bool IsRemoveType(RemoveType remover)
    {
        for(int i=0; i<matchInfos.Count; i++ )
        {
            if( matchInfos[i].Remover == remover )
                return true;
        }
        return false;
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
    private void FindMatchingTiles(int row, int col, MatchInfo findinfo)
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
            matchInfos.Add(new MatchInfo(findinfo));

        findinfo.Reset();
    }

    delegate bool compareTile(int row, int col, int offset); // isSame~
    delegate  Tile getNeighborTile(int row, int col, int offset);

    void FindMatch(int row, int col, MatchInfo findinfo, FindDirection dir)
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

            if (dir == FindDirection.Horizon)
                findinfo.direction |= MatchDir.Horizon;
            else
                findinfo.direction |= MatchDir.Vertical;

            foreach (var tile in matchCandidates)
            {
                if (tile.IsMatched == false)
                {
                    findinfo.AddTilePosition(tile.GetLocation());
                    tile.MarkSearch();
                }
                else
                {
                    Debug.LogFormat("교차점[{0},{1}] 발견", tile.row, tile.col);
                    // 이전 데이터와 합치기
                    for(int i=0; i<matchInfos.Count; i++)
                    {
                        var oldInfo = matchInfos[i];
                        if(oldInfo.Find(tile.row, tile.col))
                        {
                            findinfo.direction |= oldInfo.direction;
                            findinfo.AddTilePosition(oldInfo);
                            matchInfos.RemoveAt(i);
                            break;
                        }
                    }
                    
                }
            }
            
            // 이 부분을 함수 바깥으로 뺄 수 있나?
            CheckRemover(findinfo);
        }
        matchCandidates.Clear();
    }

    private static void CheckRemover(MatchInfo findinfo)
    {
        if (findinfo.isCrossMatch)
        {
            findinfo.Remover = RemoveType.Bomb;
            Debug.LogFormat($"[Remover] {RemoveType.Bomb.ToString()}생성");
        }
        else
        {
            if (findinfo.Length == 4)
            {
                if (findinfo.direction == MatchDir.Horizon)
                {
                    findinfo.Remover = RemoveType.VerticalRemover;
                    Debug.LogFormat($"[Remover]{RemoveType.VerticalRemover.ToString()}");
                }
                else if (findinfo.direction == MatchDir.Vertical)
                {
                    findinfo.Remover = RemoveType.HorizonRemover;
                    Debug.LogFormat($"[Remover]{RemoveType.HorizonRemover.ToString()}");
                }
            }
            else if (findinfo.Length == 5) 
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

public class MatchInfo
{
    public MatchList matchlist;

    public bool isMatch = false;

    public MatchDir direction;

    public bool isCrossMatch
    { get { return direction == MatchDir.Cross;}}

    // remover 생성 정보 필요
    public RemoveType Remover;

    public int Length { get { return matchlist.Count; } }

    public void Reset()
    {

        direction = MatchDir.None;
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
        Remover = info.Remover;
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