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
    Dictionary< int, Object> dicRemover = new Dictionary<int, Object>();

    [Inject]
    TileInput.Factory _factory;

    public GameObject tileRoot;

    public TileBuilder()
    {
        InitTileResource(); // 로딩 시간이 걸릴 수 있음. 다른 곳에서 호출 권장.
        InitRemoverResource();
    }

    // todo: 시각화에 영향을 주는 요소가 늘어난 것에 대한 수정 작업 필요
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
        tile = Resources.Load("Food5");
        dicTiles.Add(5, tile);        
    }

    private void InitRemoverResource()
    {
        Object tile;
        tile = Resources.Load("Remover1");
        dicRemover.Add((int)MatchType.Butterfly, tile);
        tile = Resources.Load("Remover4");
        dicRemover.Add((int)MatchType.Bomb, tile);        
        tile = Resources.Load("Remover5");
        dicRemover.Add((int)MatchType.KindRemover, tile);
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

    // public GameObject CreateTileResource(int type, MatchType matchType)

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

    public Sprite GetSprite(MatchType matchType)
    {
        Object objFound;
        dicRemover.TryGetValue((int)matchType, out objFound);
        var objSprite = objFound as GameObject;
        return objSprite.GetComponent<SpriteRenderer>().sprite;
    }
}

}