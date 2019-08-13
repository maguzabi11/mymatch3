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

    public MatchType removeType;

    public bool IsChecked
    {
        get; set;
    }

    int nfound;
    public bool IsMatched
    {
        get {return nfound > 0;}
    }

    public void MarkFound()
    {
        IsChecked = true;
        nfound++;
    }

    public void ResetFound()
    {
        IsChecked = false;
        nfound = 0;        
    }

    public bool IsDeleted;

    // - 그리드 상의 자신의 위치는 필요한가? 
    // - 참조할 수 있는 인접 타일 정보가 필요한가?
    GameObject gameTile;

    Point2D location;
    public int row
    { get { return location.row; }}
    public int col
    { get { return location.col; }}    

    // 설정 값들은 데이터로
    const float duration = 0.5f;

    [Inject]
    GamePanel gp;

    SignalBus _signalBus;
    TileBuilder _tilebuilder;

    [Inject]
    public void Constructor(SignalBus signalBus, TileBuilder tilebuilder)
    {
        _signalBus = signalBus;
        _tilebuilder = tilebuilder;
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
                        //TileInput.blockInput = false; // FIX: 좀 더 후에 풀어야 함.
                        Debug.Log("교환 애니 종료");
                        gp.ProcessMatchTiles();
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

    // 이런 식으로 사용하면 시그널이 불필요하게 많이 전달이 된다.
    // 비효율적으로 판단하여 시그널을 사용하지 않기로 결정.
    public void OnDeleteSignal(TileDeleteSignal signal)
    {
        if( this != signal.tile ) {
            Debug.LogFormat($"this:{this.row}{this.col}, signal:{signal.tile.row}{signal.tile.col}");
            return;
        }

        gameTile.GetComponent<SpriteRenderer>().material.DOFade(0f, 1f)
            .OnComplete( ()=> { 
                Delete();
                _signalBus.Fire(new FillTileSignal());
            });
    }

    public void Execute()
    {
        IsDeleted = true;
        if(removeType != MatchType.Normal)
            gp.RemoveChain( row, col, removeType);
        //else
        DeleteWithFade();
    } 

    public void DeleteWithFade()
    {
        Debug.LogFormat($"this:{this.row}{this.col}");
        if( gameTile == null )
            return;

        gameTile.GetComponent<SpriteRenderer>().material.DOFade(0f, duration*1.5f)
            .OnComplete( ()=> { 
                Delete();
                _signalBus.Fire(new FillTileSignal()); // 여러 타일에서 보낼 필요가 없음...
            });
    }

    public void Delete()
    {
        gameTile.SetActive(false);
        Object.Destroy(gameTile);
        IsDeleted = false;
    }
 
    // 시그널로 하니 모든 타일이 대상이되므로 불필요한 메시지를 자주받게 된다.
    public void Attract(MatchInfo matchInfo, Tile dstTile)
    {
        if( dstTile.gameTile == null || gameTile == null )
            return;

        var vecTo = dstTile.gameTile.transform.position;
        gameTile.transform.DOMove(new Vector3(vecTo.x, vecTo.y), duration * 0.5f)
            .OnComplete( () => {
                Delete();
                _signalBus.Fire(new EndTileAttractionSignal(matchInfo));
            }
            );
    }    

    public void ChangeType(int type)
    {
        this.type = type;
    }

    public void SetRemoverType(MatchType remover)
    {
        removeType = remover;
        
        if( remover != MatchType.Normal && gameTile != null)
            ApplyRemoverVisual();
    }

    private void ApplyRemoverVisual()
    {
        Debug.Assert(gameTile != null);

        // reset
        var h_remover = gameTile.transform.Find("h_remover");
        if( h_remover != null )
            h_remover.gameObject.SetActive(false);
        var v_remover = gameTile.transform.Find("v_remover");
        if( v_remover != null )
            v_remover.gameObject.SetActive(false);

        // 타입마다 처리 방식이 다름
        if( removeType == MatchType.Horizon4 )
        {
            if( h_remover != null )
                h_remover.gameObject.SetActive(true);
        }
        else if( removeType == MatchType.Vertical4 )
        {
            if( v_remover != null )
                v_remover.gameObject.SetActive(true);
        }
        else
        // MatchType.Bomb && MatchType.Butterfly && MatchType.KindRemover
        {
            gameTile.GetComponent<SpriteRenderer>().sprite = _tilebuilder.GetSprite(removeType);
            var kind = gameTile.transform.Find("Kind");
            if( kind != null )
            {
                kind.gameObject.SetActive(true);
                kind.GetComponent<SpriteRenderer>().sprite = _tilebuilder.GetTileType(type);
            }
        } 
    }

    public void ChangeTile(Sprite sprite)
    {
        // 같은 스프라이트를 가리킨다?
        if( gameTile != null)
            gameTile.GetComponent<SpriteRenderer>().sprite = sprite;
    }
}

}