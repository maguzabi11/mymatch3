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
    }

}
