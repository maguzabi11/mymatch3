using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Match3;

public class TileController : MonoBehaviour
{
    [Inject]
    TileBuilder tilebuilder;

    GamePanel gp;

    [SerializeField]
    GameObject tileRoot;

    void Awake()
    {
        tileRoot = gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitTiles();
    }

    public void InitTiles()
    {
        gp = new GamePanel(4, 5);
        gp.CreateTilesWithoutMatch3(tilebuilder, tileRoot); // 반복된 루프 호출을 피하기 위함.
        gp.SetTilePosition();
    }

    // 작업 중
    // 이름 변경 후보: TrySwapAdjacentTile
    public void ReqMoveTile(Tile tile, TileMovement move)
    {
        var location = tile.GetLocation();
        Tile adjTile = null;

        bool bYoyo = !(gp.IsMatch3Tile(location.x, location.y));

        // 방향에 따른 인접 타일과 이동할 위치 구하기, 이동 애니메이션을 설정하기
        if(move == TileMovement.Up ) 
        {
            if(location.y > 0) 
            {
                adjTile = gp.tiles[location.x, location.y-1];
                
            }
        }
        else if(move == TileMovement.Down) 
        {
            if(location.y < gp.GetLastIndexRow()) 
            {
                adjTile = gp.tiles[location.x, location.y+1];
                tile.MoveSwap(adjTile);
            }
        }
        else if(move == TileMovement.Left)
        {
             if(location.x > 0 )
             {
                adjTile = gp.tiles[location.x-1, location.y];
             }
        }
        else if(move == TileMovement.Right)
        {
            if(location.x < gp.GetLastIndexCol())
            {
                adjTile = gp.tiles[location.x+1, location.y];
            }
        }        
    }

}
