﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Zenject;

// using UniRx; // UniRx 도입 예정..

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


public class TileInput : MonoBehaviour
{
    public static bool blockInput;

    public class Factory : PlaceholderFactory<UnityEngine.Object, TileInput>
    {}

    [Inject]
    SignalBus _signalBus;

    Vector3 pos;

    public Tile refTile;

    // Start is called before the first frame update
    void Start()
    {
        pos = Vector3.zero;
    }

    void OnMouseDown()
    {
        pos = Input.mousePosition;
        Debug.Log($"OnMouseDown [{refTile.GetLocation().x}, {refTile.GetLocation().y}] {gameObject.name}, {gameObject.GetHashCode()}");                
    }

    // test 이벤트 핸들러
    void OnMouseDrag()
    {
        if( blockInput )
            return;

        // 얼만큼을 움직임으로 인식하고 끊을 것인지 정할 것
        var diff = Input.mousePosition - pos;

        if(diff.sqrMagnitude < 1f)
            return;

        var absX = Mathf.Abs(diff.x);
        var absY = Mathf.Abs(diff.y);
        if( absX < absY )
        {
            //Debug.Log($"y쪽이 더 움직임 {diff.y}");
            if( diff.y > 0f )
            {
                Debug.Log($"위로 이동");
                _signalBus.Fire(new TileMovementSignal(refTile, TileMovement.Up));
                blockInput = true;
            }
            else
            {
                Debug.Log($"아래로 이동");
                _signalBus.Fire(new TileMovementSignal(refTile, TileMovement.Down));
                blockInput = true;
            }
        }
        else if( absX > absY )
        {    
            //Debug.Log($"x쪽이 더 움직임 {diff.x}");
            if( diff.x > 0f )
            {
                Debug.Log($"오른쪽으로 이동");
                _signalBus.Fire(new TileMovementSignal(refTile, TileMovement.Right));
                blockInput = true;
            }
            else
            {
                Debug.Log($"왼쪽으로 이동");
                _signalBus.Fire(new TileMovementSignal(refTile, TileMovement.Left));
                blockInput = true;
            }
        }
    }

    void OnMouseUp()
    {
        Debug.Log($"{gameObject.name}, {gameObject.GetHashCode()}");
    }

    public void MoveSelfTest(TileMovement move)
    {
        // 인접 타일도 같이 움직여야 하므로
        // 여기서는 다른 클래스에 요청하는 걸로.
        // controller.ReqMoveTile(Tile refTile, move);

        if(move == TileMovement.Down)
            gameObject.transform. DOMoveY(-1f, 1f); // 코루틴 염두...
        // else if(TileMovement.Up)
        //     ;
        // else if(TileMovement.Left)
        //     ;
        // else if(TileMovement.Right)
        //     ;
    }


}

} // namespace