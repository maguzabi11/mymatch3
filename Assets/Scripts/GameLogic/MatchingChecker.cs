using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using MatchList = System.Collections.Generic.List<Match3.Point2D>;

namespace Match3
{

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

    public bool IsRemoveType(MatchType remover)
    {
        for(int i=0; i<matchInfos.Count; i++ )
        {
            if( matchInfos[i].matchType == remover )
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
        findinfo.SetCreationPos(row, col);

        FindMatch( row, col, findinfo, FindDirection.Horizon);
        FindMatch( row, col, findinfo, FindDirection.Vertical);
        if( findinfo.isMatch )
            AddCreatetionMatchInfo(new MatchInfo(findinfo)); 

        // 레퍼런스 게임 참조 
        if( findinfo.isMatch == false || findinfo.matchType == MatchType.Normal )
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
        var matchedInfo = findMatchInfo(tile.row, tile.col);
        if( matchedInfo == null )
            return;

        Debug.LogFormat("교차점[{0},{1}] 발견", tile.row, tile.col);
        
        if (matchedInfo.isLinearMatch) // 교차되면서 매치를 하나로 합치기
        {
            findinfo.direction |= matchedInfo.direction;
            findinfo.AddTilePosition(matchedInfo);
            matchInfos.Remove(matchedInfo);
            findinfo.creationPos = tile.GetLocation();
        }
        // 타일 위치의 중복을 허용하면서 현재는 이에 대한 검증이 안되어 있음.
        else if (matchedInfo.matchType == MatchType.Butterfly)
        {
            Debug.LogFormat("나비 발견.");
            if( findinfo.isCreationTile && findinfo.matchType != MatchType.Butterfly )
            {
                resetMatchedInfo(matchedInfo);
                findinfo.AddTilePosition(tile.GetLocation());
                matchInfos.Remove(matchedInfo);
            }
        }
        else
        {
            findinfo.AddTilePosition(tile.GetLocation());
        }
    }

    private void resetMatchedInfo(MatchInfo matchedInfo)
    {
        for( int i=0; i<matchedInfo.matchlist.Count; i++)
        {
            var pos = matchedInfo.matchlist[i];
            var tile = _gamepanel.tiles[pos.row, pos.col];
            if( tile != null)
                tile.ResetFound();
        }
    }

    private MatchInfo findMatchInfo(int row, int col)
    {
        // row, col가 들어있는 매치 정보는 중복하지 않는다고 가정 - 주의: 현재 중복 존재
        for (int i = 0; i < matchInfos.Count; i++)
        {
            if (matchInfos[i].Find(row, col))
                return matchInfos[i];
        }
        return null;
    }

    private static void CheckRemover(MatchInfo findinfo)
    {
        if (findinfo.isCrossMatch)
        {
            // 레퍼런스 확인이 필요
            // 크로스지만 개수가 6개가 넘는다면 폭탄이 아닌 파인애플로 만들어지는지 
            findinfo.matchType = MatchType.Bomb;
            Debug.LogFormat($"[Remover] {MatchType.Bomb.ToString()}생성");
        }
        else
        {
            if (findinfo.Length == 4)
            {
                if (findinfo.direction == MatchDir.Horizon)
                {
                    findinfo.matchType = MatchType.Vertical4;
                    Debug.LogFormat($"[Remover]{MatchType.Vertical4.ToString()}");
                }
                else if (findinfo.direction == MatchDir.Vertical)
                {
                    findinfo.matchType = MatchType.Horizon4;
                    Debug.LogFormat($"[Remover]{MatchType.Horizon4.ToString()}");
                }
            }
            else if (findinfo.Length == 5) 
            {
                findinfo.matchType = MatchType.KindRemover;
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
                    matchinfo.matchType = MatchType.Butterfly;
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
        if (curtile == null) return false;

        List<Tile> matchCandidates = new List<Tile>();
        var pattern = Square22Pattern.Instance;
        bool isFound = isMatchedPattern(pattern, curtile, matchCandidates);
        if (isFound)
        {
            findinfo.matchType = MatchType.Butterfly;
            findinfo.isMatch = true;

            // makeMatch 내의 루프 참조 
            foreach (var tile in matchCandidates)
            {
                bool bAddPostion = !tile.IsMatched;
                if (tile.IsMatched)
                {
                    var matchedInfo = findMatchInfo(tile.row, tile.col);
                    if (matchedInfo == null)
                        continue;

                    Debug.LogFormat("교차점[{0},{1}] 발견", tile.row, tile.col);
                    if (matchedInfo.matchType == MatchType.Normal)
                    {
                        // 해당 좌표만 제거
                        matchedInfo.matchlist.Remove(tile.GetLocation());
                        bAddPostion = true;
                    }
                }

                if (bAddPostion)
                {
                    findinfo.AddTilePosition(tile.GetLocation());
                    tile.MarkFound();
                }                
            }

            AddCreatetionMatchInfo(new MatchInfo(findinfo));
        }
        matchCandidates.Clear();
        return isFound;
    }

    private bool isMatchedPattern(PatternBase pattern,Tile curtile, List<Tile> matchCandidates)
    {
        bool isFound = false;
        for (int p = 0; p < pattern.Length; p++)
        {
            matchCandidates.Clear();
            int nblock = pattern.GetPatternLength(p);
            int i = 0;
            for (; i < nblock; i++)
            {
                var tilepos = pattern.GetPatternPos(p, i, curtile.row, curtile.col);
                if (_gamepanel.Isbound(tilepos) == false)
                    break;

                var tile = _gamepanel.tiles[tilepos.row, tilepos.col];
                if (tile == null || tile.Type != curtile.Type)
                    break;

                matchCandidates.Add(tile); // 패턴이 모두 맞아야 추가
            }

            isFound = (i == nblock);
            if (isFound)
                break;
        }

        return isFound;
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

    // 실험
    // 생성해야할 타일이 있는 매치 정보 앞에 3매치가 있는 경우
    // 3매치 앞에 삽입하도록 
    public void AddCreatetionMatchInfo(MatchInfo item)
    {
        if( matchInfos.Count == 0 )
        {
            matchInfos.Add(item);
            return;
        }
        
        int last = matchInfos.Count - 1;
        if(!matchInfos[last].isCreationTile)
            matchInfos.Insert(last, item);
        else
            matchInfos.Add(item);
    }
}


}