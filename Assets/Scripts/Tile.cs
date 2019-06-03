using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public Tile()
    { IsChecked = false; }

    public Tile(int type) :this()
    {
        this.type = type;
    }
}
