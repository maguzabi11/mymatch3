using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

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

    public Tile()
    { IsChecked = false; }

    public Tile(int type) :this()
    {
        this.type = type;
    }

    public void SetTileObject(GameObject gameobject, GameObject parent)
    {
        gameTile = gameobject;
        gameTile.transform.parent = parent.transform;
    }

    public void SetPosition(float x, float y)
    {
        gameTile.transform.position = new Vector3(x,y);
    }
}

}