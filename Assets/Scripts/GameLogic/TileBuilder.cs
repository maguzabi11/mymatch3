using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Match3
{

// 싱글턴이 적절
public class TileBuilder
{
    Dictionary< int, Object> dicTiles = new Dictionary<int, Object>();

    [Inject]
    TileInput.Factory _factory;

    public GameObject tileRoot;

    public TileBuilder()
    {
        InitTileResource(); // 로딩 시간이 걸릴 수 있음. 다른 곳에서 호출 권장.
    }

    public void InitTileResource()
    {
        Object tile;
        tile = Resources.Load("Food1");
        dicTiles.Add(1, tile); // 추후 data로 분리, list<> scrptable object
        tile = Resources.Load("Food2");
        dicTiles.Add(2, tile);
        tile = Resources.Load("Food3");
        dicTiles.Add(3, tile);
        tile = Resources.Load("Food4");
        dicTiles.Add(4, tile);
    }
   
    public GameObject CreateTileResource(int type)
    {
        Object tile;
        bool bGet = dicTiles.TryGetValue(type, out tile);
        if( bGet )
        {
            var tileInput = _factory.Create(tile);
            return tileInput.gameObject; // GameObject.Instantiate(tile); // pool 로 확장할 것.
        }
        else
            return null;        
    }

    public void BindTileResource(Tile tile)
    {
        tile.SetTileObject( CreateTileResource(tile.Type), tileRoot );
    }

    public void ChangeTileType(Tile tile, int type)
    {
        Object findtile;
        bool bGet = dicTiles.TryGetValue(type, out findtile);
        var objTile = findtile as GameObject; // 되는 건가?
        tile.ChangeType(type);
        tile.ChangeTile(objTile.GetComponent<SpriteRenderer>().sprite);
    }
}

}