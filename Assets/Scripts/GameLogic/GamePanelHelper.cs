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
            // 3, 1, 3, 3, 3
            gamepanel.tiles[0, 0] = new Tile(3);
            gamepanel.tiles[0, 1] = new Tile(1);
            gamepanel.tiles[0, 2] = new Tile(3);
            gamepanel.tiles[0, 3] = new Tile(3);
            gamepanel.tiles[0, 4] = new Tile(3);

            // 2, 3, 3, 2, 1
            gamepanel.tiles[1, 0] = new Tile(2);
            gamepanel.tiles[1, 1] = new Tile(3);
            gamepanel.tiles[1, 2] = new Tile(3);
            gamepanel.tiles[1, 3] = new Tile(2);
            gamepanel.tiles[1, 4] = new Tile(1);

            // 2, 2, 3, 3, 2
            gamepanel.tiles[2, 0] = new Tile(2);
            gamepanel.tiles[2, 1] = new Tile(2);
            gamepanel.tiles[2, 2] = new Tile(3);
            gamepanel.tiles[2, 3] = new Tile(3);
            gamepanel.tiles[2, 4] = new Tile(2);

            // 2, 1, 3, 2, 3
            gamepanel.tiles[3, 0] = new Tile(2);
            gamepanel.tiles[3, 1] = new Tile(1);
            gamepanel.tiles[3, 2] = new Tile(3);
            gamepanel.tiles[3, 3] = new Tile(2);
            gamepanel.tiles[3, 4] = new Tile(3);
        }
    }

}
