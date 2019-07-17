using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public static class GamePanelHelper
    {
        // 테스트를 위해 특정 값으로 채워 넣음
        public static void CreateSpecificTilesforTest(this GamePanel gamepanel)
        {
            // 3, 1, 3, 3, 1
            // 2, 3, 4, 2, 3
            // 2, 2, 3, 3, 2
            // 2, 1, 3, 2, 3
            CreateTileRow( gamepanel, 0, new int[]{3,1,3,3,1});
            CreateTileRow( gamepanel, 1, new int[]{2,3,4,2,3});
            CreateTileRow( gamepanel, 2, new int[]{2,2,3,3,2});
            CreateTileRow( gamepanel, 3, new int[]{2,1,3,2,3});
        }

        // 생성 유틸함수
        public static void CreateTileRow(this GamePanel gamepanel, int row, int[] types)
        {
            // 경계 검사
            if( types.Length < gamepanel.NumCol)
                return;

            for(int i = 0; i<gamepanel.NumCol; i++)
                gamepanel.CreateTileWithoutRc(row, i, types[i]);
        }

        public static void CreateforMatchTileTestCase(this GamePanel gamepanel)
        {
            // 1 1 2 3 1
            // 3 2 2 2 4
            // 1 3 2 4 1
            // 2 1 3 3 1
            CreateTileRow( gamepanel, 0, new int[]{1,1,2,3,1});
            CreateTileRow( gamepanel, 1, new int[]{3,2,2,2,4});
            CreateTileRow( gamepanel, 2, new int[]{1,3,2,4,1});
            CreateTileRow( gamepanel, 3, new int[]{2,1,3,3,1});
        }

        // 매치 가능 없는 경우
        public static void CreateNotMatchableCaseAfterDelete(this GamePanel gamepanel)
        {
            /*
                1 4 - 2 5
                2 3 - 1 5
                2 1 - 3 2
                3 1 1 2 3 
                * 쉽게 확인하기 위해 현재 정의되지 않는 5를 넣음
            */
            CreateTileRow( gamepanel, 0, new int[]{1,4,3,2,5}); // null을 넣을 수 없으므로 제거 코드 필요.
            CreateTileRow( gamepanel, 1, new int[]{2,3,3,1,5});
            CreateTileRow( gamepanel, 2, new int[]{2,1,3,3,2});
            CreateTileRow( gamepanel, 3, new int[]{3,1,1,2,3});
        }

        public static void CreateMatch4(this GamePanel gamepanel)
        {
            /*
                1 1 1 1 5
                2 3 4 2 5
                2 1 4 3 5
                3 1 1 2 5 
                * 쉽게 확인하기 위해 현재 정의되지 않는 5를 넣음
            */
            CreateTileRow( gamepanel, 0, new int[]{1,1,1,1,5});
            CreateTileRow( gamepanel, 1, new int[]{2,3,4,2,5});
            CreateTileRow( gamepanel, 2, new int[]{2,1,4,3,5});
            CreateTileRow( gamepanel, 3, new int[]{3,1,1,2,5});
        }

        public static void CreateMatchWithCross(this GamePanel gamepanel)
        {
            /*
                4 4 4 1 5
                2 3 4 2 5
                2 1 4 3 5
                3 1 1 2 5 
                * 쉽게 확인하기 위해 현재 정의되지 않는 5를 넣음
            */
            CreateTileRow( gamepanel, 0, new int[]{4,4,4,1,5});
            CreateTileRow( gamepanel, 1, new int[]{2,3,4,2,5});
            CreateTileRow( gamepanel, 2, new int[]{2,1,4,3,5});
            CreateTileRow( gamepanel, 3, new int[]{3,1,1,2,5});
        }

        public static void CreateMatchSqure(this GamePanel gamepanel)
        {

        }        
    }

}
