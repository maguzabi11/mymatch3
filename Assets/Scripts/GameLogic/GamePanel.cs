using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Match3
{
    using MatchList = List<Point2D>;

    public class GamePanel
    {
        public Tile[,] tiles;

        int numRow;
        int numCol;

        public int NumRow { get { return numRow; } }
        public int NumCol { get {return numCol; } }

        public int GetLastIndexRow() { return numRow -1; }
        public int GetLastIndexCol() { return numCol -1; }

        List<int> typeList = new List<int> {1,2,3,4,5};

        [Inject]
        TileBuilder _tilebuilder;

        [Inject]
        Tile.Factory _factory;

        [Inject]
        ScoreManager _scoreManager;

        SignalBus _signalBus;

        [Inject]
        MatchingChecker _matchingChecker;

        [Inject]
        public void Constructor(SignalBus signalBus)
        {
            _signalBus = signalBus;
            _signalBus.Subscribe<FillTileSignal>(OnFillTileSignal);
            _signalBus.Subscribe<TileDropSignal>(OnTileDropSignal);
            _signalBus.Subscribe<EndTileAttractionSignal>(OnEndAttractTileSignal);            
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

        public bool IsSwapable(int row, int col) // 두 지점을 받는 형태 고려...
        {
            // 1. 선형매치
            // 2. 2x2
            return IsMatch3Tile(row, col) || _matchingChecker.IsMatch2by2(row, col, useMatchinfo: false);
        }

        public bool IsMatch2by2(int row, int col)
        {
            return _matchingChecker.IsMatch2by2(row, col, useMatchinfo: true);
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

        // 매칭 타일 찾기
        // 1. 전체 검사
        // 2. 특정 위치에서 매칭 검사
        public int FindAllMatches()
        {
            return _matchingChecker.FindAllMatches();
        }

        public void ResetFindPropertyForTiles()
        {
            // 검사 속성 리셋
            for (int i = 0; i < numRow; i++)
                for (int j = 0; j < numCol; j++)
                    if(tiles[i, j] != null)
                        tiles[i, j].ResetFound();
        }

   
        public void OutputMatches()
        {
            _matchingChecker.OutputMatches();
        }

        public bool IsExistRemover(MatchType remover)
        {
            return _matchingChecker.IsRemoveType(remover);
        }

        public void ProcessMatchTiles()
        {
            var output = new StringBuilder();

            _nSendDeleteSignal = 0; // 생성과 제거가 공동으로 처리될 예정
            _nAttractTileSignal = 0;
            var matches = _matchingChecker.GetMatchInfo();
            for (int i = 0; i < matches.Count; i++)
            {
                output.AppendFormat($"{i}:");
                MatchList list = matches[i].matchlist;

                // 특수 타일 생성
                if (matches[i].isCreationTile)
                    CreateSpecialTile(output, matches[i]);
                else
                    DeleteMatch3Tiles(output, list);
            }

            if (matches.Count > 0)
            {
                Debug.LogFormat(output.ToString());
                CalculateScore(matches);
            }
        }

        private void CreateSpecialTile(StringBuilder output, MatchInfo matchinfo)
        {
            foreach (Point2D pos in matchinfo.matchlist)
            {
                var tile = tiles[pos.row, pos.col];
                if (tile == null)
                    continue;

                if( matchinfo.creationPos.row == pos.row && matchinfo.creationPos.col == pos.col )
                    continue;

                var dstTile = tiles[matchinfo.creationPos.row, matchinfo.creationPos.col];
                _nAttractTileSignal++;
                //_signalBus.Fire(new StartTileAttractionSignal(tile, dstTile));
                 // 모든 타일이 받는다면 다른 방법을 생각 권장...
                // 왜냐하면 tile함수를 바로 호출해도 되는 구조기 때문.
                tile.Attract(dstTile);
                tiles[pos.row, pos.col] = null; // 

                output.AppendFormat($"[{pos.row},{pos.col}]");
            }
            // 생성 시점과 제거 시점 정할 것
        }

        private void DeleteMatch3Tiles(StringBuilder output, MatchList list)
        {
            foreach (Point2D pos in list)
            {
                var tile = tiles[pos.row, pos.col];
                if (tile == null)
                    continue;

                _signalBus.Fire(new TileDeleteSignal(tile));

                // 이제 타일의 효과를 발동시켜야 한다.
                // 영향받는 타일들이 중복될 수 있음 고려할 것

                _nSendDeleteSignal++;
                tiles[pos.row, pos.col] = null;

                output.AppendFormat($"[{pos.row},{pos.col}]");
            }
        }

        private void CalculateScore(List<MatchInfo> matches)
        {
            int getPoint = _scoreManager.Calculate(matches);
            Debug.LogFormat($"Point:{getPoint}");
            Debug.LogFormat($"Score:{_scoreManager.Score}");
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

                        // 다른 타입의 타일을 대입해 본다.
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

            bool isMatch = (IsSwapable(srcPos.row, srcPos.col) || IsSwapable(dstPos.row, dstPos.col));
            //  위 함수 대신 매치 정보를 포함하는

            src.MoveSwap(dst, !isMatch);
            
            if( isMatch )
            {
                // 애니메이션 처리 전에 검사
                src.SwapLocation(dst); // 순서 테스트.

                int cntMatch = _matchingChecker.FindMatches( new Point2D[] {srcPos, dstPos} );
                // src.SwapLocation(dst); //
                
                // 필요한 경우 시그널 생성
                Debug.LogFormat($"매치! ({cntMatch})");
                return true;
            }
            else
            {
                tiles[srcPos.row,srcPos.col] = src;
                tiles[dstPos.row,dstPos.col] = dst;
                //Debug.LogFormat("교환하지 않음. yoyo 발생");
                return false;
            }            
        }


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
        #region 시그널 처리 함수, 모든 이벤트가 처리되는 것을 확인 하기 위함
        int _nSendDeleteSignal;
        int _nSendDropSignal;

        int _nAttractTileSignal; 

        // Note: OnFillTileSignal과 OnTileDropSignal는 동시에 발생하면 안됨.
        // OnFillTileSignal -> OnTileDropSignal
        public void OnFillTileSignal(FillTileSignal signal)
        {
            //Debug.LogFormat($"nSendDeleteSignal: {_nSendDeleteSignal}");
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
                if( FindAllMatches() > 0) // TODO: 전체가 아니라 Drop한 것 중심으로 검사
                    ProcessMatchTiles();
                else 
                    TileInput.blockInput = false; // 의존 문제 고려
            }
        }

        private void OnEndAttractTileSignal(EndTileAttractionSignal signal)
        {
            _nAttractTileSignal--;
             if(_nAttractTileSignal == 0)
            {
                Debug.LogFormat("특수 타일 생성하기");
                // 타일을 생성하지 않고 속성을 변경하는 것을 처리
                // signal.tile.SetRemoverType(MatchType remover);
            }

            //Debug.Assert(_nAttractTileSignal < 0);
        }        

        #endregion
    }

}