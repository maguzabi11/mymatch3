using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using DG.Tweening;

namespace Match3
{

public class Tile
{
    int type;
    public int Type
    { get { return type; } }

    public bool IsChecked
    {
        get; set;
    }

    // - 그리드 상의 자신의 위치는 필요한가? 
    // - 참조할 수 있는 인접 타일 정보가 필요한가?
    GameObject gameTile;

    Point2D location;

    public Tile()
    { IsChecked = false; }

    public Tile(int type) :this()
    {
        this.type = type;
    }

    public Tile(int type, Point2D point) :this(type)
    {
        location = point;
    }

    public void SetTileObject(GameObject gameobject, GameObject parent)
    {
        gameTile = gameobject;
        gameTile.transform.parent = parent.transform;

        var tileInput = gameTile.GetComponent<TileInput>();
        if( tileInput != null )
            tileInput.refTile = this;
    }

    public void SetPosition(float x, float y)
    {
        if( gameTile != null)
            gameTile.transform.position = new Vector3(x,y);
    }

    public void SetLocation(Point2D location)
    {
        this.location = location;
    }

    public Point2D GetLocation()
    {
        return location;
    }

    public void MoveSwap(Tile tile)
    {
        var vecTo = tile.gameTile.transform.position; // 참조 가능?
        var vecFrom = gameTile.transform.position;
        gameTile.transform.DOMove(vecTo, 1);
        tile.gameTile.transform.DOMove(vecFrom, 1);        
    }
}

}