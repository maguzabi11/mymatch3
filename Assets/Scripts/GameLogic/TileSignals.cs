using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Match3
{
public class TileMovementSignal
{
    public Tile tile;
    public TileMovement movement;

    public TileMovementSignal(Tile tile, TileMovement move)
    {
        this.tile =tile;
        movement = move;
    }
}

public class TileDeleteSignal
{
    public Tile tile;

    public TileDeleteSignal(Tile tile)
    {
        this.tile = tile;
    }
}

public class FillTileSignal
{}

public class TileDropSignal
{
    public Tile tile;
    public TileDropSignal(Tile tile)
    {
        this.tile = tile;
    }    
}

}