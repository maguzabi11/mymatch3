using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using DG.Tweening;
using Object = UnityEngine.Object;

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

    int nfound;
    public bool IsMatched
    {
        get {return nfound > 0;}
    }

    public void MarkSearch()
    {
        IsChecked = true;
        nfound++;
    }

    public void ResetSearch()
    {
        IsChecked = false;
        nfound = 0;        
    }

    // - 그리드 상의 자신의 위치는 필요한가? 
    // - 참조할 수 있는 인접 타일 정보가 필요한가?
    GameObject gameTile;

    Point2D location;

    // 설정 값들은 데이터로
    const float duration = 0.5f;

    [Inject]
    GamePanel gp;

    SignalBus _signalBus;

    [Inject]
    public void Constructor(SignalBus signalBus)
    {
        _signalBus = signalBus;
        _signalBus.Subscribe<TileDeleteSignal>(OnDeleteSignal);
    }
  

    public class Factory : PlaceholderFactory<int, Point2D, Tile>
    {}

    public Tile(int type, Point2D point)
    {
        IsChecked = false;
        this.type = type;
        location = point;
    }


    public void SetTileObject(GameObject gameobject, GameObject parent)
    {
        gameTile = gameobject;
        if( parent != null)
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

    public void MoveTo(int row, int col)
    {
        if( gameTile == null )
            return;

        // 이동할 양
        var moveY = location.row - row; // 수직 이동
        var moveX = location.col - col; // 수평 이동

        var vecTo = gameTile.transform.position;
        vecTo.x += moveX;
        vecTo.y += moveY;

        Debug.LogFormat("moveTo [{0}, {1}] -> [{2}, {3}]", 
            location.row, location.col, row, col);

        PlayMove(vecTo);
        location.row = row;
        location.col = col;
    }

    public void MoveTo(float x, float y)
    {
        if (gameTile == null)
            return;

        PlayMove(new Vector3(x,y));
    }

    private void PlayMove(Vector3 vecTo)
    {
        gameTile.transform.DOMove(vecTo, duration).SetEase(Ease.InCirc)
            .OnComplete(() =>
            {
                //Debug.Log("떨어졌어요~");
                _signalBus.Fire(new TileDropSignal(this));
            });
    }

    public void MoveSwap(Tile tile, bool bYoyo = false)
    {
        
        if( tile.gameTile == null || gameTile == null )
            return;

        var vecTo = tile.gameTile.transform.position; // 참조 가능?
        var vecFrom = gameTile.transform.position;

        if( bYoyo == false)
        {
            gameTile.transform.DOMove(new Vector3(vecTo.x, vecTo.y), duration);
            tile.gameTile.transform.DOMove(new Vector3(vecFrom.x, vecFrom.y), duration)
                .OnComplete( () => {
                        TileInput.blockInput = false; // FIX: 좀 더 후에 풀어야 함.
                        Debug.Log("교환 애니 종료");
                        gp.DeleteMatchTiles();
                    });
        }
        else
        {
            gameTile.transform.DOMove(new Vector3(vecTo.x, vecTo.y), duration)
            .SetLoops(2, LoopType.Yoyo);

            tile.gameTile.transform.DOMove(new Vector3(vecFrom.x, vecFrom.y), duration)
                .OnComplete( () => { TileInput.blockInput = false;})
                .SetLoops(2, LoopType.Yoyo);
        }
    }

    public void SwapLocation(Tile tile)
    {
        var tmpLocation = tile.GetLocation();

        Debug.LogFormat($"[{location.row},{location.col}]과 [{tmpLocation.row},{tmpLocation.col}] 교환");
        // 로케이션 교환
        
        tile.SetLocation(location);
        location = tmpLocation;
    }

    public void OnDeleteSignal(TileDeleteSignal signal)
    {
        if( this != signal.tile )
            return;

        gameTile.GetComponent<SpriteRenderer>().material.DOFade(0f, 1f)
            .OnComplete( ()=> { 
                Delete();
                _signalBus.Fire(new FillTileSignal()); // 여러 타일에서 보낼 필요가 없음...
            });
    }

    public void Delete()
    {
        gameTile.SetActive(false);
        Object.Destroy(gameTile);
    }
}

}