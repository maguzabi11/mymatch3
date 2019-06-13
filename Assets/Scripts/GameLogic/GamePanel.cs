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

        public GamePanel(int width, int height)
        {
            tiles = new Tile[width, height];
            numRow = width;
            numCol = height;
        }

        public void CreateTilesWithoutMatch3(TileBuilder builder, GameObject root)
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
                    if( nCheckHori >= 2)
                    {
                        if(type == tiles[i,j-1].Type && tiles[i,j-1].Type == tiles[i,j-2].Type )
                        {
                            tmpTypeList.Remove(type);
                            removeTypeList.Add(type);
                            type = tmpTypeList[Random.Range(0, tmpTypeList.Count)];
                            nCheckHori = 0;
                        }
                    }
                    nCheckHori++;

                    if(i >= 2)
                    {
                        if(type == tiles[i-1,j].Type && tiles[i-1,j].Type == tiles[i-2,j].Type )
                        {
                            tmpTypeList.Remove(type);
                            removeTypeList.Add(type);
                            type = tmpTypeList[Random.Range(0, tmpTypeList.Count)];
                        }
                    }
                    
                    if( removeTypeList.Count > 0 )
                    {
                        tmpTypeList.AddRange(removeTypeList);
                        removeTypeList.Clear();
                    }

                    // 
                    var newTile = new Tile(type, new Point2D(i,j)); //
                    tiles[i,j] = newTile;
                    if(builder != null)
                        builder.BindTileResource(newTile, root);
                }
            }

        }

        public bool CreateTiles()
        {
            int row = tiles.GetLength(0);
            int col = tiles.GetLength(1);

            for(int i=0; i<row; i++)
                for(int j=0; j<col; j++)
                    tiles[i,j] = new Tile(Random.Range(1, 5));

            return true;
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

        // 필요한 인터페이스
        // 매칭 타일 찾기
        // 1. 전체 검사
        // 2. 특정 위치에서 매칭 검사
        public int FindAllMatches()
        {
            ResetSearch();
            FindMatchInfo findinfo = new FindMatchInfo();

            for (int i = 0; i < numRow; i++)
            {
                for (int j = 0; j < numCol; j++)
                {
                    FindMatchingTiles(i, j, findinfo);
                    // 매치가 확인 된 순간
                    if( findinfo.isMatch )
                         matches.Add(new MatchList(findinfo.matchlist)); // 리스트 복사해야할 듯.

                    findinfo.Reset();
                }
            }

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
                    tiles[pos.x, pos.y] = null;
                }
            }
        }

        public void FillTilesToEmptyPlace()
        {
            //1. 빈 자리로 이동 (밑으로 이동 규칙)
            for(int i=0; i<numCol; i++)
            {
                bool bCopy = false;
                int iCopy = numRow-1;
                for(int j=iCopy; j>=0; j--)
                {
                    if(bCopy)
                    {
                        if( tiles[j,i] != null )
                        {
                            // 움직일 칸 수: iCopy - j
                            tiles[iCopy,i] = tiles[j,i];
                            iCopy++;
                            tiles[j,i] = null;
                        }
                    }
                    else 
                    {
                        if( tiles[j,i] == null )
                        {
                            bCopy = true;
                            iCopy = j;
                        }
                    }
                }
            }

            //2. 새로운 빈 자리는 새로운 랜덤 타일로 채운다.


        }

        public void SetTilePosition()
        {
            if(NumCol == 0 || NumRow == 0 )
                return;
            // 타일 배치
            /*
            - 홀수(3이상): i-iCenter;
            - 짝수 개일 때 위치: i - iCenter + 0.5
            */        
            int iHoriCenter = NumCol / 2;
            int iVertCenter = NumRow / 2;
            float xOffset = (NumCol%2 == 0) ? 0.5f : 0f;
            float yOffset = (NumRow%2 == 0) ? 0.5f : 0f;
            for( int i=0; i<NumRow; i++)
            {
                for( int j=0; j<NumCol; j++)
                {
                    float y = i-iVertCenter + yOffset;
                    float x = j-iHoriCenter + xOffset;
                    tiles[i,j].SetPosition(x, y);
                }
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

    }

}