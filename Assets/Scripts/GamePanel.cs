using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

        List<MatchList> matches = new List<MatchList>();

        public GamePanel(int width, int height)
        {
            tiles = new Tile[width, height];
            numRow = width;
            numCol = height;
        }

        public bool CreateTiles()
        {
            int row = tiles.GetLength(0);
            int col = tiles.GetLength(1);

            for(int i=0; i<row; i++)
                for(int j=0; j<col; j++)
                    tiles[i,j] = new Tile(Random.Range(1, 4));

            return true;
        }

        public void OutputTiles()
        {
            StringBuilder Line = new StringBuilder();

            for (int i = 0; i < numRow; i++)
            {
                for (int j = 0; j < numCol; j++)
                {
                    Line.Append($"{tiles[i, j].Type}");
                    if(j < numCol)
                        Line.Append(" ");
                }
                Debug.Log(Line.ToString());
                Line.Clear();
            }      

        }

        // 테스트를 위해 특정 값으로 채워 넣음
        public void CreateSpecificTilesforTest()
        {
            // 3, 1, 3, 3, 3
            tiles[0, 0] = new Tile(3);
            tiles[0, 1] = new Tile(1);
            tiles[0, 2] = new Tile(3);
            tiles[0, 3] = new Tile(3);
            tiles[0, 4] = new Tile(3);

            // 2, 3, 3, 2, 1
            tiles[1, 0] = new Tile(2);
            tiles[1, 1] = new Tile(3);
            tiles[1, 2] = new Tile(3);
            tiles[1, 3] = new Tile(2);
            tiles[1, 4] = new Tile(1);

            // 2, 2, 3, 3, 2
            tiles[2, 0] = new Tile(2);
            tiles[2, 1] = new Tile(2);
            tiles[2, 2] = new Tile(3);
            tiles[2, 3] = new Tile(3);
            tiles[2, 4] = new Tile(2);

            // 2, 1, 3, 2, 3
            tiles[3, 0] = new Tile(2);
            tiles[3, 1] = new Tile(1);
            tiles[3, 2] = new Tile(3);
            tiles[3, 3] = new Tile(2);
            tiles[3, 4] = new Tile(3);
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
            for (int i = 0; i < numRow; i++)
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