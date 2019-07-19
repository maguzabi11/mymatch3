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

    public void ResetSearch()
    {
        // matchInfos. 2by2일 때 문제가 됨.
        // 리셋 하는 부분을 위치, 조건을 수정해야한다.
        _gamepanel.ResetFindPropertyForTiles();
        findinfo.Reset();
        matchInfos.Clear();
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
        else        
            IsMatch2by2(row, col);

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
        if (dir == FindDirection.Horizon)
        {
            num = _gamepanel.NumCol - 1;
            compareTile1 = _gamepanel.IsSameLeft;
            compareTile2 = _gamepanel.IsSameRight;
            getTile1 = _gamepanel.GetLeftTile;
            getTile2 = _gamepanel.GetRightTile;
        }
        else
        {
            num = _gamepanel.NumRow - 1;
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
            if (!bStopLeft && compareTile1(row, col, i))
                matchCandidates.Add(getTile1(row, col, i));
            else if (bStopRight)
                break;
            else
                bStopLeft = true;

            if (!bStopRight && compareTile2(row, col, i))
                matchCandidates.Add(getTile2(row, col, i));
            else if (bStopLeft)
                break;
            else
                bStopRight = true;
        }
        
        if (matchCandidates.Count >= 2)
            makeMatch(findinfo, dir, matchCandidates);
        matchCandidates.Clear();
    }

    // 발견된 매치 타일로 새로운 매치 정보 만들기
    private void makeMatch(MatchInfo findinfo, FindDirection dir, List<Tile> matchCandidates)
    {
        // 버그: 이제는 무조건 매치를 하면 안되고
        //  타일이 이미 만들어진 것과 겹친 부분이 있으면 
        //  우선 순위에 따라 어떻게 처리할지가 결정되어야 한다.
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
                tile.MarkFound();
            }
            else
                mergeMachedInfo(findinfo, tile);
        }

        // 이 부분을 함수 바깥으로 뺄 수 있나?
        CheckRemover(findinfo);
    }

    // 이미 매치된 타일에 대한 처리 과정
    private void mergeMachedInfo(MatchInfo findinfo, Tile tile)
    {
        // 해당 매치를 찾아, 이전 데이터와 합치기               
        for (int i = 0; i < matchInfos.Count; i++)
        {
            var oldInfo = matchInfos[i];
            if (oldInfo.Find(tile.row, tile.col))
            {
                Debug.LogFormat("교차점[{0},{1}] 발견", tile.row, tile.col);
                if (oldInfo.Remover == RemoveType.Normal)
                {
                    findinfo.direction |= oldInfo.direction;
                    findinfo.AddTilePosition(oldInfo);
                    matchInfos.RemoveAt(i);
                    break;
                }
                else if (oldInfo.Remover == RemoveType.Butterfly)
                {
                    // TODO: oldInfo.matchlist 가리키는 타일 속성을 리셋하기
                    Debug.LogFormat("나비 발견. 나비 제거");
                    findinfo.AddTilePosition(tile.GetLocation());
                    matchInfos.RemoveAt(i);
                    break;
                }
            }
        }
    }

    private MatchInfo findMatchInfo(int row, int col)
    {
        // row, col가 들어있는 매치 정보는 중복하지 않는다고 가정
        MatchInfo match = null;
        for (int i = 0; i < matchInfos.Count; i++)
        {
            if (matchInfos[i].Find(row, col))
            {
                match = matchInfos[i];
                break;
            }
        }

        return match;
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

    // useMatchinfo: true일 때 테스트 용도로 사용
    public bool IsMatch2by2(int row, int col, bool useMatchinfo)
    {
        if( useMatchinfo )
            findinfo.AddTilePosition(row, col);
        return IsMatch2by2Simple(row, col, useMatchinfo ? findinfo : null);
    }

    // 1. swap 가능 확인 용
    // 2. 매치 정보 기록 유무 필요
    private bool IsMatch2by2Simple(int row, int col, MatchInfo matchinfo)
    {
        var curtile = _gamepanel.tiles[row, col];
        if( curtile == null) return false;

        bool isFound = false;
        var pattern = Square22Pattern.Instance; // 추후 패턴 일반화
        for(int p=0; p<pattern.Length; p++)
        {
            int nblock = pattern.GetPatternLength(p);
            int i=0;
            for(; i<nblock; i++)
            {
                var tilepos = pattern.GetPatternPos(p, i, row, col);

                if( _gamepanel.Isbound(tilepos) == false )
                    break;

                var tile = _gamepanel.tiles[tilepos.row, tilepos.col];
                if(tile == null || tile.Type != curtile.Type)
                    break;

                if( matchinfo != null)
                    matchinfo.AddTilePosition(tilepos); // 패턴이 모두 맞아야 추가가 가능함...
            }

            isFound = (i == nblock);
            if(isFound)
            {
                if( matchinfo != null )
                {
                    // todo : 일반화를 위해서는 함수로 빼야함.
                    //matchinfo.AddTilePosition(curtile.GetLocation());
                    matchinfo.Remover = RemoveType.Butterfly;
                    matchinfo.isMatch = true;
                    // 생성위치도 필요
                    matchInfos.Add(new MatchInfo(matchinfo));                    
                }
                return true;
            }
        }
        return false;
    }

    //
    private bool IsMatch2by2(int row, int col)
    {
        var curtile = _gamepanel.tiles[row, col];
        if( curtile == null) return false;

        List<Tile> matchCandidates = new List<Tile>();
        bool isFound = false;
        var pattern = Square22Pattern.Instance; // 추후 패턴 일반화
        for(int p=0; p<pattern.Length; p++)
        {
            int nblock = pattern.GetPatternLength(p);
            int i=0;
            for(; i<nblock; i++)
            {
                var tilepos = pattern.GetPatternPos(p, i, row, col);
                if( _gamepanel.Isbound(tilepos) == false )
                    break;

                var tile = _gamepanel.tiles[tilepos.row, tilepos.col];
                if(tile == null || tile.Type != curtile.Type || tile.IsMatched)
                    break;

                matchCandidates.Add(tile); // 패턴이 모두 맞아야 추가가 가능함...
            }

            isFound = (i == nblock);
            if(isFound)
            {
                foreach (var tile in matchCandidates)
                {
                    findinfo.AddTilePosition(tile.GetLocation());
                    tile.MarkFound();
                }

                findinfo.Remover = RemoveType.Butterfly;
                findinfo.isMatch = true;

                // 생성위치도 필요
                matchInfos.Add(new MatchInfo(findinfo));
                
                matchCandidates.Clear();
                return true;
            }            
            matchCandidates.Clear();
        }
        return false;
    }

    // GamePanel에서 옮겨옴
    public bool IsMatchablePlace(int row, int col, int type)
    {
        var pattern = MatchablePattern.Instance;
        // 패턴 쪽으로 이동할듯
        for(int p=0; p<pattern.Length; p++)
        {
            var point1 = pattern.GetPatternPos(p, 0, row, col);
            var point2 = pattern.GetPatternPos(p, 1, row, col);
            if( _gamepanel.Isbound(point1) && _gamepanel.Isbound(point2))
            {
                var tile1 = _gamepanel.tiles[point1.row, point1.col];
                var tile2 = _gamepanel.tiles[point2.row, point2.col];
                if(tile1 == null || tile2 == null )
                    continue;

                // 모두 같은 타입인지 확인 (코드가 길다...)
                if(type == tile1.Type &&
                    type == tile2.Type)
                    return true;
            }                    
        }
        return false;
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