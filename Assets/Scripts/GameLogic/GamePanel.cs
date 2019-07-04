using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Match3
{
    using MatchList = List<Point2D>;
    using Random = UnityEngine.Random;

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

    public class GamePanel
    {
        public Tile[,] tiles;

        int numRow;
        int numCol;

        public int NumRow { get { return numRow; } }
        public int NumCol { get {return numCol; } }

        public int GetLastIndexRow() { return numRow -1; }
        public int GetLastIndexCol() { return numCol -1; }

        List<MatchList> matches = new List<MatchList>();

        List<int> typeList = new List<int> {1,2,3,4};

        [Inject]
        TileBuilder _tilebuilder;

        [Inject]
        Tile.Factory _factory;

        SignalBus _signalBus;

        [Inject]
        public void Constructor(SignalBus signalBus)
        {
            _signalBus = signalBus;
            _signalBus.Subscribe<FillTileSignal>(OnFillTileSignal);
            _signalBus.Subscribe<TileDropSignal>(OnTileDropSignal);
        }

        public GamePanel()
        {}

        public GamePanel(int width, int height)
        {
            CreatePanel( width, height);
        }

        public void CreatePanel(int width, int height)
        {
            tiles = new Tile[width, height]; // 재할당
            numRow = width;
            numCol = height;
        }

        public void CreateTilesWithoutMatch()
        {
            // match 확인하는 카운팅
            int nCheckHori = 0;

            // 랜덤을 여러 번 시행하지 않기 위해 리스트로 관리
            List<int> tmpTypeList = new List<int>(typeList);
            List<int> removeTypeList = new List<int>();

            for(int i=0; i<numRow; i++)
            {   
                nCheckHori = 0;
                for(int j=0; j<numCol; j++)
                {
                    int type = tmpTypeList[Random.Range(0, tmpTypeList.Count)];
                    if (nCheckHori >= 2)
                    {
                        if (type == tiles[i, j - 1].Type && tiles[i, j - 1].Type == tiles[i, j - 2].Type)
                        {
                            tmpTypeList.Remove(type);
                            removeTypeList.Add(type);
                            type = tmpTypeList[Random.Range(0, tmpTypeList.Count)];
                            nCheckHori = 0;
                        }
                    }
                    nCheckHori++;

                    if (i >= 2)
                    {
                        if (type == tiles[i - 1, j].Type && tiles[i - 1, j].Type == tiles[i - 2, j].Type)
                        {
                            tmpTypeList.Remove(type);
                            removeTypeList.Add(type);
                            type = tmpTypeList[Random.Range(0, tmpTypeList.Count)];
                        }
                    }

                    if (removeTypeList.Count > 0)
                    {
                        tmpTypeList.AddRange(removeTypeList);
                        removeTypeList.Clear();
                    }

                    // 
                    CreateTile(i, j, type);
                }
            }

        }

        private void CreateTile(int i, int j, int type)
        {
            var newTile = _factory.Create(type, new Point2D(i, j));//
            tiles[i, j] = newTile;
            if (_tilebuilder != null)
                _tilebuilder.BindTileResource(newTile);
        }

        public void CreateTileWithoutRc(int i, int j, int type)
        {
            //var newTile = _factory.Create(type, new Point2D(i, j));//
            tiles[i, j] = new Tile(type, new Point2D(i, j));
        }

        public void OutputTiles()
        {
            StringBuilder Line = new StringBuilder();

            for (int i = 0; i < numRow; i++)
            {
                for (int j = 0; j < numCol; j++)
                {
                    if( tiles[i, j] != null )
                        Line.Append($"{tiles[i, j].Type}");
                    else
                        Line.Append("-");

                    if(j < numCol)
                        Line.Append(" ");
                }
                Debug.Log(Line.ToString());
                Line.Clear();
            }      

        }

        // helper 
        public bool IsSameLeft(int row, int col, int offset)
        {
            int left = col - 1 - offset;
            return (left >= 0 && tiles[row, col].Type == tiles[row, left].Type);
        }

        public bool IsSameRight(int row, int col, int offset)
        {
            int right = col + 1 + offset;
            return (right < numCol && tiles[row, col].Type == tiles[row, right].Type);
        }

        public bool IsSameUp(int row, int col, int offset)
        {
            int up = row - 1 - offset;
            return (up >= 0 && tiles[row, col].Type == tiles[up, col].Type);
        }

        public bool IsSameDown(int row, int col, int offset)
        {
            int down = row + 1 + offset;
            return (down < numRow && tiles[row, col].Type == tiles[down, col].Type);
        }

        public Tile GetLeftTile(int row, int col, int offset)
        {
            int left = col - 1 - offset;
            return (left >= 0) ? tiles[row, left] : null;
        }
        public Tile GetRightTile(int row, int col, int offset)
        {
            int right = col + 1 + offset;
            return (right < numCol) ? tiles[row, right] : null;
        }
        public Tile GetUpTile(int row, int col, int offset)
        {
            int up = row - 1 - offset;
            return (up >= 0) ? tiles[up, col] : null;
        }
        public Tile GetDownTile(int row, int col, int offset)
        {
            int down = row + 1 + offset;
            return (down < numRow) ? tiles[down, col] : null;
        }                        

        public bool IsMatch3Tile(int row, int col)
        {
            // 가로 검사
            if(IsSameLeft(row, col, 0))
            {
                if (IsSameLeft(row, col, 1))
                    return true;
                else if (IsSameRight(row, col, 0))
                    return true;
            }
            else if(IsSameRight(row, col, 0))
            {
                if (IsSameRight(row, col, 1))
                    return true;
            }

            // 세로 검사
            if(IsSameUp(row, col, 0))
            {
                if (IsSameUp(row, col, 1))
                    return true;
                else if (IsSameDown(row, col, 0))
                    return true;
            }
            else if(IsSameDown(row, col, 0))
            {
                if (IsSameDown(row, col, 1))
                    return true;
            }

            return false;
        }

        // 검색에 필요한 정보
        class FindMatchInfo
        {
            public MatchList matchlist = new MatchList();

            public bool isMatch = false;

            public int Length
            {
                get { return matchlist.Count; }
            }

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

        public int FindMatches(Point2D[] poslist)
        {
            ResetSearch();
            FindMatchInfo findinfo = new FindMatchInfo();

            for (int i = 0; i < poslist.Length; i++)
                FindMatchingTiles(poslist[i].row, poslist[i].col, findinfo);

            return matches.Count;
        }


        // 매칭 타일 찾기
        // 1. 전체 검사
        // 2. 특정 위치에서 매칭 검사
        public int FindAllMatches()
        {
            ResetSearch();
            FindMatchInfo findinfo = new FindMatchInfo();

            for (int i = 0; i < numRow; i++)
                for (int j = 0; j < numCol; j++)
                    FindMatchingTiles(i, j, findinfo);

            return matches.Count;
        }

        private void ResetSearch()
        {
            // 검사 속성 리셋
            matches.Clear();
            for (int i = 0; i < numRow; i++)
                for (int j = 0; j < numCol; j++)
                    tiles[i, j].ResetSearch();
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
                num = numCol-1;
                compareTile1 = IsSameLeft;
                compareTile2 = IsSameRight;
                getTile1 = GetLeftTile;
                getTile2 = GetRightTile;
            }
            else
            {
                num = numRow-1;
                compareTile1 = IsSameUp;
                compareTile2 = IsSameDown;
                getTile1 = GetUpTile;
                getTile2 = GetDownTile;
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


        private void FindMatchingTiles(int row, int col, FindMatchInfo findinfo)
        {
            Tile baseTile = tiles[row, col];
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

        // 모든 인접을 검사하는 함수 : 현재 호출하지 않음
        private bool FindMatchingTile(int row, int col, FindMatchInfo findinfo)
        {
            Tile baseTile = tiles[row, col];
            if (baseTile.IsChecked)
                return false;
            else
                baseTile.IsChecked = true;

            findinfo.AddTilePosition(row, col);

            // 3 매치 확인
            if( !findinfo.isMatch )
                findinfo.isMatch = IsMatch3Tile(row,  col);

            // 검사 범위 인접 4개
            // 상
            int up = row - 1;
            if ( up >= 0 && IsSameTile(up, col, baseTile))
                FindMatchingTile(up, col, findinfo);

            // 하
            int down = row + 1;
            if ( down < numRow && IsSameTile(down, col, baseTile))
                FindMatchingTile(down, col, findinfo);
                

            // 좌
            int left = col - 1;
            if ( left >= 0 && IsSameTile(row, left, baseTile))
                FindMatchingTile(row, left, findinfo);
                

            // 우
            int right = col + 1;
            if ( right < numCol && IsSameTile(row, right, baseTile))
                FindMatchingTile(row, right, findinfo);

            return false;
        }

        //
        private bool IsSameTile(int x, int y, Tile baseTile)
        {
            Tile targetTile = tiles[x, y];
            return (targetTile.IsChecked == false && baseTile.Type == targetTile.Type);
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

        public void DeleteMatchTiles()
        {
            var output = new StringBuilder();

            _nSendDeleteSignal = 0;
            for(int i=0; i<matches.Count; i++)
            {
                MatchList list = matches[i];
                output.AppendFormat($"{i}:");
                foreach(Point2D pos in list)
                {
                    var tile = tiles[pos.row, pos.col];
                    _signalBus.Fire(new TileDeleteSignal(tile));
                    _nSendDeleteSignal++;

                    tiles[pos.row, pos.col] = null;

                    output.AppendFormat($"[{pos.row},{pos.col}]");
                }
            }

            // test
            if( matches.Count > 0)
            {
                Debug.LogFormat(output.ToString());
            }

        }

        private bool IsEmptyPlace(int x, int y)
        {
             return tiles[x, y] == null;
        }

        public void FillTilesToEmptyPlace()
        {
            _nSendDropSignal = 0;

            List<Point2D> createList = new List<Point2D>();
            //1. 빈 자리로 이동 (밑으로 이동 규칙)
            for(int i=0; i<numCol; i++)
            {
                int numEmpty = MoveTileToEmptySpace(i);
                if (numEmpty > 0)
                {
                    CreateTileAndSetPosition(numEmpty, i);
                    
                    for(int row=0; row<numEmpty; row++)
                        createList.Add(new Point2D(row, i));
                }
            }

            // 매치 가능 없을 때 처리는 여기로
            int num = NumMatchable();
            Debug.LogFormat($"matchable count: {num}");
            if(num == 0 )
            {
                List<int> tmpTypeList = new List<int>();
                bool isFinished = false;
                // 확인 후 함수로 추출... (params: in createList )
                foreach( var pos in createList)
                {
                    tmpTypeList.AddRange(typeList);

                    // 매치가능한 타일을 먼저 알아내고 
                    while( tmpTypeList.Count > 0)
                    {
                        int type = tmpTypeList[Random.Range(0, tmpTypeList.Count)];
                        bool isDifferant = (tiles[pos.row, pos.col].Type != type);
                        if( isDifferant && IsMatchablePlace(pos.row, pos.col, type) )
                        {
                            Debug.LogFormat($"try to find the matchable type:[{pos.row},{pos.col}] {type}");
                            
                            // 만들었던 타일을 지우고 다시 만든다.
                            ChangeTile(pos.row, pos.col, type);
                            isFinished = true;
                            break;
                        }
                        else
                        {
                            tmpTypeList.Remove(type);
                        }
                    }

                    if(isFinished)
                        break;
                }
            }            

            createList.Clear();
        }

        private void ChangeTile(int row, int col, int type)
        {
            _tilebuilder.ChangeTileType(tiles[row, col], type);
        }

        private int MoveTileToEmptySpace(int col)
        {
            bool bCopy = false;
            int itoCopy = numRow-1;
            int numEmpty = 0;
            for (int j = itoCopy; j >= 0; j--) // 바닥에서 위로
            {
                if (bCopy)
                {
                    if (!IsEmptyPlace(j, col))
                    {
                        // 움직일 칸 수: iCopy - j
                        tiles[itoCopy, col] = tiles[j, col];
                        tiles[itoCopy, col].MoveTo(itoCopy, col);
                        tiles[j, col] = null;

                        _nSendDropSignal++;
                        itoCopy--;
                    }
                    else
                        numEmpty++;
                }
                else
                {
                    if (IsEmptyPlace(j, col))
                    {
                        bCopy = true;
                        itoCopy = j;
                        numEmpty++;
                    }
                }
            }

            return numEmpty;
        }

        private void CreateTileAndSetPosition(int numEmpty, int col)
        {
            // 남은 빈 자리 확인
            Debug.LogFormat("{0}열 빈 자리 ", col);
            int iHoriCenter = NumCol / 2;
            int iVertCenter = NumRow / 2;
            float xOffset = GetOffset(NumCol);
            float yOffset = GetOffset(NumRow);
            float startOffset = numEmpty + 1;
            int row = 0;

            while (row < numEmpty)
            {
                int tileType = GetRemovableTile(row, col);
                CreateTile(row, col, typeList[tileType]);
                
                float x = col - iHoriCenter + xOffset;
                float y = iVertCenter - row + yOffset;
                float fromY = y + startOffset;
                tiles[row, col].SetPosition(x, fromY);
                tiles[row, col].MoveTo(x, y);

                Debug.LogFormat($"[{row},{col}]");
                _nSendDropSignal++;
                row++;
            }
        }

        int GetRemovableTile(int row, int col)
        {
            // 패턴으로 체크

            // 없거나 하면 랜덤생성
            return Random.Range(0, typeList.Count);
        }

        // 홀수, 짝수에 따라 배치 조정
        Func<int, float> GetOffset = (v) => { return (v%2 == 0) ? 0.5f : 0f;};

        public void SetTilePosition()
        {
            if(NumCol == 0 || NumRow == 0 )
                return;

            int iHoriCenter = NumCol / 2;
            int iVertCenter = NumRow / 2;
            float xOffset = GetOffset(NumCol);
            float yOffset = GetOffset(NumRow);
            for( int i=0; i<NumRow; i++)
            {
                for( int j=0; j<NumCol; j++)
                {
                    float x = j-iHoriCenter + xOffset;
                    float y = iVertCenter-i + yOffset;
                    tiles[i,j].SetPosition(x, y);
                }
            }
        }

        public SwapTileResult TrySwapTile(Tile tile, TileMovement move)
        {
            SwapTileResult result;
            Tile adjTile = GetAdjacentTile(tile, move);
            if( adjTile != null ) 
                result = SwapTile(tile, adjTile) ? SwapTileResult.Success : SwapTileResult.Failure;
            else
                return SwapTileResult.NoTile;

            // 리턴 false: 인접 없는 경우, 매치가 발생되지 않는 경우
            return result;
        }

        public Tile GetAdjacentTile(Tile tile, TileMovement move)
        {
            var location = tile.GetLocation();
            Tile adjTile = null;
            if(move == TileMovement.Up && location.row > 0) 
                adjTile = tiles[location.row-1, location.col];
            else if(move == TileMovement.Down && location.row < GetLastIndexRow() )
                adjTile = tiles[location.row+1, location.col];
            else if(move == TileMovement.Left && location.col > 0)
                adjTile = tiles[location.row, location.col-1];
            else if(move == TileMovement.Right && location.col < GetLastIndexCol())
                adjTile = tiles[location.row, location.col+1];
            return adjTile;
        }

        public bool SwapTile(Tile src, Tile dst )
        {
            var srcPos = src.GetLocation();
            var dstPos = dst.GetLocation();

            // 둘이 바뀌었다고 가정하고 두 인접 타일에 대해 확인을 해야한다.
            tiles[srcPos.row,srcPos.col] = dst;
            tiles[dstPos.row,dstPos.col] = src;

            bool isMatch = (IsMatch3Tile(srcPos.row, srcPos.col) || IsMatch3Tile(dstPos.row, dstPos.col));
            //  위 함수 대신 매치 정보를 포함하는

            src.MoveSwap(dst, !isMatch);
            
            if( isMatch )
            {
                // 애니메이션 처리 전에 검사
                int cntMatch = FindMatches( new Point2D[] {srcPos, dstPos} );
                src.SwapLocation(dst);
                // 필요한 경우 시그널 생성
                Debug.LogFormat("매치!");
                return true;
            }
            else
            {
                tiles[srcPos.row,srcPos.col] = src;
                tiles[dstPos.row,dstPos.col] = dst;
                Debug.LogFormat("교환하지 않음. yoyo 발생");
                return false;
            }            
        }

        public bool IsInRangeWidth(int i)
        {
            return (i>=0) && (i<numRow);
        }
        public bool IsInRangeHeigth(int i)
        {
            return (i>=0) && (i<numCol);
        }

        // 3. 특정 타일 제거
        // - 매칭 타일
        // - 가로 세로 라인 제거

        // 6. 콤보

        // todo 
        // - tile pool 필요

        // 매치 가능 수
        public int NumMatchable()
        {
            //List<Point2D> list = new List<Point2D>();
            int count = 0;
            for (int i = 0; i < numRow; i++)
            {
                for (int j = 0; j < numCol; j++)
                {
                    if(tiles[i,j] != null && IsMatchablePlace(i, j, tiles[i,j].Type))
                    {
                        //list.Add(new Point2D(i,j));
                        count++;
                    }
                } 
            }
            return count;
        }

        // 현재 위치에서 매치 할 수 있는지 여부를 확인.
        public bool IsMatchablePlace(int row, int col, int type)
        {
            var pattern = MatchablePattern.Instance;
            // 패턴 쪽으로 이동할듯
            for(int p=0; p<pattern.Length; p++)
            {
                var point1 = pattern.GetPatternPos(p, 0, row, col);
                var point2 = pattern.GetPatternPos(p, 1, row, col);
                if( Isbound(point1) && Isbound(point2))
                {
                    if(tiles[point1.row, point1.col] == null || tiles[point2.row, point2.col] == null )
                        continue;

                    // 모두 같은 타입인지 확인 (코드가 길다...)
                    if(type == tiles[point1.row, point1.col].Type &&
                        type == tiles[point2.row, point2.col].Type)
                        return true;
                }                    
            }
            return false;
        }

        public bool Isbound(Point2D pos)
        {
            if( pos.col < 0 || pos.col >= NumCol) return false;
            if( pos.row < 0 || pos.row >= NumRow) return false;

            return true;
        }
        

        ///
        #region 시그널 처리 함수
        int _nSendDeleteSignal;
        int _nSendDropSignal;

        // Note: OnFillTileSignal과 OnTileDropSignal는 동시에 발생하면 안됨.
        // OnFillTileSignal -> OnTileDropSignal
        public void OnFillTileSignal(FillTileSignal signal)
        {
            Debug.LogFormat($"nSendDeleteSignal: {_nSendDeleteSignal}");
            _nSendDeleteSignal--;
            if( _nSendDeleteSignal == 0)
            {
                Debug.LogFormat("지워진 타일 채우고 떨어뜨리기");
                FillTilesToEmptyPlace();
            }
        }

        private void OnTileDropSignal(TileDropSignal signal)
        {
            _nSendDropSignal--;
            if(_nSendDropSignal == 0)
            {
                Debug.LogFormat("다시 매치 검사하기");
                if( FindAllMatches() > 0)
                    DeleteMatchTiles();
                else 
                    TileInput.blockInput = false; // 의존 문제 고려
            }
        }

        #endregion
    }

}