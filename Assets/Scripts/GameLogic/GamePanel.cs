﻿using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Match3
{
    using MatchList = List<Point2D>;
    using Random = UnityEngine.Random;

    public struct Point2D
    {
        public int x;
        public int y;

        public Point2D(int x, int y) : this()
        {
            this.x = x;
            this.y = y;
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

        public void CreateTilesWithoutMatch3()
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

            public void Reset()
            {
                matchlist.Clear();
                isMatch = false;
            }

            public void AddTilePosition(int x, int y)
            {
                matchlist.Add(new Point2D(x, y));
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
                FindMatchingTiles(poslist[i].x, poslist[i].y, findinfo);

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
                    tiles[i, j].IsChecked = false;
        }

        void FindHoriMatch(int row, int col, FindMatchInfo findinfo)
        {
            int cntMatch = 1;
            bool bStopLeft = false;
            bool bStopRight = false;
            for (int i = 0; i < numCol-1; i++)
            {
                if(!bStopLeft && IsSameLeft(row, col, i))
                {
                    cntMatch++;
                    findinfo.AddTilePosition(row, col - (i+1));
                    tiles[row, col - (i+1)].IsChecked = true;
                }
                else
                {
                    if(bStopRight) return;
                    bStopLeft = true;
                }

                if(!bStopRight && IsSameRight(row, col, i))
                {
                    cntMatch++;
                    findinfo.AddTilePosition(row, col + (i+1));
                    tiles[row, col + (i+1)].IsChecked = true;
                }
                else
                {
                    if(bStopLeft) return;
                    bStopRight = true;
                }

                if(cntMatch >= 3)
                    findinfo.isMatch = true;
            }   
        }

        void FindVertMatch(int row, int col, FindMatchInfo findinfo)
        {
            int cntMatch = 1;
            bool bStopUp = false;
            bool bStopDown = false;
            for (int i = 0; i < numRow-1; i++)
            {
                if(!bStopUp && IsSameUp(row, col, i))
                {
                    cntMatch++;
                    findinfo.AddTilePosition(row - (i+1), col);
                    tiles[row - (i+1), col].IsChecked = true;
                }
                else
                {
                    if(bStopDown) return;
                    bStopUp = true;
                }

                if(!bStopDown && IsSameDown(row, col, i))
                {
                    cntMatch++;
                    findinfo.AddTilePosition(row + (i+1), col);
                    tiles[row + (i+1), col].IsChecked = true;
                }
                else
                {
                    if(bStopUp) return;
                    bStopDown = true;
                }

                if(cntMatch >= 3)
                    findinfo.isMatch = true;
            }
        }

        private void FindMatchingTiles(int row, int col, FindMatchInfo findinfo)
        {
            Tile baseTile = tiles[row, col];
            if (baseTile.IsChecked)
                return;
            else
                baseTile.IsChecked = true;

            findinfo.AddTilePosition(row, col);
            
            FindHoriMatch( row, col, findinfo);
            FindVertMatch( row, col, findinfo);

            // 매치가 확인 된 순간
            if( findinfo.isMatch )
                    matches.Add(new MatchList(findinfo.matchlist)); // 리스트 복사해야할 듯.

            findinfo.Reset();
        }

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
                    str.AppendFormat($"[{pos.x},{pos.y}] ");
                str.AppendLine();
                Debug.Log(str.ToString());
                str.Clear();
            }
        }

        public void DeleteMatchTiles()
        {
            for(int i=0; i<matches.Count; i++)
            {
                MatchList list = matches[i];
                foreach(Point2D pos in list)
                {
                    var tile = tiles[pos.x, pos.y];
                    _signalBus.Fire(new TileDeleteSignal(tile));

                    tiles[pos.x, pos.y] = null;
                }
            }
        }

        private bool IsEmptyPlace(int x, int y)
        {
             return tiles[x, y] == null;
        }

        // 이동 위치 계산 버그 있음. 
        public void FillTilesToEmptyPlace()
        {
            //1. 빈 자리로 이동 (밑으로 이동 규칙)
            for(int i=0; i<numCol; i++)
            {
                bool bCopy = false;
                int iCopy = numRow-1;
                for(int j=iCopy; j>=0; j--) // 바닥에서 위로
                {
                    if(bCopy)
                    {
                        if( !IsEmptyPlace(j, i) )
                        {
                            // 움직일 칸 수: iCopy - j
                            tiles[iCopy,i] = tiles[j,i];
                            tiles[iCopy,i].MoveTo(iCopy,i);
                            tiles[j,i] = null;

                            iCopy--;
                        }
                    }
                    else 
                    {
                        if( IsEmptyPlace(j, i) )
                        {
                            bCopy = true;
                            iCopy = j;
                        }
                    }
                }

                if( iCopy < numRow-1)
                    CreateTileAndSetPosition(iCopy, i);
            }
        }

        private void CreateTileAndSetPosition(int nNew, int col)
        {
            // 남은 빈 자리 확인
            Debug.LogFormat("{0}열 빈 자리 ", col);
            int iHoriCenter = NumCol / 2;
            int iVertCenter = NumRow / 2;
            float xOffset = GetOffset(NumCol);
            float yOffset = GetOffset(NumRow);
            float startOffset = nNew + 1;
            while (nNew >= 0)
            {
                CreateTile(nNew, col, typeList[Random.Range(0, typeList.Count)]);
                float y = iVertCenter - nNew + yOffset;
                float fromY = y + startOffset;
                float x = col - iHoriCenter + xOffset;
                tiles[nNew, col].SetPosition(x, fromY);
                tiles[nNew, col].MoveTo(x, y);

                Debug.LogFormat($"[{nNew},{col}]");
                nNew--;
            }
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

        public bool TrySwapTile(Tile tile, TileMovement move)
        {
            bool bSwap = false;
            Tile adjTile = GetAdjacentTile(tile, move);
            if( adjTile != null ) 
                bSwap = SwapTile(tile, adjTile);                

            // 리턴 false: 인접 없는 경우, 매치가 발생되지 않는 경우
            return bSwap;
        }

        public Tile GetAdjacentTile(Tile tile, TileMovement move)
        {
            var location = tile.GetLocation();
            Tile adjTile = null;
            if(move == TileMovement.Up && location.x > 0) 
                adjTile = tiles[location.x-1, location.y];
            else if(move == TileMovement.Down && location.x < GetLastIndexRow() )
                adjTile = tiles[location.x+1, location.y];
            else if(move == TileMovement.Left && location.y > 0)
                adjTile = tiles[location.x, location.y-1];
            else if(move == TileMovement.Right && location.y < GetLastIndexCol())
                adjTile = tiles[location.x, location.y+1];
            return adjTile;
        }

        Tile GetUpTile(Tile tile, int offset = 0)
        {
            var location = tile.GetLocation();
            return (location.x - offset > 0) ? tiles[location.x-(1+offset), location.y] : null;
        }

        public bool SwapTile(Tile src, Tile dst )
        {
            var srcPos = src.GetLocation();
            var dstPos = dst.GetLocation();

            // 둘이 바뀌었다고 가정하고 두 인접 타일에 대해 확인을 해야한다.
            tiles[srcPos.x,srcPos.y] = dst;
            tiles[dstPos.x,dstPos.y] = src;

            bool isMatch = (IsMatch3Tile(srcPos.x, srcPos.y) || IsMatch3Tile(dstPos.x, dstPos.y));
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
                tiles[srcPos.x,srcPos.y] = src;
                tiles[dstPos.x,dstPos.y] = dst;
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

        // 4. 규칙에 따라 타일 이동 시키기
        // -> 채우고 이동하는 듯... (확인필요)

        // 5. 빈 곳 타일 채우기

        // 6. 콤보

        // todo 
        // - tile pool 필요

        ///
        #region 시그널 처리 함수
        bool isFillTileSignal = false;
        public void OnFillTileSignal(FillTileSignal signal)
        {
            // 여러 번 들어와도 한번만 처리
            if(isFillTileSignal)
                return;

            isFillTileSignal = true;
            FillTilesToEmptyPlace();
        }

        

        #endregion
    }

}