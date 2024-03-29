﻿using System.Collections;
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

public class EndTileAttractionSignal
{
    public MatchInfo matchInfo;
    public MatchType match;

    public EndTileAttractionSignal(MatchInfo matchInfo)
    {
        this.matchInfo = matchInfo;
        match = matchInfo.matchType;
    }

}

public class SpecialTileCreateSignal
{
    public MatchInfo match;

    public SpecialTileCreateSignal(MatchInfo match)
    {
        this.match = match;
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