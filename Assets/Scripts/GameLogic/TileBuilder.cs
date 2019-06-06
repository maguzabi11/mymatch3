using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{

// 싱글턴이 적절
public class TileBuilder
{
    Dictionary< int, GameObject> dicTiles = new Dictionary<int, GameObject>();


    public TileBuilder()
    {
        InitTileResource(); // 로딩 시간이 걸릴 수 있음. 다른 곳에서 호출 권장.
    }

    public void InitTileResource()
    {
        GameObject tile;
        tile = Resources.Load<GameObject>("Food1");
        dicTiles.Add(1, tile); // 추후 data로 분리, list<> scrptable object
        tile = Resources.Load<GameObject>("Food2");
        dicTiles.Add(2, tile);
        tile = Resources.Load<GameObject>("Food3");
        dicTiles.Add(3, tile);
        tile = Resources.Load<GameObject>("Food4");
        dicTiles.Add(4, tile);
    }

    public GameObject CreateTileResource(int type)
    {
        GameObject tile;
        bool bGet = dicTiles.TryGetValue(type, out tile);
        if( bGet )
            return GameObject.Instantiate(tile); // pool 로 확장할 것.
        else
            return null;        
    }

    public void BindTileResource(Tile tile, GameObject root)
    {
        tile.SetTileObject( CreateTileResource(tile.Type), root );
    }
}

}