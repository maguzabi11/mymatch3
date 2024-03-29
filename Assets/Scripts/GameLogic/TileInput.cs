﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Zenject;

// using UniRx; // UniRx 도입 예정..

namespace Match3
{


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
        Debug.Log($"OnMouseDown [{refTile.GetLocation().row}, {refTile.GetLocation().col}] {gameObject.name}, {gameObject.GetHashCode()}");                
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
                blockInput = true;
                _signalBus.Fire(new TileMovementSignal(refTile, TileMovement.Up));
            }
            else
            {
                Debug.Log($"아래로 이동");
                blockInput = true;
                _signalBus.Fire(new TileMovementSignal(refTile, TileMovement.Down));
            }
        }
        else if( absX > absY )
        {    
            //Debug.Log($"x쪽이 더 움직임 {diff.x}");
            if( diff.x > 0f )
            {
                Debug.Log($"오른쪽으로 이동");
                blockInput = true;
                _signalBus.Fire(new TileMovementSignal(refTile, TileMovement.Right));
            }
            else
            {
                Debug.Log($"왼쪽으로 이동");
                blockInput = true;
                _signalBus.Fire(new TileMovementSignal(refTile, TileMovement.Left));
            }
        }
    }

    void OnMouseUp()
    {
        Debug.Log($"{gameObject.name}, {gameObject.GetHashCode()}");
    }


}

} // namespace